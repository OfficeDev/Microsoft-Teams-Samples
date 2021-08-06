// Adaptive Card for tab to show in stage view
const adaptiveCardWithLink = () => ({
    $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
    body: [
        {
            type: 'TextBlock',
            size: 'Medium',
            weight: 'Bolder',
            text: 'Click the button to open the url in tab stage view'
        },
        {
            type: 'ActionSet',
            actions: [
                {
                    type: "Action.Submit",
                    title: "View via card",
                    data: {
                        msteams: {
                            type: "invoke",
                            value: {
                                type: "tab/tabInfoAction",
                                tabInfo: {
                                    contentUrl:  process.env.BaseUrl + "/content",
                                    websiteUrl: process.env.BaseUrl + "/content",
                                    name: "Stage view",
                                    entityId: "entityId"
                                 }
                                }
                            }
                    }
                },
                /*{
                   // type: "Action.OpenUrl",
                    title: "View via Deep Link",
                    url: "https://teams.microsoft.com/l/stage/4dec86fc-335d-497b-b66f-afcdf4e0c22d/0?context={'contentUrl':'https://a081fe9ce994.ngrok.io/content','websiteUrl':'https://a081fe9ce994.ngrok.io/content','name':'Contoso'}"
                }*/
            ]
        }
    ],
    type: 'AdaptiveCard',
    version: '1.4'
});

const getDeepLinkTabStatic = (subEntityId, ID, Desc,AppID)=> {
    let taskContext = encodeURI(`{"subEntityId": "${subEntityId}"}`);
      return {
       linkUrl:"https://teams.microsoft.com/l/entity/"+AppID+"/com.contoso.DeeplLinkBot.help?context=" + taskContext,
       ID:ID,
       TaskText:Desc
      }    
 }

module.exports = {
    adaptiveCardWithLink,
    getDeepLinkTabStatic
};