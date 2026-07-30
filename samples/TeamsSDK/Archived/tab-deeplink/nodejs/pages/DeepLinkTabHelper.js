// Helpers for building Microsoft Teams deep link URLs.
// See https://learn.microsoft.com/microsoftteams/platform/concepts/build-and-test/deep-links

const buildChannelWebUrl = () =>
    encodeURIComponent(`${process.env.Base_URL || ''}/ChannelDeepLink.html&label=DeepLink`);

const GetDeepLinkTabChannel = (subEntityId, ID, Desc, channelId, AppID, EntityID) => {
    const taskContext = encodeURIComponent(`{"subEntityId": "${subEntityId}","channelId":"${channelId}"}`);

    return {
        linkUrl: `https://teams.microsoft.com/l/entity/${AppID}/${EntityID}?webUrl=${buildChannelWebUrl()}&context=${taskContext}`,
        ID: ID,
        TaskText: Desc
    };
};

const GetDeepLinkTabStatic = (subEntityId, ID, Desc, AppID) => {
    const taskContext = encodeURI(`{"subEntityId": "${subEntityId}"}`);

    return {
        linkUrl: `https://teams.microsoft.com/l/entity/${AppID}/${process.env.Tab_Entity_Id}?context=${taskContext}`,
        ID: ID,
        TaskText: Desc
    };
};

// Builds a deeplink that opens the meeting side panel for an active meeting chat.
const GetDeepLinkToMeetingSidePanel = (ID, Desc, AppID, baseUrl, chatId, contextType) => {
    const taskContext = encodeURI(`{"chatId":"${chatId}","contextType":"${contextType}"}`);
    const encodedUrl = encodeURIComponent(`${baseUrl}/ChannelDeepLink.html`);

    return {
        linkUrl: `https://teams.microsoft.com/l/entity/${AppID}/${process.env.Channel_Entity_Id}?webUrl=${encodedUrl}&context=${taskContext}`,
        ID: ID,
        TaskText: Desc
    };
};

module.exports = {
    GetDeepLinkTabChannel,
    GetDeepLinkTabStatic,
    GetDeepLinkToMeetingSidePanel
};


