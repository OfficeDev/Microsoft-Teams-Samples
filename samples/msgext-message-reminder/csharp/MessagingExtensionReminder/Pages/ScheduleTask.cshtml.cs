using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MessagingExtensionReminder.Pages
{
    public class ScheduleTaskModel
        : PageModel
    {
        public string? Title { get; set; }
        public string? Description { get; set; }

        public void OnGet(string? title, string? description)
        {
            Title = title ?? string.Empty;
            Description = description ?? string.Empty;
        }
    }
}
