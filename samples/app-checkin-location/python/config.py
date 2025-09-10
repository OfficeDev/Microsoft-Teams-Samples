#!/usr/bin/env python3
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os

""" Bot Configuration """


class DefaultConfig:
    """ Bot Configuration """

    PORT = 3978
    APP_TYPE = os.environ.get("MicrosoftAppType", "SingleTenant")
    APP_ID = os.environ.get("MicrosoftAppId", "")
    APP_PASSWORD = os.environ.get("MicrosoftAppPassword", "")
    APP_TENANT_ID = os.environ.get("MicrosoftAppTenantId", "")

    # Properties used by the Bot Framework
    @property
    def MicrosoftAppType(self):
        return self.APP_TYPE

    @property
    def MicrosoftAppId(self):
        return self.APP_ID
        
    @property
    def MicrosoftAppPassword(self):
        return self.APP_PASSWORD
        
    @property
    def MicrosoftAppTenantId(self):
        return self.APP_TENANT_ID
