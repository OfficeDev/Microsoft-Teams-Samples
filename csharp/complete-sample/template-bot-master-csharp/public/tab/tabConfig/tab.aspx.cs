using System;
using System.IO;
using System.Text;
using System.Web.UI.WebControls;

namespace Microsoft.Teams.TemplateBotCSharp.src.tab
{
    public partial class tab : System.Web.UI.Page
    {
        private bool IsValidFilePath { get; set; }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string DirectoryName = Request.MapPath("~/src/dialogs/");
                if (Directory.Exists(DirectoryName))
                {
                    string[] sourceCodeFiles = Directory.GetFiles(DirectoryName, "*.*", SearchOption.AllDirectories);
                    ListOfCodeFiles.DataSource = sourceCodeFiles;
                    ListOfCodeFiles.DataBind();
                }
            }
        }
        protected void ListOfCodeFiles_OnItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
            {
                LinkButton fileNameLink = (LinkButton)e.Item.FindControl("nameOfFile");
                String fullName = (String)e.Item.DataItem;
                fileNameLink.Text = fullName.Substring(fullName.LastIndexOf("\\") + 1);
                fileNameLink.CommandArgument = fullName.Substring(fullName.LastIndexOf("\\") + 1);
            }
        }
        protected void ListOfCodeFiles_OnItemCommand(object sender, RepeaterCommandEventArgs e)
        {
            StringBuilder fileCodeLines= new StringBuilder();

            if (e.CommandName == "GOTO")
            {
                string[] dirs = Directory.GetDirectories(Request.MapPath("~/src/dialogs/"), "*", SearchOption.AllDirectories);

                if (dirs.Length > 0)
                {
                    foreach (string dirName in dirs)
                    {
                        string dirPath = dirName.Substring(dirName.IndexOf("src"), (dirName.Length - dirName.IndexOf("src")));
                        dirPath = "~/" + dirPath.Replace("\\", "/") + "/";

                        fileCodeLines = ReadFileContentFromPath(Request.MapPath(dirPath) + (String)e.CommandArgument); 

                        //No need to read the files once we got the content
                        if(fileCodeLines.Length!=0)
                        {
                            break;
                        }
                    }

                    if (!IsValidFilePath)
                    {
                        fileCodeLines = ReadFileContentFromPath(Request.MapPath("~/src/dialogs/") + (String)e.CommandArgument);
                    }

                    fileName.Text = (String)e.CommandArgument;

                    divFileContent.Style.Add("display", "block");
                    fileContent.Text = fileCodeLines.ToString();
                }
            }
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
                string[] codeLines = File.ReadAllLines(filePath);
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