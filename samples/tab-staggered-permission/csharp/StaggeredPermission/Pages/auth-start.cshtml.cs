using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace StaggeredPermission.Pages
{
    public class authStart : PageModel
    {
	/// <summary>
	/// IConfiguration instance for fetching app settings.
	/// </summary>
        private readonly IConfiguration _configuration;

	/// <summary>
	/// Model entity for storing clientId of the app.
	/// </summary>
        public string ClientId { get; set; }

	/// <summary>
	/// Initialize the AuthStart Class.
	/// </summary>
        public authStart(IConfiguration configuration)
        {
            _configuration = configuration;
        }

	/// <summary>
	/// Handler method called when get request is made to auth-start page.
	/// </summary>
        public void OnGet()
        {
            ClientId = _configuration["MicrosoftAppId"];
        }
	}
}