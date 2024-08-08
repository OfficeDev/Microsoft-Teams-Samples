using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace AppCompleteSample.Controllers
{
    public class BotInfoController : Controller
    {
        private bool IsValidFilePath { get; set; }

        [Route("BotInfo")]
        public ActionResult BotInfo()
        {
            List<BotInfoDetails> listData = new List<BotInfoDetails>();

            string DirectoryName = System.IO.Path.GetFullPath("src/dialogs/");

            string[] sourceCodeFiles;

            if (Directory.Exists(DirectoryName))
            {
                sourceCodeFiles = Directory.GetFiles(DirectoryName, "*.*", SearchOption.AllDirectories);
                
                foreach (var botfileName in sourceCodeFiles)
                {
                    var fileNameLink = botfileName.Substring(botfileName.LastIndexOf("\\") + 1);
                    listData.Add(new BotInfoDetails() { Name = fileNameLink });
                }
            }
            return View(listData);
        }

        [HttpPost]
        public ActionResult JqAJAX(string fileName)
        {
            StringBuilder fileCodeLines = new StringBuilder();
            FileDetalisModel fileDetails = new FileDetalisModel();

            string[] dirs = Directory.GetDirectories(System.IO.Path.GetFullPath("src/dialogs/"), "*", SearchOption.AllDirectories);

            if (dirs.Length > 0)
            {
                foreach (string dirName in dirs)
                {
                    string dirPath = dirName.Substring(dirName.IndexOf("src"), (dirName.Length - dirName.IndexOf("src")));
                    dirPath = dirPath.Replace("\\", "/") + "/";

                    fileCodeLines = ReadFileContentFromPath(System.IO.Path.GetFullPath(dirPath) + fileName);

                    //No need to read the files once we got the content
                    if (fileCodeLines.Length != 0)
                    {
                        break;
                    }
                }

                if (!IsValidFilePath)
                {
                    fileCodeLines = ReadFileContentFromPath(System.IO.Path.GetFullPath("src/dialogs/") + fileName);
                }

                fileDetails.FileName = fileName;
                fileDetails.FileCodeLines = fileCodeLines.ToString();
            }
            return Json(fileDetails);
        }

        /// <summary>
        /// Read the source file content
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private StringBuilder ReadFileContentFromPath(string filePath)
        {
            StringBuilder fileContent = new StringBuilder();
            try
            {
                string[] codeLines = System.IO.File.ReadAllLines(filePath);
                IsValidFilePath = true;

                foreach (string line in codeLines)
                {
                    fileContent.Append("<br/>");
                    fileContent.Append("\t" + line);
                }
            }
            catch
            {
                // Ignore the File here
            }
            return fileContent;
        }

    }
}