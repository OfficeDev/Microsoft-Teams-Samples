# Building Message Extensions for Microsoft Copilot for Microsoft 365

TABLE OF CONTENTS

* [Welcome](./Exercise%2000%20-%20Welcome.md) 
* [Exercise 1](./Exercise%2001%20-%20Set%20up.md) - Set up your development Environment 
* [Exercise 2](./Exercise%2002%20-%20Run%20sample%20app.md) - Run the sample as a Message Extension
* Exercise 3 - Run the sample as a Copilot plugin **(THIS PAGE)**
* [Exercise 4](./Exercise%2003%20-%20Add%20a%20new%20command.md) - Add a new command
* [Exercise 5](./Exercise%2005%20-%20Code%20tour.md) - Code tour

## Exercise 2 - Run the sample as a Copilot plugin

> [!NOTE]  
> To perform the following exercise, your account must have a valid license for Copilot for Microsoft 365.

## Step 1 - Test in Microsoft Copilot for Microsoft 365 (single parameter)

Begin by clicking the "Try the new Teams" switch to move to the new Teams client application.

> IMPORTANT - Microsoft Copilot for Microsoft 365 only works in the "New" Teams. Please don't miss this step! 

> If you restart your debugger after switching to "New" teams, you may get an error message after the debugger starts. This is a known problem; please just close the error dialog and continue testing.

In the left navigation, click on "Copilot" to open Copilot.

Check the lower left of the chat user interface, below the compose box. You should see a plugin icon 1️⃣ . Click this and enable the Northwind Inventory plugin 2️⃣ .

![Small panel with a toggle for each plugin](./images/03-02-Plugin-Panel.png)

For best results, start a new chat by typing "New chat" or by clicking on the **New chat** icon at the top right before each prompt or set of related prompts.

![Copilot showing its new chat screen](./images/03-01-New-Chat.png)

Here are some prompts to try that use only a single parameter of the message extension:

* "Find information about Chai in Northwind Inventory"

* "Find discounted seafood in Northwind. Show a table with the products, supplier names, average discount rate, and revenue per period."

See if this last one also locates any of the documents you uploaded to your OneDrive.

As you're testing, watch the log messages within your application. You should be able to see when Copilot calls your plugin. For example, after requesting "discounted seafood items", Copilot issued this query using the `discountSearch` command.

![Log file shows a discount search for seafood](./images/03-02a-Query-Log1.png)

You may see citations of the Northwind data in 3 forms. If there's a single reference, Copilot may show the whole card.

![Adaptive card for Chai embedded in a Copilot response](./images/03-03a-response-on-chai.png)

If there are multiple references, Copilot may show a small number next to each. You can hover over these numbers to display the adaptive card. References will also be listed below the response.

![Reference numbers embedded in a Copilot response - hovering over the number shows the adaptive card](./images/03-03-Response-on-Chai.png)

Try using these adaptive cards to take action on the products. Notice that this doesn't affect earlier responses from Copilot.

Feel free to try making up your own prompts. You'll find that they only work if Copilot is able to query the plugin for the required information. This underscores the need to anticipate the kinds of prompts users will issue, and providing corresponding types of queries for each one. Having multiple parameters will make this more powerful!

## Step 2 - Test in Microsoft Copilot for Microsoft 365 (multiple parameters)

In this exercise, you'll try some prompts that exercise the multi-parameter feature in the sample plugin. These prompts will request data that can be retrieved by name, category, inventory status, supplier city, and stock level, as defined in [the manifest](../appPackage/manifest.json).

For example, try prompting "Find Northwind beverages with more than 100 items in stock". To respond, Copilot must identify products:

* where the category is "beverages"
* where inventory status is "in stock"
* where the stock level is more than 100

If you look at the log file, you can see that Copilot was able to understand this requirement and fill in 3 of the parameters in the first message extension command.

![Screen shot of log showing a query for categoryName=beverages and stockLevel=100- ](./images/03-06-Find-Northwind-Beverages-with-more-than-100.png)

The plugin code applies all three filters, providing a result set of just 4 products. Using the information on the 4 resulting adaptive cards, Copilot renders a result similar to this:

![Copilot produced a bulleted list of products with references](./images/03-06b-Find-Northwind-Beverages-with-more-than-100.png)

By using this prompt, Copilot might look also in your OneDrive files to find the payment terms with each supplier's contract. In this case, you will notice that some of the references won't have the Northwind Inventory icon, but the Word one.

![Copilot extracted payment terms from contracts in SharePoint](./images/03-06c-PaymentTerms.png)

Here are some more prompts to try:

* "Find Northwind dairy products that are low on stock. Show me a table with the product, supplier, units in stock and on order."

* "We’ve been receiving partial orders for Tofu. Find the supplier in Northwind and draft an email summarizing our inventory and reminding them they should stop sending partial orders per our MOQ policy."

* "Northwind will have a booth at Microsoft Community Days  in London. Find products with local suppliers and write a LinkedIn post to promote the booth and products."

    Request an enhancement by prompting,

    "Emphasize how delicious the products are and encourage people to attend our booth"

* "What beverage is high in demand due to social media that is low stock in Northwind in London. Reference the product details to update stock."

Which prompts work best for you? Try making up your own prompts and observe your log messages to see how Copilot accesses your plugin.

### Troubleshooting tip
If you're facing challenges while testing your plugin, you can enable 'developer mode'. Developer mode provides information on the plugin selected by the Copilot orchestrator to respond to the prompt. It also shows the available functions in the plugin and the API call's status code.

To enable developer mode, type the following into Copilot:
```
-developer on
```
For additional information on common problems and how to fix them, see the  [troubleshooting](Troubleshooting.md) guide.

Now just execute your prompt. This time, the output will look like this: 

![The developer mode in action](./images/03-03b-developer-mode.png)

As you can notice, below the response generated by Copilot, we have a table that provides us insightful information about what happened behind the scenes:

- Under **Enabled plugins**, we can see that Copilot has identified that the Northwind Inventory plugin is enabled.
- Under **Matched functions**, we can see that Copilot has determined that the Northwind inventory plugin offers three functions: `inventorySearch`, `discountSearch`, and `companySearch`.
- Under **Selected functions for execution**, we can see that Copilot has selected the `inventorySearch` function to respond to the prompt.
- Under **Function execution details**, we can see some detailed information about the execution, like the HTTP response returned by the plugin to the Copilot engine.

## Congratulations

You have completed Exercise 3.
Please proceed to [Exercise 4](./Exercise%2004%20-%20Add%20a%20new%20command.md), in which you will add a new command to the messaging extension.
