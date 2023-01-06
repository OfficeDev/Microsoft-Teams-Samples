// <copyright file="decryption-helper.js" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

const certHelper = require('./certificate-helper');

class DecryptionHelper {
    static async getDecryptedContent(notification) {

        if (notification) {
            
            var payLoad = this.processEncryptedNotification(notification);
            return payLoad;
        }
    }

    /**
    * Processes an encrypted notification
    * @param  {object} notification - The notification containing encrypted content
    */
    static processEncryptedNotification(notification) {
        // Decrypt the symmetric key sent by Microsoft Graph
        const symmetricKey = certHelper.decryptSymmetricKey(
            notification[0].encryptedContent.dataKey,
            process.env.PRIVATE_KEY_PATH
        );

        // Validate the signature on the encrypted content
        const isSignatureValid = certHelper.verifySignature(
            notification[0].encryptedContent.dataSignature,
            notification[0].encryptedContent.data,
            symmetricKey
        );

        if (isSignatureValid) {
            // Decrypt the payload
            const decryptedPayload = certHelper.decryptPayload(
                notification[0].encryptedContent.data,
                symmetricKey
            );

            return JSON.parse(decryptedPayload);
        }
    }
}

exports.DecryptionHelper = DecryptionHelper;