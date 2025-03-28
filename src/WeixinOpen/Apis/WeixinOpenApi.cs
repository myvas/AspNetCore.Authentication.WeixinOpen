using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Myvas.AspNetCore.Authentication.WeixinOpen.Internal;

internal class WeixinOpenApi : IWeixinOpenApi
{
    private readonly ILogger _logger;

    public WeixinOpenApi(IOptionsMonitor<WeixinOpenOptions> optionsAccessor, ILogger<WeixinOpenApi> logger)
    {
        _logger = logger;
    }

    private static async Task<string> Display(HttpResponseMessage response)
    {
        var output = new StringBuilder();
        output.Append($"Status: {(int)response.StatusCode} {response.StatusCode};");
        output.Append("Headers: " + response.Headers.ToString() + ";");
        output.Append("Body: " + await response.Content.ReadAsStringAsync() + ";");
        return output.ToString();
    }

    /// <summary>
    /// 通过code换取网页授权access_token
    /// </summary>
    /// <param name="refreshToken">refresh_token拥有较长的有效期（30天），当refresh_token失效的后，需要用户重新授权，所以，请开发者在refresh_token即将过期时（如第29天时），进行定时的自动刷新并保存好它。</param>
    /// <returns></returns>
    public async Task<OAuthTokenResponse> GetToken(HttpClient backchannel, string tokenEndpoint, string appId, string appSecret, string code, CancellationToken cancellationToken = default)
    {
        var tokenRequestParameters = new Dictionary<string, string>()
        {
            ["appid"] = appId,
            ["secret"] = appSecret,
            ["code"] = code,
            ["grant_type"] = "authorization_code"
        };

        var requestUrl = QueryHelpers.AddQueryString(tokenEndpoint, tokenRequestParameters);

        var response = await backchannel.GetAsync(requestUrl, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger?.LogDebug(requestUrl);
            var error = "OAuth token endpoint failure: " + await Display(response);
            _logger?.LogError(error);
            return OAuthTokenResponse.Failed(new Exception(error));
        }

        var content = await response.Content.ReadAsStringAsync();
        // { 
        //    "access_token":"ACCESS_TOKEN", 
        //    "expires_in":7200, 
        //    "refresh_token":"REFRESH_TOKEN",
        //    "openid":"OPENID", 
        //    "scope":"SCOPE",
        //    "unionid": "o6_bmasdasdsad6_2sgVt7hMZOPfL"
        //}
        var payload = JsonDocument.Parse(content);
        var errcode = payload.RootElement.GetString("errcode");
        var errmsg = payload.RootElement.GetString("errmsg");
        if (!string.IsNullOrEmpty(errcode))
        {
            var error = "OAuth token endpoint failure: " + await Display(response);
            _logger?.LogError(error);
            return OAuthTokenResponse.Failed(new Exception(error));
        }

        //payload.Add("token_type", "");
        return OAuthTokenResponse.Success(payload);
    }

    /// <summary>
    /// 刷新或续期access_token使用。由于access_token有效期（目前为2个小时）较短，当access_token超时后，可以使用refresh_token进行刷新。
    /// </summary>
    /// <param name="refreshToken">refresh_token拥有较长的有效期（30天），当refresh_token失效的后，需要用户重新授权，所以，请开发者在refresh_token即将过期时（如第29天时），进行定时的自动刷新并保存好它。</param>
    /// <returns></returns>
    [Obsolete("Avoid this function—it performs unnecessary deserialization.")]
    public async Task<OAuthTokenResponse> RefreshToken2(HttpClient backchannel, string refreshTokenEndpoint, string appId, string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenRequestParameters = new Dictionary<string, string>()
        {
            ["appid"] = appId,
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken
        };

        var requestUrl = QueryHelpers.AddQueryString(refreshTokenEndpoint, tokenRequestParameters);

        var response = await backchannel.GetAsync(requestUrl, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = "OAuth refresh token endpoint failure: " + await Display(response);
            _logger?.LogError(error);
            return OAuthTokenResponse.Failed(new Exception(error));
        }

        //var content = await response.Content.ReadAsStringAsync();
        //{
        //    "access_token":"ACCESS_TOKEN",
        //    "expires_in":7200,
        //    "refresh_token":"REFRESH_TOKEN",
        //    "openid":"OPENID",
        //    "scope":"SCOPE"
        //}
#if NET6_0_OR_GREATER
        using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
#else
        using var responseStream = await response.Content.ReadAsStreamAsync();
#endif
        var responseJson = await JsonSerializer.DeserializeAsync<WeixinOpenRefreshTokenResultJson>(responseStream, new JsonSerializerOptions(), cancellationToken);
        //var payload = JsonDocument.Parse(content);
        //if (!string.IsNullOrEmpty(payload.RootElement.GetString("errcode")))
        if (!responseJson.Succeeded)
        {
            var error = "OAuth refresh token failed: " + await Display(response);
            _logger?.LogError(error);
            return OAuthTokenResponse.Failed(new Exception(error));
        }
        if (!responseJson.Validate())
        {
            var error = "OAuth refresh token failed: Invalid response";
            _logger?.LogError(error);
            return OAuthTokenResponse.Failed(new Exception(error));
        }
        var resultDto = new
        {
            access_token = responseJson.AccessToken,
            refresh_token = responseJson.RefreshToken,
            expires_in = responseJson.ExpiresIn,
            //token_type = "Bearer",
            openid = responseJson.OpenId,
            scope = responseJson.Scope
        };
#if NET6_0_OR_GREATER
        var resultJdoc = JsonSerializer.SerializeToDocument(resultDto);
#else
        var jsonString = JsonSerializer.Serialize(resultDto);
        var resultJdoc = JsonDocument.Parse(jsonString);
#endif
        return OAuthTokenResponse.Success(resultJdoc);
    }

    public async Task<OAuthTokenResponse> RefreshToken(HttpClient backchannel, string refreshTokenEndpoint, string appId, string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenRequestParameters = new Dictionary<string, string>()
        {
            ["appid"] = appId,
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken
        };

        var requestUrl = QueryHelpers.AddQueryString(refreshTokenEndpoint, tokenRequestParameters);

        var response = await backchannel.GetAsync(requestUrl, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = "OAuth refresh token failed: " + await Display(response);
            _logger?.LogError(error);
            return OAuthTokenResponse.Failed(new Exception(error));
        }

        //var content = await response.Content.ReadAsStringAsync();
        //{
        //    "access_token":"ACCESS_TOKEN",
        //    "expires_in":7200,
        //    "refresh_token":"REFRESH_TOKEN",
        //    "openid":"OPENID",
        //    "scope":"SCOPE"
        //}
#if NET5_0_OR_GREATER
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
#else
        var content = await response.Content.ReadAsStringAsync();
#endif
        var payload = JsonDocument.Parse(content);
        if (!string.IsNullOrEmpty(payload.RootElement.GetString("errcode")))
        {
            var error = "OAuth refresh token failed: " + await Display(response);
            _logger?.LogError(error);
            return OAuthTokenResponse.Failed(new Exception(error));
        }
        if (string.IsNullOrEmpty(payload.RootElement.GetString("access_token")))
        {
            var error = "OAuth refresh token failed: Response missing required access token";
            _logger?.LogError(error);
            return OAuthTokenResponse.Failed(new Exception(error));
        }
        return OAuthTokenResponse.Success(payload);
    }

    /// <summary>
    /// 检验授权凭证（access_token）是否有效。
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    public async Task<bool> ValidateToken(HttpClient backchannel, string validateTokenEndpoint, string accessToken, string openId, CancellationToken cancellationToken = default)
    {
        var tokenRequestParameters = new Dictionary<string, string>()
        {
            ["access_token"] = accessToken,
            ["openid"] = openId
        };

        var requestUrl = QueryHelpers.AddQueryString(validateTokenEndpoint, tokenRequestParameters);

        var response = await backchannel.GetAsync(requestUrl, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = "OAuth validate token endpoint failure: " + await Display(response);
            _logger?.LogError(error);
            return false;
        }

        var content = await response.Content.ReadAsStringAsync();
        var payload = JsonDocument.Parse(content);
        try
        {
            var errcode = payload.RootElement.GetInt32("errcode");
            return (errcode == 0);
        }
        catch { }
        return false;
    }

    /// <summary>
    /// 获取用户个人信息（UnionID机制）
    /// </summary>
    /// <param name="accessToken"></param>
    /// <returns></returns>
    public async Task<JsonDocument> GetUserInfo(HttpClient backchannel, string userInformationEndpoint, string accessToken, string openid, WeixinOpenLanguageCodes languageCode = WeixinOpenLanguageCodes.zh_CN, CancellationToken cancellationToken = default)
    {
        var tokenRequestParameters = new Dictionary<string, string>()
        {
            ["access_token"] = accessToken,
            ["openid"] = openid,
            ["lang"] = languageCode.ToString()
        };

        var requestUrl = QueryHelpers.AddQueryString(userInformationEndpoint, tokenRequestParameters);

        var response = await backchannel.GetAsync(requestUrl, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = "OAuth user information endpoint failure: " + await Display(response);
            _logger?.LogError(error);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        //{
        //    "openid": "OPENID",
        //    "nickname": "NICKNAME",
        //    "sex": 1,
        //    "province": "PROVINCE",
        //    "city": "CITY",
        //    "country": "COUNTRY",
        //    "headimgurl": "http://wx.qlogo.cn/mmopen/g33456789012345678901234567890123456789012345678901234567890123456789012345678901234567899012345678901e/0",
        //    "privilege": [
        //        "PRIVILEGE1",
        //        "PRIVILEGE2"
        //    ],
        //    "unionid": "o6_b**********6_2***********L"
        //}
        var payload = JsonDocument.Parse(content);
        if (!string.IsNullOrEmpty(payload.RootElement.GetString("errcode")))
        {
            var error = "OAuth user information endpoint failure: " + await Display(response);
            _logger?.LogError(error);
            return null;
        }

        return payload;
    }
}
