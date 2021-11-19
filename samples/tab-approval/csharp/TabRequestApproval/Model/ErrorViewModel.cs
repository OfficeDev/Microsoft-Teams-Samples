// <copyright file="ErrorViewModel.cs" company="Microsoft">
// Copyright (c) Microsoft. All Rights Reserved.
// </copyright>

namespace TabRequestApproval.Model
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
