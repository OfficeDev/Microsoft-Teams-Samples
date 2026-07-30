using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
namespace AppCompleteSample.Controllers
{
    /// <summary>
    /// Controller to handle bot information requests.
    /// </summary>
    public class BotInfoController : Controller
    {
        private bool isValidFilePath;

        /// <summary>
        /// Gets the bot information.
        /// </summary>
        /// <returns>A view with the list of bot information details.</returns>
        [Route("BotInfo")]
        public ActionResult BotInfo()
        {
            var listData = new List<BotInfoDetails>();
            var directoryName = Path.GetFullPath("src/dialogs/");

            if (Directory.Exists(directoryName))
            {
                var sourceCodeFiles = Directory.GetFiles(directoryName, "*.*", SearchOption.AllDirectories);

                foreach (var botFileName in sourceCodeFiles)
                {
                    var fileNameLink = Path.GetFileName(botFileName);
                    listData.Add(new BotInfoDetails { Name = fileNameLink });
                }
            }

            return View(listData);
        }

        /// <summary>
        /// Handles AJAX requests to get file details.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>A JSON result with the file details.</returns>
        [HttpPost]
        public async Task<ActionResult> JqAJAX(string fileName)
        {
            var fileCodeLines = new StringBuilder();
            var fileDetails = new FileDetailsModel();
            var dirs = Directory.GetDirectories(Path.GetFullPath("src/dialogs/"), "*", SearchOption.AllDirectories);

            if (dirs.Length > 0)
            {
                foreach (var dirName in dirs)
                {
                    var dirPath = dirName.Substring(dirName.IndexOf("src")).Replace("\\", "/") + "/";
                    fileCodeLines = await ReadFileContentFromPathAsync(Path.GetFullPath(dirPath) + fileName);

                    if (fileCodeLines.Length != 0)
                    {
                        break;
                    }
                }

                if (!isValidFilePath)
                {
                    fileCodeLines = await ReadFileContentFromPathAsync(Path.GetFullPath("src/dialogs/") + fileName);
                }

                fileDetails.FileName = fileName;
                fileDetails.FileCodeLines = fileCodeLines.ToString();
            }

            return Json(fileDetails);
        }

        /// <summary>
        /// Reads the source file content.
        /// </summary>
        /// <param name="filePath">The path of the file.</param>
        /// <returns>A StringBuilder with the file content.</returns>
        private async Task<StringBuilder> ReadFileContentFromPathAsync(string filePath)
        {
            var fileContent = new StringBuilder();

            try
            {
                var codeLines = await System.IO.File.ReadAllLinesAsync(filePath);
                isValidFilePath = true;

                foreach (var line in codeLines)
                {
                    fileContent.Append("<br/>");
                    fileContent.Append("\t" + line);
                }
            }
            catch
            {
                // Ignore the file here
            }

            return fileContent;
        }
    }
}