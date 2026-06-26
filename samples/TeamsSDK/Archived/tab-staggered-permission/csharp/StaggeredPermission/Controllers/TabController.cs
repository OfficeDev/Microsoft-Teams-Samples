using StaggeredPermission.helper;
using StaggeredPermission.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace StaggeredPermission.Controllers
{
    public class TabController : Controller
    {
        public readonly IConfiguration _configuration;

        public TabController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Get user's profile photo.
        [HttpPost]
        [Route("GetUserPhoto")]
        public async Task<JsonResult> GetUserPhoto(string idToken)
        {
            try
            {
                var bearerToken = await AuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, idToken);
                var client = new SimpleGraphClient(bearerToken);
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
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return Json(JsonConvert.SerializeObject(new { error = "Unable to fetch user photo.", details = ex.Message }));
            }
        }

        // Get user's mails.
        [HttpPost]
        [Route("GetUserMails")]
        public async Task<JsonResult> GetUserMails(string idToken)
        {
            try
            {
                var bearerToken = await AuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, idToken);
                var emails = new List<UserEmail>();
                var client = new SimpleGraphClient(bearerToken);

                var mails = await client.GetMailsAsync();

                foreach (var mail in mails)
                {
                    if (mail.Sender != null &&
                        mail.ToRecipients?.Any() == true &&
                        !string.IsNullOrEmpty(mail.Subject))
                    {
                        var userEntity = new UserEmail
                        {
                            FromMail = mail.Sender.EmailAddress?.Address,
                            ToMail = mail.ToRecipients.FirstOrDefault()?.EmailAddress?.Address,
                            Subject = mail.Subject,
                            Time = mail.SentDateTime?.ToString()
                        };

                        emails.Add(userEntity);
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
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return Json(JsonConvert.SerializeObject(new { error = "Unable to fetch user mails.", details = ex.Message }));
            }
        }

        // Get user details based on jwt id token.
        [HttpPost]
        [Route("decodeToken")]
        public JsonResult DecodeToken(string accessToken)
        {
            try
            {
                object nameObj = null, emailObj = null;
                var handler = new JwtSecurityTokenHandler();
                var decodedValue = handler.ReadJwtToken(accessToken);
                if (decodedValue.Payload.ContainsKey("name"))
                {
                    decodedValue.Payload.TryGetValue("name", out nameObj);
                }

                if (decodedValue.Payload.ContainsKey("preferred_username"))
                {
                    decodedValue.Payload.TryGetValue("preferred_username", out emailObj);
                }

                var userInfo = new UserData()
                {
                    Name = nameObj?.ToString(),
                    Email = emailObj?.ToString()
                };

                var jsonString = JsonConvert.SerializeObject(userInfo);
                return Json(jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return Json(JsonConvert.SerializeObject(new { error = "Unable to decode token.", details = ex.Message }));
            }
        }
    }
}