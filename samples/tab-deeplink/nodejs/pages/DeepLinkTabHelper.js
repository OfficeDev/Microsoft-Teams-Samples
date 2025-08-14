var encodedWebUrl = encodeURIComponent(`${this.BaseURL}/ChannelDeepLink.html&label=DeepLink`);

GetDeepLinkTabChannel = (subEntityId, ID, Desc, channelId,AppID,EntityID)=>{

    let taskContext = encodeURIComponent(`{"subEntityId": "${subEntityId}","channelId":"${channelId}"}`);

     return {
      linkUrl:"https://teams.microsoft.com/l/entity/"+AppID+"/"+EntityID+"?webUrl=" + encodedWebUrl + "&context=" + taskContext,
      ID:ID,
      TaskText:Desc
     }
}

GetDeepLinkTabStatic = (subEntityId, ID, Desc, AppID) => {
    let taskContext = encodeURI(`{"subEntityId": "${subEntityId}"}`);

     return {
      linkUrl:"https://teams.microsoft.com/l/entity/"+AppID+"/"+process.env.Tab_Entity_Id +"?context=" + taskContext,
      ID:ID,
      TaskText:Desc
     }    
}

// creating deeplink for meeting sidepanel
GetDeepLinkToMeetingSidePanel = (ID, Desc,AppID,baseUrl,chatId,contextType) =>{
   let jsoncontext = "{"+"\"chatId\":\"" + chatId + "\","+"\"contextType\":\"" + contextType + "\""+"}";
   let taskContext = encodeURI(jsoncontext);
   let encodedUrl = encodeURIComponent(baseUrl + "/ChannelDeepLink.html");

   return {
      linkUrl:"https://teams.microsoft.com/l/entity/"+AppID+"/"+process.env.Channel_Entity_Id+"?webUrl="+encodedUrl +"&context=" + taskContext,
      ID:ID,
      TaskText:Desc
     }   
}

module.exports= {
   GetDeepLinkTabChannel,GetDeepLinkTabStatic, GetDeepLinkToMeetingSidePanel
}






