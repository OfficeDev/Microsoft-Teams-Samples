# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import json
import os
from helper.certificate_helper import CertificateHelper

class DecryptionHelper:
    @staticmethod
    def process_encrypted_notification(notification):
        # If no encryptedContent or decryption not supported, fallback to clear payload
        try:
            encrypted_content = notification[0].get('encryptedContent')
            if not encrypted_content:
                raise Exception("No encrypted content found")
        except Exception:
            # fallback to resourceData
            item = notification[0] if isinstance(notification, list) and notification else {}
            resource_data = item.get('resourceData', {})
            return {
                'createdDateTime': resource_data.get('createdDateTime'),
                'displayName': resource_data.get('displayName'),
                'changeType': item.get('changeType'),
                'userId': resource_data.get('userId'),
                'tenantId': resource_data.get('tenantId')
            }
        
        # Attempt decryption
        try:
            # Extract encrypted content fields
            data_key = encrypted_content.get('dataKey')
            data_signature = encrypted_content.get('dataSignature')
            encrypted_data = encrypted_content.get('data')
            
            print(f"Encrypted content fields - dataKey: {'present' if data_key else 'missing'}, "
                  f"dataSignature: {'present' if data_signature else 'missing'}, "
                  f"data: {'present' if encrypted_data else 'missing'}")
            
            if not all([data_key, data_signature, encrypted_data]):
                raise Exception("Missing required encrypted content fields")
            
            # Decrypt the symmetric key
            private_key_path = os.getenv('PRIVATE_KEY_PATH', 'helper/key.pem')
            print(f"Using private key path: {private_key_path}")
            
            if not os.path.isabs(private_key_path):
                private_key_path = os.path.join(os.path.dirname(__file__), '..', private_key_path)
            
            print(f"Absolute private key path: {private_key_path}")
            print(f"Private key file exists: {os.path.exists(private_key_path)}")
            
            try:
                symmetric_key = CertificateHelper.decrypt_symmetric_key(data_key, private_key_path)
                print(f"Symmetric key decrypted successfully, length: {len(symmetric_key)} bytes")
                
                # Verify signature
                print("Verifying signature...")
                if CertificateHelper.verify_signature(data_signature, encrypted_data, symmetric_key):
                    print("Signature verification successful")
                    # Decrypt the payload
                    print("Decrypting payload...")
                    decrypted_payload = CertificateHelper.decrypt_payload(encrypted_data, symmetric_key)
                    print(f"Payload decrypted successfully, length: {len(decrypted_payload)} characters")
                    resource = json.loads(decrypted_payload)
                    resource['changeType'] = notification[0].get('changeType')
                    return resource
                else:
                    print("Signature verification failed")
                    raise Exception("Signature verification failed")
                    
            except Exception as decrypt_error:
                print(f"Decryption/verification failed: {decrypt_error}")
                # If decryption fails due to certificate mismatch, 
                # check if we can extract data from resourceData instead
                raise decrypt_error
                
        except Exception as e:
            print(f"Decryption failed: {e}")
            print("Falling back to resourceData extraction...")
            
            # Enhanced fallback: try to extract more information from the notification
            notification_item = notification[0] if notification else {}
            resource_data = notification_item.get('resourceData', {})
            
            # Try to extract user information from various fields
            fallback_data = {
                'createdDateTime': resource_data.get('createdDateTime'),
                'displayName': resource_data.get('displayName'),
                'changeType': notification_item.get('changeType'),
                'userId': resource_data.get('userId'),
                'tenantId': resource_data.get('tenantId'),
                'id': resource_data.get('id'),
                'type': resource_data.get('@odata.type'),
                'resourceId': notification_item.get('resourceData', {}).get('id')
            }
            
            print(f"Extracted fallback data: {fallback_data}")
            return fallback_data

