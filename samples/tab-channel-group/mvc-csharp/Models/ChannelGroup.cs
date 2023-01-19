using System;

namespace ChannelGroupTabMVC.Models
{
    public class ChannelGroup
    {
        public string Message { get; set; }

        public string GetRed()
        {
            Message = "ChannelGroup.cs says: 'You chose Red!";
            return Message;
        }

        public string GetGray()
        {
            Message = "ChannelGroup.cs says: 'You chose Gray!";
            return Message;
        }

        public string GetStatus()
        {
            Message = "Success!";
            return Message;
        }
    }

}