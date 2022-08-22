using System.Collections.Generic;

namespace ChannelNotification.Model
{
    public class GlobalVariable
    {
      public static Dictionary<string, ChannelResource> dictnotification = new Dictionary<string, ChannelResource>();
      public static  List<ChannelResource> listchannelResource = new List<ChannelResource>();
    }
}
