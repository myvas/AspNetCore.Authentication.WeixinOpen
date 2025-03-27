using System;
using System.Collections.Generic;
using System.Text;

namespace Myvas.AspNetCore.Authentication
{
    public class WeixinOpenScopes
    {
        /// <summary>
        /// auth
        /// </summary>
        /// <remarks>其中snsapi_base属于基础接口，若应用已拥有其它scope权限，则默认拥有snsapi_base的权限。
        /// 使用snsapi_base可以让移动端网页授权绕过跳转授权登录页请求用户授权的动作，直接跳转第三方网页带上授权临时票据（code），
        /// 但会使得用户已授权作用域（scope）仅为snsapi_base，从而导致无法获取到需要用户授权才允许获得的数据和基础功能。
        /// </remarks>
        /// <seealso href="/sns/oauth2/access_token"/>
        /// <seealso href="/sns/oauth2/refresh_token"/>
        /// <seealso href="/sns/auth"/>
        public const string snsapi_base = "snsapi_base";

        /// <summary>
        /// userinfo
        /// </summary>
        /// <seealso href="/sns/userinfo"/>
        public const string snsapi_userinfo = "snsapi_userinfo";

        /// <summary>
        /// 微信开放平台@网站应用微信登录
        /// </summary>
        public const string snsapi_login = "snsapi_login";

    }
}
