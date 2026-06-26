"""
Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.
"""

import shutil
from pathlib import Path


def clean():
    base = Path("./")

    for e in base.rglob("**/*"):
        if (
            e.match("dist")
            or e.match("__pycache__")
            or e.match(".pytest_cache")
            or e.match("coverage")
        ):
            if e.is_dir():
                print(e)
                shutil.rmtree(e)
