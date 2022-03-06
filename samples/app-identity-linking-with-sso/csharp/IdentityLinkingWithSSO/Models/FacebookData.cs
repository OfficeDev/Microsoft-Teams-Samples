// <copyright file="FacebookData.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyrightusing Microsoft.Graph;

using Microsoft.Graph;
using static IdentityLinkingWithSSO.helper.FacebookProfile;

namespace IdentityLinkingWithSSO.Models
{
    /// <summary>
    /// User data model class.
    /// </summary>
    public class FacebookData
    {
        public bool isFacebookSignedIn { get; set; }

        public Picture facebookProfileUrl { get; set; }

        public string facebookName { get; set; }

        public static class SignInIndicator
        {
            public static bool is_fb_signed_in { get; set; }

            public static bool is_fb_signed_in_search { get; set; }
        }

    }
}
