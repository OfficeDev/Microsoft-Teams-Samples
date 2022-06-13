namespace AppInMeeting
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AppInMeeting.Models;
    using AppInMeeting.Services;
    using Microsoft.AspNetCore.SignalR;

    /// <summary>
    /// A SignalR Hub class
    /// </summary>
    public class ChatHub : Hub
    {
        private readonly TasksService _taskService;

        public ChatHub(TasksService taskService)
        {
            _taskService = taskService;
        }

        /// <summary>
        /// Method to send the message to all clients.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="message"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task SendMessage(string user, string message, string status, string meetingId)
        {
            try
            {
                var taskList = new List<TaskInfoModel>();
                taskList.Add(new TaskInfoModel { TaskDescription = message, UserName = user });

                if (status == "todo")
                {
                    _taskService.ToDoDictionary.AddOrUpdate(meetingId, taskList, (key, value) => { value.Add(new TaskInfoModel { TaskDescription = message, UserName = user }); return value; });
                }
                if (status == "doing")
                {
                    _taskService.DoingDictionary.AddOrUpdate(meetingId, taskList, (key, value) => { value.Add(new TaskInfoModel { TaskDescription = message, UserName = user }); return value; });
                }
                if (status == "done")
                {
                    _taskService.DoneDictionary.AddOrUpdate(meetingId, taskList, (key, value) => { value.Add(new TaskInfoModel { TaskDescription = message, UserName = user }); return value; });
                }
                await Clients.All.SendAsync("ReceiveMessage", user, message, status);
            }
            catch(Exception ex)
            {

            }
        }
    }
}
