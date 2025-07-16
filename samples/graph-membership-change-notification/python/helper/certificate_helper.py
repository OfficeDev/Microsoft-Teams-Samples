# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

import os
import base64
import hmac
import hashlib
from cryptography.hazmat.primitives import serialization, hashes
from cryptography.hazmat.primitives.asymmetric import rsa, padding
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.backends import default_backend

class CertificateHelper:
    @staticmethod
    def decrypt_symmetric_key(data_key: str, private_key_path: str) -> bytes:
        """
        Decrypt the symmetric key using RSA private key
        Matches Node.js: crypto.privateDecrypt(asymmetricKey, encryptedKey)
        """
        try:
            # Decode the base64 encoded data key
            encrypted_key = base64.b64decode(data_key)
            print(f"Encrypted key length: {len(encrypted_key)} bytes")
            
            # Load the private key from file
            with open(private_key_path, 'rb') as key_file:
                private_key = serialization.load_pem_private_key(
                    key_file.read(),
                    password=None,
                    backend=default_backend()
                )
            
            print(f"Private key size: {private_key.key_size} bits")
            
            # Node.js crypto.privateDecrypt uses PKCS1v15 padding by default
            # Try PKCS1v15 first (Node.js default)
            try:
                print("Trying PKCS1v15 padding (Node.js default)")
                decrypted_key = private_key.decrypt(encrypted_key, padding.PKCS1v15())
                print(f"Successfully decrypted with PKCS1v15 padding")
                print(f"Decrypted key length: {len(decrypted_key)} bytes")
                return decrypted_key
            except Exception as pkcs_error:
                print(f"PKCS1v15 failed: {pkcs_error}")
                
                # Try OAEP with SHA-1 as fallback
                try:
                    print("Trying OAEP with SHA-1 as fallback")
                    decrypted_key = private_key.decrypt(
                        encrypted_key,
                        padding.OAEP(
                            mgf=padding.MGF1(algorithm=hashes.SHA1()),
                            algorithm=hashes.SHA1(),
                            label=None
                        )
                    )
                    print(f"Successfully decrypted with OAEP SHA-1")
                    print(f"Decrypted key length: {len(decrypted_key)} bytes")
                    return decrypted_key
                except Exception as oaep_error:
                    print(f"OAEP SHA-1 also failed: {oaep_error}")
                    raise Exception(f"Both padding schemes failed. PKCS1v15: {pkcs_error}, OAEP: {oaep_error}")
            
        except Exception as e:
            print(f"Error decrypting symmetric key: {e}")
            raise

    @staticmethod
    def verify_signature(data_signature: str, signed_payload: str, symmetric_key: bytes) -> bool:
        """
        Verify HMAC SHA256 signature
        Matches Node.js: hmac.write(signedPayload, 'base64'); return encodedSignature === hmac.digest('base64');
        """
        try:
            print(f"Verifying signature with symmetric key length: {len(symmetric_key)} bytes")
            
            # Create HMAC SHA256 with the symmetric key
            hmac_calculator = hmac.new(symmetric_key, digestmod=hashlib.sha256)
            
            # Node.js: hmac.write(signedPayload, 'base64')
            # This means we need to decode the base64 signed payload first
            signed_payload_bytes = base64.b64decode(signed_payload)
            hmac_calculator.update(signed_payload_bytes)
            
            # Get the calculated signature in base64
            calculated_signature = base64.b64encode(hmac_calculator.digest()).decode('utf-8')
            
            print(f"Expected signature: {data_signature[:20]}...")
            print(f"Calculated signature: {calculated_signature[:20]}...")
            
            # Compare with the provided signature
            result = data_signature == calculated_signature
            print(f"Signature verification result: {result}")
            return result
            
        except Exception as e:
            print(f"Error verifying signature: {e}")
            return False

    @staticmethod
    def decrypt_payload(encrypted_payload: str, symmetric_key: bytes) -> str:
        """
        Decrypt payload using AES-256-CBC
        Matches Node.js implementation exactly:
        - Copy the initialization vector from the symmetric key (first 16 bytes)
        - Use AES-256-CBC with the full symmetric key and IV
        - Decrypt from base64 directly
        """
        try:
            print(f"Decrypting payload with symmetric key length: {len(symmetric_key)} bytes")
            
            # Node.js: const iv = Buffer.alloc(16, 0); symmetricKey.copy(iv, 0, 0, 16);
            # Copy the initialization vector from the symmetric key (first 16 bytes)
            iv = symmetric_key[:16]
            print(f"Using IV from symmetric key: {len(iv)} bytes")
            
            # Node.js: const decipher = crypto.createDecipheriv('aes-256-cbc', symmetricKey, iv);
            # Create AES-256-CBC cipher with the full symmetric key
            cipher = Cipher(
                algorithms.AES(symmetric_key),  # Use full symmetric key (should be 32 bytes for AES-256)
                modes.CBC(iv),
                backend=default_backend()
            )
            
            # Node.js: let decryptedPayload = decipher.update(encryptedPayload, 'base64', 'utf8');
            #          decryptedPayload += decipher.final('utf8');
            decryptor = cipher.decryptor()
            
            # Decode the base64 payload and decrypt
            encrypted_data = base64.b64decode(encrypted_payload)
            print(f"Encrypted data length: {len(encrypted_data)} bytes")
            
            # Decrypt the data
            padded_plaintext = decryptor.update(encrypted_data) + decryptor.finalize()
            print(f"Decrypted padded data length: {len(padded_plaintext)} bytes")
            
            # Remove PKCS7 padding
            if len(padded_plaintext) > 0:
                padding_length = padded_plaintext[-1]
                if padding_length <= len(padded_plaintext) and padding_length > 0:
                    plaintext = padded_plaintext[:-padding_length]
                else:
                    plaintext = padded_plaintext
                
                # Convert to UTF-8 string
                result = plaintext.decode('utf-8')
                print(f"Successfully decrypted payload, length: {len(result)} characters")
                return result
            else:
                raise Exception("Empty decrypted data")
            
        except Exception as e:
            print(f"Error decrypting payload: {e}")
            raise  # Re-raise the exception instead of returning original data

    @staticmethod
    def get_serialized_certificate(cert_path: str) -> str:
        """
        Get the certificate contents from the certificate file and remove markers
        Matches Node.js getSerializedCertificate function
        """
        try:
            with open(cert_path, 'r') as cert_file:
                cert_content = cert_file.read()
            
            # Remove the markers from the string, leaving just the certificate
            # This matches the Node.js regex: .replace(/(\r\n|\n|\r|-|BEGIN|END|CERTIFICATE|\s)/gm, '')
            import re
            clean_cert = re.sub(r'(\r\n|\n|\r|-|BEGIN|END|CERTIFICATE|\s)', '', cert_content)
            return clean_cert
            
        except Exception as e:
            print(f"Error reading certificate: {e}")
            return ""
