using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace StaggeredPermission.Pages
{
    public class authEnd : PageModel
    {
        private readonly IConfiguration _configuration;

        public string ClientId { get; set; }

        public authEnd(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {
            ClientId = _configuration["MicrosoftAppId"];
        }
    }
}
