const path = require('path');
const dotenv = require('dotenv');
const ENV_FILE = path.join(__dirname, '.env');
dotenv.config({ path: ENV_FILE });

var task1Values = {"subEntityId": "topic1"}
var jsontext1=JSON.stringify(task1Values);
var task1Context=encodeURI(jsontext1);
var Task1DeepLink= "https://teams.microsoft.com/l/entity/"+process.env.MicrosoftAppId+"/com.contoso.DeeplLinkBot.help?context=" + task1Context;


var task2Values = {"subEntityId": "topic2"}
var jsontext2=JSON.stringify(task2Values);
var task2Context=encodeURI(jsontext2);
var Task2DeepLink= "https://teams.microsoft.com/l/entity/"+process.env.MicrosoftAppId+"/com.contoso.DeeplLinkBot.help?context=" + task2Context;


var task3Values = {"subEntityId": "topic3"}
var jsontext3=JSON.stringify(task3Values);
var task3Context=encodeURI(jsontext3);
var Task3DeepLink= "https://teams.microsoft.com/l/entity/"+process.env.MicrosoftAppId+"/com.contoso.DeeplLinkBot.help?context=" + task3Context;

var SubTask ={
    Task1DeepLink:Task1DeepLink,
    Task2DeepLink:Task2DeepLink,
    Task3DeepLink:Task3DeepLink
}

module.exports= SubTask;

