using System.Net;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Net.Http.Headers;

namespace Myvas.AspNetCore.Authentication.WeixinOpen.Tests;

public static class FakeTencentServerBuilder
{
    public static TestServer CreateServer()
    {
        return TestServerBuilderBase.CreateServer(null, null, async context =>
        {
            var req = context.Request;
            switch (req.Path.Value)
            {
                //https://api.weixin.qq.com
                case "/sns/oauth2/access_token":
                    {
                        var appid = req.Query["appid"];
                        var appsecret = req.Query["secret"];
                        var code = req.Query["code"];
                        var grant_type = req.Query["grant_type"];
                        if (grant_type != "authorization_code") throw new InvalidOperationException();
                        if (appid != "APPID"){
                            context.Response.Headers.TryAdd(HeaderNames.ContentType, "application/json");
                            var content = TestFile.ReadAllText("invalid-appid.json");
                            await context.Response.WriteAsync(content);
                            return true;                            
                        }
                        if (appsecret != "APPSECRET") {
                            context.Response.Headers.TryAdd(HeaderNames.ContentType, "application/json");
                            var content = TestFile.ReadAllText("invalid-appsecret.json");
                            await context.Response.WriteAsync(content);
                            return true;                            
                        }
                        if (code != "TestCode")
                        {
                            context.Response.Headers.TryAdd(HeaderNames.ContentType, "application/json");
                            var content = TestFile.ReadAllText("invalid-code.json");
                            await context.Response.WriteAsync(content);
                            return true;
                        }
                        else
                        {
                            context.Response.Headers.TryAdd(HeaderNames.ContentType, "application/json");
                            var content = TestFile.ReadAllText("get_token.json");
                            await context.Response.WriteAsync(content);
                            return true;
                        }
                    }
                case "/sns/oauth2/refresh_token":
                    {
                        var appid = req.Query["appid"];
                        //var appsecret = req.Query["secret"];
                        var refresh_token = req.Query["refresh_token"];
                        var grant_type = req.Query["grant_type"];
                        if (grant_type != "refresh_token") throw new InvalidOperationException();
                        if (appid != "APPID"){
                            context.Response.Headers.TryAdd(HeaderNames.ContentType, "application/json");
                            var content = TestFile.ReadAllText("invalid-appid.json");
                            await context.Response.WriteAsync(content);
                            return true;                            
                        }
                        if (refresh_token != "REFRESH_TOKEN")
                        {
                            context.Response.Headers.TryAdd(HeaderNames.ContentType, "application/json");
                            var content = TestFile.ReadAllText("invalid-refresh_token.json");
                            await context.Response.WriteAsync(content);
                            return true;
                        }
                        else
                        {
                            context.Response.Headers.TryAdd(HeaderNames.ContentType, "application/json");
                            var content = TestFile.ReadAllText("refresh_token.json");
                            await context.Response.WriteAsync(content);
                            return true;
                        }
                    }
                case "/sns/auth":
                    {
                        var access_token = req.Query["access_token"];
                        var openid = req.Query["openid"];
                        if (access_token != "ACCESS_TOKEN"){
                            context.Response.Headers.TryAdd(HeaderNames.ContentType, "application/json");
                            var content = TestFile.ReadAllText("invalid-access_token.json");
                            await context.Response.WriteAsync(content);
                            return true;                            
                        }
                        if (openid != "OPENID")
                        {
                            context.Response.Headers.TryAdd(HeaderNames.ContentType, "application/json");
                            var content = TestFile.ReadAllText("invalid-openid.json");
                            await context.Response.WriteAsync(content);
                            return true;
                        }
                        else
                        {
                            context.Response.Headers.TryAdd(HeaderNames.ContentType, "application/json");
                            var content = TestFile.ReadAllText("validate_token.json");
                            await context.Response.WriteAsync(content);
                            return true;
                        }
                    }
                case "/sns/userinfo":
                    {
                        var access_token = req.Query["access_token"];
                        var openid = req.Query["openid"];
                        if (access_token != "ACCESS_TOKEN") throw new NotImplementedException();
                        if (openid != "OPENID")
                        {
                            context.Response.Headers.TryAdd(HeaderNames.ContentType, "application/json");
                            var content = TestFile.ReadAllText("invalid-openid.json");
                            await context.Response.WriteAsync(content);
                            return true;
                        }
                        else
                        {
                            context.Response.Headers.TryAdd(HeaderNames.ContentType, "application/json");
                            var content = TestFile.ReadAllText("userinfo.json");
                            await context.Response.WriteAsync(content);
                            return true;
                        }
                    }
                //https://open.weixin.qq.com
                case "connect/qrconnect":
                    {
                        // Get query from the request
                        var redirect_uri = req.Query["redirect_uri"];
                        var state = req.Query["state"];

                        // Prepare the additional query for the response
                        var query = new Dictionary<string, string>()
                        {
                            ["code"] = "TestCode",
                            ["state"] = state
                        };

                        // Parse the existing URI
                        var uriBuilder = new UriBuilder(redirect_uri);

                        // Parse existing query (if any)
                        var queryParams = HttpUtility.ParseQueryString(uriBuilder.Query);

                        // Add new parameters (overwrite if they already exist)
                        foreach (var kvp in query)
                        {
                            queryParams[kvp.Key] = kvp.Value;
                        }

                        // Rebuild the query
                        uriBuilder.Query = queryParams.ToString();

                        // Redirect
                        context.Response.Redirect(uriBuilder.ToString());
                        return true;
                    }
                default:
                    return false;//throw new NotImplementedException();
            }
        });
    }
}