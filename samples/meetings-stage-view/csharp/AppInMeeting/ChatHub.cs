namespace AppInMeeting
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.SignalR;

    /// <summary>
    /// A SignalR Hub class
    /// </summary>
    public class ChatHub: Hub
    {
        /// <summary>
        /// Method to send the message to all clients.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="message"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task SendMessage(string user, string message, string status)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message, status);
        }
    }
}
