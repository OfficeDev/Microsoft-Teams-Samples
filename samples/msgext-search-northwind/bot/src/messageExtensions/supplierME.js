const axios = require("axios");
const ACData = require("adaptivecards-templating");
const { CardFactory } = require("botbuilder");

class SupplierME {

    // Get suppliers given a query
    async handleTeamsMessagingExtensionQuery (context, query) {

        try {
            const response = await axios.get(
                `https://services.odata.org/V4/Northwind/Northwind.svc/Suppliers` +
                `?$filter=contains(tolower(CompanyName),tolower('${query}'))` +
                `&$orderby=CompanyName&$top=8`
            );

            const attachments = [];
            response.data.value.forEach((supplier) => {

                const imageUrl = `https://picsum.photos/seed/${supplier.SupplierID}/300/150`;

                const itemAttachment = CardFactory.heroCard(supplier.CompanyName);
                const previewAttachment = CardFactory.thumbnailCard(supplier.CompanyName,
                    `${supplier.City}, ${supplier.Country}`);

                previewAttachment.content.tap = {
                    type: "invoke",
                    value: {    // Values passed to selectItem when an item is selected
                        queryType: 'supplierME',
                        SupplierID: supplier.SupplierID,
                        imageUrl: imageUrl,
                        Address: supplier.Address || "",
                        City: supplier.City || "",
                        CompanyName: supplier.CompanyName || "unknown",
                        ContactName: supplier.ContactName || "",
                        ContactTitle: supplier.ContactTitle || "",
                        Country: supplier.Country || "",
                        Fax: supplier.Fax || "",
                        Phone: supplier.Phone || "",
                        PostalCode: supplier.PostalCode || "",
                        Region: supplier.Region || ""
                    },
                };
                const attachment = { ...itemAttachment, preview: previewAttachment };
                attachments.push(attachment);
            });

            return {
                composeExtension: {
                    type: "result",
                    attachmentLayout: "list",
                    attachments: attachments,
                }
            };

        } catch (error) {
            console.log(error);
        }
    };

    handleTeamsMessagingExtensionSelectItem (context, selectedValue) {

        // Read card from JSON file
        const templateJson = require('../cards/supplierCard.json');
        const template = new ACData.Template(templateJson);
        const card = template.expand({
            $root: selectedValue
        });

        const resultCard = CardFactory.adaptiveCard(card);

        return {
            composeExtension: {
                type: "result",
                attachmentLayout: "list",
                attachments: [resultCard]
            },
        };

    };

    async linkQuery (context, query) {

        const url = query.url;

        // Ensure the host name ends with northwindtraders.com
        const host = new URL(url).hostname;
        if (host.endsWith("northwindtraders.com")) {

            // Get the supplier ID from the URL
            const supplierID = new URL(url).searchParams.get("supplierID");
            if (supplierID) {

                // Make a thumbnail card to show if the supplier is not found
                let attachment = CardFactory.thumbnailCard("Supplier not found");
                try {
                    // Get the supplier details from the Northwind OData service
                    const supplierResponse = await axios.get(
                        `https://services.odata.org/V4/Northwind/Northwind.svc/Suppliers(${supplierID})`
                    );

                    if (supplierResponse.data?.SupplierID) {

                        const supplier = supplierResponse.data;
                        const imageUrl = `https://picsum.photos/seed/${supplier.SupplierID}/300/150`
                        // Read card from JSON file
                        const templateJson = require('../cards/supplierCard.json');
                        const template = new ACData.Template(templateJson);
                        const card = template.expand({
                                $root: {  
                                    queryType: 'LinkUnfurl',
                                    SupplierID: supplier.SupplierID,
                                    imageUrl: imageUrl,
                                    Address: supplier.Address || "",
                                    City: supplier.City || "",
                                    CompanyName: supplier.CompanyName || "unknown",
                                    ContactName: supplier.ContactName || "",
                                    ContactTitle: supplier.ContactTitle || "",
                                    Country: supplier.Country || "",
                                    Fax: supplier.Fax || "",
                                    Phone: supplier.Phone || "",
                                    PostalCode: supplier.PostalCode || "",
                                    Region: supplier.Region || ""
                                }
                        });

                        attachment = CardFactory.adaptiveCard(card);
                        attachment.preview = CardFactory.thumbnailCard(supplier.CompanyName,
                               `${supplier.City}, ${supplier.Country}`,[imageUrl]);

                       
                        
                    }

                } catch (error) {
                    console.log(error);
                }

                const response = {
                    composeExtension: {
                        attachmentLayout: 'list',
                        type: 'result',
                        attachments: [attachment]
                    }
                };
                return response;

            }
        }
    }
}

module.exports.SupplierME = new SupplierME();