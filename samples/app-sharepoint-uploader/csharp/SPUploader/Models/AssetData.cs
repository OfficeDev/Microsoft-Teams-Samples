// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MessageExtension_SP.Models
{
    /// <summary>
    /// AsserData
    /// </summary>
    public class AssetData
    {
        public string ApproverName { get; set; }
        public string approverId { get; set; }
        public string SubmittedBy { get; set; }
        public string DateOfSubmission { get; set; }
        public string SubitteTo { get; set; }
        public string NameOfDocument { get; set; }

        public string DocName { get; set; }
        public string url { get; set; }
        public string userChat { get; set; }
        public string userMRI { get; set; }
    }
}
