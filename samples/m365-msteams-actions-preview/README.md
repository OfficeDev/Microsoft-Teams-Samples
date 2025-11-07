# Actions across Microsoft 365 (private preview)

This project contains private preview documentation for (Microsoft 365 extended Teams apps) Actions in Microsoft 365 app(microsoft365.com), including the following:

- [DevDoc] [Actions developer instructions](./actions_developer_instructions_v3.pdf)

- [Sample] See instructions in below section for running the sample code. 
  
- [Actions overview - user flow]
  
https://github.com/OfficeDev/m365-msteams-actions-preview/assets/104946886/5b72b5d6-471b-48b8-8430-64f6d73d856a


- [Actions overview - developer flow]


https://github.com/OfficeDev/m365-msteams-actions-preview/assets/104946886/bc9969f3-848b-4c05-bfca-c5d1d8c2ade4


- [Enable Actions for users - admin flow]



https://github.com/OfficeDev/m365-msteams-actions-preview/assets/104946886/b593965b-f760-4703-be80-7b1bb1531fc2



# Getting started with the sample code

The frontend is a React app and the backend is hosted on Azure i.e Azure SQL database and Azure functions. You will need an Azure subscription to run the app.
If you do not have Azure subscription, then no worries, this app has the ability to use browser's localStorage to test the app locally without using any api or database.

## User experience

[Demo video]

https://github.com/OfficeDev/m365-msteams-actions-preview/assets/104946886/f8022208-2807-40f1-8b61-23bd64d113df

## Prerequisite to use this sample
- [Node.js](https://nodejs.org/), supported versions: 14, 16, 18 (preview)
- A Microsoft 365 account. If you do not have Microsoft 365 account, apply one from [Microsoft 365 developer program](https://developer.microsoft.com/en-us/microsoft-365/dev-program)
- Latest [Teams Toolkit Visual Studio Code Extension](https://aka.ms/teams-toolkit) or [TeamsFx CLI](https://aka.ms/teamsfx-cli)
- [VS Code](https://code.visualstudio.com/)
- [Teamsfx-cli](https://www.npmjs.com/package/@microsoft/teamsfx-cli)
- [Teams Toolkit](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension)
- An [Azure subscription](https://azure.microsoft.com/en-us/free/) (Optional)

## Minimal path to awesome
1. Run the app locally with browser's local storage
1. Run the app locally with Azure Subscription

[Video tutorial] 

https://github.com/OfficeDev/m365-msteams-actions-preview/assets/104946886/12036b94-05c3-445b-a5e7-c79f4f5c842d



### Option 1: Run the app locally with browser's local storage
To debug the app
1. Open the folder in Visual Studio Code with Teams Toolkit extension installed.
1. Open Debug View (`Ctrl+Shift+D`) and select "Debug in the Microsoft 365 app (Edge) without backend" in dropdown list.
1. Press "F5" to debug the app in the Microsoft 365 app in a browser window.
   
### Option 2: Run the app locally with Azure Subscription
To debug the project, you will need to configure an Azure SQL Database to be used locally:
1. [Create an Azure SQL Database](https://docs.microsoft.com/en-us/azure/azure-sql/database/single-database-create-quickstart?tabs=azure-portal)
1. [Add IP address of your computer into allowlist of firewall of Azure SQL Server](https://docs.microsoft.com/en-us/azure/azure-sql/database/firewall-configure#from-the-database-overview-page)
1. Use [query editor](https://docs.microsoft.com/en-us/azure/azure-sql/database/connect-query-portal) with below query to create a table:
    ```sql
    CREATE TABLE Todo
    (
        id INT IDENTITY PRIMARY KEY,
        description NVARCHAR(128) NOT NULL,
        objectId NVARCHAR(36),
        itemId NVARCHAR(128),
        channelOrChatId NVARCHAR(128),
        isCompleted TinyInt NOT NULL default 0,
    )

    ```
1. Open **env/.env.local.user** file, uncomment and set the values of below config with the Azure SQL Database you just created:
    ```
    SECRET_SQL_ENDPOINT=
    SECRET_SQL_DATABASE_NAME=
    SECRET_SQL_USER_NAME=
    SECRET_SQL_PASSWORD=
    ```
1. Edit the `LOCAL_STORAGE` value to `false` in **env/.env.local** file.
1. Open Debug View (`Ctrl+Shift+D`) and select "Debug in the Microsoft 365 app (Edge)" in dropdown list.
1. Press "F5" to debug the app in the Microsoft 365 app in a browser window.

### (Optional) Deploy the app to Azure

>Here are the instructions to run the sample in **Visual Studio Code**. You can also try to run the app using TeamsFx CLI tool, refer to [Try the Sample with TeamsFx CLI](cli.md)

1. Clone the repo to your local workspace or directly download the source code.
1. Download [Visual Studio Code](https://code.visualstudio.com) and install [Teams Toolkit Visual Studio Code Extension](https://aka.ms/teams-toolkit).
1. Open the project in Visual Studio Code.
1. Create an **env/.env.dev.user** file, and set value for `SECRET_SQL_USER_NAME` and `SECRET_SQL_PASSWORD`
1. Open the command palette and select `Teams: Provision in the cloud`. You will be asked to input admin name and password of SQL. The toolkit will help you to provision Azure SQL.
1. Once provision is completed, open the command palette and select `Teams: Deploy to the cloud`.
1. Open **env/.env.dev** file, you could get the database name in `PROVISIONOUTPUT__AZURESQLOUTPUT__DATABASENAME` output. [Set IP address of your computer into server-level IP firewall rule from the database overview page](https://docs.microsoft.com/en-us/azure/azure-sql/database/firewall-configure#from-the-database-overview-page).
1. In Azure portal, find the database by `databaseName` and use [query editor](https://docs.microsoft.com/en-us/azure/azure-sql/database/connect-query-portal) with below query to create a table:
    ```sql
    CREATE TABLE Todo
    (
        id INT IDENTITY PRIMARY KEY,
        description NVARCHAR(128) NOT NULL,
        objectId NVARCHAR(36),
        itemId NVARCHAR(128),
        channelOrChatId NVARCHAR(128),
        isCompleted TinyInt NOT NULL default 0,
    )
    ```

### Preview the app in Teams
1. Once deployment is completed, you can preview the app running in Azure. In Visual Studio Code, open `Run and Debug` and select `Launch Remote in the Microsoft 365 app (Edge)` or `Launch Remote in the Microsoft 365 app (Chrome)` in the dropdown list and Press `F5` or the green arrow button to open a browser.


## Feedback
We really appreciate your feedback! If you encounter any issue or error, please report issues to us following the [Supporting Guide](https://github.com/OfficeDev/TeamsFx-Samples/blob/dev/SUPPORT.md). Meanwhile you can make [recording](https://aka.ms/teamsfx-record) of your journey with our product, they really make the product better. Thank you!

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
