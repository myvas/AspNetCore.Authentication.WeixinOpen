using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Myvas.AspNetCore.Authentication.WeixinOpen
{
    /// <summary>
    /// Defines constants for the well-known claim types that can be assigned to a subject.
    /// This class cannot be inherited.
    /// </summary>
    public static class WeixinOpenClaimTypes
    {
        /// <summary>
        /// urn:weixin:unionid
        /// </summary>
        public const string UnionId = "urn:weixin:unionid";

        #region snsapi_base
        /// <summary>
        /// urn:weixin:openid, should be <see cref="ClaimTypes.NameIdentifier"/>
        /// </summary>
        public const string OpenId = "urn:weixin:openid";

        /// <summary>
        /// urn:weixin:scope
        /// </summary>
        public const string Scope = "urn:weixin:scope";
        #endregion
        #region snsapi_userinfo
        /// <summary>
        /// urn:weixin:nickname, should be <see cref="ClaimTypes.Name"/>
        /// </summary>
        public const string NickName = "urn:weixin:nickname";

        /// <summary>
        /// urn:weixin:headimgurl
        /// </summary>
        public const string HeadImageUrl = "urn:weixin:headimgurl";

        /// <summary>
        /// urn:weixin:sex, should be <see cref="ClaimTypes.Gender"/>
        /// </summary>
        public const string Sex = "urn:weixin:sex";

        /// <summary>
        /// urn:weixin:country, should be <see cref="ClaimTypes.Country"/>
        /// </summary>
        public const string Country = "urn:weixin:country";

        /// <summary>
        /// urn:weixin:province, should be <see cref="ClaimTypes.StateOrProvince"/>
        /// </summary>
        public const string Province = "urn:weixin:province";

        /// <summary>
        /// urn:weixin:city
        /// </summary>
        public const string City = "urn:weixin:city";
        
        /// <summary>
        /// urn:weixin:privilege，map JArray to multiple claims
        /// </summary>
        public const string Privilege = "urn:weixin:privilege";
        #endregion
    }
}
