# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import asyncio
from app import app

# Main entry point - starts the Teams bot application
async def main():
    # Start the Teams AI application server
    await app.start()
    print("\nBot started successfully")

# Run the bot when script is executed directly
if __name__ == "__main__":
    asyncio.run(main())
