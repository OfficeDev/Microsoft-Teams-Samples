using ConsoleApp1;
using ConsoleApp1.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{

    //Structs used to hold file data
    public struct FileData
    {
        public String name;
        public String contents;
    }
  
    class Program
    {
        // Information about the respository
        static string owner = "OfficeDev";
        static string repoName = "Microsoft-Teams-Samples";
        static string access_token = "";
        static string rootReadmeContent = "";
        static string branch = "v-abt/readme_update";

        //Get all files from a repo
        public static async Task<DirectoryInformation> getRepo()
        {
            HttpClient client = new HttpClient();
            DirectoryInformation root = await readRootDirectory("root", client, String.Format("https://api.github.com/repos/{0}/{1}/contents?ref={2}", owner, repoName, branch), access_token);
            client.Dispose();
            return root;
        }

        // Recursively get the contents of all files and subdirectories within a directory 
        private static async Task<DirectoryInformation> readRootDirectory(String name, HttpClient client, string uri, string access_token)
        {
            // Get repository contents from root folder.
            List<RepositoryContent> dirContents = await getRepositoryContents(string.Empty);

            // Getting only the root README file.
            RepositoryContent rootReadme = dirContents.Find(x => x.name == "README.md");

            // Getting the file contents
            string rootReadmeContent = await getFileContent(rootReadme.download_url);

            //read in data
            DirectoryInformation result = new DirectoryInformation();
            result.name = name;
            result.subDirs = new List<DirectoryInformation>();
            result.files = new List<FileData>();

            FileData data;
            data.name = rootReadme.name;
            data.contents = rootReadmeContent;

            result.files.Add(data);

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

            // Iterating through all samples in README file.
            foreach (string sampleString in currentSamples)
            {
                var splitSampleString = sampleString.Split(":");
                var sampleKey = string.Join("", splitSampleString[0].Split('[', ']')).Trim();
                var sampleFolderPathArray = splitSampleString[1].Split('/');
                var samplePath = string.Join("/", sampleFolderPathArray[0], sampleFolderPathArray[1]).Trim();

                List<RepositoryContent> sampleFolderContent = await getRepositoryContents(samplePath);
                foreach(var content in sampleFolderContent)
                {
                    List<RepositoryContent> languageContent = await getRepositoryContents(samplePath + "/" + content.name);

                    // Getting only the project README file.
                    RepositoryContent projReadme = languageContent.Find(x => x.name == "README.md");

                    // Getting the file contents
                    var projectReadmeContent = await getFileContent(projReadme.download_url);

                    var updateResponse = await UpdateFile(samplePath + "/" + content.name + "/" + projReadme.name);
                }
                //HttpClient tempClient = new HttpClient();
                //DirectoryInformation sub = await readRootDirectory(samplePath, tempClient, file._links.self, access_token);
            }

              return result;
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

        // Method to update file.
        public static async Task<string> UpdateFile(string path)
        {
            HttpClient sampleClient = new HttpClient();
            sampleClient.DefaultRequestHeaders
                                .Accept
                                .Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            sampleClient.DefaultRequestHeaders.Add("Authorization",
                "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(String.Format("{0}:{1}", access_token, "x-oauth-basic"))));
            sampleClient.DefaultRequestHeaders.Add("User-Agent", "lk-github-client");

            var content = new StringContent("{\"content\":\"someValue\"}", Encoding.UTF8, "application/json");

            //parse result
            HttpResponseMessage response = await sampleClient.PutAsync(String.Format("https://api.github.com/repos/{0}/{1}/contents/{2}?ref={3}", owner, repoName, path, branch), content);
            String jsonStr = await response.Content.ReadAsStringAsync();
            response.Dispose();
            sampleClient.Dispose();

            return jsonStr;
        }
    }
}

public class MainClass
{
    public static void Main()
    {
        var task = Program.getRepo();
        task.Wait();
        var dir = task.Result;
    }
}
