using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace MeetingLiveCoding.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string description)
        {
            if(description != "")
            await Clients.All.SendAsync("ReceiveMessage", user, description);
        }
    }
}