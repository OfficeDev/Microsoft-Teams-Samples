import {
  tabHeaderPrivateChannel,
  tabHeaderSharedChannel,
  tabHeaderPublicChannel,
  privateChannelLink,
  sharedChannelLink,
  publicChannelLink
} from './tab-constants.js';

microsoftTeams.app.initialize().then(() => {
  microsoftTeams.app.getContext().then((context) => {
    let header = "";
    let link = "";

    if (context.channel?.membershipType === "Private") {
      header = tabHeaderPrivateChannel;
      link = privateChannelLink;
    } else if (context.channel?.membershipType === "Shared") {
      header = tabHeaderSharedChannel;
      link = sharedChannelLink;
    } else {
      header = tabHeaderPublicChannel;
      link = publicChannelLink;
    }

    document.getElementById("tabHeader").textContent = header;

    const anchor = document.getElementById("tabLink");
    anchor.href = link;

    const contextJson = document.getElementById("contextJson");
    contextJson.textContent = JSON.stringify(context, null, 2);
    contextJson.style.whiteSpace = "pre-wrap";
    contextJson.style.fontFamily = "monospace";
  });
});
