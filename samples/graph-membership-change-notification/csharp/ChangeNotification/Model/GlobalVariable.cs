using System.Collections.Generic;

namespace ChangeNotification.Model
{
    public class GlobalVariable
    {
      public static Dictionary<string, ChannelResource> dictnotification = new Dictionary<string, ChannelResource>();
      public static  List<ChannelResource> channelResourceList = new List<ChannelResource>();
    }
}
