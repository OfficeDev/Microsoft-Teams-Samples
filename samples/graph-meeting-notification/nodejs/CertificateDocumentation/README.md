# Create and self signed pem certificate

## Generate certificate using Azure key vault.

1. Open [Azure key vault](https://docs.microsoft.com/en-us/azure/key-vault/key-vault-whatis) and navigate to `Certificates` section.
2. Click on `Generate/Import` button to create a certificate. 

![Generate certificate](GenerateCertificate.png)

3. Fill the required details. Make sure `Method of Certificate Creation` is **Generate** and content type is **PEM**.

![Fill details](CreateCertificateForm.png)

4. Click on *Current version of certificate*.

![Certificate Operation](CertificateOperations.png)

5. It will open a menu, click on `Download in PFX/PEM format`

![Download certificate](PFXFormat.png)

## Install certificate.
Once the download is completed open the downloaded certificate in notepad. The downloaded certificate must be in .pem format.

The certificate will contain bot the base64 string and private key

1. Copy the content between `-----BEGIN CERTIFICATE-----` and `-----END CERTIFICATE-----`. This will be the value of `Base64EncodedCertificate` in appsettings.json file.

2. Copy the content from `-----BEGIN PRIVATE KEY-----` and `-----END PRIVATE KEY-----`. (Including the `BEGIN` and `END` strings). Inside helper folder of this project create a new .pem file and paste the copied key. The key must contain `-----BEGIN PRIVATE KEY-----` and `-----END PRIVATE KEY-----` strings.

3. The name of the file which stores the priavate key is the value of `PRIVATE_KEY_PATH`. The file must be present inside helper folder of this project.