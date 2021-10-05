// Creating a SignalR connection for real time updates.
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

// This method receives the messages and appends it to containers.  
connection.on("ReceiveMessage", (user, description, status) => {
    if (user != null) {
        var detailsClass = window.innerWidth > 600 ? "details" : "details-sidepanel";
        document.getElementById(status).innerHTML += (`<div class=${detailsClass}>
                <div class="description" title="${description}">${description}</div>
                <div class="userName">--${user}</div>
          `);
    }
});

connection.start().catch(err => console.error(err.toString()));

// Method to send message to update entered details.
const updateDetails = (data, status) => {
    connection.invoke("SendMessage", data.userName, data.taskDescription, status).catch(err => console.log(err.toString()));
}