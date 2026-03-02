// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { App } from '@microsoft/teams.apps'
import { DevtoolsPlugin } from '@microsoft/teams.dev'
import axios from 'axios'
import * as fs from 'fs'
import * as path from 'path'
import { fileURLToPath } from 'url'

const __dirname = path.dirname(fileURLToPath(import.meta.url))

// ---- Helper Functions ----

function createMessageResponse(message: string) {
  return {
    composeExtension: {
      type: "message" as const,
      text: message,
    },
  }
}

// ---- Search Functions ----

async function searchWikipedia(query: string) {
  try {
    const response = await axios.get(
      `https://en.wikipedia.org/w/api.php?action=query&format=json&list=search&srsearch=${encodeURIComponent(query)}&utf8=1`,
      { headers: { "User-Agent": "BotMessageExtensions/1.0 (Teams Bot; +https://example.com)" } }
    )

    const searchResults = response.data?.query?.search || []
    const attachments = searchResults.slice(0, 8).map((result: any) => {
      const title = result.title || "No Title"
      const snippet = (result.snippet || "No snippet").replace(/<[^>]*>/g, "")

      const card = {
        type: "AdaptiveCard",
        version: "1.4",
        body: [
          { type: "TextBlock", text: title, weight: "Bolder", size: "Large" },
          { type: "TextBlock", text: snippet, wrap: true, isSubtle: true },
        ],
      }

      return {
        contentType: "application/vnd.microsoft.card.adaptive",
        content: card,
        preview: {
          contentType: "application/vnd.microsoft.card.thumbnail",
          content: { title, text: snippet },
        },
      }
    })

    return { composeExtension: { type: "result", attachmentLayout: "list", attachments } }
  } catch (error) {
    console.error("Wikipedia search error:", error)
    return createMessageResponse("Failed to search Wikipedia")
  }
}

async function searchNuGetPackages(query: string) {
  try {
    const response = await axios.get(
      `https://azuresearch-usnc.nuget.org/query?q=id:${encodeURIComponent(query)}&prerelease=true`
    )

    const data = response.data?.data || []
    const attachments = data.map((item: any) => {
      const packageId = item.id || ""
      const description = item.description || ""

      const card = {
        type: "AdaptiveCard",
        version: "1.4",
        body: [
          { type: "TextBlock", text: packageId, weight: "Bolder", size: "Large" },
        ],
      }

      return {
        contentType: "application/vnd.microsoft.card.adaptive",
        content: card,
        preview: {
          contentType: "application/vnd.microsoft.card.thumbnail",
          content: { title: packageId, text: description },
        },
      }
    })

    return { composeExtension: { type: "result", attachmentLayout: "list", attachments } }
  } catch (error) {
    console.error("NuGet search error:", error)
    return createMessageResponse("Failed to search NuGet packages")
  }
}

// ---- Expert Finder Functions ----

async function getGraphAccessToken(): Promise<string> {
  const response = await axios.post(
    `https://login.microsoftonline.com/${process.env.TENANT_ID}/oauth2/v2.0/token`,
    new URLSearchParams({
      client_id: process.env.CLIENT_ID || "",
      client_secret: process.env.CLIENT_SECRET || "",
      scope: "https://graph.microsoft.com/.default",
      grant_type: "client_credentials",
    }).toString(),
    { headers: { "Content-Type": "application/x-www-form-urlencoded" } }
  )

  if (!response.data?.access_token) {
    throw new Error("No access token in response.")
  }
  return response.data.access_token
}

function buildExpertAttachment(
  displayName: string, jobTitle: string, department: string,
  email: string, officeLocation: string, phone: string,
  userPrincipalName: string, skills?: string | null, about?: string | null
) {
  const bodyItems: any[] = [
    {
      type: "ColumnSet",
      columns: [
        {
          type: "Column",
          width: "auto",
          items: [
            { type: "Image", url: "https://cdn-icons-png.flaticon.com/512/3135/3135715.png", size: "Medium", style: "Person" },
          ],
        },
        {
          type: "Column",
          width: "stretch",
          items: [
            { type: "TextBlock", text: displayName, weight: "Bolder", size: "Medium" },
            { type: "TextBlock", text: jobTitle, isSubtle: true, spacing: "None", size: "Small" },
            { type: "TextBlock", text: department, isSubtle: true, spacing: "None", size: "Small" },
          ],
        },
      ],
    },
    {
      type: "FactSet",
      facts: [
        { title: "Email", value: email },
        { title: "Office", value: officeLocation },
        { title: "Phone", value: phone },
      ],
    },
  ]

  if (skills) {
    bodyItems.push({ type: "TextBlock", text: `**Skills:** ${skills}`, wrap: true, size: "Small" })
  }

  const card = {
    type: "AdaptiveCard",
    version: "1.4",
    body: bodyItems,
    actions: [
      { type: "Action.OpenUrl", title: "Chat", url: `https://teams.microsoft.com/l/chat/0/0?users=${encodeURIComponent(userPrincipalName)}` },
      { type: "Action.OpenUrl", title: "Email", url: `mailto:${email}` },
    ],
  }

  const previewText = skills ? `${jobTitle} | ${skills}` : `${jobTitle} | ${department}`

  return {
    contentType: "application/vnd.microsoft.card.adaptive",
    content: card,
    preview: {
      contentType: "application/vnd.microsoft.card.hero",
      content: {
        title: displayName,
        text: previewText,
        images: [{ url: "https://cdn-icons-png.flaticon.com/512/3135/3135715.png" }],
      },
    },
  }
}

async function searchExpertsByName(query: string) {
  if (!query?.trim()) {
    return createMessageResponse("Please enter a name to search for experts in your organization.")
  }

  try {
    const accessToken = await getGraphAccessToken()

    const graphUrl = `https://graph.microsoft.com/v1.0/users?$search="displayName:${query}"&$select=id,displayName,jobTitle,department,mail,officeLocation,businessPhones,userPrincipalName&$top=10&$count=true`

    const response = await axios.get(graphUrl, {
      headers: {
        Authorization: `Bearer ${accessToken}`,
        ConsistencyLevel: "eventual",
      },
    })

    const users = response.data?.value || []

    if (users.length === 0) {
      return createMessageResponse(`No users found matching '${query}'.`)
    }

    const attachments = users.map((user: any) => {
      const displayName = user.displayName || "Unknown"
      const jobTitle = user.jobTitle || "N/A"
      const department = user.department || "N/A"
      const email = user.mail || "N/A"
      const officeLocation = user.officeLocation || "N/A"
      const phone = user.businessPhones?.[0] || "N/A"
      const userPrincipalName = user.userPrincipalName || ""

      return buildExpertAttachment(displayName, jobTitle, department, email, officeLocation, phone, userPrincipalName, null)
    })

    return { composeExtension: { type: "result", attachmentLayout: "list", attachments } }
  } catch (error: any) {
    console.error("Expert search by name error:", error?.message)
    return createMessageResponse(`Failed to search experts: ${error?.message}`)
  }
}

function searchExpertsBySkill(query: string) {
  if (!query?.trim()) {
    return createMessageResponse("Please enter a skill or topic to find experts.")
  }

  try {
    let expertsFilePath = path.join(__dirname, "experts.json")
    if (!fs.existsSync(expertsFilePath)) {
      expertsFilePath = path.join(".", "experts.json")
    }

    const expertsJson = fs.readFileSync(expertsFilePath, "utf-8")
    const experts = JSON.parse(expertsJson)

    const queryLower = query.toLowerCase()
    const matchedExperts = experts
      .filter((e: any) => {
        const skills = e.skills || []
        const jobTitle = (e.jobTitle || "").toLowerCase()
        const department = (e.department || "").toLowerCase()
        const about = (e.about || "").toLowerCase()

        const skillMatch = skills.some((s: string) => s.toLowerCase().includes(queryLower))
        const titleMatch = jobTitle.includes(queryLower)
        const deptMatch = department.includes(queryLower)
        const aboutMatch = about.includes(queryLower)

        return skillMatch || titleMatch || deptMatch || aboutMatch
      })
      .slice(0, 10)

    if (matchedExperts.length === 0) {
      return createMessageResponse(`No experts found with skill or topic '${query}'. Try: C#, Azure, React, Python, Security, DevOps, etc.`)
    }

    const attachments = matchedExperts.map((expert: any) => {
      const displayName = expert.displayName || "Unknown"
      const jobTitle = expert.jobTitle || "N/A"
      const department = expert.department || "N/A"
      const email = expert.email || "N/A"
      const officeLocation = expert.officeLocation || "N/A"
      const phone = expert.phone || "N/A"
      const skillsList = expert.skills ? expert.skills.join(", ") : "N/A"
      const about = expert.about || null

      return buildExpertAttachment(displayName, jobTitle, department, email, officeLocation, phone, email, skillsList, about)
    })

    return { composeExtension: { type: "result", attachmentLayout: "list", attachments } }
  } catch (error: any) {
    console.error("Expert search by skill error:", error?.message)
    return createMessageResponse(`Failed to search experts: ${error?.message}`)
  }
}

function handleExpertSelectItem(selectedItem: any) {
  const displayName = selectedItem.displayName || "Unknown"
  const jobTitle = selectedItem.jobTitle || "N/A"
  const department = selectedItem.department || "N/A"
  const email = selectedItem.email || "N/A"
  const officeLocation = selectedItem.officeLocation || "N/A"
  const phone = selectedItem.phone || "N/A"
  const userPrincipalName = selectedItem.userPrincipalName || email
  const skills = selectedItem.skills || "N/A"
  const about = selectedItem.about || ""

  const bodyItems: any[] = [
    {
      type: "ColumnSet",
      columns: [
        {
          type: "Column",
          width: "auto",
          items: [
            { type: "Image", url: "https://cdn-icons-png.flaticon.com/512/3135/3135715.png", size: "Large", style: "Person" },
          ],
        },
        {
          type: "Column",
          width: "stretch",
          items: [
            { type: "TextBlock", text: displayName, weight: "Bolder", size: "Large" },
            { type: "TextBlock", text: jobTitle, isSubtle: true, spacing: "None" },
            { type: "TextBlock", text: department, isSubtle: true, spacing: "None", size: "Small" },
          ],
        },
      ],
    },
    {
      type: "FactSet",
      facts: [
        { title: "Email", value: email },
        { title: "Office", value: officeLocation },
        { title: "Phone", value: phone },
        { title: "Skills", value: skills },
      ],
    },
  ]

  if (about) {
    bodyItems.push({ type: "TextBlock", text: "**About**", weight: "Bolder", spacing: "Medium" })
    bodyItems.push({ type: "TextBlock", text: about, wrap: true, isSubtle: true })
  }

  const card = {
    type: "AdaptiveCard",
    version: "1.4",
    body: bodyItems,
    actions: [
      { type: "Action.OpenUrl", title: "Chat in Teams", url: `https://teams.microsoft.com/l/chat/0/0?users=${encodeURIComponent(userPrincipalName)}` },
      { type: "Action.OpenUrl", title: "Send Email", url: `mailto:${email}` },
    ],
  }

  return {
    composeExtension: {
      type: "result" as const,
      attachmentLayout: "list" as const,
      attachments: [
        { contentType: "application/vnd.microsoft.card.adaptive", content: card },
      ],
    },
  }
}

// ---- Card Response Functions ----

function getAdaptiveCardResponse() {
  const adaptiveCardContent = {
    $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
    type: "AdaptiveCard",
    version: "1.0",
    body: [
      {
        speak: "Tom's Pie is a pizza restaurant which is rated 9.3 by customers.",
        type: "ColumnSet",
        columns: [
          {
            type: "Column",
            width: "2",
            items: [
              { type: "TextBlock", text: "**PIZZA**" },
              { type: "TextBlock", text: "Tom's Pie", weight: "bolder", size: "extraLarge", spacing: "none" },
              { type: "TextBlock", text: "4.2 $", isSubtle: true, spacing: "none" },
              { type: "TextBlock", text: '**Matt H. said** "I\'m compelled to give this place 5 stars due to the number of times I\'ve chosen to eat here this past year!"', size: "small", wrap: true },
            ],
          },
          {
            type: "Column",
            width: "1",
            items: [
              { type: "Image", url: "https://picsum.photos/300?image=882", size: "auto" },
            ],
          },
        ],
      },
    ],
    actions: [
      { type: "Action.OpenUrl", title: "More Info", url: "https://www.youtube.com/watch?v=CH2seLS5Wb0" },
    ],
  }

  return {
    composeExtension: {
      type: "result",
      attachmentLayout: "list",
      attachments: [
        {
          contentType: "application/vnd.microsoft.card.adaptive",
          content: adaptiveCardContent,
          preview: {
            contentType: "application/vnd.microsoft.card.thumbnail",
            content: { title: "Adaptive Card", text: "Please select to get Adaptive card" },
          },
        },
      ],
    },
  }
}

function getConnectorCardResponse() {
  const connectorCardContent = {
    "@context": "https://schema.org/extensions",
    "@type": "MessageCard",
    sections: [
      {
        activityTitle: "Steve Tweeted",
        activitySubtitle: "With #MicrosoftTeams",
      },
      {
        facts: [
          { name: "Posted By:", value: "Steve" },
          { name: "Posted At:", value: "12:00 PM" },
          { name: "Tweet:", value: "Hello Everyone!! Good Afternoon :-)" },
        ],
        text: "A tweet with #MicrosoftTeams has been posted:",
      },
    ],
    potentialAction: [
      {
        "@context": "http://schema.org",
        "@type": "ViewAction",
        name: "View all Tweets",
        target: ["https://twitter.com/search?q=%23MicrosoftTeams"],
      },
    ],
  }

  return {
    composeExtension: {
      type: "result",
      attachmentLayout: "list",
      attachments: [
        {
          contentType: "application/vnd.microsoft.teams.card.o365connector",
          content: connectorCardContent,
          preview: {
            contentType: "application/vnd.microsoft.card.thumbnail",
            content: { title: "O365 Connector Card", text: "Please select to get Connector card" },
          },
        },
      ],
    },
  }
}

function getResultGridResponse() {
  const imagesDir = path.join(__dirname, "public/Images")
  const attachments: any[] = []

  if (fs.existsSync(imagesDir)) {
    const files = fs.readdirSync(imagesDir).filter((f) => f.endsWith(".jpg"))
    for (const file of files) {
      attachments.push({
        contentType: "application/vnd.microsoft.card.thumbnail",
        content: { title: file },
      })
    }
  }

  return {
    composeExtension: {
      type: "result",
      attachmentLayout: "grid",
      attachments,
    },
  }
}

// ---- Submit Action Handlers ----

function handleCreateCard(data: any) {
  const title = data?.title || "Default Title"
  const description = data?.description || "Default Description"

  const card = {
    type: "AdaptiveCard",
    version: "1.4",
    body: [
      { type: "TextBlock", text: "Custom Card Created", weight: "Bolder", size: "Large", color: "Good" },
      { type: "TextBlock", text: title, weight: "Bolder", size: "Medium" },
      { type: "TextBlock", text: description, wrap: true, isSubtle: true },
    ],
  }

  return {
    composeExtension: {
      type: "result" as const,
      attachmentLayout: "list" as const,
      attachments: [
        { contentType: "application/vnd.microsoft.card.adaptive", content: card },
      ],
    },
  }
}

function handleGetMessageDetails(activity: any) {
  const messagePayload = activity.value?.messagePayload
  const messageText = messagePayload?.body?.content || "No message content"
  const messageId = messagePayload?.id || "Unknown"

  const card = {
    type: "AdaptiveCard",
    version: "1.4",
    body: [
      { type: "TextBlock", text: "Message Details", weight: "Bolder", size: "Large", color: "Accent" },
      { type: "TextBlock", text: `Message ID: ${messageId}`, wrap: true },
      { type: "TextBlock", text: `Content: ${messageText}`, wrap: true },
    ],
  }

  return {
    composeExtension: {
      type: "result" as const,
      attachmentLayout: "list" as const,
      attachments: [
        { contentType: "application/vnd.microsoft.card.adaptive", content: card },
      ],
    },
  }
}

// ---- App Setup ----

const app = new App({
  clientId: process.env.CLIENT_ID,
  clientSecret: process.env.CLIENT_SECRET,
  tenantId: process.env.TENANT_ID,
  plugins: [new DevtoolsPlugin()],
})

// Message handler
app.on("message", async ({ send, activity }) => {
  const text = (activity.text || "").toLowerCase()

  const replyText = text.includes("help")
    ? "I'm the Search Messaging Extension Bot!\n\nUse me in the compose area to search for NuGet packages or Wikipedia articles."
    : `You said: ${activity.text}\n\nType 'help' to learn about this bot.`

  await send(replyText)
})

// Link unfurling
app.on("message.ext.query-link", async ({ activity }) => {
  const url = activity.value?.url

  if (!url) {
    return createMessageResponse("No URL provided")
  }

  const card = {
    type: "AdaptiveCard",
    version: "1.4",
    body: [
      { type: "TextBlock", text: "Link Preview", weight: "Bolder", size: "Medium" },
      { type: "TextBlock", text: `URL: ${url}`, isSubtle: true, wrap: true },
      { type: "TextBlock", text: "This is a preview of the linked content generated by the message extension.", wrap: true, size: "Small" },
    ],
  }

  return {
    composeExtension: {
      type: "result",
      attachmentLayout: "list",
      attachments: [
        {
          contentType: "application/vnd.microsoft.card.adaptive",
          content: card,
          preview: {
            contentType: "application/vnd.microsoft.card.thumbnail",
            content: { title: "Link Preview", text: url },
          },
        },
      ],
    },
  }
})

// Submit action
app.on("message.ext.submit", async ({ activity }) => {
  const commandId = activity.value?.commandId
  const data = activity.value?.data

  switch (commandId) {
    case "createCard":
      return handleCreateCard(data)
    case "getMessageDetails":
      return handleGetMessageDetails(activity)
    default:
      return createMessageResponse(`Unknown command: ${commandId}`)
  }
})

// Search query
// @ts-expect-error - Return type compatibility issue with MessagingExtensionResponse
app.on("message.ext.query", async ({ activity }) => {
  const commandId = activity.value?.commandId
  const parameters = activity.value?.parameters || []
  const searchQuery = parameters[0]?.value || ""

  if (commandId === "wikipediaSearch") {
    return await searchWikipedia(searchQuery)
  } else if (commandId === "searchByName") {
    return await searchExpertsByName(searchQuery)
  } else if (commandId === "searchBySkill") {
    return searchExpertsBySkill(searchQuery)
  } else if (commandId === "searchQuery") {
    switch (searchQuery.toLowerCase()) {
      case "adaptive card":
        return getAdaptiveCardResponse()
      case "connector card":
        return getConnectorCardResponse()
      case "result grid":
        return getResultGridResponse()
      default:
        return await searchNuGetPackages(searchQuery)
    }
  }

  return { status: 400 }
})

// Select item
app.on("message.ext.select-item", async ({ activity }) => {
  const selectedItem = activity.value

  if (!selectedItem) {
    return createMessageResponse("No item selected")
  }

  // Check if this is an expert finder selection
  if (selectedItem.source === "expertFinder") {
    return handleExpertSelectItem(selectedItem)
  }

  const packageId = selectedItem.packageId || selectedItem.title || "Unknown"
  const version = selectedItem.version || ""
  const description = selectedItem.description || ""
  const projectUrl = selectedItem.projectUrl || ""

  const card = {
    type: "AdaptiveCard",
    version: "1.4",
    body: [
      { type: "TextBlock", text: `${packageId}, ${version}`, weight: "Bolder", size: "Large" },
      { type: "TextBlock", text: description, wrap: true, isSubtle: true },
      { type: "TextBlock", text: `NuGet: https://www.nuget.org/packages/${packageId}`, wrap: true, size: "Small" },
      { type: "TextBlock", text: `Project: ${projectUrl}`, wrap: true, size: "Small" },
    ],
  }

  return {
    composeExtension: {
      type: "result" as const,
      attachmentLayout: "list" as const,
      attachments: [
        { contentType: "application/vnd.microsoft.card.adaptive", content: card },
      ],
    },
  }
})

app.start().catch(console.error)
