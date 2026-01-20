// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace ChangeNotification.Helpers
{
    using ChangeNotification.Model;
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;

    public class DecryptionHelper
    {
        public static string GetDecryptedContent(Encryptedcontent encryptedContent, string certificateThumbprint)
        {
            byte[] decryptedSymmetricKey = null;
            using (X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                certStore.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certCollection = certStore.Certificates.Find(X509FindType.FindByThumbprint,certificateThumbprint,false);
               
                // Get the first cert with the thumbprint
                if (certCollection.Count > 0)
                {
                    X509Certificate2 cert = certCollection[0];

                    // Decrypt using OAEP padding.
                    using var privateKey = cert.GetRSAPrivateKey();
                    decryptedSymmetricKey = privateKey.Decrypt(Convert.FromBase64String(encryptedContent.dataKey), RSAEncryptionPadding.OaepSHA1);

                    // Use certificate
                    Console.WriteLine(cert.FriendlyName);

                }
                certStore.Close();
            }

            // Can now use decryptedSymmetricKey with the AES algorithm.
            byte[] encryptedPayload = Convert.FromBase64String(encryptedContent.data);
            byte[] expectedSignature = Convert.FromBase64String(encryptedContent.dataSignature);
            byte[] actualSignature;

            using (HMACSHA256 hmac = new HMACSHA256(decryptedSymmetricKey))
            {
                actualSignature = hmac.ComputeHash(encryptedPayload);
            }

            using Aes aesProvider = Aes.Create(); // Fix for SYSLIB0021: Use Aes.Create() instead of AesCryptoServiceProvider
            aesProvider.Key = decryptedSymmetricKey;
            aesProvider.Padding = PaddingMode.PKCS7;
            aesProvider.Mode = CipherMode.CBC;

            // Obtain the initialization vector from the symmetric key itself.
            int vectorSize = 16;
            byte[] iv = new byte[vectorSize];
            Array.Copy(decryptedSymmetricKey, iv, vectorSize);
            aesProvider.IV = iv;

            // Decrypt the resource data content.
            using var decryptor = aesProvider.CreateDecryptor();
            using MemoryStream msDecrypt = new MemoryStream(encryptedPayload);
            using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new StreamReader(csDecrypt);
            return srDecrypt.ReadToEnd();
        }
    }
}
