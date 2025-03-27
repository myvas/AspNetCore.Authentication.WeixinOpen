using Microsoft.AspNetCore.Authentication.OAuth;
using System.Text.Json;

namespace Myvas.AspNetCore.Authentication.WeixinOpen.Internal;

internal interface IWeixinOpenApi
{
    Task<OAuthTokenResponse> GetToken(HttpClient backchannel, string tokenEndpoint, string appId, string appSecret, string code, CancellationToken cancellationToken = default);
    Task<OAuthTokenResponse> RefreshToken(HttpClient backchannel, string refreshTokenEndpoint, string appId, string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> ValidateToken(HttpClient backchannel, string validateTokenEndpoint, string accessToken, string openId, CancellationToken cancellationToken = default);
    Task<JsonDocument> GetUserInfo(HttpClient backchannel, string userInformationEndpoint, string accessToken, string openId, WeixinOpenLanguageCodes languageCode = WeixinOpenLanguageCodes.zh_CN, CancellationToken cancellationToken = default);
}