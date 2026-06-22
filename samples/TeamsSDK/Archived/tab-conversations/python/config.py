#!/usr/bin/env python3
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os

""" App Configuration """


class DefaultConfig:
    """ App Configuration """

    PORT = int(os.environ.get("PORT", 3978))