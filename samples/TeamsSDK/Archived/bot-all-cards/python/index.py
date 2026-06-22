# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import asyncio
from app import app


async def main():
    await app.start()
    print("\nBot started successfully")


if __name__ == "__main__":
    asyncio.run(main())
