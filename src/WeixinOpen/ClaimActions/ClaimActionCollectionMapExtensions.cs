using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using System;
using System.Collections.Generic;
using System.Text;

namespace Myvas.AspNetCore.Authentication.WeixinOpen.Extensions
{
    public static class ClaimActionCollectionExtensions
    {
        public static void MapJsonKeyArray(this ClaimActionCollection collection, string claimType, string jsonKey)
        {
            collection.Add(new WeixinOpenJsonKeyArrayClaimAction(claimType, null, jsonKey));
        }

        public static void MapJsonKeyArray(this ClaimActionCollection collection, string claimType, string jsonKey, string valueType)
        {
            collection.Add(new WeixinOpenJsonKeyArrayClaimAction(claimType, valueType, jsonKey));
        }
    }
}
