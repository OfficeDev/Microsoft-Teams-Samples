// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { App } from '@microsoft/teams.apps'

const app = new App()

// In-memory storage for tracking sent messages (activity ID -> message text)
const activityLog = new Map<string, string>()

// Handle incoming messages - echo back the user's message
app.on('message', async ({ send, activity }) => {
  const echoText = `echo: "${activity.text}"`
  const response = await send(echoText)
  
  // Log the activity ID and message text for reaction tracking
  if (response?.id) {
    activityLog.set(response.id, echoText)
  }
})

// Handle reactions added to messages
app.on('messageReaction', async ({ send, activity }) => {
  const reactionsAdded = activity.reactionsAdded
  
  if (reactionsAdded && reactionsAdded.length > 0) {
    for (const reaction of reactionsAdded) {
      const replyToId = activity.replyToId
      const originalMessage = replyToId ? activityLog.get(replyToId) : null
      
      if (!originalMessage) {
        const response = await send(`Activity ${replyToId} not found in the log.`)
        if (response?.id) {
          activityLog.set(response.id, `Activity ${replyToId} not found in the log.`)
        }
      } else {
        const reactionText = `You added '${reaction.type}' regarding '${originalMessage}'`
        const response = await send(reactionText)
        if (response?.id) {
          activityLog.set(response.id, reactionText)
        }
      }
    }
  }
  
  const reactionsRemoved = activity.reactionsRemoved
  
  if (reactionsRemoved && reactionsRemoved.length > 0) {
    for (const reaction of reactionsRemoved) {
      const replyToId = activity.replyToId
      const originalMessage = replyToId ? activityLog.get(replyToId) : null
      
      if (!originalMessage) {
        const response = await send(`Activity ${replyToId} not found in the log.`)
        if (response?.id) {
          activityLog.set(response.id, `Activity ${replyToId} not found in the log.`)
        }
      } else {
        const reactionText = `You removed '${reaction.type}' regarding '${originalMessage}'`
        const response = await send(reactionText)
        if (response?.id) {
          activityLog.set(response.id, reactionText)
        }
      }
    }
  }
})

app.start().catch(console.error)
