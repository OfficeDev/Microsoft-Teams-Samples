// <copyright file="ErrorViewModel.cs" company="Microsoft">
// Copyright (c) Microsoft. All Rights Reserved.
// </copyright>

namespace TabExternalAuth.Models
{
    /// <summary>
    /// Error view model class.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Request id for the api request
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Returns true or false if request id is empty
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}