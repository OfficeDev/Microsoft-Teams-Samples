using Microsoft;
using System.Collections.Generic;
using TeamsToDoAppConnector.Models;

namespace TeamsToDoAppConnector.Repository
{
    /// <summary>
    /// Represents the Task repository class which stores the temporary task data.
    /// </summary>
    public class TaskRepository
    {
        public static List<TaskItem> Tasks { get; set; } = new List<TaskItem>();

        static TaskRepository()
        {
            Tasks.Add(new TaskItem
            {
                Title = "Get the bills",
                Assigned = "Alex",
                Description = "Get the travel and accomodation bills",
                Guid = Guid.NewGuid().ToString()
            });

            Tasks.Add(new TaskItem
            {
                Title = "Add a new team member",
                Assigned = "John",
                Description = "New member Rin joined, please add her in team.",
                Guid = Guid.NewGuid().ToString()
            });

            Tasks.Add(new TaskItem
            {
                Title = "Create new tenant",
                Assigned = "Vishal",
                Description = "Get new tenant for testing.",
                Guid = Guid.NewGuid().ToString()
            });
        }
    }
}