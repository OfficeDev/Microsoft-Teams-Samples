const config = {
        BOT_ID: process.env.BOT_ID,
        BOT_PASSWORD: process.env.BOT_PASSWORD, 
        AZURE_OPENAI_API_KEY: process.env.AZURE_OPENAI_API_KEY,
        AZURE_OPENAI_ENDPOINT: process.env.AZURE_OPENAI_ENDPOINT,
        AZURE_OPENAI_DEPLOYMENT_NAME: process.env.AZURE_OPENAI_DEPLOYMENT_NAME,
        BOT_ENDPOINT:process.env.BOT_ENDPOINT,
        Token: process.env.Token,
        Base64EncodedCertificate: process.env.Base64EncodedCertificate,
        EncryptionCertificateId: process.env.EncryptionCertificateId,
        PRIVATE_KEY_PATH : process.env.PRIVATE_KEY_PATH,
        OPENSSL_CONFIG: process.env.OPENSSL_CONFIG,
        Account_Name: process.env.Account_Name,
        Account_Key: process.env.Account_Key,
        Table_Name: process.env.Table_Name,
        partitionKey: process.env.partitionKey,
        AI_Model: process.env.AI_Model,
        SubscriptionURL: process.env.SubscriptionURL,
        SystemPrompt:  process.env.SystemPrompt,
        LocalTimeZone: process.env.LocalTimeZone,
        APPINSIGHTS_INSTRUMENTATIONKEY: process.env.APPINSIGHTS_INSTRUMENTATIONKEY,
        APPINSIGHTS_CONNECTIONSTRING: process.env.APPINSIGHTS_CONNECTIONSTRING,

  };

module.exports = config;
