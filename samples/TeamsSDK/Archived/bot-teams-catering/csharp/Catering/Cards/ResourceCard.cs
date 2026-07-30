using AdaptiveCards;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System.IO;

namespace Catering.Cards
{
    public class CardResource
    {
        private readonly string _fileName;
        private readonly string _filePath;

        public CardResource(string fileName)
        {
            _fileName = fileName;
            _filePath = Path.Combine(".", "Resources", fileName);
        }

        public string AsJson()
        {
            return File.ReadAllText(_filePath);
        }

        public string AsJson<T>(T data)
        {
            var cardJson = AsJson();
            var jsonData = JsonConvert.SerializeObject(data);
            var token = Newtonsoft.Json.Linq.JToken.Parse(cardJson);
            var dataToken = Newtonsoft.Json.Linq.JToken.Parse(jsonData);
            token.Merge(dataToken);
            return token.ToString();
        }

        public object AsJObject()
        {
            var json = AsJson();
            return JsonConvert.DeserializeObject(json) ?? new object();
        }

        public object AsJObject<T>(T data)
        {
            var json = AsJson(data);
            return JsonConvert.DeserializeObject(json) ?? new object();
        }

        public Attachment AsAttachment()
        {
            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = AsJObject(),
            };
        }

        public Attachment AsAttachment<T>(T data)
        {
            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = AsJObject(data),
            };
        }
    }
}
