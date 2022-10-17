---
page_type: sample
description: Demonstrating a feature where user can scan a product and mark it as approved/rejected.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "16/11/2021 00:15:13"
urlFragment: officedev-microsoft-teams-samples-tab-product-inspection-csharp
---

# Product Inspection

This sample app demonstrate a feature where user can scan a product, capture a image and mark it as approved/rejected.

## Feature of the sample.

![ProductInspection](ProductInspection/Images/PreviewImg.gif)

	

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1

  ```bash
  # determine dotnet version
  dotnet --version
  ```

- [ngrok](https://ngrok.com/) or equivalent tunnelling solution

## To try this sample
  
- Clone the repository 
   ```bash
   git clone https://github.com/OfficeDev/microsoft-teams-samples.git
   ```

- Build your solution

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `samples/tab-product-inspection/csharp/ProductInspection` folder
  - Select `ProductInspection.csproj` file
  - Press `F5` to run the project

- Setup ngrok
  ```bash
  ngrok http -host-header=rewrite 3978
  ```

- Config changes
   - Press F5 to run the project
   - Update the ngrok in manifest
   - Zip all three files present in manifest folder

## Interacting with the app in Teams Meeting
Interact with Product Inspection by clicking on the App icon.
1. Once the app is clicked, Product Inspection appears with the default product list.

   ![ProductInspection](ProductInspection/Images/product-listImg.png)
   
2. On click on "Inspect product" button, scanner will open when scan the product bar code user can Approve or Reject the product.

	![ProductInspection](ProductInspection/Images/product-statusImg.png)

3. On click on "View product status" button, scanner will open when scan the product bar code and user can view the detail of the product.

   <img src="ProductInspection/Images/view-product-statusImg.png" alt="drawing" width="100px"/>



- [Upload app manifest file]
 - **Edit** the `manifest.json` contained in the  `Manifest` folder to replace your Microsoft App Id (that was created when you registered your bot earlier) *everywhere* you see the place holder string `<<Manifest-id>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)and ngrok url *everywhere* you see the place holder string `<<base-url>>`
    - **Zip** up the contents of the `Manifest` folder to create a `manifest.zip`
    - **Upload** the `manifest.zip` to Teams (in the Apps view click "Upload a custom app")
