"""
Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.
"""

import subprocess


def fmt():
    subprocess.run(["poetry", "run", "black", "src", "scripts"], check=True)
    subprocess.run(["poetry", "run", "isort", "src", "scripts"], check=True)
