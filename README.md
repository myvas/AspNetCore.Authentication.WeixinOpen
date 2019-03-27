# AspNetCore.Authentication.WeixinOpen
An ASP.NET Core authentication middleware: WeixinOpen for https://open.weixin.qq.com (微信开放平台/微信扫码登录)

微信开放平台/微信扫码登录：须open.weixin.qq.com账号网站应用接入，用户使用微信扫码并确认后登入网站。

* nuget: https://www.nuget.org/packages/Myvas.AspNetCore.Authentication.WeixinOpen
* github: https://github.com/myvas/AspNetCore.Authentication.WeixinOpen


# Brothers and sisters
## AspNetCore.Authentication.QQConnect
An ASP.NET Core authentication middleware: QQConnect for https://connect.qq.com (腾讯QQ互联/QQ登录）

腾讯QQ互联/QQ登录：用户通过点击“QQ登录”图标按钮，或使用手机QQ扫码登入网站。

* nuget: https://www.nuget.org/packages/AspNetCore.Authentication.QQConnect
* github: https://github.com/myvas/AspNetCore.Authentication.QQConnect

## AspNetCore.Authentication.WeixinAuth
An ASP.NET Core authentication middleware: WeixinAuth for https://mp.weixin.qq.com （微信公众号/微信网页授权登录）

微信公众号/微信网页授权登录，须mp.weixin.qq.com账号，微信内置浏览器用户访问网站时自动登入网站。

* nuget: https://www.nuget.org/packages/AspNetCore.Authentication.WeixinAuth
* github: https://github.com/myvas/AspNetCore.Authentication.WeixinAuth

## Demo Online
* github: https://github.com/myvas/AspNetCore.Authentication.Demo
* demo: https://demo.auth.myvas.com

![alt https://demo.auth.myvas.com Weixin QrCode](http://mmbiz.qpic.cn/mmbiz_jpg/lPe5drS9euRQR1eCK5cGXaibHYL6vBR4pGLB34ju2hXCiaMQiayOU8w5GMfEH7WZsVNTnhLTpnzAC9xfdWuTT89OA/0)

## How to Use
### 微信开放平台
- 开发者注册: https://open.weixin.qq.com
- 管理中心:

创建网站应用，配置授权回调域（例如：auth.myvas.com )，记下AppId，获取AppSecret

### ConfigureServices
```csharp
services.AddAuthentication()
    // 微信开放平台登录：须open.weixin.qq.com账号网站应用接入，用户扫描微信二维码并确认后登入网站。
    .AddWeixinOpen(options => 
    {
        options.AppId = Configuration["WeixinOpen:AppId"];
        options.AppSecret = Configuration["WeixinOpen:AppSecret"];

        options.CallbackPath = "signin-weixinopen"; //默认
    };
```

### Configure
```csharp
    app.UseAuthentication();
```

### Dev
* .NET Core SDK 2.1.505
* 下载[微信开发者工具](https://mp.weixin.qq.com/debug/wxadoc/dev/devtools/download.html)


