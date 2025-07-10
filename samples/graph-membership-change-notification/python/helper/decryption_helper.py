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
            enc = notification[0].get('encryptedContent')
        except Exception:
            enc = None
        if not enc:
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
            symmetric_key = CertificateHelper.decrypt_symmetric_key(
                enc['dataKey'], os.getenv('PRIVATE_KEY_PATH')
            )
            if CertificateHelper.verify_signature(enc.get('dataSignature'), enc.get('data'), symmetric_key):
                decrypted_payload = CertificateHelper.decrypt_payload(enc.get('data'), symmetric_key)
                resource = json.loads(decrypted_payload)
                resource['changeType'] = notification[0].get('changeType')
                return resource
        except Exception:
            pass
        # If decryption failed or signature invalid, fallback
        item = notification[0]
        resource_data = item.get('resourceData', {})
        return {
            'createdDateTime': resource_data.get('createdDateTime'),
            'displayName': resource_data.get('displayName'),
            'changeType': item.get('changeType'),
            'userId': resource_data.get('userId'),
            'tenantId': resource_data.get('tenantId')
        }
