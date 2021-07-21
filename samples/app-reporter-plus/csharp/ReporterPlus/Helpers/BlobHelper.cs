using AdaptiveCards.Templating;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Azure.Storage.Blobs;
using ReporterPlus.Models;
using Azure.Storage.Blobs.Models;

namespace ReporterPlus.Helpers
{
    public class BlobHelper
    {
        public static async Task<string> UploadToBlob(TaskModuleSubmitDataDeserializer taskModuleOutput, ITurnContext turnContext)
        {
            List<string> imageURL = new List<string>();
            string audioURL = taskModuleOutput.data.RecorderOutput;
            string requestID = taskModuleOutput.data.RequestId;
            byte[] streamConverter;
            string assignedToUserImageUrl;
            string submittedByUserImageUrl;
            List<Images> imgURL = new List<Images> {  };
            var blobHttpHeader = new BlobHttpHeaders();
            blobHttpHeader.ContentType = "image/jpeg";

            BlobContainerClient container = new BlobContainerClient(Constants.BlobConnectionString, Constants.BlobContainerName);
            BlobClient blob;

            var assignedToUserImage = GetImage(taskModuleOutput.data.AssignedTo.objectId).Result;
            if(assignedToUserImage != null)
            {
                blob = container.GetBlobClient(requestID + "-" + taskModuleOutput.data.AssignedTo.objectId + ".jpg");
                blob.UploadAsync(assignedToUserImage, blobHttpHeader);
                assignedToUserImageUrl = blob.Uri.ToString();
            }
            else
            {
                assignedToUserImageUrl = Constants.MemberGenericImageUrl;
            }

            var submittedByUserImage = GetImage(turnContext.Activity.From.AadObjectId).Result;
            if (submittedByUserImage != null)
            {
                blob = container.GetBlobClient(requestID + "-" + turnContext.Activity.From.AadObjectId + ".jpg");
                blob.UploadAsync(submittedByUserImage, blobHttpHeader);
                submittedByUserImageUrl = blob.Uri.ToString();
            }
            else
            {
                submittedByUserImageUrl = Constants.MemberGenericImageUrl;
            }

            if (taskModuleOutput.data.ImagesOutput.Length != 0)
            {
                foreach (string item in taskModuleOutput.data.ImagesOutput)
                {
                    imageURL.Add(item);
                }
            }
            else
            {
                imageURL = null;
            }

            if(imageURL!= null){
                foreach (var item in imageURL)
                {
                    var imgs = new Images { url = item };
                    imgURL.Add(imgs);
                }
            }

            string[] blobStorage = { ".", "Resources", "BlobStorage.json" };
            var blobStorageText = File.ReadAllText(Path.Combine(blobStorage));
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(blobStorageText);

            var payloadData = new
            {
                requestID = requestID,
                status = Status.Pending,
                itemName = taskModuleOutput.data.ItemName,
                itemCode = taskModuleOutput.data.ItemCode,
                assignedToName = taskModuleOutput.data.AssignedTo.displayName,
                assignedToId = taskModuleOutput.data.AssignedTo.objectId,
                assignedToMail = taskModuleOutput.data.AssignedTo.email,
                submittedByName = turnContext.Activity.From.Name,
                submittedById = turnContext.Activity.From.AadObjectId,
                submittedByMail = taskModuleOutput.data.SubmittedByMail,
                assignedToUserImage = assignedToUserImageUrl,
                submittedByUserImage = submittedByUserImageUrl,
                imageURL = imgURL,
                audioURL = audioURL,
                comments = taskModuleOutput.data.Comments,
                conversationId = turnContext.Activity.Conversation.Id,
                messageId = "",
            };

            var blobTextString = template.Expand(payloadData);
            streamConverter = Encoding.ASCII.GetBytes(blobTextString);
            blob = container.GetBlobClient(requestID + ".txt");
            await blob.UploadAsync(new MemoryStream(streamConverter));
            return requestID;
        }

        public static async Task<BlobDataDeserializer> GetBlob(string reqId, string request, string messageId = null)
        {
            var fileName = reqId + ".txt";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Constants.BlobConnectionString);
            CloudBlobClient serviceClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = serviceClient.GetContainerReference(Constants.BlobContainerName);
            CloudBlockBlob blob = container.GetBlockBlobReference(fileName);
            if (Status.Approved.Equals(request) || Status.Rejected.Equals(request) || messageId != null)
            {
                await UpdateBlob(blob, request, messageId);
            }
            string contents = blob.DownloadTextAsync().Result;
            var responseJson = JsonConvert.DeserializeObject<BlobDataDeserializer>(contents);
            return responseJson;
        }

        private static async Task<string> UpdateBlob(CloudBlockBlob blob, string request, string messageId = null)
        {
            string contents = blob.DownloadTextAsync().Result;
            var contentsJson = JObject.Parse(contents);
            if (request != null)
            {
                contentsJson["status"] = request;
            }
            else
            {
                contentsJson["messageId"] = messageId;
            }
            var contentString = contentsJson.ToString();
            await blob.UploadTextAsync(contentString);
            return "Success";
        }

        private static async Task<Stream> GetImage(string userId)
        {
            var graphClient = GraphClient.GetGraphClient(Constants.MicrosoftAppId, Constants.MicrosoftAppPassword, Constants.TenantId);
            try
            {
                Stream stream = await graphClient.Users[userId].Photo.Content
                .Request()
                .GetAsync();
                return stream;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }
    }
}
