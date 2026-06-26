"""
Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.
"""

import subprocess


def ci():
    subprocess.run(["poetry", "check"], check=True)
    subprocess.run(["poetry", "run", "lint"], check=True)
    subprocess.run(["poetry", "run", "test"], check=True)
    subprocess.run(["poetry", "build"], check=True)
