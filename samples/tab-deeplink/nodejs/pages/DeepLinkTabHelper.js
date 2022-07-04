var encodedWebUrl=encodeURIComponent(`${this.BaseURL}/ChannelDeepLink.html&label=DeepLink`);
GetDeepLinkTabChannel = (subEntityId, ID, Desc, channelId,AppID,EntityID)=>{

   let taskContext = encodeURIComponent(`{"subEntityId": "${subEntityId}","channelId":"${channelId}"}`);
     return {
      linkUrl:"https://teams.microsoft.com/l/entity/"+AppID+"/"+EntityID+"?webUrl=" + encodedWebUrl + "&context=" + taskContext,
      ID:ID,
      TaskText:Desc
     }
   }

GetDeepLinkTabStatic = (subEntityId, ID, Desc,AppID)=>{
   let taskContext = encodeURI(`{"subEntityId": "${subEntityId}"}`);
     return {
      linkUrl:"https://teams.microsoft.com/l/entity/"+AppID+"/"+process.env.Tab_Entity_Id +"?context=" + taskContext,
      ID:ID,
      TaskText:Desc
     }    
}

module.exports= {
   GetDeepLinkTabChannel,GetDeepLinkTabStatic
}






