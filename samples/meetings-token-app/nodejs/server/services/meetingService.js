const { BotFrameworkAdapter, TurnContext, TeamsInfo, MessageFactory } = require('botbuilder');
const {ActivityTypes} = require('botframework-schema')
const { MicrosoftAppCredentials, ConnectorClient } = require('botframework-connector');
const fetch = require('node-fetch');
const storeService = require('./storageService');
const MeetingMetadata = require('../model/meetingMetadata');
const UserToken = require('../model/userToken')
const UserInfo = require('../model/userInfo')
const UserRole = require('../model/userRole')
const Conversation = require('../model/conversation')
const TokenStatus = require('../model/tokenStatus')
const MeetingSummary = require('../model/meetingSummary')

getMeetingSummary = (meetingId) => {
    let meetingMetadata;
    if(!storeService.storeCheck("meetingmetadata")){
        meetingMetadata = new MeetingMetadata(meetingId, 0, 0);
        storeService.storeSave("meetingmetadata", meetingMetadata);
    }else{
        meetingMetadata = storeService.storeFetch("meetingmetadata");
    }
    let userTokens;
    if(!storeService.storeCheck("usertokens")){
        userTokens = []
    }else{
        userTokens = storeService.storeFetch("usertokens");
    }
    let userTokensTemp = []
    userTokens.forEach(x => {
        if(x.Status == TokenStatus.Waiting) userTokensTemp.push(x);
        if(x.Status == TokenStatus.Current) userTokensTemp.push(x);
    })
    let meetingSummary = new MeetingSummary(meetingMetadata, userTokensTemp);
    return meetingSummary 
}

generateToken = (meetingId, participant) => {
    let meetingMetadata;
    if(!storeService.storeCheck("meetingmetadata")){
        meetingMetadata = new MeetingMetadata(meetingId, 0, 0);
        storeService.storeSave("meetingmetadata", meetingMetadata);
    }else{
        meetingMetadata = storeService.storeFetch("meetingmetadata");
    }
    let userTokens;
    if(!storeService.storeCheck("usertokens")){
        userTokens = []
    }else{
        userTokens = storeService.storeFetch("usertokens");
    }

    let conversation = new Conversation(participant.conversation.id)
    let userRole = new UserRole(participant.meeting.role, conversation)
    let tokenStatus = TokenStatus.Waiting
    var ut_current = userTokens.find(ut => ut.Status == TokenStatus.Current);
    if(!ut_current){
        tokenStatus = TokenStatus.Current
        meetingMetadata.CurrentToken = meetingMetadata.MaxTokenIssued + 1
    }
    // if(meetingMetadata.MaxTokenIssued == 0){
    //     tokenStatus = TokenStatus.Current
    // }
    const { aadObjectId, name, email } = participant.user

    var ut = userTokens.find(ut => (ut.UserInfo.AadObjectId == aadObjectId && (ut.Status == TokenStatus.Waiting || ut.Status == TokenStatus.Current)));
    // console.log(ut);
    if(ut){
        if(ut.Status == TokenStatus.Waiting || ut.Status == TokenStatus.Current ){
            console.log("ut Status " + ut.Status);
            return ut
        }else{
            userTokens.filter(ut => ut.UserInfo.AadObjectId == aadObjectId)
            // console.log(userTokens);
        }
    }

    let userInfo = new UserInfo(aadObjectId, name, email, userRole)
    meetingMetadata.MaxTokenIssued = meetingMetadata.MaxTokenIssued + 1;
    let tokenNumber = meetingMetadata.MaxTokenIssued;

    let userToken = new UserToken(userInfo, tokenNumber, tokenStatus)
    userTokens.push(userToken)
    storeService.storeSave("usertokens", userTokens);
    // if(meetingMetadata.CurrentToken == 0){
    //     meetingMetadata.CurrentToken = 1
    // }
    storeService.storeSave("meetingmetadata", meetingMetadata);
    // console.log(userToken);
    return userToken
}

getUserInfo = (participant) => {
    let conversation = new Conversation(participant.conversation.id)
    let userRole = new UserRole(participant.meeting.role, conversation)
    const { aadObjectId, name, email } = participant.user
    let userInfo = new UserInfo(aadObjectId, name, email, userRole)
    return userInfo;
}

ackToken = (meetingId, participant) => {
    let meetingMetadata;
    if(!storeService.storeCheck("meetingmetadata")){
        meetingMetadata = new MeetingMetadata(meetingId, 0, 0);
        storeService.storeSave("meetingmetadata", meetingMetadata);
    }else{
        meetingMetadata = storeService.storeFetch("meetingmetadata");
    }
    let userTokens;
    if(!storeService.storeCheck("usertokens")){
        userTokens = []
    }else{
        userTokens = storeService.storeFetch("usertokens");
    }
    let currentIndex
    userTokens.forEach((ut, i) => {
        // console.log(ut);
        // console.log(i);
        if(ut.Status == TokenStatus.Current){
            ut.Status = TokenStatus.Serviced
            meetingMetadata.CurrentToken = 0
            currentIndex = i
        }
    });

    if(userTokens[currentIndex + 1]){
        userTokens[currentIndex + 1].Status = TokenStatus.Current
        meetingMetadata.CurrentToken = meetingMetadata.CurrentToken + 1
    }
    // console.log(userTokens);
    storeService.storeSave("usertokens", userTokens);
    storeService.storeSave("meetingmetadata", meetingMetadata);
    // console.log(userTokens);
    console.log(meetingMetadata);
    /////////////////////PostStatusChangeNotification////////////////////////
    let nextIndex = currentIndex + 1
    if(userTokens[nextIndex]){
        let user = userTokens[nextIndex].UserInfo.Name
        let token = userTokens[nextIndex].TokenNumber
        postNotification(user, token);
    }

    let userTokensTemp = []
    userTokens.forEach(x => {
        if(x.Status == TokenStatus.Waiting) userTokensTemp.push(x);
        if(x.Status == TokenStatus.Current) userTokensTemp.push(x);
    })
    let meetingSummary = new MeetingSummary(meetingMetadata, userTokensTemp);
    return meetingSummary;
}

skipToken = (meetingId, participant) => {
    let meetingMetadata;
    if(!storeService.storeCheck("meetingmetadata")){
        meetingMetadata = new MeetingMetadata(meetingId, 0, 0);
        storeService.storeSave("meetingmetadata", meetingMetadata);
    }else{
        meetingMetadata = storeService.storeFetch("meetingmetadata");
    }
    let userTokens;
    if(!storeService.storeCheck("usertokens")){
        userTokens = []
    }else{
        userTokens = storeService.storeFetch("usertokens");
    }
    let currentIndex
    userTokens.forEach((ut, i) => {
        if(ut.Status == TokenStatus.Current){
            ut.Status = TokenStatus.NotUsed
            meetingMetadata.CurrentToken = 0
            currentIndex = i
        }
    });
    if(userTokens[currentIndex + 1]){
        userTokens[currentIndex + 1].Status = TokenStatus.Current
        meetingMetadata.CurrentToken = meetingMetadata.CurrentToken + 1
    }
    storeService.storeSave("usertokens", userTokens);

    storeService.storeSave("meetingmetadata", meetingMetadata);
    console.log(meetingMetadata);
    let nextIndex = currentIndex + 1
    if(userTokens[nextIndex]){
        let user = userTokens[nextIndex].UserInfo.Name
        let token = userTokens[nextIndex].TokenNumber
        postNotification(user, token);
    }
    let userTokensTemp = []
    userTokens.forEach(x => {
        if(x.Status == TokenStatus.Waiting) userTokensTemp.push(x);
        if(x.Status == TokenStatus.Current) userTokensTemp.push(x);
    })
    let meetingSummary = new MeetingSummary(meetingMetadata, userTokensTemp);
    return meetingSummary;
}

postNotification = async (user, token)=>{
    try {
        let serviceURI = storeService.storeFetch("serviceurl");
        let conversationId = storeService.storeFetch("conversationid");
        // const credentials = new MicrosoftAppCredentials(process.env.BotId, process.env.BotPassword);
        // const connector = new ConnectorClient(credentials, { baseUri: serviceURI });
        const adapter = new BotFrameworkAdapter({
            appId: process.env.BotId,
            appPassword: process.env.BotPassword
        });
        let reference = {
            "serviceUrl" : serviceURI,
            "channelId": 'msteams',
            "conversation": {
                "id": conversationId
            }
        }
        await adapter.continueConversation(reference, async (context) => {
            try {
                // console.log(context);
                let teamsAppId = process.env.clientId
                // let teamsAppId = process.env.BotId
                let baseUrl = require('../../appManifest/manifest.json')['developer']['websiteUrl']

                // let contentBubbleUrlWithParam = `${baseUrl}/bubble?user%3D${user}%26token%3D${token}`
                let contentBubbleUrlWithParam = `${baseUrl}/bubble?user=${user}&token=${token}`
                contentBubbleUrlWithParam = encode(contentBubbleUrlWithParam)
                console.log(contentBubbleUrlWithParam);
                let externalResourceUrl = `https://teams.microsoft.com/l/bubble/${teamsAppId}?url=${contentBubbleUrlWithParam}&height=180&width=280&title=Token Update`
                console.log(externalResourceUrl);
                let bubbleText = `Current Token: ${token}\nParticipant Name: ${user}`
                const replyActivity = MessageFactory.text(bubbleText); // this could be an adaptive card instead
                replyActivity.channelData = {
                    notification: {
                        alertInMeeting: true,
                        externalResourceUrl: externalResourceUrl
                    }
                };
                await context.sendActivity(replyActivity);
            } catch (error) {
                console.error("Error in continueConversation callback:", error);
            }
        });
    } catch (error) {
        console.error("Error in postNotification:", error);
    }
}

encode = (url)=>{
    return url.replace(/&/g, '%26').replace(/=/g, '%3D').replace(/ /g, '%20')
}


module.exports = {
    getMeetingSummary,
    generateToken,
    getUserInfo,
    ackToken,
    skipToken
}