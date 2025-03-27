using System.Text.Json.Serialization;

public class WeixinOpenUserInfoResultJson : WeixinOpenResultJson
{
    [JsonPropertyName("openid")]
    public string OpenId { get; set; }

    [JsonPropertyName("nickname")]
    public string Nickname { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    /// <remarks>2021年9月27日发公告不再提供</remarks>
    /// <seealso href="https://open.weixin.qq.com/cgi-bin/announce?action=getannouncement&key=11632660190BzIea&version=&lang=en_US&token=">微信公众平台用户信息相关接口调整公告</seealso>
    [JsonPropertyName("sex")]
    public int? Sex { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    /// <remarks>2021年9月27日发公告不再提供</remarks>
    /// <seealso href="https://open.weixin.qq.com/cgi-bin/announce?action=getannouncement&key=11632660190BzIea&version=&lang=en_US&token=">微信公众平台用户信息相关接口调整公告</seealso>
    [JsonPropertyName("province")]
    public string Province { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    /// <remarks>2021年9月27日发公告不再提供</remarks>
    /// <seealso href="https://open.weixin.qq.com/cgi-bin/announce?action=getannouncement&key=11632660190BzIea&version=&lang=en_US&token=">微信公众平台用户信息相关接口调整公告</seealso>
    [JsonPropertyName("city")]
    public string City { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    /// <remarks>2021年9月27日发公告不再提供</remarks>
    /// <seealso href="https://open.weixin.qq.com/cgi-bin/announce?action=getannouncement&key=11632660190BzIea&version=&lang=en_US&token=">微信公众平台用户信息相关接口调整公告</seealso>
    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("headimgurl")]
    public string HeadImgUrl { get; set; }

    [JsonPropertyName("privilege")]
    public List<string> Privilege { get; set; }

    [JsonPropertyName("unionid")]
    public string UnionId { get; set; }

    public bool Validate()
    {
        if (!Succeeded) return false;
        if (string.IsNullOrEmpty(OpenId)) return false;
        return true;
    }
}
