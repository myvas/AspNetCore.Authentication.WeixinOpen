using Microsoft.AspNetCore.Authentication.OAuth;

namespace Myvas.AspNetCore.Authentication.WeixinOpen.Internal;

internal static class OAuthTokenResponseExtensions
{
    public static string GetStringByKey(this OAuthTokenResponse response, string key)
    {
        if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
        return response?.Response?.RootElement.GetString(key);
    }

    public static string GetUnionId(this OAuthTokenResponse response)
    {
        return response?.GetStringByKey("unionid");
    }

    public static string GetOpenId(this OAuthTokenResponse response)
    {
        return response?.GetStringByKey("openid");
    }

    public static string GetScope(this OAuthTokenResponse response)
    {
        return response?.GetStringByKey("scope");
    }

    public static string GetErrorCode(this OAuthTokenResponse response)
    {
        return response?.GetStringByKey("errcode");
    }

    public static string GetErrorMsg(this OAuthTokenResponse response)
    {
        return response?.GetStringByKey("errmsg");
    }
}
