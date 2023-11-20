const axios = require("axios");
const { CardFactory } = require("botbuilder");

class NorthwindLinkME {

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
                        const flagUrl = this.#getFlagUrl(supplier.Country);
                        attachment = CardFactory.thumbnailCard(supplier.CompanyName,
                            `${supplier.City}, ${supplier.Country}`, [flagUrl]);

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

    // Get a flag image URL given a country name
    // Thanks to https://flagpedia.net for providing flag images
    #getFlagUrl (country) {

        const COUNTRY_CODES = {
            "australia": "au",
            "brazil": "br",
            "canada": "ca",
            "denmark": "dk",
            "france": "fr",
            "germany": "de",
            "finland": "fi",
            "italy": "it",
            "japan": "jp",
            "netherlands": "nl",
            "norway": "no",
            "singapore": "sg",
            "spain": "es",
            "sweden": "se",
            "uk": "gb",
            "usa": "us"
        };

        return `https://flagcdn.com/32x24/${COUNTRY_CODES[country.toLowerCase()]}.png`;

    };
}

module.exports.NorthwindLinkME = new NorthwindLinkME();