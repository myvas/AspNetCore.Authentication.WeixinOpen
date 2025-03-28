﻿using Microsoft.AspNetCore.Authentication.OAuth.Claims;

namespace Myvas.AspNetCore.Authentication.WeixinOpen.Internal;

internal static class ClaimActionCollectionMapExtensions
{
    public static void MapJsonKeyArray(this ClaimActionCollection collection, string claimType, string jsonKey)
    {
        collection.Add(new JsonKeyArrayClaimAction(claimType, null, jsonKey));
    }

    public static void MapJsonKeyArray(this ClaimActionCollection collection, string claimType, string jsonKey, string valueType)
    {
        collection.Add(new JsonKeyArrayClaimAction(claimType, valueType, jsonKey));
    }
}
