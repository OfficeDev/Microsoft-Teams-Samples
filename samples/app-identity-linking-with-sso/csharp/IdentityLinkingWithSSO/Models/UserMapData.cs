// <copyright file="UserData.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyrightusing Microsoft.Graph;

namespace IdentityLinkingWithSSO.Models
{
    /// <summary>
    /// User map data model class.
    /// </summary>
    public class UserMapData
    {
        public string AadId { get; set; }

        public bool isAadSignedIn { get; set; }

        public string AadToken { get; set; }

        public string FacebookId { get; set; }

        public string FacebookToken { get; set; }

        public bool isFacebookSignedIn { get; set; }

        public string GoogleId { get; set; }

        public string GoogleToken { get; set; }

        public bool isGoogleSignedIn { get; set; }
    }
}
