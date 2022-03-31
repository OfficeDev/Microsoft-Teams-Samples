using MeetingLiveCoding.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MeetingLiveCoding.Hubs
{
    public class ChatHub : Hub
    {
        static HttpClient client = new HttpClient();

        public async Task SendMessage(string user, string description, string questionId, string meetingId, string baseUrl)
        {
            if (description != "")
            {
                var meetingDataEntity = new PostMeetingData()
                {
                    QuestionId = questionId,
                    MeetingId = meetingId,
                    Description = description
                };
                var json = JsonConvert.SerializeObject(meetingDataEntity);
                var meetingData = new StringContent(json, Encoding.UTF8, "application/json");
                await client.PostAsync(baseUrl + "/api/editorState/update", meetingData);
                await Clients.All.SendAsync("ReceiveMessage", user, description, questionId, meetingId);
            }
           
        }
    }
}