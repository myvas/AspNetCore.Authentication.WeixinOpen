using System.Text.Json.Serialization;

public class WeixinOpenRefreshTokenResultJson : WeixinOpenResultJson
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }

    [JsonPropertyName("openid")]
    public string OpenId { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; }

    public bool Validate()
    {
        if (!Succeeded) return false;
        if ((ExpiresIn ?? 0) <= 0) return false;
        if (string.IsNullOrEmpty(AccessToken)) return false;
        if (string.IsNullOrEmpty(RefreshToken)) return false;
        return true;
    }
}
