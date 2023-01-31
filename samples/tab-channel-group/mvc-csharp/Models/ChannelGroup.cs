using System;

namespace ChannelGroupTabMVC.Models
{
    public class ChannelGroup
    {
        public string Message { get; set; }

        public string GetRed()
        {
            Message = "ChannelGroup.cs says: 'You chose Red! - Testing YMAL";
            return Message;
        }

        public string GetGray()
        {
            Message = "ChannelGroup.cs says: 'You chose Gray! - Testing YMAL";
            return Message;
        }

        public string GetStatus()
        {
            Message = "Success!";
            return Message;
        }
    }

}