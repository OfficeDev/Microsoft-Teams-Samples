namespace TeamsTalentMgmtApp.Models.DatabaseContext
{
    public sealed class Location
    {
        public int LocationId { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string LocationAddress => $"{City}{(string.IsNullOrEmpty(State) ? string.Empty : $", {State}")}";
    }
}
