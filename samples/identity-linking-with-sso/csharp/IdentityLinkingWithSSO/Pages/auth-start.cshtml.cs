using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace IdentityLinkingWithSSO.Pages
{
    public class authStart : PageModel
    {
        private readonly IConfiguration _configuration;

        public string ClientId { get; set; }

        public authStart(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {
            ClientId = _configuration["MicrosoftAppId"];
        }
    }
}
