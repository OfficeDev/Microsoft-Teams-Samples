const { TableClient, TableServiceClient } = require("@azure/data-tables");
const { randomUUID } = require("crypto");
const fs = require("fs");
const path = require("path");

(async () => {
    const connectionString = process.argv[2] ? process.argv[2] : "UseDevelopmentStorage=true";
    const reset = process.argv[3] === "--reset" || process.argv[3] === "-r" ? true : false;

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
    // Get a flag image URL given a country name
    // Thanks to https://flagpedia.net for providing flag images
    function getFlagUrl(country) {
        return `https://flagcdn.com/32x24/${COUNTRY_CODES[country.toLowerCase()]}.png`;
    };

    async function getTables(tableServiceClient) {
        let tables = [];
        for await (const table of tableServiceClient.listTables()) {
            tables.push(table.name)
        }
        return tables;
    }

    const tableServiceClient = TableServiceClient.fromConnectionString(connectionString);

    if (reset) {
        const tables = await getTables(tableServiceClient);
        tables.forEach(async table => {
            const tableClient = TableClient.fromConnectionString(connectionString, table);
            console.log(`Deleting table: ${table}`);
            await tableClient.deleteTable();
        });
        let tablesExist = true;
        while (tablesExist) {
            console.log("Waiting for tables to be deleted...");
            const tables = await getTables(tableServiceClient);
            if (tables.length === 0) {
                tablesExist = false;
                console.log("All tables deleted.");
            }
            await new Promise(resolve => setTimeout(resolve, 1000));
        }
    }

    const tables = ["Categories", "Customers", "Employees", "Orders", "OrderDetails", "Products", "Suppliers"];
    const rowKeyColumnNames = ["CategoryID", "CustomerID", "EmployeeID", "OrderID", null, "ProductID", "SupplierID"];
    const generateImage = [false, true, false, false, false, true, true];
    const generateFlag = [false, true, false, false, false, false, true];

    tables.forEach(async (table, index) => {
        const tables = await getTables(tableServiceClient);
        if (tables.includes(table)) {
            console.log(`Table ${table} already exists, skipping...`);
            return;
        }

        console.log(`Creating table: ${table}`);
        let tableCreated = false;
        while (!tableCreated) {
            try {
                await tableServiceClient.createTable(table);
                tableCreated = true;
            } catch (err) {
                if (err.statusCode === 409) {
                    console.log('Table is marked for deletion, retrying in 5 seconds...');
                    await new Promise(resolve => setTimeout(resolve, 5000));
                } else {
                    throw err;
                }
            }
        }

        const tableClient = TableClient.fromConnectionString(connectionString, table);
        const jsonString = fs.readFileSync(path.resolve(__dirname, "db", `${table}.json`), "utf8");
        const entities = JSON.parse(jsonString);
        for (const entity of entities[table]) {
            const rowKeyColumnName = rowKeyColumnNames[index];
            const rowKey = rowKeyColumnName ? entity[rowKeyColumnName].toString() : randomUUID();
            console.log(`Added entity to ${table} with key ${rowKey}`);

            // If we're on a table that needs an image and one wasn't in the JSON, make a random one
            if (generateImage[index] && !("ImageURL" in entity)) {
                entity["ImageUrl"] = `https://picsum.photos/seed/${rowKey}/200/300`;
            }
            // If we're on a table that needs a flag image, make it here
            if (generateFlag[index]) {
                entity["FlagUrl"] = getFlagUrl(entity["Country"]);
            }
            await tableClient.createEntity({
                partitionKey: table,
                rowKey,
                ...entity
            });
        }
    });

})();