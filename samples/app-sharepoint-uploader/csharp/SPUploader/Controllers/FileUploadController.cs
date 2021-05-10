// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using MeetingExtension_SP.Models;
using MeetingExtension_SP.Repositories;
using MessageExtension_SP.Handlers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using MessageExtension_SP.Models.ViewModels;

namespace MeetingExtension_SP.Controllers
{
    /// <summary>
    /// File upload controller
    /// </summary>
    public class FileUploadController : Controller
    {
        private readonly IConfiguration configuration;
        //private readonly ISharepointRepository repository;
        private readonly IWebHostEnvironment hostingEnvironment;

        public FileUploadController(IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            // this.repository = repository;
            this.configuration = configuration;
            this.hostingEnvironment = hostingEnvironment;
        }
      
        /// <summary>
        /// Index
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// upload 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        /// <summary>
        /// Upload file to sharepoint repo
        /// </summary>
        /// <param name="fileUpload"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Upload(FileUploadViewModel fileUpload)
        {
            if (ModelState.IsValid)
            {
                string uploadsfolder = Path.Combine(this.hostingEnvironment.WebRootPath, "Files");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + fileUpload.File.FileName;
                string fileLocation = @"wwwroot/Files/" + uniqueFileName;
                if (fileUpload.File != null)
                {
                    // Write it to server.
                    using (FileStream fs = System.IO.File.Create(fileLocation))
                    {
                        await fileUpload.File.CopyToAsync(fs);
                    }

                    SharePointRepository repository = new SharePointRepository(configuration);

                    if (await repository.UploadFileToSPAsync(fileLocation, true))
                    {
                        var tempFilePath = @"Temp/TempFile.txt";
                        System.IO.File.WriteAllText(tempFilePath, fileLocation);

                        //send the card to channel based on team member role                      
                        ChannelHandler channelHandler = new ChannelHandler();
                        await channelHandler.SendConversation(configuration);
                        
                        return View("Create");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to upload your file. Please try again later.");
                    }
                }
            }

            return View(fileUpload);
        }
    }
}
