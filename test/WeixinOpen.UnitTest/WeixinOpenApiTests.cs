using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Myvas.AspNetCore.Authentication.WeixinOpen.Internal;

namespace Myvas.AspNetCore.Authentication.WeixinOpen.Tests;

public class WeixinOpenApiTests
{
    TestServer _tencentServer;
    TestServer _testServer;
    public WeixinOpenApiTests()
    {
        _tencentServer = FakeTencentServerBuilder.CreateServer();

        _testServer = TestServerBuilderBase.CreateServer(null, services =>
        {
            services.AddLogging().AddTransient<IWeixinOpenApi, WeixinOpenApi>();
        }, null);
    }

    [Fact]
    public async Task GetToken()
    {
        CancellationToken cancellationToken = default;
        var backchannel = _tencentServer.CreateClient();

        var api = _testServer.Services.GetRequiredService<IWeixinOpenApi>();
        var endpoint = WeixinOpenDefaults.TokenEndpoint;
        var appId = "APPID";
        var appSecret = "APPSECRET";
        var code = "TestCode";
        var result = await api.GetToken(backchannel, endpoint, appId, appSecret, code, cancellationToken);
        Assert.Null(result.Error);
        Assert.NotNull(result.Response);
        Assert.Equal("ACCESS_TOKEN", result.AccessToken);
    }

    [Fact]
    public async Task GetUserInfo()
    {
        CancellationToken cancellationToken = default;
        var backchannel = _tencentServer.CreateClient();

        var api = _testServer.Services.GetRequiredService<IWeixinOpenApi>();
        var endpoint = WeixinOpenDefaults.TokenEndpoint;
        var appId = "APPID";
        var appSecret = "APPSECRET";
        var code = "TestCode";
        var result = await api.GetToken(backchannel, endpoint, appId, appSecret, code, cancellationToken);
        Assert.Null(result.Error);
        Assert.NotNull(result.Response);
        Assert.Equal("ACCESS_TOKEN", result.AccessToken);

        var accessToken = result.AccessToken;
        endpoint = WeixinOpenDefaults.UserInformationEndpoint;
        var openId = "OPENID";
        var languageCode = WeixinOpenLanguageCodes.zh_CN;
        var result2 = await api.GetUserInfo(backchannel, endpoint, accessToken, openId, languageCode, cancellationToken);
        Assert.Equal("OPENID", result2.GetString("openid"));
        Assert.Equal("UNIONID", result2.GetString("unionid"));
    }

    [Fact]
    public async Task RefreshToken()
    {
        CancellationToken cancellationToken = default;
        var backchannel = _tencentServer.CreateClient();

        var api = _testServer.Services.GetRequiredService<IWeixinOpenApi>();
        var endpoint = WeixinOpenDefaults.TokenEndpoint;
        var appId = "APPID";
        var appSecret = "APPSECRET";
        var code = "TestCode";
        var result = await api.GetToken(backchannel, endpoint, appId, appSecret, code, cancellationToken);
        Assert.Null(result.Error);
        Assert.NotNull(result.Response);
        Assert.Equal("ACCESS_TOKEN", result.AccessToken);

        var accessToken = result.AccessToken;
        endpoint = WeixinOpenDefaults.RefreshTokenEndpoint;
        var result2 = await api.RefreshToken(backchannel, endpoint, appId, accessToken, cancellationToken);
        Assert.Null(result2.Error);
        Assert.NotNull(result2.Response);
        Assert.Equal("NEW_ACCESS_TOKEN", result2.AccessToken);
        Assert.Equal("ACCESS_TOKEN", result2.RefreshToken);
        Assert.Equal("Bearer", result2.TokenType ?? "Bearer");
        Assert.Equal("OPENID", result2.GetOpenId());
        Assert.Equal("SCOPE", result2.GetScope());
    }

    [Fact]
    public async Task ValidateToken()
    {
        CancellationToken cancellationToken = default;
        var backchannel = _tencentServer.CreateClient();

        var api = _testServer.Services.GetRequiredService<IWeixinOpenApi>();
        var endpoint = WeixinOpenDefaults.TokenEndpoint;
        var appId = "APPID";
        var appSecret = "APPSECRET";
        var code = "TestCode";
        var result = await api.GetToken(backchannel, endpoint, appId, appSecret, code, cancellationToken);
        Assert.Null(result.Error);
        Assert.NotNull(result.Response);
        Assert.Equal("ACCESS_TOKEN", result.AccessToken);

        var accessToken = result.AccessToken;
        endpoint = WeixinOpenDefaults.ValidateTokenEndpoint;
        var openId = "OPENID";
        var result2 = await api.ValidateToken(backchannel, endpoint, accessToken, openId, cancellationToken);
        Assert.True(result2);
    }
}