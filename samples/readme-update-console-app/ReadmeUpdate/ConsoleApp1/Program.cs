using ConsoleApp1;
using ConsoleApp1.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
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

        //Get all files from a repo
        public static async Task<DirectoryInformation> getRepo()
        {
            HttpClient client = new HttpClient();
            DirectoryInformation root = await readRootDirectory("root", client, String.Format("https://api.github.com/repos/{0}/{1}/contents/", owner, repoName), access_token);
            client.Dispose();
            return root;
        }

        // Recursively get the contents of all files and subdirectories within a directory 
        private static async Task<DirectoryInformation> readRootDirectory(String name, HttpClient client, string uri, string access_token)
        {
            //get the directory contents
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization",
                "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(String.Format("{0}:{1}", access_token, "x-oauth-basic"))));
            request.Headers.Add("User-Agent", "lk-github-client");

            //parse result
            HttpResponseMessage response = await client.SendAsync(request);
            String jsonStr = await response.Content.ReadAsStringAsync(); ;
            response.Dispose();

            List<FileInformation> dirContents = JsonConvert.DeserializeObject<List<FileInformation>>(jsonStr);

            // Getting only the root README file.
            FileInformation rootReadme = dirContents.Find(x => x.name == "README.md");

            // Getting the file contents
            HttpRequestMessage downLoadUrl = new HttpRequestMessage(HttpMethod.Get, rootReadme.download_url);
            downLoadUrl.Headers.Add("Authorization",
                        "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(String.Format("{0}:{1}", access_token, "x-oauth-basic"))));
            request.Headers.Add("User-Agent", "lk-github-client");

            HttpResponseMessage contentResponse = await client.SendAsync(downLoadUrl);
            rootReadmeContent = await contentResponse.Content.ReadAsStringAsync();
            contentResponse.Dispose();

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
                var samplePath = splitSampleString[1];
                //HttpClient tempClient = new HttpClient();
                //DirectoryInformation sub = await readRootDirectory(samplePath, tempClient, file._links.self, access_token);
            }

                //foreach (FileInfo file in dirContents)
                //{
                //    if (file.type == "dir")
                //    { //read in the subdirectory
                //        Directory sub = await readRootDirectory(file.name, client, file._links.self, access_token);
                //        result.subDirs.Add(sub);
                //    }
                //    else
                //    {
                //        //get the file contents;
                //        HttpRequestMessage downLoadUrl = new HttpRequestMessage(HttpMethod.Get, file.download_url);
                //        downLoadUrl.Headers.Add("Authorization",
                //            "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(String.Format("{0}:{1}", access_token, "x-oauth-basic"))));
                //        request.Headers.Add("User-Agent", "lk-github-client");

                //        HttpResponseMessage contentResponse = await client.SendAsync(downLoadUrl);
                //        String content = await contentResponse.Content.ReadAsStringAsync();
                //        contentResponse.Dispose();

                //        FileData data;
                //        data.name = file.name;
                //        data.contents = content;

                //        result.files.Add(data);
                //    }
                //}
              return result;
        }

        // Method to generate Stream from string to search for a line.
        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        // Method to append information to readme file.
        public static void UpdateReadme()
        {

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
