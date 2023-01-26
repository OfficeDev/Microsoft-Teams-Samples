// <copyright file="ContentBubble.cshtml.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Content_Bubble_Bot.Pages
{
    public class ContentBubbleModel : PageModel
    {
        [FromQuery(Name = "topic")]
        public string Topic { get; set; }
        public void OnGet()
        {
        }
    }
}