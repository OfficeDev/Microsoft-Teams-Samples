
namespace PersonalTabMVC.Models
{
    public class PersonalTab
    {
        public string Message { get; set; }

        public string GetColor()
        {
            Message = "PersonalTab.cs says: 'Hello ";
            return Message;
        }

        
    }
}