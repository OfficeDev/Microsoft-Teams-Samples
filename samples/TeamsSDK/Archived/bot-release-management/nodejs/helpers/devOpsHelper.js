const { Constant } = require("../models/constant");
const GraphHelper = require("./graphHelper");

class DevOpsHelper {

    /**
     * Maps the incoming payload to release management task object
     * @param {*} workItem Incoming workitem payload.
     * @returns Mapped release management task object.
     */
    static async MapToReleaseManagementTask (workItem) {
        const graphHelper = new GraphHelper();
        const fields = workItem.resource.fields;
        const isAssignedToPresent = Object.prototype.hasOwnProperty.call(fields, Constant.AssignedTo);
        const createdByEmail = DevOpsHelper.GetEmailOrName(fields[Constant.CreatedByKey], false);
        const createdByName = DevOpsHelper.GetEmailOrName(fields[Constant.CreatedByKey], true);
        const assignedToEmail = isAssignedToPresent ? DevOpsHelper.GetEmailOrName(fields[Constant.AssignedTo], false) : '';
        const assignedToName = isAssignedToPresent ? DevOpsHelper.GetEmailOrName(fields[Constant.AssignedTo], true) : '';

        const emailList = [createdByEmail];

        if (isAssignedToPresent)
        {
            emailList.push(assignedToEmail);
        }

        const validEmailList = DevOpsHelper.ValidateMails(emailList);
        const createdByProfileImage = await graphHelper.GetProfilePictureByUserPrincipalNameAsync(createdByEmail);
        const assignedToProfileImage = isAssignedToPresent ? await graphHelper.GetProfilePictureByUserPrincipalNameAsync(assignedToEmail) : '';

        const releaseManagementTask = 
        {
            AssignedToName: assignedToName,
            AssignedToProfileImage: assignedToProfileImage,
            CreatedByName: createdByName,
            CreatedByProfileImage: createdByProfileImage,
            NotificationId: workItem.notificationId,
            GroupChatMembers: validEmailList,
            State: fields[Constant.StateKey],
            TaskTitle: fields[Constant.TaskTitleKey],
            WorkitemUrl: workItem.resource._links.html.href
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
        const validEmailList = [];
        const regexp = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        emailList.forEach (email => {
            const normalizedEmail = String(email || '').trim().toLowerCase();
            const isValidEmail = regexp.test(normalizedEmail);
            if (isValidEmail)
            {
                validEmailList.push(String(email).trim());
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
        if (!formattedString || typeof formattedString !== 'string') {
            return '';
        }

        const splitString = formattedString.split('<');
        if (isName) {
            return String(splitString[0] || '').trim();
        }

        if (splitString.length < 2) {
            return '';
        }

        return String(splitString[1]).replace('>', '').trim();
    }
}

module.exports.DevOpsHelper = DevOpsHelper;