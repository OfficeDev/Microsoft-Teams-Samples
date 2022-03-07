// <copyright file="GoogleData.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyrightusing Microsoft.Graph;

namespace IdentityLinkingWithSSO.Models
{
    /// <summary>
    /// User data model class.
    /// </summary>
    public class GoogleData
    {
        public bool isGoogleSignedIn { get; set; }

        public string googleProfileUrl { get; set; }

        public string googleName { get; set; }

        public string googleEmail { get; set; }

        public static class SignInIndicatorGoogle
        {
            public static bool is_google_signed_in { get; set; }

            public static bool is_google_signed_in_search { get; set; }
        }
    }
}
