// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AppCheckinLocation
{
    public class ConfigOptions
    {
        public string ApplicationBaseUrl { get; set; }
        public TeamsConfigOptions Teams { get; set; }
    }

    public class TeamsConfigOptions
    {
        public string BotType { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TenantId { get; set; }
    }
}