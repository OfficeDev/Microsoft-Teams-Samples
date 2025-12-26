// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace TeamsAuth0Bot
{
    public class ConfigOptions
    {
        public TeamsConfigOptions Teams { get; set; }
        public Auth0ConfigOptions Auth0 { get; set; }
        public string ApplicationUrl { get; set; }
    }

    public class TeamsConfigOptions
    {
        public string BotType { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TenantId { get; set; }
    }

    public class Auth0ConfigOptions
    {
        public string Domain { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}