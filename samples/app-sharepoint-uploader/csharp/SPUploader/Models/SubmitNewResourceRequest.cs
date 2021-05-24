// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MeetingExtension_SP.Models
{
    /// <summary>
    /// Action type
    /// </summary>
    public class ActionType
    {       
        public ActionTypes action { get; set; }
        public string data { get; set; }
    }

    /// <summary>
    /// SubmitNewResourceRequest
    /// </summary>
    public class SubmitNewResourceRequest :ActionType
    {
        public string WebhookURL { get; set; }
        public NewResourceInformation ResourceInfo { get; set; }
        public string AppId { get; set; }
    }

    /// <summary>
    /// ActionTypes
    /// </summary>
    public class ActionTypes
    {
        public string type { get; set; }
        public string title { get; set; }
        public string verb { get; set; }
    }
}
