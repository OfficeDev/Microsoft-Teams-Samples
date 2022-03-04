// <copyright file="ApiError.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Web.Errors
{
    using Newtonsoft.Json;

    /// <summary>
    /// API Error Response.
    /// </summary>
    public class ApiError
    {
        /// <summary>
        /// Gets or sets error.
        /// </summary>
        [JsonProperty("error")]
        public CustomError Error { get; set; }
    }

    /// <summary>
    /// API Error.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Exception")]
    public class CustomError
    {
        /// <summary>
        /// Gets or sets Error code.
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets Error message.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets API InnerError.
        /// </summary>
        [JsonProperty("innererror")]
        public InnerError InnerError { get; set; }
    }

    /// <summary>
    /// InnerError.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Exception")]
    public class InnerError
    {
        /// <summary>
        /// Gets or sets InnerError message.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}