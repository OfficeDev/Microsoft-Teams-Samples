// <copyright file="ErrorViewModel.cs" company="Microsoft">
// Copyright (c) Microsoft. All Rights Reserved.
// </copyright>

namespace TeamsAuthSSO.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
