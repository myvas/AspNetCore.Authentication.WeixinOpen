﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Myvas.AspNetCore.Authentication.WeixinOpen.Internal
{
    internal static class WeixinOpenClaimsExtensions
    {
        public static ClaimsIdentity AddOptionalClaim(this ClaimsIdentity identity,
            string type, string value, string issuer)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            // Don't update the identity if the claim cannot be safely added.
            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(value))
            {
                return identity;
            }

            identity.AddClaim(new Claim(type, value, ClaimValueTypes.String, issuer ?? ClaimsIdentity.DefaultIssuer));
            return identity;
        }
    }
}
