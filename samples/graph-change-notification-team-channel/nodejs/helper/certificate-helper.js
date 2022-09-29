// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

const fs = require('fs');
const path = require('path');
const os = require('os');
const crypto = require('crypto');
const pem = require('pem');

/**
 * Configures path to openssl.exe if needed
 */
function ensureOpenSsl() {
  if (os.platform() === 'win32') {
    const pathOpenSSL = process.env.OPENSSL_CONF
      ? process.env.OPENSSL_CONF.replace('.cfg', '.exe')
      : 'C:/Program Files/OpenSSL-Win64/bin/openssl.exe';
    pem.config({ pathOpenSSL: pathOpenSSL });
  }
}

/**
 * @param  {string} keyPath - The relative path to the file containing the private key
 * @returns {string} Contents of the private key file
 */
function getPrivateKey(keyPath) {
  const key = fs.readFileSync(path.join(__dirname, keyPath), 'utf8');
  return key;
}

module.exports = {
  /**
   * Creates a self-signed certificate (public/private key pair) if none exists
   * @param  {string} certPath - The relative path to the public key file
   * @param  {string} keyPath - The relative path to the private key file
   * @param  {string} password - The password to use to protect the private key
   */
  createSelfSignedCertificateIfNotExists: async (
    certPath,
    keyPath,
    password
  ) => {
    const certFullPath = path.join(__dirname, certPath);
    return new Promise((resolve, reject) => {
      if (!fs.existsSync(certFullPath)) {
        ensureOpenSsl();
        pem.createCertificate(
          {
            selfSigned: true,
            serviceKeyPassword: password,
            days: 365,
          },
          (err, result) => {
            if (err) {
              console.error(err);
              reject(err.message);
            } else {
              fs.writeFileSync(certFullPath, result.certificate);
              fs.writeFileSync(
                path.join(__dirname, keyPath),
                result.serviceKey
              );
              resolve(true);
            }
          }
        );
      } else {
        resolve(true);
      }
    });
  },
  /**
   * Gets the certificate contents from the certificate file
   * @param  {string} certPath = The relative path to the certificate
   * @returns {string} The contents of the certificate
   */
  getSerializedCertificate: (certPath) => {
    const cert = fs.readFileSync(path.join(__dirname, certPath));
    // Remove the markers from the string, leaving just the certificate
    return cert
      .toString()
      .replace(/(\r\n|\n|\r|-|BEGIN|END|CERTIFICATE|\s)/gm, '');
  },
  /**
   * Decrypts the encrypted symmetric key sent by Microsoft Graph
   * @param  {string} encodedKey - A base64 string containing an encrypted symmetric key
   * @param  {string} keyPath - The relative path to the private key file to decrypt with
   * @returns {Buffer} The decrypted symmetric key
   */
  decryptSymmetricKey: (encodedKey, keyPath) => {
    const asymmetricKey = getPrivateKey(keyPath);
    const encryptedKey = Buffer.from(encodedKey, 'base64');
    const decryptedSymmetricKey = crypto.privateDecrypt(
      asymmetricKey,
      encryptedKey
    );
    return decryptedSymmetricKey;
    },
  /**
   * Decrypts the payload data using the one-time use symmetric key
   * @param  {string} encryptedPayload - The base64-encoded encrypted payload
   * @param  {Buffer} symmetricKey - The one-time use symmetric key sent by Microsoft Graph
   * @returns {string} - The decrypted payload
   */
  decryptPayload: (encryptedPayload, symmetricKey) => {
    // Copy the initialization vector from the symmetric key
    const iv = Buffer.alloc(16, 0);
    symmetricKey.copy(iv, 0, 0, 16);

    // Create a decipher object
    const decipher = crypto.createDecipheriv('aes-256-cbc', symmetricKey, iv);

    // Decrypt the payload
    let decryptedPayload = decipher.update(encryptedPayload, 'base64', 'utf8');
    decryptedPayload += decipher.final('utf8');
    return decryptedPayload;
  },
  /**
   * @param  {string} encodedSignature - The base64-encoded signature
   * @param  {string} signedPayload - The base64-encoded signed payload
   * @param  {Buffer} symmetricKey - The one-time use symmetric key
   * @returns {boolean} - True if signature is valid, false if invalid
   */
  verifySignature: (encodedSignature, signedPayload, symmetricKey) => {
    const hmac = crypto.createHmac('sha256', symmetricKey);
    hmac.write(signedPayload, 'base64');
    return encodedSignature === hmac.digest('base64');
  },
};