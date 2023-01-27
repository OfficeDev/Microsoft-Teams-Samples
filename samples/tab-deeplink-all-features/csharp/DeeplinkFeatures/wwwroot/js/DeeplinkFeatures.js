// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// App Initilization
microsoftTeams.app.initialize();

// Select People for audio and video call.
function selectPeople() {
    microsoftTeams.app.initialize().then(() => {
        microsoftTeams.people.selectPeople({ setSelected: [], openOrgWideSearchInChatOrChannel: true, singleSelect: false }).then((people) => {
            if (people) {
                document.getElementById("Attendee").value = (people.map((p) => p.email)).join(",");
            }
        }).catch((error) => {
            console.log(error);
        });
    });
}

// function to make audio and video calls 
function onCallDeepLinkButtonClick(callModalities) {
    microsoftTeams.app.initialize().then(() => {
        microsoftTeams.people.selectPeople({ setSelected: [], openOrgWideSearchInChatOrChannel: true, singleSelect: false }).then((people) => {
            if (people) {
                if (microsoftTeams.call.isSupported()) {
                    microsoftTeams.call.startCall({
                        targets: people.map((p) => p.email),
                        requestedModalities: [callModalities]
                    })
                        .then((result) => {
                            console.log(result);
                        }).catch((error) => {
                            console.log(error);
                        });
                }
                else {
                    alert("Call isn't supported");
                }
            }
        }).catch((error) => {
            console.log(error);
        });
    });
}

// Opens aplication profile dialog. 
function onApplicationProfileDialog() {
    // Initilize appInstallDialog from microsoftteams.
    var appInstallDialog = microsoftTeams.appInstallDialog;

    if (appInstallDialog.isSupported()) {
        const dialogPromise = appInstallDialog.openAppInstallDialog({ appId: env.AppId });
        dialogPromise.
            then((result) => { console.log(result) }).
            catch((error) => { console.log(error) });
    }
    else {
        console.log("openAppInstallDialog isn't supported");
    }
}

// Genereate deeplink to naviagte within your app
function onNavigatewithinyourapp() {
    // Initilize microsoftTeams Pages 
    var pages = microsoftTeams.pages;

    // Initilize microsoftTeams app
    var app = microsoftTeams.app;

    app.initialize().then(app.getContext).then((context) => {
        if (pages.isSupported()) {
            const navPromise = pages.navigateToApp({ appId: env.AppId, pageId: context.page.id, subPageId: context.page.subPageId, channelId: context.channel.id });
            navPromise.
                then((result) => { console.log(result) }).
                catch((error) => { console.log(error) });
        }
        else {
            console.log("navigateToApp isn't supported");
        }
    });
}

// Opens a meeting dialog to schedule meetings 
function onSchedulingDialogClick() {
    var subjectInput = document.getElementById("Subject");
    var startTimeInput = document.getElementById("StartTime");
    var endTimeInput = document.getElementById("EndTime");
    var contentInput = document.getElementById("Content");
    var attendeeInput = document.getElementById("Attendee");

    if (subjectInput.value.trim() !== "" && startTimeInput.value !== "" && endTimeInput.value !== "" && attendeeInput.value !== "") {

        var meetingDetails =
        {
            attendees: attendeeInput.value,
            content: contentInput.value.trim(),
            endTime: endTimeInput.value,
            startTime: startTimeInput.value,
            subject: subjectInput.value.trim(),
        };

        microsoftTeams.app.openLink(`https://teams.microsoft.com/l/meeting/new?subject=${meetingDetails.subject}&startTime=${meetingDetails.startTime}&endTime=${meetingDetails.endTime}&content=${meetingDetails.content}&attendees=${meetingDetails.attendees}`)
    }
}

// function to navigate chat with application
function navigateToChatWithApplication() {
    microsoftTeams.app.openLink(`https://teams.microsoft.com/l/entity/${env.AppId}/conversations`);
}

// navigates to new chat window where you can start new chat.
function navigateOnStartNewChat() {
    // Declare microsoftTeams chat
    var chat = microsoftTeams.chat;

    // Declare microsoftTeams.app
    var app = microsoftTeams.app;

    app.initialize().then(app.getContext).then((context) => {
        // get the current user.
        let user = context.user.loginHint;

        if (chat.isSupported()) {
            const chatPromise = chat.openGroupChat({ users: [user] });
            chatPromise.
                then((result) => { console.log(result) }).
                catch((error) => { console.log(error) });
        }
        else {
            console.log("openGroupChat isn't supported");
        }

    });
}

// navigates to teams chat window.
function navigateToTeamsChat() {
    let app = microsoftTeams.app;

    app.initialize().then(app.getContext).then((context) => {
        const queryParameters = {
            channelId: context.channel.id,
            tenantId: context.user.tenant.id,
            groupId: context.team.groupId,
            parentMessageId: context.app.parentMessageId,
            teamName: context.team.displayName,
            channelName: context.channel.displayName
        }

        microsoftTeams.app.openLink(`https://teams.microsoft.com/l/message/${queryParameters.channelId}/tenantId=${queryParameters.tenantId}&groupId=${queryParameters.groupId}&parentMessageId=${queryParameters.parentMessageId}&teamName=${queryParameters.teamName}&channelName=${queryParameters.channelName}`)
    });
}
