// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace Microsoft.BotBuilderSamples.Pages
{
    public class CustomFormModel : PageModel
    {
        public CustomFormModel(IConfiguration config)
        {
            TeamsAppId = config["TeamsAppId"];
        }

        public string TeamsAppId { get; private set; }
    }
}
