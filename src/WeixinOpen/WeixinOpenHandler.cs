﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Myvas.AspNetCore.Authentication.WeixinOpen.Internal;
using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Base64UrlTextEncoder = Microsoft.AspNetCore.Authentication.Base64UrlTextEncoder;

namespace Myvas.AspNetCore.Authentication;

internal class WeixinOpenHandler : RemoteAuthenticationHandler<WeixinOpenOptions>
{
    protected HttpClient Backchannel => Options.Backchannel;

    /// <summary>
    /// The handler calls methods on the events which give the application control at certain points where processing is occurring. 
    /// If it is not provided a default instance is supplied which does nothing when the methods are called.
    /// </summary>
    protected new OAuthEvents Events
    {
        get { return (OAuthEvents)base.Events; }
        set { base.Events = value; }
    }

    private readonly IWeixinOpenApi _api;

#if NET8_0_OR_GREATER
    /// <summary>
    /// Initializes a new instance of <see cref="WeixinOpenHandler"/>.
    /// </summary>
    /// <inheritdoc />
    [Obsolete("ISystemClock is obsolete, use TimeProvider on AuthenticationSchemeOptions instead.")]
    public WeixinOpenHandler(
        IWeixinOpenApi api,
        IOptionsMonitor<WeixinOpenOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, loggerFactory, encoder, clock)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
    }

    /// <summary>
    /// Initializes a new instance of <see cref="WeixinOpenHandler"/>.
    /// </summary>
    /// <inheritdoc />
    public WeixinOpenHandler(
        IWeixinOpenApi api,
        IOptionsMonitor<WeixinOpenOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder)
        : base(options, loggerFactory, encoder)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
    }
#else
    /// <summary>
    /// Initializes a new instance of <see cref="WeixinOpenHandler"/>.
    /// </summary>
    /// <inheritdoc />
    public WeixinOpenHandler(
        IWeixinOpenApi api,
        IOptionsMonitor<WeixinOpenOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, loggerFactory, encoder, clock)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
    }
#endif

    //protected const string CorrelationPrefix = ".AspNetCore.Correlation.";
    protected const string CorrelationMarker = "N";
    protected const string CorrelationProperty = ".xsrf";
    //protected const string AuthSchemeKey = ".AuthScheme";
    //protected static readonly RandomNumberGenerator CryptoRandom = RandomNumberGenerator.Create();

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        if (string.IsNullOrEmpty(properties.RedirectUri))
        {
            properties.RedirectUri = OriginalPathBase + OriginalPath + Request.QueryString;
        }

        // OAuth2 10.12 CSRF
        GenerateCorrelationId(properties);

        var authorizationEndpoint = BuildChallengeUrl(properties, BuildRedirectUri(Options.CallbackPath));
        var redirectContext = new RedirectContext<OAuthOptions>(
            Context, Scheme, Options,
            properties, authorizationEndpoint);
        await Events.RedirectToAuthorizationEndpoint(redirectContext);

        var location = Context.Response.Headers[HeaderNames.Location];
        if (location == StringValues.Empty)
        {
            location = "(not set)";
        }
        var cookie = Context.Response.Headers[HeaderNames.SetCookie];
        if (cookie == StringValues.Empty)
        {
            cookie = "(not set)";
        }
        Logger.HandleChallenge(location, cookie);
    }

    /// <summary>
    /// 生成网页授权调用URL，用于获取code。（然后可以用此code换取网页授权access_token）
    /// </summary>
    /// <param name="properties"></param>
    /// <param name="redirectUri">跳转回调redirect_uri，应当使用https链接来确保授权code的安全性。请在传入前使用UrlEncode对链接进行处理。</param>
    /// <returns></returns>
    protected virtual string BuildChallengeUrl(AuthenticationProperties properties, string redirectUri)
    {
        // 注意：参数只有五个，顺序不能改变！微信对该链接做了正则强匹配校验，如果链接的参数顺序不对，授权页面将无法正常访问!!!
        var queryStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        queryStrings.Add("appid", Options.AppId);
        queryStrings.Add("redirect_uri", redirectUri);
        queryStrings.Add("response_type", "code");

        var scope = PickAuthenticationProperty(properties, OAuthChallengeProperties.ScopeKey, FormatScope, Options.Scope);
        queryStrings.Add(OAuthChallengeProperties.ScopeKey, scope);

        // 测试表明properties添加returnUrl和scheme后，state为1264字符，此时报错：state参数过长。
        // 所以properties只能存放在Cookie中，state作为Cookie值的索引键。
        // 腾讯规定state最长128字节，所以properties只能存放在Cookie中，state作为Cookie值的索引键。
        // https://mp.weixin.qq.com/wiki?t=resource/res_main&id=mp1421140842
        var correlationId = properties.Items[CorrelationProperty];
        queryStrings.Add("state", correlationId);

        var protectedProperties = Options.StateDataFormat.Protect(properties);
        // Clean up old cookies with pattern: "Options.CorrelationCookie.Name + Scheme.Name + "." + correlationId + "." + CorrelationMarker"
        var deprecatedCookieNames = Context.Request.Cookies.Keys.Where(x => x.StartsWith(Options.CorrelationCookie.Name + Scheme.Name + "."));// && x.EndsWith("."+CorrelationMarker));
        deprecatedCookieNames.ToList().ForEach(x => Context.Response.Cookies.Delete(x));//, cookieOptions));
                                                                                        // Append a response cookie for state/properties
#if NET8_0_OR_GREATER
        var cookieOptions = Options.CorrelationCookie.Build(Context, TimeProvider.GetUtcNow());
#else
        var cookieOptions = Options.CorrelationCookie.Build(Context, Clock.UtcNow);
#endif
        Context.Response.Cookies.Append(FormatStateCookieName(correlationId), protectedProperties, cookieOptions);

        var authorizationUrl = QueryHelpers.AddQueryString(Options.AuthorizationEndpoint, queryStrings);
        return authorizationUrl + "#wechat_redirect";
    }

    #region To satisfy too big protected properties, we should store it to cookie '.{CorrelationCookieName}.{SchemeName}.{CorrelationMarker}.{CorrelationId|state}'
    protected virtual string FormatCorrelationCookieName(string correlationId)
    {
        //return Options.CorrelationCookie.Name + correlationId;
        return Options.CorrelationCookie.Name + Scheme.Name + "." + correlationId;
    }

    protected virtual string FormatStateCookieName(string correlationId)
    {
        //return Options.CorrelationCookie.Name + CorrelationMarker + "." + correlationId;
        return Options.CorrelationCookie.Name + Scheme.Name + "." + CorrelationMarker + "." + correlationId;
    }

    /// <inheritdoc/>
    protected override void GenerateCorrelationId(AuthenticationProperties properties)
    {
        //base.GenerateCorrelationId(properties);

        if (properties == null)
        {
            throw new ArgumentNullException(nameof(properties));
        }

        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        var correlationId = Base64UrlTextEncoder.Encode(bytes);
#if NET8_0_OR_GREATER
        var cookieOptions = Options.CorrelationCookie.Build(Context, TimeProvider.GetUtcNow());
#else
        var cookieOptions = Options.CorrelationCookie.Build(Context, Clock.UtcNow);
#endif

        properties.Items[CorrelationProperty] = correlationId;

        //var cookieName = Options.CorrelationCookie.Name + correlationId;
        var cookieName = FormatCorrelationCookieName(correlationId);

        Response.Cookies.Append(cookieName, CorrelationMarker, cookieOptions);
    }

    /// <inheritdoc/>
    protected override bool ValidateCorrelationId(AuthenticationProperties properties)
    {
        //return base.ValidateCorrelationId(properties);

        if (properties == null)
        {
            throw new ArgumentNullException(nameof(properties));
        }

        if (!properties.Items.TryGetValue(CorrelationProperty, out var correlationId))
        {
            Logger.LogWarning($"The CorrectionId not found in '{Options.CorrelationCookie.Name!}'");
            return false;
        }
        properties.Items.Remove(CorrelationProperty);

        //var cookieName = Options.CorrelationCookie.Name + correlationId;
        var cookieName = FormatCorrelationCookieName(correlationId);

        var correlationCookie = Request.Cookies[cookieName];
        if (string.IsNullOrEmpty(correlationCookie))
        {
            Logger.LogWarning($"The CorrelationCookie not found in '{cookieName}'");
            return false;
        }

#if NET8_0_OR_GREATER
        var cookieOptions = Options.CorrelationCookie.Build(Context, TimeProvider.GetUtcNow());
#else
        var cookieOptions = Options.CorrelationCookie.Build(Context, Clock.UtcNow);
#endif
        Response.Cookies.Delete(cookieName, cookieOptions);

        if (!string.Equals(correlationCookie, CorrelationMarker, StringComparison.Ordinal))
        {
            Logger.LogWarning($"Unexcepted CorrelationCookieValue: '{cookieName}'='{correlationCookie}'");
            return false;
        }

        return true;
    }
    #endregion

    #region Pick value from AuthenticationProperties
    private static string PickAuthenticationProperty<T>(
        AuthenticationProperties properties,
        string name,
        Func<T, string> formatter,
        T defaultValue)
    {
        string value = null;
        var parameterValue = properties.GetParameter<T>(name);
        if (parameterValue != null)
        {
            value = formatter(parameterValue);
        }
        else if (!properties.Items.TryGetValue(name, out value))
        {
            value = formatter(defaultValue);
        }

        // Remove the parameter from AuthenticationProperties so it won't be serialized into the state
        properties.Items.Remove(name);

        return value;
    }

    private static string PickAuthenticationProperty(
        AuthenticationProperties properties,
        string name,
        string defaultValue = null)
        => PickAuthenticationProperty(properties, name, x => x, defaultValue);
    #endregion

    protected virtual string FormatScope(IEnumerable<string> scopes)
            => string.Join(",", scopes); // OAuth2 3.3 space separated, but weixin not

    protected virtual List<string> SplitScope(string scope)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(scope)) return result;
        return scope.Split(',').ToList();
    }

    protected override async Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
    {
        var query = Request.Query;

        var error = query["error"];
        if (!StringValues.IsNullOrEmpty(error))
        {
            var failureMessage = new StringBuilder();
            failureMessage.Append(error);
            var errorDescription = query["error_description"];
            if (!StringValues.IsNullOrEmpty(errorDescription))
            {
                failureMessage.Append(";Description=").Append(errorDescription);
            }
            var errorUri = query["error_uri"];
            if (!StringValues.IsNullOrEmpty(errorUri))
            {
                failureMessage.Append(";Uri=").Append(errorUri);
            }

            return HandleRequestResult.Fail(failureMessage.ToString());
        }

        var state = query["state"]; // ie. correlationId
        if (StringValues.IsNullOrEmpty(state))
        {
            return HandleRequestResult.Fail("The oauth state was missing.");
        }

        var stateCookieName = FormatStateCookieName(state);
        var protectedProperties = Request.Cookies[stateCookieName];
        if (string.IsNullOrEmpty(protectedProperties))
        {
            Logger.LogError($"The protected properties not found in cookie '{stateCookieName}'");
            return HandleRequestResult.Fail($"The oauth state cookie was missing: Cookie: {stateCookieName}");
        }
        else
        {
            Logger.LogDebug($"The protected properties found in cookie '{stateCookieName}' with value '{protectedProperties}'");
        }

        var properties = Options.StateDataFormat.Unprotect(protectedProperties);

        if (properties == null)
        {
            return HandleRequestResult.Fail($"The oauth state cookie was invalid: Cookie: {stateCookieName}");
        }

        // OAuth2 10.12 CSRF
        if (!ValidateCorrelationId(properties))
        {
            return HandleRequestResult.Fail("Correlation failed.", properties);
        }

        // Cleanup state & correlation cookie
        Response.Cookies.Delete(stateCookieName);
        var correlationCookieName = FormatCorrelationCookieName(state);
        Response.Cookies.Delete(correlationCookieName);
        Logger.LogDebug($"Cookies deleted: '{stateCookieName}' and '{correlationCookieName}'");

        var code = query["code"];

        if (StringValues.IsNullOrEmpty(code))
        {
            Logger.LogWarning("Code was not found.");
            return HandleRequestResult.Fail("Code was not found.", properties);
        }

        //var codeExchangeContext = new OAuthCodeExchangeContext(properties, code, BuildRedirectUri(Options.CallbackPath));
        //using var tokens = await ExchangeCodeAsync(codeExchangeContext);
        using var tokens = await ExchangeCodeAsync(code, BuildRedirectUri(Options.CallbackPath));

        if (tokens.Error != null)
        {
            return HandleRequestResult.Fail(tokens.Error, properties);
        }

        if (string.IsNullOrEmpty(tokens.AccessToken))
        {
            return HandleRequestResult.Fail("Failed to retrieve access token.", properties);
        }

        var identity = new ClaimsIdentity(ClaimsIssuer);

        if (Options.SaveTokens)
        {
            var authTokens = new List<AuthenticationToken>();

            authTokens.Add(new AuthenticationToken { Name = WeixinOpenTokenNames.access_token, Value = tokens.AccessToken });
            if (!string.IsNullOrEmpty(tokens.RefreshToken))
            {
                authTokens.Add(new AuthenticationToken { Name = WeixinOpenTokenNames.refresh_token, Value = tokens.RefreshToken });
            }
            if (!string.IsNullOrEmpty(tokens.TokenType))
            {
                authTokens.Add(new AuthenticationToken { Name = WeixinOpenTokenNames.token_type, Value = tokens.TokenType });
            }
            if (!string.IsNullOrEmpty(tokens.GetOpenId()))
            {
                authTokens.Add(new AuthenticationToken { Name = WeixinOpenTokenNames.openid, Value = tokens.GetOpenId() });
            }
            if (!string.IsNullOrEmpty(tokens.GetUnionId()))
            {
                authTokens.Add(new AuthenticationToken { Name = WeixinOpenTokenNames.unionid, Value = tokens.GetUnionId() });
            }
            if (!string.IsNullOrEmpty(tokens.GetScope()))
            {
                authTokens.Add(new AuthenticationToken { Name = WeixinOpenTokenNames.scope, Value = tokens.GetScope() });
            }
            if (!string.IsNullOrEmpty(tokens.ExpiresIn))
            {
                int value;
                if (int.TryParse(tokens.ExpiresIn, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                {
                    // https://www.w3.org/TR/xmlschema-2/#dateTime
                    // https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.110).aspx
#if NET8_0_OR_GREATER
                    var expiresAt = TimeProvider.GetUtcNow() + TimeSpan.FromSeconds(value);
#else
                    var expiresAt = Clock.UtcNow + TimeSpan.FromSeconds(value);
#endif
                    authTokens.Add(new AuthenticationToken
                    {
                        Name = WeixinOpenTokenNames.expires_at,
                        Value = expiresAt.ToString("o", CultureInfo.InvariantCulture)
                    });
                }
            }

            properties.StoreTokens(authTokens); //ExternalLoginInfo.AuthenticationTokens
        }


        var ticket = await CreateTicketAsync(identity, properties, tokens);
        if (ticket != null)
        {
            return HandleRequestResult.Success(ticket);
        }
        else
        {
            return HandleRequestResult.Fail("Failed to retrieve user information from remote server.", properties);
        }
    }

    /// <summary>
    /// Step 2：通过code获取access_token
    /// </summary> 
    protected virtual async Task<OAuthTokenResponse> ExchangeCodeAsync(string code, string redirectUri)
    {
        return await _api.GetToken(Options.Backchannel, Options.TokenEndpoint, Options.AppId, Options.AppSecret, code, Context.RequestAborted);
    }

    //protected override async Task<OAuthTokenResponse> ExchangeCodeAsync(OAuthCodeExchangeContext context)
    //{
    //    var code = context.Code;
    //    return await _api.GetToken(Options.Backchannel, Options.TokenEndpoint, Options.AppId, Options.AppSecret, code, Context.RequestAborted);
    //}

    /// <summary>
    /// Call the OAuthServer and get a user's information.
    /// The context object will have the Identity, AccessToken, and UserInformationEndpoint available.
    /// Using this information, we can query the auth server for claims to attach to the identity.
    /// A particular OAuthServer's endpoint returns a json object with a roles member and a name member.
    /// We call this endpoint with HttpClient, parse the result, and attach the claims to the Identity.
    /// </summary>
    /// <param name="identity"></param>
    /// <param name="properties"></param>
    /// <param name="tokens"></param>
    /// <returns></returns>
    protected virtual async Task<AuthenticationTicket> CreateTicketAsync(
        ClaimsIdentity identity,
        AuthenticationProperties properties,
        OAuthTokenResponse tokens)
    {
        if (identity == null)
        {
            throw new ArgumentNullException(nameof(identity));
        }
        if (properties == null)
        {
            throw new ArgumentNullException(nameof(properties));
        }
        if (tokens == null)
        {
            throw new ArgumentNullException(nameof(tokens));
        }

        var openid = tokens.GetOpenId();
        var unionid = tokens.GetUnionId();
        var scope = tokens.GetScope();

        var userInfoPayload = await _api.GetUserInfo(Options.Backchannel, Options.UserInformationEndpoint, tokens.AccessToken, openid, WeixinOpenLanguageCodes.zh_CN, Context.RequestAborted);

        var renewUserInfoPayloadDoc = userInfoPayload.AppendElement("scope", scope);

        var context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties, Context, Scheme, Options, Backchannel, tokens, renewUserInfoPayloadDoc.RootElement);
        context.RunClaimActions();

        await Events.CreatingTicket(context);
        return new AuthenticationTicket(context.Principal, context.Properties, Scheme.Name);
    }
}
