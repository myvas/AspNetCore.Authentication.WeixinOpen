﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Myvas.AspNetCore.Authentication.WeixinOpen.Tests;

public class TestHttpMessageHandler : HttpMessageHandler
{
    public Func<HttpRequestMessage, HttpResponseMessage> Sender { get; set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
    {
        if (Sender != null)
        {
            return Task.FromResult(Sender(request));
        }

        return Task.FromResult<HttpResponseMessage>(null);
    }
}
