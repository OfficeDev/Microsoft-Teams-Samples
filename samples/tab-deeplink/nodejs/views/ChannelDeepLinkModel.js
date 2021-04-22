const path = require('path');
const dotenv = require('dotenv');
const ENV_FILE = path.join(__dirname, '.env');
dotenv.config({ path: ENV_FILE });

var encodedWebUrl=encodeURIComponent(process.env.BaseURL+"/ChannelDeepLink.html&label=DeepLink");
var jsontext1=JSON.stringify({"subEntityId":"topic1","channelId":""});
var task1Context=encodeURIComponent(jsontext1);


var Task1Link ={
     linkUrl:"https://teams.microsoft.com/l/entity/"+process.env.MicrosoftAppId+"/DeepLinkApp?webUrl=" + encodedWebUrl + "&context=" + task1Context,
   ID:1
}


var jsontext2=JSON.stringify({"subEntityId":"topic2","channelId":""});
var task2Context=encodeURIComponent(jsontext2);
var Task2Link ={
    linkUrl:"https://teams.microsoft.com/l/entity/"+process.env.MicrosoftAppId+"/DeepLinkApp?webUrl=" + encodedWebUrl + "&context=" + task2Context,
    ID:2
 }

 
 var jsontext3=JSON.stringify({"subEntityId":"topic3","channelId":""});
 var task3Context=encodeURIComponent(jsontext3);
 var Task3Link ={
    linkUrl:"https://teams.microsoft.com/l/entity/"+process.env.MicrosoftAppId+"/DeepLinkApp?webUrl=" + encodedWebUrl + "&context=" + task3Context,
    ID:3
 }

var ChannelDeepLinkModel =[Task1Link,Task2Link,Task3Link]








