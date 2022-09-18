const { X509Certificate } = require('crypto');
let pki = require('node-forge').pki;

class DecryptionHelper {
    static async getDecryptedContent(notification) {

        if (notification) {
            this.processEncryptedNotification(notification);
        }
        else {
            require('node-forge').pki.crea
            var cert = new X509Certificate();
            cert.checkPrivateKey
        }
    }

    /**
    * Processes an encrypted notification
    * @param  {object} notification - The notification containing encrypted content
    */
    static processEncryptedNotification(notification) {
        // Decrypt the symmetric key sent by Microsoft Graph
        const symmetricKey = certHelper.decryptSymmetricKey(
            notification.encryptedContent.dataKey,
            process.env.PRIVATE_KEY_PATH
        );

        // Validate the signature on the encrypted content
        const isSignatureValid = certHelper.verifySignature(
            notification.encryptedContent.dataSignature,
            notification.encryptedContent.data,
            symmetricKey
        );

        if (isSignatureValid) {
            // Decrypt the payload
            const decryptedPayload = certHelper.decryptPayload(
                notification.encryptedContent.data,
                symmetricKey
            );

            // Send the notification
            emitNotification(notification.subscriptionId, {
                resource: JSON.parse(decryptedPayload)
            });
        }
    }
}

exports.DecryptionHelper = DecryptionHelper;