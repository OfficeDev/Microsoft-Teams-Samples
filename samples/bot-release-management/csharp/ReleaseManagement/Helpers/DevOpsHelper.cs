// <copyright file="DevOpsHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ReleaseManagement.Helpers
{
    using Microsoft.Extensions.Options;
    using ReleaseManagement.Models;
    using ReleaseManagement.Models.Configuration;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Maps the incoming workitem payload to resource model.
    /// </summary>
    public class DevOpsHelper
    {
        private readonly GraphHelper graphHelper;

        public DevOpsHelper(IOptions<AzureSettings> azureSettings)
        {
            this.graphHelper = new(azureSettings);
        }

        /// <summary>
        /// Maps the workitem object to release management task object.
        /// </summary>
        /// <param name="workItem">Incoming workitem payload.</param>
        /// <returns>Mapped release management task object.</returns>
        public async Task<ReleaseManagementTask> MapToReleaseManagementTask (WorkItem workItem)
        {
            var isAssignedToPresent = workItem.Resource.Fields.TryGetValue(Constant.AssignedTo, out string assignedToString);
            IEnumerable<string> emailList = workItem.Resource.Fields[Constant.StakeHolderTeamKey].Split(',');
            emailList = emailList.Append(GetEmailOrName(workItem.Resource.Fields[Constant.CreatedByKey], false));

            if (isAssignedToPresent)
            {
                emailList = emailList.Append(GetEmailOrName(assignedToString, false));
            }

            var validEmailList = ValidateMails(emailList);

            var releaseManagementTask = new ReleaseManagementTask
            {
                AssignedToName = isAssignedToPresent ? GetEmailOrName(assignedToString, true) : "",
                AssignedToProfileImage = isAssignedToPresent ? await graphHelper.GetProfilePictureByUserPrincipalNameAsync(GetEmailOrName(assignedToString, false)) : "",
                NotificationId = workItem.NotificationId,
                StakeholderTeam  = string.Join(", ", ValidateMails(workItem.Resource.Fields[Constant.StakeHolderTeamKey].Split(','))),
                GroupChatMembers  = validEmailList,
                State = workItem.Resource.Fields[Constant.StateKey],
                TaskTitle = workItem.Resource.Fields[Constant.TaskTitleKey],
                WorkitemUrl = (workItem.Resource._links["html"])["href"],
                CreatedByName = GetEmailOrName(workItem.Resource.Fields[Constant.CreatedByKey], true),
                CreatedByProfileImage = await graphHelper.GetProfilePictureByUserPrincipalNameAsync(GetEmailOrName(workItem.Resource.Fields[Constant.CreatedByKey], false))
            };

            return releaseManagementTask;
        }

        /// <summary>
        /// Validates mail list.
        /// </summary>
        /// <param name="emailList">List of mails to validate.</param>
        /// <returns>Enumerable of valid mails.</returns>
        private static IEnumerable<string> ValidateMails (IEnumerable<string> emailList)
        {
            var validEmailList = Enumerable.Empty<string>();
            foreach (var email in emailList)
            {
                Regex regex = new Regex(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
                RegexOptions.CultureInvariant | RegexOptions.Singleline);
                bool isValidEmail = regex.IsMatch(email.Trim());
                if (isValidEmail)
                {
                    validEmailList = validEmailList.Append(email.Trim().ToLower());
                }
            }

            return validEmailList;
        }

        /// <summary>
        /// Gets the name or mail from formatted string.
        /// </summary>
        /// <param name="formattedString">Incoming formatted string.</param>
        /// <param name="isName">Flags shows if name is required, else return mail.</param>
        /// <returns>Name or mail based isName flag.</returns>
        private static string GetEmailOrName(string formattedString, bool isName)
        {
            var result = string.Empty; 
            var splitResult = formattedString.Split('<', '>');
            if (splitResult.Length > 0)
            {
                result = isName ? splitResult[0] : splitResult[1];
            }

            return result;
        }
    }
}
