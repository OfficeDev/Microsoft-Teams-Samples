using System;
using System.Collections.Generic;
using System.Text;

namespace MSGraphSearchSample.Models
{
    public class AppConfigOptions
    {
        public string MicrosoftAppId { get; set; }
        public string MicrosoftAppPassword { get; set; }
        public string MicrosoftAppTenantId { get; set; }
        public string ConnectionName { get; set; }
        public int SearchSizeThreshold { get; set; }
        public int SearchPageSize { get; set; }
    }
}
