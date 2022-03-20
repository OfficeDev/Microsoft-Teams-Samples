using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppInMeeting.Pages
{
    public class AppSpecificPartModel : PageModel
    {
        public string PartName { get; set; }
        public void OnGet(string partName)
        {
            PartName = partName;
        }
    }
}
