using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AppInMeeting.Models
{
    public class TaskInfoModel
    {
        [Required]
        public string TaskDescription { get; set; }

        [Required]
        public string UserName { get; set; }
    }
}
