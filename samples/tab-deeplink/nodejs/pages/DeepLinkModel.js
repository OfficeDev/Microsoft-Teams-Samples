var task1Values = {"subEntityId": "topic1"}
var jsontext1=JSON.stringify(task1Values);
var task1Context=encodeURI(jsontext1);
var Task1Link ={
   linkUrl:"https://teams.microsoft.com/l/entity/690412fe-6f2a-4de8-a6e1-e856afc15692/com.contoso.DeeplLinkBot.help?context=" + task1Context,
   ID:1,
   Desc: "Bots"
}

var task2Values = {"subEntityId": "topic2"}
var jsontext2=JSON.stringify(task2Values);
var task2Context=encodeURI(jsontext2);
var Task2Link ={
    linkUrl:"https://teams.microsoft.com/l/entity/690412fe-6f2a-4de8-a6e1-e856afc15692/com.contoso.DeeplLinkBot.help?context=" + task2Context,
    ID:2,
    Desc:"Messaging Extension"
 }

var task3Values = {"subEntityId": "topic3"}
var jsontext3=JSON.stringify(task3Values);
var task3Context=encodeURI(jsontext3);
var Task3Link ={
    linkUrl:"https://teams.microsoft.com/l/entity/690412fe-6f2a-4de8-a6e1-e856afc15692/com.contoso.DeeplLinkBot.help?context=" + task3Context,
    ID:3,
    Desc: "Adaptive Card"
 }
 
var DeepLinkModel =[Task1Link,Task2Link,Task3Link]


