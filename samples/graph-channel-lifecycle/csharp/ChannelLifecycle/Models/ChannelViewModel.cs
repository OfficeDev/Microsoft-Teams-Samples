using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChannelLifecycle.Models
{
    public class ChannelViewModel
    {
        public List<ChannelModel> channelList { get; set; }
        public List<string> TabsList { get; set; }
        public List<string> Members { get; set; }
        public string TenantId { get; set; }
        public string GroupId { get; set; }
    }

    public class ChannelModel
    {     
        public string ChannelId { get; set; }
        public string ChannelName { get; set; }
        public string ChannelDesc { get; set; }



    }
}
