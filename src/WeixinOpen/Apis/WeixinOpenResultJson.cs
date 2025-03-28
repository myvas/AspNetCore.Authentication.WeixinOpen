using System.Text.Json.Serialization;

public class WeixinOpenResultJson
{
    [JsonPropertyName("errcode")]
    public int? ErrorCode { get; set; }

    [JsonPropertyName("errmsg")]
    public string ErrorMessage { get; set; }

    [JsonIgnore]
    public virtual bool Succeeded { get => (ErrorCode ?? 0) == 0; }
}