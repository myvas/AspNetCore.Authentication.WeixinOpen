using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Myvas.AspNetCore.Authentication;
using System.Net.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public class WeixinOpenPostConfigureOptions : IPostConfigureOptions<WeixinOpenOptions>
    {
        private readonly IDataProtectionProvider _dp;

        public WeixinOpenPostConfigureOptions(IDataProtectionProvider dataProtection)
        {
            _dp = dataProtection;
        }

        public void PostConfigure(string name, WeixinOpenOptions options)
        {
            options.DataProtectionProvider = options.DataProtectionProvider ?? _dp;
            if (options.Backchannel == null)
            {
                options.Backchannel = new HttpClient(options.BackchannelHttpHandler ?? new HttpClientHandler());
                options.Backchannel.DefaultRequestHeaders.UserAgent.ParseAdd("Microsoft ASP.NET Core OAuth handler");
                options.Backchannel.Timeout = options.BackchannelTimeout;
                options.Backchannel.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10 MB
            }

            if (options.StateDataFormat == null)
            {
                var dataProtector = options.DataProtectionProvider.CreateProtector(
                    typeof(WeixinOpenHandler).FullName, name, "v1");
                options.StateDataFormat = new PropertiesDataFormat(dataProtector);
            }
        }
    }
}
