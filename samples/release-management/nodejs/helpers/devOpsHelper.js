const { Constant } = require("../models/constant");

class DevOpsHelper {

    /**
     * Maps the incoming payload to release management task object
     * @param {*} workItem Incoming workitem payload.
     * @returns Mapped release management task object.
     */
    static MapToReleaseManagementTask (workItem) {
        var isAssignedToPresent = workItem.resource.fields.hasOwnProperty(Constant.AssignedTo);
        var emailList = workItem.resource.fields[Constant.StakeHolderTeamKey].split(',');
        emailList.push(DevOpsHelper.GetEmailOrName(workItem.resource.fields[Constant.CreatedByKey], false));

        if (isAssignedToPresent)
        {
            emailList.push(DevOpsHelper.GetEmailOrName(workItem.resource.fields[Constant.AssignedTo], false));
        }

        var validEmailList = DevOpsHelper.ValidateMails(emailList);

        var releaseManagementTask = 
        {
            AssignedToName: isAssignedToPresent ? DevOpsHelper.GetEmailOrName(workItem.resource.fields[Constant.AssignedTo], true) : "",
            Id: workItem.id,
            StakeholderTeam: validEmailList,
            State: workItem.resource.fields[Constant.StateKey],
            TaskTitle: workItem.resource.fields[Constant.TaskTitleKey],
        };

        return releaseManagementTask;
    }

    /**
     * Validates mail list.
     * @param {string[]} emailList List of mails to validate.
     * @returns Array of valid mails.
     */
    static ValidateMails (emailList)
    {
        var validEmailList = [];
        emailList.forEach (email => {
            var regexp = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
            var isValidEmail = regexp.test(String(email.trim()).toLowerCase());
            if (isValidEmail)
            {
                validEmailList.push(email.trim());
            }
        });
        return validEmailList;
    }

    /**
     * Gets the name or mail from formatted string.
     * @param {string} formattedString 
     * @param {boolean} isName 
     * @returns Name or mail based isName flag.
     */
    static GetEmailOrName(formattedString, isName)
    {
        var result = ""; 
        var splitString = formattedString.split('<');
        if (splitString.length > 0)
        {
            result = isName ? splitString[0] : splitString[1].replace('>','');
        }
        return result.trim();
    }
}

module.exports.DevOpsHelper = DevOpsHelper;