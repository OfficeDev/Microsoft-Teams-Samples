---
page_type: sample
description: This sample implements a Teams message extension that can be used as a plugin for Microsoft Copilot for Microsoft 365. The message extension showcases copilot handoff along with allowings users to query the Northwind Database.
products:
- office-teams
- copilot-m365
languages:
- typescript
---

# Northwind inventory message extension sample

![License.](https://img.shields.io/badge/license-MIT-green.svg)

This sample implements a Teams message extension that can be used as a plugin for Microsoft Copilot for Microsoft 365. The message extension allows users to query the [Northwind Database](https://learn.microsoft.com/dotnet/framework/data/adonet/sql/linq/downloading-sample-databases).

![Screenshot of the sample extension working in Copilot in Microsoft Teams](./lab/images/03-03a-response-on-chai.png)

## Prerequisites

- [Node.js 18.x](https://nodejs.org/download/release/v18.18.2/)
- [Visual Studio Code](https://code.visualstudio.com/)
- [Teams Toolkit](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension)
- You will need a Microsoft work or school account with [permissions to upload custom Teams applications](https://learn.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading). The account will also need a Microsoft Copilot for Microsoft 365 license to use the extension in Copilot.

## Setup and use the sample

For instructions on setting up and running the sample, see the [lab exercises](./lab/Exercise%2000%20-%20Welcome.md).

## Example prompts for Copilot

Here are some ideas for prompts to try. If you don't get the result you expect, try typing "new chat" and then trying again.

### Single parameter prompts

- *Find Chai in Northwind Inventory*

- *Who supplies discounted produce to Northwind?*

- *Find high revenue products in Northwind. Have there been any ad campaigns for these products?*

  > [!NOTE]
  > The ad campaign details are in the [sample documents](./sampleDocs/).

### Multi-parameter prompts

- *Find northwind dairy products that are low on stock. Show me a table with the product, supplier, units in stock and on order. Reference the details for each product.*

  (then)

  *OK can you draft an email to our procurement team asking them if we've had any delivery issues with these suppliers?*

- *Find Northwind beverages with more than 100 units in stock*

  (then)

  *What are the payment terms for these suppliers?*

  > [!NOTE]
  > The answer to the 2nd question is in the [sample documents](./sampleDocs/).

- *We’ve been receiving partial orders for Tofu. Find the supplier in Northwind and draft an email summarizing our inventory and reminding them they should stop sending partial orders per our MOQ policy.*

  > [!NOTE]
  > The MOQ policy is in one of the [sample documents](./sampleDocs/).

- *Northwind will have a booth at Microsoft Community Days  in London. Find products with local suppliers and write a LinkedIn post to promote the booth and products.*

  (then)

  *Emphasize how delicious the products are and encourage people to visit our booth at the conference*

- *What beverage is high in demand due to social media that is low stock in Northwind in London. Reference the product details to update stock.*

  > [!NOTE]
  > There is a document that discusses a social media campaign for one of the products in the [sample documents](./sampleDocs/).

### Copilot handoff to bot

- Copilot welcome screen
![Welcome screen for copilot](./lab/images/startScreen.png)

- Select the handoff to bot button
![Handoff action button](./lab/images/action-btn.png)

- Request is handoff to bot
![Bot response](./lab/images/handoff.png)


![](https://m365-visitor-stats.azurewebsites.net/SamplesGallery/officedev-copilot-for-m365-plugins-samples-msgext-northwind-inventory-ts)