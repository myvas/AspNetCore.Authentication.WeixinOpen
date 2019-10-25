using Microsoft.AspNetCore.Authentication.OAuth;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Myvas.AspNetCore.Authentication.WeixinOpen.Internal
{
    internal interface IWeixinOpenApi
    {
        Task<bool> ValidateToken(HttpClient backchannel, string validateTokenEndpoint, string appId, string accessToken, CancellationToken cancellationToken);
        Task<JsonDocument> GetUserInfo(HttpClient backchannel, string userInformationEndpoint, string accessToken, string openid, CancellationToken cancellationToken, WeixinOpenLanguageCodes languageCode = WeixinOpenLanguageCodes.zh_CN);
        Task<OAuthTokenResponse> RefreshToken(HttpClient backchannel, string refreshTokenEndpoint, string appId, string refreshToken, CancellationToken cancellationToken);
        Task<OAuthTokenResponse> GetToken(HttpClient backchannel, string tokenEndpoint, string appId, string appSecret, string code, CancellationToken cancellationToken);
    }
}