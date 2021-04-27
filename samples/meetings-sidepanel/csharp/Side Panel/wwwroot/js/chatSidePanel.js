"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

connection.on("ReceiveMessage", function (user, message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var li = document.createElement("li");
    li.textContent = msg;
    document.getElementById("agendaList").appendChild(li);
});

connection.start().then(function () {
    }).catch(function (err) {
    return console.error(err.toString());
});

//Toggle Agenda Add Button
function closeAgendaInput() {
    var user = "";
    var message = document.getElementById("agendaInput").value;
    connection.invoke("SendMessage", user, message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
};