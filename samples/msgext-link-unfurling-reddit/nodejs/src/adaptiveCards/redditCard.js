/**
 * Creates an Adaptive Card for displaying Reddit post information
 * @param {object} postData - Formatted Reddit post data
 * @returns {object} Adaptive Card JSON
 */
function createRedditCard(postData) {
  const card = {
    type: "AdaptiveCard",
    body: [
      {
        type: "TextBlock",
        text: `[${postData.title}](${postData.url})`,
        size: "Large",
        wrap: true,
        maxLines: 2,
      },
      {
        type: "ColumnSet",
        columns: [
          {
            type: "Column",
            width: "Auto",
            items: [
              {
                type: "TextBlock",
                text: `Score: ${postData.score}`,
              },
            ],
          },
          {
            type: "Column",
            width: "Auto",
            items: [
              {
                type: "TextBlock",
                text: `Comments: [${postData.numComments}](https://www.reddit.com/r/${postData.subreddit}/comments/${postData.id})`,
              },
            ],
          },
          {
            type: "Column",
            width: "Stretch",
            items: [
              {
                type: "TextBlock",
                text: `/r/${postData.subreddit}`,
                horizontalAlignment: "Right",
                size: "Default",
                weight: "Bolder",
              },
            ],
          },
        ],
      },
      {
        type: "Image",
        url: postData.thumbnail,
        horizontalAlignment: "Center",
        separator: true,
      },
      {
        type: "ColumnSet",
        columns: [
          {
            type: "Column",
            width: "Auto",
            items: [
              {
                type: "TextBlock",
                text: `Posted by [/u/${postData.author}](https://www.reddit.com/u/${postData.author})`,
                size: "Small",
                weight: "Lighter",
              },
            ],
          },
          {
            type: "Column",
            width: "Auto",
            items: [
              {
                type: "TextBlock",
                text: `{{DATE(${new Date(postData.created).toISOString()})}}`,
                horizontalAlignment: "Right",
                size: "Small",
                weight: "Lighter",
              },
            ],
          },
        ],
      },
    ],
    actions: [
      {
        type: "Action.OpenUrl",
        title: "Open in Reddit",
        url: postData.url,
      },
    ],
    $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
    version: "1.4",
  };

  // Add thumbnail or text based on availability
  if (!postData.thumbnail || postData.thumbnail === "self" || postData.thumbnail === "default") {
    // Remove the image element and add text instead
    card.body[2] = {
      type: "TextBlock",
      text: postData.selftext || "Preview Not Available",
      wrap: true,
      separator: true,
    };
  }

  return card;
}


module.exports = { createRedditCard };
