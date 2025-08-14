// <copyright file="InMeetingNotificationModel.cshtml.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InMeetingNotificationsBot.Pages
{
    public class InMeetingNotificationModel : PageModel
    {
        [FromQuery(Name = "topic")]
        public string Topic { get; set; }
        public void OnGet()
        {
        }
    }
}