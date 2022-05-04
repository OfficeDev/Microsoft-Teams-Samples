using Microsoft.Bot.Schema;
using MSGraphSearchSample.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MSGraphSearchSample.Services
{
    public class FileService : IFileService
    {
        public string GetCard(string cardName)
        {
            var filePath = Path.Combine(".", "Resources", $"{cardName}.json");
            var card = File.ReadAllText(filePath);
            return card;
        }
    }
}
