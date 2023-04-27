using StaggeredPermission.helper;
using StaggeredPermission.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace StaggeredPermission.Controllers
{
    public class TabController : Controller
    {
        public readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TabController(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;

        }

        // Get user's profile photo.
        [HttpPost]
        [Route("GetUserPhoto")]
        public async Task<JsonResult> GetUserPhoto(string idToken)
        {
            try
            {
                var bearerToken = await AuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, _httpClientFactory, _httpContextAccessor, idToken);
                var emails = new List<UserEmail>();
                var client = new SimpleGraphClient(bearerToken);
                var me = await client.GetMeAsync();
                var title = !string.IsNullOrEmpty(me.JobTitle) ?
                            me.JobTitle : "Unknown";

                var photo = await client.GetPhotoAsync();

                var userInfo = new UserData()
                {
                    Photo = photo
                };

                var jsonString = JsonConvert.SerializeObject(userInfo);
                return Json(jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        // Get user's mails.
        [HttpPost]
        [Route("GetUserMails")]
        public async Task<JsonResult> GetUserMails(string idToken)
        {
            try
            {
                var bearerToken = await AuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, _httpClientFactory, _httpContextAccessor, idToken);
                var emails = new List<UserEmail>();
                UserEmail entity;
                var client = new SimpleGraphClient(bearerToken);

                var mails = await client.GetMailsAsync();

                foreach (var mail in mails.CurrentPage)
                {
                    if (mail.Sender != null &&
                        mail.ToRecipients.Any() &&
                        mail.Subject.Any())
                    {
                        entity = new UserEmail();
                        entity.FromMail = mail.Sender.EmailAddress.Address.ToString();
                        entity.ToMail = mail.ToRecipients.ElementAt(0).EmailAddress.Address.ToString();
                        entity.Subject = mail.Subject.ToString();
                        entity.Time = mail.SentDateTime.ToString();
                        emails.Add(entity);
                    }
                }

                var userInfo = new UserData()
                {
                    Details = emails
                };

                var jsonString = JsonConvert.SerializeObject(userInfo);
                return Json(jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        // Get user details based on jwt id token.
        [HttpPost]
        [Route("decodeToken")]
        public async Task<JsonResult> DecodeToken(string accessToken)
        {

            try
            {
              //  string name = "", email = "";
                object nameObj = null, emailObj = null;
                var handler = new JwtSecurityTokenHandler();
                var decodedValue = handler.ReadJwtToken(accessToken);
                if (decodedValue.Payload.ContainsKey("name"))
                {
                    decodedValue.Payload.TryGetValue("name", out nameObj);
                }

                if (decodedValue.Payload.ContainsKey("unique_name"))
                {
                    decodedValue.Payload.TryGetValue("unique_name", out emailObj);
                }

                var userInfo = new UserData()
                {
                    Name = nameObj.ToString(),
                    Email = emailObj.ToString()
                };

                var jsonString = JsonConvert.SerializeObject(userInfo);
                return Json(jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }
    }
}