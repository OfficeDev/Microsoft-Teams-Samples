using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MSGraphSearchSample.Interfaces
{
    public interface IMSTeamsService
    {
        Task<bool> SendMessageToChannel(string message, string serviceUrl);


    }
}
