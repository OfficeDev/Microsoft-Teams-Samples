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
        private static readonly Regex EmailValidationRegex = new(
            @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

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
        public async Task<ReleaseManagementTask> MapToReleaseManagementTask(WorkItem workItem)
        {
            var isAssignedToPresent = workItem.Resource.Fields.TryGetValue(Constant.AssignedTo, out string assignedToString);
            var emailList = new List<string>(workItem.Resource.Fields[Constant.StakeHolderTeamKey].Split(','));
            emailList.Add(GetEmailOrName(workItem.Resource.Fields[Constant.CreatedByKey], false));

            if (isAssignedToPresent)
            {
                emailList.Add(GetEmailOrName(assignedToString, false));
            }

            var validEmailList = ValidateMails(emailList);

            var releaseManagementTask = new ReleaseManagementTask
            {
                AssignedToName = isAssignedToPresent ? GetEmailOrName(assignedToString, true) : string.Empty,
                AssignedToProfileImage = isAssignedToPresent ? await graphHelper.GetProfilePictureByUserPrincipalNameAsync(GetEmailOrName(assignedToString, false)) : string.Empty,
                NotificationId = workItem.NotificationId,
                StakeholderTeam = string.Join(", ", ValidateMails(workItem.Resource.Fields[Constant.StakeHolderTeamKey].Split(','))),
                GroupChatMembers = validEmailList,
                State = workItem.Resource.Fields[Constant.StateKey],
                TaskTitle = workItem.Resource.Fields[Constant.TaskTitleKey],
                WorkitemUrl = (workItem.Resource.Links["html"])["href"],
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
        private static IEnumerable<string> ValidateMails(IEnumerable<string> emailList)
        {
            var validEmailList = new List<string>();
            foreach (var email in emailList)
            {
                var trimmedEmail = email.Trim();
                if (EmailValidationRegex.IsMatch(trimmedEmail))
                {
                    validEmailList.Add(trimmedEmail.ToLower());
                }
            }

            return validEmailList;
        }

        /// <summary>
        /// Gets the name or mail from formatted string.
        /// </summary>
        /// <param name="formattedString">Incoming formatted string.</param>
        /// <param name="isName">Flag shows if name is required, else return mail.</param>
        /// <returns>Name or mail based isName flag.</returns>
        private static string GetEmailOrName(string formattedString, bool isName)
        {
            var splitResult = formattedString.Split('<', '>');
            return splitResult.Length > 1 ? (isName ? splitResult[0].Trim() : splitResult[1].Trim()) : string.Empty;
        }
    }
}
