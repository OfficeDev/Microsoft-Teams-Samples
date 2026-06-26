// Initialize the library.
microsoftTeams.app.initialize();

// Select people for fetching the attendees.
function selectPeople() {
    microsoftTeams.people.selectPeople({ setSelected: [], openOrgWideSearchInChatOrChannel: true, singleSelect: false }).then((people) => {
        if (people) {
            document.getElementById("Attendee").value = (people.map((p) => p.email)).join(",");
        }
    }).catch((error) => {
        console.log(error);
    });
}

// function which redirects the user to start new chat.
function startNewChat() {
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

// function to navigate chat with application
function navigateToChatWithApplication() {
    microsoftTeams.app.openLink(`https://teams.microsoft.com/l/entity/${env.AppId}/conversations`);
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

// function to navigate chat with application
function onCallDeepLinkButtonClick(callModalities) {
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
}

// function to open scheduling dialog workflow
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

// function to open app installation dialog workflow
// Below app id is for harcoded for polly app which is already available in store.
function onAppInstallDialogClick() {
    microsoftTeams.app.openLink("https://teams.microsoft.com/l/app/1542629c-01b3-4a6d-8f76-1938b779e48d");
}