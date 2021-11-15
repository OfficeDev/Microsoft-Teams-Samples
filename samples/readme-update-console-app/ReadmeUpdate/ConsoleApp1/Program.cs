using ConsoleApp1;
using ConsoleApp1.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        // Information about the respository
        static string owner = "OfficeDev";
        static string repoName = "Microsoft-Teams-Samples";
        static string access_token = "";
        static string rootReadmeContent = "";
        static string branch = "v-abt/readme_update";

        // Information to show in initial template.
        static string[] languages;
        static string title = "";
        static string description = "";
        static int totalFilesUpdated = 0;

        //Get all files from a repo
        public static async Task<RepositoryContent> getRepo()
        {
            HttpClient client = new HttpClient();
            RepositoryContent root = await readRootDirectory("root", client, String.Format("https://api.github.com/repos/{0}/{1}/contents?ref={2}", owner, repoName, branch), access_token);
            client.Dispose();

            Console.WriteLine(totalFilesUpdated);
            return root;
        }

        // Recursively get the contents of all files and subdirectories within a directory 
        private static async Task<RepositoryContent> readRootDirectory(String name, HttpClient client, string uri, string access_token)
        {
            // Get repository contents from root folder.
            List<RepositoryContent> dirContents = await getRepositoryContents(string.Empty);

            // Getting only the root README file.
            RepositoryContent rootReadme = dirContents.Find(x => x.name == "README.md");

            // Getting the file contents
            string rootReadmeContent = await getFileContent(rootReadme.download_url);

            List<string> currentSamples = new List<string>();
            string line;
            using (StreamReader file = new StreamReader(GenerateStreamFromString(rootReadmeContent)))
            {
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains(":samples/"))
                    {
                        currentSamples.Add(line);
                    }
                }
            }

            Console.WriteLine("Total samples: " + currentSamples.Count);

            // Iterating through all samples in README file.
            foreach (string sampleString in currentSamples)
            {
                var splitSampleString = sampleString.Split(":");
                var sampleKey = string.Join("", splitSampleString[0].Split('[', ']')).Trim();
                var sampleFolderPathArray = splitSampleString[1].Split('/');
                var samplePath = string.Join("/", sampleFolderPathArray[0], sampleFolderPathArray[1]).Trim();

                // Getting sample details by regex
                string pattern = ".*" + sampleKey + ".*";
                var match = Regex.Match(rootReadmeContent, pattern).Value;
                var matchSlpit = match.Split('|');
                title = matchSlpit[2].Trim();
                description = matchSlpit[3].Trim();

                // Getting the languages available for a given sample.
                List<RepositoryContent> sampleFolderContent = await getRepositoryContents(samplePath);
                languages = sampleFolderContent.Where(item => item.type == "dir").Select(item => item.name).ToArray();

                List<RepositoryContent> languageContent = await getRepositoryContents(samplePath + "/" + sampleFolderPathArray[2]);

                // Getting only the project README file.
                RepositoryContent projReadme = languageContent.Find(x => x.name == "README.md");

                if(projReadme == null)
                {
                    continue;
                }

                // Getting the file contents
                var projectReadmeContent = await getFileContent(projReadme.download_url);
                var commitInformation = await getCommitInformation(samplePath + "/" + sampleFolderPathArray[2] + "/README.md");
                var projectReadmeCreatedDate = Convert.ToString(commitInformation[0].Commit.author.date);
                if (!projectReadmeContent.Contains("page_type:"))
                {
                    string initialContent = $@"
page_type: sample

description: {description}

products:
- office-teams
- office
- office-365

language(s):
- {sampleFolderPathArray[2]}

extensions:

contentType: samples

createdDate: {projectReadmeCreatedDate}

";

                    string updatedReadmeContent = initialContent + projectReadmeContent;

                    //TODO: Uncomment this while running app.
                    //var updateResponse = await UpdateFile(samplePath + "/" + sampleFolderPathArray[2] + "/" + projReadme.name, projReadme.sha, updatedReadmeContent);

                    UpdateLocalFile(samplePath + "\\" + sampleFolderPathArray[2] + "\\" + projReadme.name, updatedReadmeContent);
                    Console.WriteLine("Updated " + samplePath + "/" + sampleFolderPathArray[2] + "/" + projReadme.name);
                    totalFilesUpdated += 1;
                }
                Console.WriteLine("Already updated " + samplePath + "/" + sampleFolderPathArray[2] + "/" + projReadme.name);
            }
            return rootReadme;
        }

        // Recursively get the contents of all files and subdirectories within a directory 
        private static async Task<List<RepositoryContent>> getRepositoryContents(string path)
        {
            HttpClient sampleClient = new HttpClient();

            //get the directory contents
            HttpRequestMessage request = path == string.Empty ?
                 new HttpRequestMessage(HttpMethod.Get, String.Format("https://api.github.com/repos/{0}/{1}/contents?ref={2}", owner, repoName, branch))
                 : new HttpRequestMessage(HttpMethod.Get, String.Format("https://api.github.com/repos/{0}/{1}/contents/{2}?ref={3}", owner, repoName, path, branch));
            request.Headers.Add("Authorization",
                "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(String.Format("{0}:{1}", access_token, "x-oauth-basic"))));
            request.Headers.Add("User-Agent", "lk-github-client");

            //parse result
            HttpResponseMessage response = await sampleClient.SendAsync(request);
            String jsonStr = await response.Content.ReadAsStringAsync();
            response.Dispose();
            sampleClient.Dispose();

            List<RepositoryContent> repoContents = JsonConvert.DeserializeObject<List<RepositoryContent>>(jsonStr);
            return repoContents;
        }

        // Get commit information. 
        private static async Task<List<CommitInformation>> getCommitInformation(string path)
        {
            HttpClient sampleClient = new HttpClient();
            
            //get the directory contents
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, String.Format("https://api.github.com/repos/{0}/{1}/commits?path={2}&sha={3}", owner, repoName, path, branch));
            request.Headers.Add("Authorization",
                "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(String.Format("{0}:{1}", access_token, "x-oauth-basic"))));
            request.Headers.Add("User-Agent", "lk-github-client");

            //parse result
            HttpResponseMessage response = await sampleClient.SendAsync(request);
            String jsonStr = await response.Content.ReadAsStringAsync();
            response.Dispose();
            sampleClient.Dispose();

            List<CommitInformation> repoContents = JsonConvert.DeserializeObject<List<CommitInformation>>(jsonStr);
            return repoContents;
        }

        // Get file content.
        private static async Task<string> getFileContent(string downloadUrl)
        {
            HttpClient sampleClient = new HttpClient();

            // Getting the file contents
            HttpRequestMessage downLoadUrl = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
            downLoadUrl.Headers.Add("Authorization",
                        "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(String.Format("{0}:{1}", access_token, "x-oauth-basic"))));

            HttpResponseMessage contentResponse = await sampleClient.SendAsync(downLoadUrl);
            rootReadmeContent = await contentResponse.Content.ReadAsStringAsync();
            contentResponse.Dispose();

            return rootReadmeContent;
        }

        // Method to generate Stream from string to search for a line.
        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        // Method to update file in github branch.
        //public static async Task<string> UpdateFile(string path, string sha, string projectReadmeContent)
        //{
        //    HttpClient sampleClient = new HttpClient();
        //    sampleClient.DefaultRequestHeaders
        //                        .Accept
        //                        .Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        //    sampleClient.DefaultRequestHeaders.Add("Authorization",
        //        "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(String.Format("{0}:{1}", access_token, "x-oauth-basic"))));
        //    sampleClient.DefaultRequestHeaders.Add("User-Agent", "lk-github-client");

        //    var updateObj = new UpdateParams
        //    {
        //        content = Base64StringEncode(projectReadmeContent),
        //        message = "",
        //        sha = sha,
        //        branch = branch
        //    };

        //    var content = new StringContent(JsonConvert.SerializeObject(updateObj), Encoding.UTF8, "application/json");

        //    //parse result
        //    HttpResponseMessage response = await sampleClient.PutAsync(String.Format("https://api.github.com/repos/{0}/{1}/contents/{2}", owner, repoName, path), content);
        //    String jsonStr = await response.Content.ReadAsStringAsync();
        //    response.Dispose();
        //    sampleClient.Dispose();

        //    return jsonStr;
        //}

        private static string Base64StringEncode(string originalString)
        {
            var bytes = Encoding.UTF8.GetBytes(originalString);

            var encodedString = Convert.ToBase64String(bytes);

            return encodedString;
        }

        // MEthod to update file locally.
        public static bool UpdateLocalFile(string path, string projectReadmeContent)
        {
            string filePath = @"C:\Users\v-abt\Documents\GitHub\Microsoft-Teams-Samples\" + path;
            using (StreamWriter newTask = new StreamWriter(filePath, false))
            {
                newTask.WriteLine(projectReadmeContent);
            }
            return true;
        }
    }
}

public class MainClass
{
    public static void Main()
    {
        var isCompleted = LocalFolder.readRootDirectory();
        //var task = Program.getRepo();
        //task.Wait();
        //var dir = task.Result;
    }
}
