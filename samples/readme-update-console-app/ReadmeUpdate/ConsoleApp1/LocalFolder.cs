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
        static string rootReadmeContent = "";
        static string localFolderPath = @"C:\Users\v-abt\Documents\GitHub\Microsoft-Teams-Samples\";

        // Information to show in initial template.
        static string[] languages;
        static string title = "";
        static string description = "";
        static int totalFilesUpdated = 0;

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
                title = matchSlpit[2].Trim();
                description = matchSlpit[3].Trim();

                if (!Directory.Exists(localFolderPath + samplePath + "\\" + sampleFolderPathArray[2] + "\\README.md"))
                {
                    continue;
                }

                // Getting the file contents
                var projectReadmeContent = File.ReadAllText(localFolderPath + samplePath + "\\" + sampleFolderPathArray[2] + "\\README.md"); 
                var createdTime = File.GetCreationTime(localFolderPath + samplePath + "\\" + sampleFolderPathArray[2] + "\\README.md");
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
- {sampleFolderPathArray[2]}

extensions:

contentType: samples

createdDate: {projectReadmeCreatedDate}

";

                    string updatedReadmeContent = initialContent + projectReadmeContent;

                    UpdateLocalFile(samplePath + "\\" + sampleFolderPathArray[2] + "\\README.md", updatedReadmeContent);
                    Console.WriteLine("Updated " + samplePath + "/" + sampleFolderPathArray[2] + "/README.md");
                    totalFilesUpdated += 1;
                }
                Console.WriteLine("Already updated " + samplePath + "/" + sampleFolderPathArray[2] + "/README.md");
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
            string filePath = @"C:\Users\v-abt\Documents\GitHub\Microsoft-Teams-Samples\" + path;
            using (StreamWriter newTask = new StreamWriter(filePath, false))
            {
                newTask.WriteLine(projectReadmeContent);
            }
            return true;
        }
    }
}
