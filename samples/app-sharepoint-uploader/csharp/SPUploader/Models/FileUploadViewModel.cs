using MeetingExtension_SP.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MessageExtension_SP.Models.ViewModels
{
    public class FileUploadViewModel: NewResourceInformation
    {
        [Required]
        public IFormFile File { get; set; }
    }
}
