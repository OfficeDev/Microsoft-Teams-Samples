namespace GraphTeamsTag.Models
{
    public class MeetingEventUpdateTodo:MeetingCreation
    {
        public IEnumerable<TeamTagMember> MembersToBeAdded { get; set; }
        public IEnumerable<TeamTagMember> MembersToBeDeleted { get; set; }
    }
}
