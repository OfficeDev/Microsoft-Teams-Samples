const axios = require("axios");
const credential = require("./credential");
const teamsAppId = process.env.teamsAppId;
async function callChatAPI(
  originalUrl,
  requestType,
  originalRequestBody,
  userId,
  chatId
) {
  const { url, requestBody } = await resolveChatPlaceHolders(
    originalUrl,
    originalRequestBody,
    userId,
    chatId
  );
  console.log(
    `url: ${url}, requestBody: ${requestBody}, requestType: ${requestType}`
  );
  try {
    return await callAPI(
      url,
      requestType,
      requestBody ? JSON.parse(requestBody) : null
    );
  } catch (error) {
    console.log("Error callChatAPI:", error.message);
    return {
      error: error.message,
    };
  }
}

async function callTeamAPI(
  originalUrl,
  requestType,
  originalRequestBody,
  userId,
  teamId,
  channelId
) {
  const { url, requestBody } = await resolveTeamPlaceHolders(
    originalUrl,
    originalRequestBody,
    userId,
    teamId,
    channelId
  );
  console.log(
    `url: ${url}, requestBody: ${requestBody}, requestType: ${requestType}`
  );
  try {
    return await callAPI(
      url,
      requestType,
      requestBody ? JSON.parse(requestBody) : null
    );
  } catch (error) {
    console.log("Error callChatAPI:", error.message);
    return {
      error: error.message,
    };
  }
}

async function callAPI(url, requestType, requestBody) {
  const token = await credential.getToken(
    "https://graph.microsoft.com/.default"
  );
  axios.defaults.headers.common["Authorization"] = `Bearer ${token.token}`;
  switch (requestType) {
    case "GET":
      return (await axios.get(url)).data;
    case "POST":
      return (await axios.post(url, requestBody)).data;
    case "PUT":
      return (await axios.put(url, requestBody)).data;
    case "PATCH":
      return (await axios.patch(url, requestBody)).data;
    case "DELETE":
      return (await axios.delete(url)).data;
    default:
      throw new Error("Invalid request type");
  }
}

async function resolveChatPlaceHolders(
  originalUrl,
  originalRequestBody,
  userId,
  chatId
) {
  url = originalUrl.replace("{chatId}", chatId);
  url = url.replace("{userId}", userId);
  requestBody = originalRequestBody.replace("{chatId}", chatId);
  requestBody = requestBody.replace("{userId}", userId);

  if (
    url.includes("{tabId}") ||
    requestBody.includes("{tabId}") ||
    url.includes("{installedAppId}") ||
    requestBody.includes("{installedAppId}")
  ) {
    const { tabId, installedAppId } =
      await getDefaultTabIdAndInstalledAppIdInChat(chatId);
    url = url.replace("{tabId}", tabId);
    url = url.replace("{installedAppId}", installedAppId);
    requestBody = requestBody.replace("{tabId}", tabId);
    requestBody = requestBody.replace("{installedAppId}", installedAppId);
  }
  return { url, requestBody };
}

async function resolveTeamPlaceHolders(
  originalUrl,
  originalRequestBody,
  userId,
  teamId,
  channelId
) {
  url = originalUrl.replace("{teamId}", teamId);
  url = url.replace("{channelId}", channelId);
  url = url.replace("{userId}", userId);
  requestBody = originalRequestBody.replace("{teamId}", teamId);
  requestBody = requestBody.replace("{channelId}", channelId);
  requestBody = requestBody.replace("{userId}", userId);

  if (
    url.includes("{tabId}") ||
    requestBody.includes("{tabId}") ||
    url.includes("{installedAppId}") ||
    requestBody.includes("{installedAppId}")
  ) {
    const { tabId, installedAppId } =
      await getDefaultTabIdAndInstalledAppIdInTeam(teamId, channelId);
    url = url.replace("{tabId}", tabId);
    url = url.replace("{installedAppId}", installedAppId);
    requestBody = requestBody.replace("{tabId}", tabId);
    requestBody = requestBody.replace("{installedAppId}", installedAppId);
  }
  return { url, requestBody };
}

async function getDefaultTabIdAndInstalledAppIdInChat(chatId) {
  const token = await credential.getToken(
    "https://graph.microsoft.com/.default"
  );
  axios.defaults.headers.common["Authorization"] = `Bearer ${token.token}`;
  const result = await axios.get(
    `https://graph.microsoft.com/v1.0/chats/${chatId}/tabs?$expand=teamsApp`
  );
  let tabId;
  let installedAppId;
  for (let i = 0; i < result.data.value.length; i++) {
    if (result.data.value[i].teamsApp.externalId === teamsAppId) {
      tabId = result.data.value[i].id;
      installedAppId = result.data.value[i].teamsApp.id;
    }
  }
  console.log(`tabId: ${tabId}, installedAppId: ${installedAppId}`);
  return { tabId, installedAppId };
}

async function getDefaultTabIdAndInstalledAppIdInTeam(teamId, channelId) {
  const token = await credential.getToken(
    "https://graph.microsoft.com/.default"
  );
  axios.defaults.headers.common["Authorization"] = `Bearer ${token.token}`;
  const result = await axios.get(
    `https://graph.microsoft.com/v1.0/teams/${teamId}/channels/${channelId}/tabs?$expand=teamsApp`
  );
  let tabId;
  let installedAppId;
  for (let i = 0; i < result.data.value.length; i++) {
    if (result.data.value[i].teamsApp.externalId === teamsAppId) {
      tabId = result.data.value[i].id;
      installedAppId = result.data.value[i].teamsApp.id;
    }
  }
  console.log(`tabId: ${tabId}, installedAppId: ${installedAppId}`);
  return { tabId, installedAppId };
}

module.exports = {
  callChatAPI,
  callTeamAPI,
};
