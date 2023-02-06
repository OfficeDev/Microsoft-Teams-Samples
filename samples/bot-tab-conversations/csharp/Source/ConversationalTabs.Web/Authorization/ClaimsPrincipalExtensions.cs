// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Web.Authorization;

using System.Security.Claims;
using Microsoft.Identity.Web;

/// <summary>
/// ClaimsPrincipal extensions.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets user oid claim from claimsPrincipal.
    /// </summary>
    /// <param name="claimsPrincipal">ClaimsPrincipal.</param>
    /// <returns>Return user's id.</returns>
    public static string GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirstValue(ClaimConstants.Oid) ??
            claimsPrincipal.FindFirstValue(ClaimConstants.ObjectId);
    }
}
