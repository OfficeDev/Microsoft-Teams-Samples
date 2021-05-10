// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Newtonsoft.Json;

namespace MeetingExtension_SP.Models.Sharepoint
{
    /// <summary>
    /// base for sharepoint doc library
    /// </summary>
    public class SharePointBase
    {
        [JsonProperty("odata.nextLink")]
        public string nextLink { get; set; }
    }
}
