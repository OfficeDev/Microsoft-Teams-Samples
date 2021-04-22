
const path = require('path');
const dotenv = require('dotenv');
const ENV_FILE = path.join(__dirname, '.env');
dotenv.config({ path: ENV_FILE });

var encodedWebUrl=encodeURIComponent(process.env.BaseURL+"/ChannelDeepLink.html&label=DeepLink");
Task1DeepLink = (channelId)=>{
   let task1Context1 = encodeURIComponent(`{"subEntityId": "topic1","channelId":"${channelId}"}`);
     return {
      linkUrl:"https://teams.microsoft.com/l/entity/"+process.env.MicrosoftAppId+"/DeepLinkApp?webUrl=" + encodedWebUrl + "&context=" + task1Context1,
      ID:1
     }
}

Task2DeepLink = (channelId)=>{
   let task2Context1 = encodeURIComponent(`{"subEntityId": "topic2","channelId":"${channelId}"}`);  
     return {
      linkUrl:"https://teams.microsoft.com/l/entity/"+process.env.MicrosoftAppId+"/DeepLinkApp?webUrl=" + encodedWebUrl + "&context=" + task2Context1,
      ID:2
     }
}

Task3DeepLink = (channelId)=>{
   let task3Context1 = encodeURIComponent(`{"subEntityId": "topic3","channelId":"${channelId}"}`);
     return {
      linkUrl:"https://teams.microsoft.com/l/entity/"+process.env.MicrosoftAppId+"/DeepLinkApp?webUrl=" + encodedWebUrl + "&context=" + task3Context1,
      ID:3
     }
}

var ChannelDeepLinkModel ={
   Task1DeepLink:Task1DeepLink,
   Task2DeepLink:Task2DeepLink,
   Task3DeepLink:Task3DeepLink 
}

module.exports= ChannelDeepLinkModel;






