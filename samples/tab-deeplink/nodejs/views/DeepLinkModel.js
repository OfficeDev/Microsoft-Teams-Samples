const path = require('path');
const dotenv = require('dotenv');
// Import required bot configuration.
const ENV_FILE = path.join(__dirname, '.env');
dotenv.config({ path: ENV_FILE });
var task1Values = {"subEntityId": "topic1"}
var jsontext1=JSON.stringify(task1Values);
var task1Context=encodeURI(jsontext1);
var Task1Link ={
   linkUrl:"https://teams.microsoft.com/l/entity/"+process.env.MicrosoftAppId+"/com.contoso.DeeplLinkBot.help?context=" + task1Context,
   ID:1
}


var task2Values = {"subEntityId": "topic2"}
var jsontext2=JSON.stringify(task2Values);
var task2Context=encodeURI(jsontext2);
var Task2Link ={
    linkUrl:"https://teams.microsoft.com/l/entity/"+process.env.MicrosoftAppId+"/com.contoso.DeeplLinkBot.help?context=" + task2Context,
    ID:2
 }

 
var task3Values = {"subEntityId": "topic3"}
var jsontext3=JSON.stringify(task3Values);
var task3Context=encodeURI(jsontext3);
var Task3Link ={
    linkUrl:"https://teams.microsoft.com/l/entity/"+process.env.MicrosoftAppId+"/com.contoso.DeeplLinkBot.help?context=" + task3Context,
    ID:3
 }

var DeepLinkModel =[Task1Link,Task2Link,Task3Link]



