using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Myvas.AspNetCore.Authentication;
using Myvas.AspNetCore.Authentication.WeixinOpen.Internal;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 微信开放平台@网站应用微信登录
    /// https://open.weixin.qq.com/cgi-bin/showdocument?action=dir_list&t=resource/res_list&verify=1&id=open1419316505&token=&lang=zh_CN
    /// </summary>
    public static class WeixinOpenAuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddWeixinOpen(this AuthenticationBuilder builder, Action<WeixinOpenOptions> setupAction = null)
            => builder.AddWeixinOpen(WeixinOpenDefaults.AuthenticationScheme, setupAction);

        public static AuthenticationBuilder AddWeixinOpen(this AuthenticationBuilder builder, string authenticationScheme, Action<WeixinOpenOptions> setupAction = null)
            => builder.AddWeixinOpen(authenticationScheme, WeixinOpenDefaults.DisplayName, setupAction);

        public static AuthenticationBuilder AddWeixinOpen(
            this AuthenticationBuilder builder,
            string authenticationScheme,
            string displayName,
            Action<WeixinOpenOptions> setupAction)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (setupAction != null)
            {
                builder.Services.Configure(setupAction);
            }

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<WeixinOpenOptions>, WeixinOpenPostConfigureOptions>());
            builder.Services.TryAddTransient<IWeixinOpenApi, WeixinOpenApi>();
            //return builder.AddOAuth<WeixinOpenOptions, WeixinOpenHandler>(authenticationScheme, displayName, setupAction);
            return builder.AddRemoteScheme<WeixinOpenOptions, WeixinOpenHandler>(authenticationScheme, displayName, setupAction);
        }
    }
}
