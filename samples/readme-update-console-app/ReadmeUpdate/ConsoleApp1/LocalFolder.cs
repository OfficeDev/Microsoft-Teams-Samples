using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ConsoleApp1
{
    public static class LocalFolder
    {
        // Information about the respository
        static string localFolderPath = @"C:\Users\v-abt\Documents\GitHub\Microsoft-Teams-Samples\";

        // Information to show in initial template.
        static string title = "";
        static string description = "";
        static int totalFilesUpdated = 0;
        static string language = "";

        // Recursively get the contents of all files and subdirectories within a directory 
        public static bool readRootDirectory()
        {
            // Getting the file contents
            string rootReadmeContent = File.ReadAllText(localFolderPath + "README.md");

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
                var samplePath = string.Join("\\", sampleFolderPathArray[0], sampleFolderPathArray[1]).Trim();

                // Getting sample details by regex
                string pattern = ".*" + sampleKey + ".*";
                var match = Regex.Match(rootReadmeContent, pattern).Value;
                var matchSlpit = match.Split('|');
                title = matchSlpit[2]?.Trim();
                description = matchSlpit[3]?.Trim();

                var projectReadmePath = localFolderPath + samplePath + "\\";
                if (sampleFolderPathArray.Length == 3)
                {
                    projectReadmePath += sampleFolderPathArray[2];
                    language = sampleFolderPathArray[2];
                }
                else
                {
                    language = "js";
                }
                if (!Directory.Exists(projectReadmePath))
                {
                    continue;
                }

                // Getting the file contents
                var projectReadmeContent = File.ReadAllText(projectReadmePath + "\\README.md");
                var createdTime = File.GetCreationTime(projectReadmePath + "\\README.md");
                var projectReadmeCreatedDate = Convert.ToString(createdTime);
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
- {language}

extensions:

contentType: samples

createdDate: {projectReadmeCreatedDate}

";

                    string updatedReadmeContent = initialContent + projectReadmeContent;

                    UpdateLocalFile(projectReadmePath + "\\README.md", updatedReadmeContent);
                    Console.WriteLine("Updated " + projectReadmePath + "/README.md");
                    totalFilesUpdated += 1;
                }
                Console.WriteLine("Already updated " + projectReadmePath + "/README.md");
            }
            return true;
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

        // MEthod to update file locally.
        public static bool UpdateLocalFile(string path, string projectReadmeContent)
        {
            using (StreamWriter newTask = new StreamWriter(path, false))
            {
                newTask.WriteLine(projectReadmeContent);
            }
            return true;
        }
    }
}
