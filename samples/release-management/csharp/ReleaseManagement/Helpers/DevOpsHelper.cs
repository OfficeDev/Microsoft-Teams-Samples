// <copyright file="DevOpsHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using ReleaseManagement.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ReleaseManagement.Helpers
{
    public class DevOpsHelper
    {
        /// <summary>
        /// Maps the workitem object to release management task object.
        /// </summary>
        /// <param name="workItem">Incoming workitem payload.</param>
        /// <returns>Mapped release management task object.</returns>
        public static ReleaseManagementTask MapToReleaseManagementTask (WorkItem workItem)
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
                AssignedToName = isAssignedToPresent ? GetEmailOrName(assignedToString, true) : "",
                Id = workItem.Id,
                StakeholderTeam  = validEmailList,
                State = workItem.Resource.Fields[Constant.StateKey],
                TaskTitle = workItem.Resource.Fields[Constant.TaskTitleKey],
            };

            return releaseManagementTask;
        }

        /// <summary>
        /// Validates mail list.
        /// </summary>
        /// <param name="emailList">List of mails to validate.</param>
        /// <returns>List of valid mails.</returns>
        private static List<string> ValidateMails (List<string> emailList)
        {
            var validEmailList = new List<string>();
            foreach (var email in emailList)
            {
                Regex regex = new Regex(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
                RegexOptions.CultureInvariant | RegexOptions.Singleline);
                bool isValidEmail = regex.IsMatch(email.Trim());
                if (isValidEmail)
                {
                    validEmailList.Add(email.Trim());
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
