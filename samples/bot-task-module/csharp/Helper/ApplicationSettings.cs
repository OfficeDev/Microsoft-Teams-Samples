// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using AdaptiveCards;
using System.Collections.Generic;
using System.Configuration;
using System.Web;

namespace Microsoft.Teams.Samples.TaskModule.Web.Helper
{
    public  class ApplicationSettings
    {
        public string BaseUrl { get; set; }
        public string MicrosoftAppId { get; set; }

    }
}
