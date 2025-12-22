
const certHelper = require('./certificate-helper');

class DecryptionHelper {
    /**
    * Processes an encrypted notification
    * @param  {object} notification - The notification containing encrypted content
    */
    static processEncryptedNotification(notification) {
        // Decrypt the symmetric key sent by Microsoft Graph
        const symmetricKey = certHelper.decryptSymmetricKey(
            notification[0].encryptedContent.dataKey,
            process.env.PRIVATE_KEY_PATH,
           
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

            var resource = {};
            resource = JSON.parse(decryptedPayload);
            resource.changeType = notification[0].changeType;
        }

        // return decrypted object 
        return resource
    }
}

exports.DecryptionHelper = DecryptionHelper;