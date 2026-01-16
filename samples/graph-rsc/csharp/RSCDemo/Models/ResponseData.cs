// <copyright file="ResponseData.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;

namespace RSCWithGraphAPI.Models
{
    /// <summary>
    /// Class for graph api response data
    /// </summary>
    public class ResponseData
    {
        /// <summary>
        /// List of installed app
        /// </summary>
        public List<AppData> Value { get; set; }
    }

    /// <summary>
    /// Class for installed app data
    /// </summary>
    public class AppData
    {
        /// <summary>
        /// Id of the installed app
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Teams app defination of the installed app
        /// </summary>
        public AppDefination TeamsAppDefinition { get; set; }
    }

    /// <summary>
    /// Teams app defination of the installed app
    /// </summary>
    public class AppDefination
    {
        /// <summary>
        /// Id of the installed app
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Display name of the installed app
        /// </summary>
        public string DisplayName { get; set; }
    }
}