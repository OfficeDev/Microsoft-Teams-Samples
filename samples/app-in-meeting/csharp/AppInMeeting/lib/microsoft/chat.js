const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

//This method receive the message and Append to our list  
connection.on("ReceiveMessage", (user, description, status) => {
    const divId = status;
    document.getElementById(divId).innerHTML += (`<div class="details">
                Description: ${description}</br>
                Assigned To:${user}</div>
          `);
});

connection.start().catch(err => console.error(err.toString()));

const updateDetails = (data) => {
    connection.invoke("SendMessage", data.userName, data.taskDescription, data.taskStatus).catch(err => console.log(err.toString()));
}
