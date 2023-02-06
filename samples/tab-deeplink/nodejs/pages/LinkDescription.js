microsoftTeams.app.initialize();  
microsoftTeams.app.getContext().then((context) => { 
    if (context.page.frameContext === "sidePanel") {
        document.getElementById("side-panel-content").style.display = "block";
         document.getElementById('list-content').style.display = "none";
        document.getElementById('taskList').style.display = "none";
        document.getElementById("extended-deeplink").style.display = "none";
        document.getElementById("side-panel-deeplink").style.display = "none";
     }
     else if (context.page.frameContext === "meetingStage") {

        fetch(`${window.location.origin}/api/getAppId`).then(response => response.json()).then(data => {
            deepLinkString = `https://teams.microsoft.com/l/entity/${data.microsoftAppId}/DeepLinkApp?context={"chatId": "${context.chat.id}","contextType":"chat"}`;
            
            document.getElementById("side-panel-deeplink").style.display = "block";
            document.getElementById('list-content').style.display = "none";
            document.getElementById('taskList').style.display = "none";
            document.getElementById("extended-deeplink").style.display = "none";
        });
    }                       
    else if (context.page.subPageId === "topic1") {                                  
         document.getElementById("taskDiv").innerHTML = "Bots";
         document.getElementById("taskContent").innerHTML = "A bot also referred to as a chatbot or conversational bot is an app that runs simple and repetitive automated tasks performed by the users, such as customer service or support staff. Examples of bots in everyday use include, bots that provide information about the weather, make dinner reservations, or provide travel information. A bot interaction can be a quick question and answer, or it can be a complex conversation that provides access to services.For more details <a href='https://docs.microsoft.com/en-us/microsoftteams/platform/bots/what-are-bots' target='_blank'>Click here</a>";
     }

     else if (context.page.subPageId === "topic2") {
         document.getElementById("taskDiv").innerHTML = "Messaging Extension";
         document.getElementById("taskContent").innerHTML = "Messaging extensions allow the users to interact with your web service through buttons and forms in the Microsoft Teams client. They can search or initiate actions in an external system from the compose message area, the command box, or directly from a message. You can send back the results of that interaction to the Microsoft Teams client in the form of a richly formatted card. This document gives an overview of the messaging extension, tasks performed under different scenarios, working of messaging extension, action and search commands, and link unfurling.For more details <a href='https://docs.microsoft.com/en-us/microsoftteams/platform/messaging-extensions/what-are-messaging-extensions' target='_blank'>Click here</a>";
     }

     else if (context.page.subPageId === "topic3") {
         document.getElementById("taskDiv").innerHTML = "Adaptive Card";
         document.getElementById("taskContent").innerHTML ="Adaptive cards are a new cross product specification for cards in Microsoft products including Bots, Cortana, Outlook, and Windows. They are the recommended card type for new Teams development. For general information from the Adaptive cards team see Adaptive Cards Overview. You can use adaptive cards anywhere you can use existing Hero cards, Office365 cards, and Thumbnail cards.For more details <a href='https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/what-are-cards' target='_blank'>Click here</a>";
     }   
     else{    
        var s=DeepLinkModel;
        var html ='';
        s.forEach(x=> {
            var uri = "ChannelDetails.html?id="+x.ID;
            let listContent = document.getElementById("list-content");
            let anchorTag = document.createElement("a");
            let lineBreakTag = document.createElement("br");
            anchorTag.href = uri;
            anchorTag.innerHTML = x.Desc;

            listContent.appendChild(anchorTag);
            listContent.appendChild(lineBreakTag);
        });
    }
}); 