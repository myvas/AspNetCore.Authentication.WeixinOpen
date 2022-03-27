# Myvas.AspNetCore.Authentication Family
* QQConnect: [Here](https://github.com/myvas/AspNetCore.Authentication.QQConnect)
* WeixinOpen: this repo
* WeixinAuth: [Here](https://github.com/myvas/AspNetCore.Authentication.WeixinAuth)

# What's this?
An ASP.NET Core authentication middleware for https://open.weixin.qq.com (微信开放平台/微信扫码登录)

微信开放平台/微信扫码登录：须微信开放平台(open.weixin.qq.com)账号，用户使用微信扫码并确认后登入网站。

* nuget: [Myvas.AspNetCore.Authentication.WeixinOpen](https://www.nuget.org/packages/Myvas.AspNetCore.Authentication.WeixinOpen)

# How to Use?
## 1.Create account
在微信开放平台(https://open.weixin.qq.com)上创建网站应用，配置授权回调域（例如：auth.myvas.com )，记下AppId，获取AppSecret。

## 2.Configure
```csharp
    app.UseAuthentication();
```

3.ConfigureServices
```csharp
services.AddAuthentication()
    // using Myvas.AspNetCore.Authentication;
    .AddWeixinOpen(options => 
    {
        options.AppId = Configuration["WeixinOpen:AppId"];
        options.AppSecret = Configuration["WeixinOpen:AppSecret"];

        options.CallbackPath = "/signin-weixinopen"; //默认
    };
```

```
说明：

(1)同一用户在同一微信公众号即使重复多次订阅/退订，其OpenId也不会改变。

(2)同一用户在不同微信公众号中的OpenId是不一样的。

(3)若同时运营了多个微信公众号，可以在微信开放平台上开通开发者账号，并在“管理中心/公众账号”中将这些公众号添加进去，就可以获取到同一用户在这些公众号中保持一致的UnionId。
```

# Dev
* [.NET Core 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
* [微信开发者工具](https://mp.weixin.qq.com/debug/wxadoc/dev/devtools/download.html)

# Demo Online
* Demo website: [Here](https://demo.auth.myvas.com)
* Demo source code: [Here](https://github.com/myvas/AspNetCore.Authentication.Demo)
