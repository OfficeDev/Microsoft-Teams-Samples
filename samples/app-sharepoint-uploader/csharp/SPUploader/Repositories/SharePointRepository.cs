// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using MeetingExtension_SP.Models.Sharepoint;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MeetingExtension_SP.Repositories
{
    /// <summary>
    /// Sharepoint Repository
    /// </summary>
    public class SharePointRepository
    {
        private readonly string sharePointSiteBaseUrl;
        private readonly IConfiguration configuration;

        public SharePointRepository(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.sharePointSiteBaseUrl = configuration["SharePointBaseURL"];
        }

        /// <summary>
        /// GetAllItemsAsync will get the sharepoint uploaded documents
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public async Task<T[]> GetAllItemsAsync<T>(string folderPath)
        {
            try
            {
                string nextUrl = string.Empty;
                dynamic result = null;

                nextUrl = this.sharePointSiteBaseUrl + "/_api/Web/GetFolderByServerRelativeUrl("+"'"+folderPath+"'"+")/Files";
               
                HttpClient httpClient = new HttpClient();
                AuthProvider authProvider = new AuthProvider(configuration);

                do
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, nextUrl);
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await authProvider.GetAccessTokenAsync());

                    var response = await httpClient.GetAsync(nextUrl);                    
                    var responseString = await response.Content.ReadAsStringAsync();
                    var curResult = JsonConvert.DeserializeObject<SPWrapper<T>>(responseString);
                    if (result == null)
                    {
                        result = curResult;
                    }
                    else
                    {
                        var curArray = curResult.Value;
                        var res = (SPWrapper<T>)result;
                        var resArray = res.Value;

                        var mergerArray = new T[curArray.Length + resArray.Length];
                        curArray.CopyTo(mergerArray, 0);
                        resArray.CopyTo(mergerArray, curArray.Length);
                        result.Value = mergerArray;
                    }

                    nextUrl = curResult.nextLink;
                }
                while (!string.IsNullOrEmpty(nextUrl));
                return result.Value;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// UploadFileToSPAsync will upload the file to sharepoint
        /// </summary>
        /// <param name="fileLocation"></param>
        /// <param name="isTemp"></param>
        /// <returns></returns>
        public async Task<bool> UploadFileToSPAsync(string fileLocation, bool isTemp)
        {

            string filename = Path.GetFileName(fileLocation).Split('_')[1];
            string endPointUrl = "";
            if (!isTemp) {
                endPointUrl = this.sharePointSiteBaseUrl + "/_api/web/GetFolderByServerRelativeUrl('" + configuration["ApprovedFolder"] + "')/files/add(url='" + filename + "',overwrite =true)";
            }
            else
            {
                endPointUrl = this.sharePointSiteBaseUrl + "/_api/web/GetFolderByServerRelativeUrl('" + configuration["StagingFolder"] + "')/files/add(url='" + filename + "',overwrite =true)";
            }
           
            AuthProvider authProvider = new AuthProvider(configuration);

            string token = await authProvider.GetAccessTokenAsync();

            using (FileStream stream = new FileStream(fileLocation, FileMode.Open, FileAccess.Read))
            {
                BinaryReader br = new BinaryReader(stream);
                long numBytes = new FileInfo(fileLocation).Length;
                byte[] bytefile = br.ReadBytes((int)numBytes);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endPointUrl);
                request.Method = "POST";
                request.Headers.Add("binaryStringRequestBody", "true");
                request.Accept = "application/json";
                request.Headers.Add("Authorization", "Bearer " + token);
                request.GetRequestStream().Write(bytefile, 0, bytefile.Length);
                try
                {
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();


                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
    }
}
