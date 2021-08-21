const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

//This method receive the message and Append to our list  
connection.on("ReceiveMessage", (user, description, status) => {
    var detailsClass = window.innerWidth > 600 ? "details" : "details-sidepanel";
    document.getElementById(status).innerHTML += (`<div class=${detailsClass}>
                Description: ${description}</br>
                Assigned To:${user}</div>
          `);
});

connection.start().catch(err => console.error(err.toString()));

const updateDetails = (data) => {
    alert(data.userName)
    connection.invoke("SendMessage", data.userName, data.taskDescription, data.taskStatus).catch(err => console.log(err.toString()));
}

var screenWidth = window.innerWidth;
var meetingId;
var tenantId;
var userId;
var userName;
microsoftTeams.initialize();
$(document).ready(function () {
    //microsoftTeams.getContext(function (context) {
    //    console.log(context);
    //    meetingId = context.meetingId;
    //    tenantId = context.tid;
    //    userId = context.userObjectId;
    //})
    /*var chatSection = document.getElementById("chatSection");*/

    if (screenWidth > 600) {
        /*chatSection.className = "chatSectionStage"*/
        $("#todo, #doing, #done").addClass("grid-item");
        $("#todo, #doing, #done").removeClass("grid-item-sidepanel");

        $("#boardDiv").addClass("chat-window");
        $("#boardDiv").removeClass("chat-window-sidepanel");
    }
    else {
        /*chatSection.className = "chatSectionSidePanel";*/
        $("#todo, #doing, #done").addClass("grid-item-sidepanel");
        $("#todo, #doing, #done").removeClass("grid-item");

        $("#boardDiv").addClass("chat-window-sidepanel");
        $("#boardDiv").removeClass("chat-window");
    }
});

const openTaskModule1 = () => {
    let taskInfo = {
        title: "Custom Form",
        height: 510,
        width: 430,
        url: "https://8489-122-175-234-83.ngrok.io/Index",
    };
  
    microsoftTeams.tasks.startTask(taskInfo, submitHandler);
    //microsoftTeams.tasks.startTask(taskInfo, updateDetails);
};

submitHandler1 = (err, result) => {
    console.log(`Submit handler - err: ${err}`);
    console.log(`Submit handler - result\rName: ${result.name}\rEmail: ${result.email}\rFavorite book: ${result.favoriteBook}`);
};

// Send task details.
const sendTaskDetails1 = () => {
    
    microsoftTeams.initialize();
    var taskStatus = document.querySelector(".task-status");
    var taskDescription = document.querySelector(".task-description")
    var userName = document.querySelector(".user-name")
    var isValid = true;
    $('.task-status,.task-description,.user-name').each(function (e) {
        if ($.trim($(this).val()) == '') {
            isValid = false;
            $(this).css({
                "border": "1px solid red"
            });
        }
        else {
            $(this).css({
                "border": "",
                "background": ""
            });
        }
    });
    if (isValid == false) {
        e.preventDefault();
        return false;
    }
    const taskDetails = {
        "taskStatus": taskStatus.value,
        "taskDescription": taskDescription.value,
        "userName": userName.value
    };
    var customerInfo = {
        name: "name",
        email: "email",
        favoriteBook: "favouriteBook"
    }
   // alert("sendDetails end");
   // microsoftTeams.tasks.submitTask(customerInfo, "1234");
    //microsoftTeams.tasks.submitTask(taskDetails);
    updateDetails(taskDetails);
    return true;
}