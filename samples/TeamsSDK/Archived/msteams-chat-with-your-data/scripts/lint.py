"""
Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.
"""

import subprocess


def lint():
    subprocess.run(["poetry", "run", "pylint", "src", "scripts", "tests"], check=True)
    subprocess.run(["poetry", "run", "mypy", "-p", "src"], check=True)
