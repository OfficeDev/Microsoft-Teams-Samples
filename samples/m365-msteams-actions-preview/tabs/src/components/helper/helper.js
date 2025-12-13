// import { Icon } from '@fluentui/react/lib/Icon';

import { Link } from "@fluentui/react-components";

const MimeType = {
    PDF: "application/pdf",
    Excel: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    Word: "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
    PowerPoint: "application/vnd.openxmlformats-officedocument.presentationml.presentation"
}

/**
 * @param mimeType
 * Returns the icon url based on file's mimeType
*/
export function getImageIcon(mimeType) {
    switch (mimeType) {
        case MimeType.PDF:
            return "https://spoppe-b.azureedge.net/files/fabric-cdn-prod_20211104.001/assets/item-types/32_1.5x/pdf.svg";
        case MimeType.Excel:
            return "https://spoppe-b.azureedge.net/files/fabric-cdn-prod_20211104.001/assets/item-types/32_1.5x/xlsx.svg";
        case MimeType.Word:
            return "https://spoppe-b.azureedge.net/files/fabric-cdn-prod_20211104.001/assets/item-types/32_1.5x/docx.svg";
        case MimeType.PowerPoint:
            return "https://spoppe-b.azureedge.net/files/fabric-cdn-prod_20211104.001/assets/item-types/32_1.5x/pptx.svg";
        default: return "FileCode";
    }
}

/**
 * @returns Attachment Element
 */
export function getSingleAttachmentElement(attachment) {
    if (attachment) {
        return (<div style={{ display: "flex", alignItems: "center" }}>
            <span style={{ paddingRight: "5px" }}>
                <img height={16} width={16} src={getImageIcon(attachment.file.mimeType)} alt={attachment.name} />
            </span>
            <Link href={attachment.webUrl} target="_blank">{attachment.name}</Link>
        </div>
        );
    }
    return "";
}

/**
 * Login
 */
export async function loginBtnClick(credential, scope) {
    try {
        // Popup login page to get user's access token
        await credential.login(scope);
    } catch (err) {
        console.log(err);
        if (err instanceof Error && err.message?.includes("CancelledByUser")) {
            const helpLink = "https://aka.ms/teamsfx-auth-code-flow";
            err.message +=
                '\nIf you see "AADSTS50011: The reply URL specified in the request does not match the reply URLs configured for the application" ' +
                "in the popup window, you may be using unmatched version for TeamsFx SDK (version >= 0.5.0) and Teams Toolkit (version < 3.3.0) or " +
                `cli (version < 0.11.0). Please refer to the help link for how to fix the issue: ${helpLink}`;
        }

        alert("Login failed: " + err);
    }
}
/**
 * @param apiClient
 * @param url
 * @param method
 * @param options
 * @param params
 * @returns response from the api
 */
export async function callFunctionWithErrorHandling(apiClient, url, method, options, params) {
    var message = [];
    var funcErrorMsg = "";
    try {
        const response = await apiClient.request({
            method: method,
            url: url,
            data: options,
            params,
        });
        message = response.data;
    } catch (err) {
        if (err.response && err.response.status && err.response.status === 404) {
            funcErrorMsg =
                'There may be a problem with the deployment of Azure Function App, please deploy Azure Function (Run command palette "TeamsFx - Deploy Package") first before running this App';
        } else if (err.message === "Network Error") {
            funcErrorMsg =
                "Cannot call Azure Function due to network error, please check your network connection status and ";
            if (err.config.url.indexOf("localhost") >= 0) {
                funcErrorMsg +=
                    'make sure to start Azure Function locally (Run "npm run start" command inside api folder from terminal) first before running this App';
            } else {
                funcErrorMsg +=
                    'make sure to provision and deploy Azure Function (Run command palette "TeamsFx - Provision Resource" and "TeamsFx - Deploy Package") first before running this App';
            }
        } else {
            funcErrorMsg = err.toString();
            if (err.response?.data?.error) {
                funcErrorMsg += ": " + err.response.data.error;
            }
            alert(funcErrorMsg);
        }
    }
    return message;
}