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
    let appId = "<<Your_App_ID>>"; // you will get app-Id from teams admin portal, app profile page.
    microsoftTeams.app.openLink(`https://teams.microsoft.com/l/app/${appId}`);
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
    let MicrosoftAppID = "<<Microsoft-App-ID>>"; // Replace placeholder <<Microsoft-App-ID>> with your MicrosoftAppId / Bot-Id.
    microsoftTeams.app.openLink(`https://teams.microsoft.com/l/entity/${MicrosoftAppID}/conversations`);
}

// navigates to new chat window where you can start new chat.
function navigateOnStartNewChat() {
    let app = microsoftTeams.app;
    app.initialize().then(app.getContext).then((context) => {
        let currentUser = context.user.loginHint;
        microsoftTeams.app.openLink(`https://teams.microsoft.com/l/chat/0/0?users=${currentUser}`)
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
        microsoftTeams.app.openLink(`https://teams.microsoft.com/l/message/${queryParameters.channelId}/1648741500652?tenantId=${queryParameters.tenantId}&groupId=${queryParameters.groupId}&parentMessageId=${queryParameters.parentMessageId}&teamName=${queryParameters.teamName}&channelName=${queryParameters.channelName}`)
    });
}
