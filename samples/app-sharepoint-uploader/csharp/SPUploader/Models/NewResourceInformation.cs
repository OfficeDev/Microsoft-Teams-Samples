// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MeetingExtension_SP.Models
{
    /// <summary>
    /// NewResourceInformation
    /// </summary>
    public class NewResourceInformation
    {       
        public string Title { get; set; }
      
        public string Description { get; set; }

        public string FileName { get; set; }

        public string SharePointFilePath { get; set; }
    }
}
