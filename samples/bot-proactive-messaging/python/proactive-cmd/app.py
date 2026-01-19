# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

"""
Proactive Command-Line Tool

This tool sends proactive messages to Microsoft Teams using conversation coordinates.
It demonstrates throttling and retry policies for reliable message delivery.

Commands:
  sendUserMessage      - Send a proactive message to a user (1-on-1 conversation)
  sendChannelThread    - Send a proactive message to a channel thread

Usage:
  python app.py sendUserMessage --app-id="<bot-id>" --app-password="<password>" ...
  python app.py sendChannelThread --app-id="<bot-id>" --app-password="<password>" ...
"""

import asyncio
import argparse
import sys
from message_sender import MessageSender


def create_parser():
    """Create the command-line argument parser."""
    parser = argparse.ArgumentParser(
        description="Send proactive messages to Microsoft Teams",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  # Send message to a user
  python app.py sendUserMessage \\
    --app-id="12345678-1234-1234-1234-123456789012" \\
    --app-password="your-password" \\
    --tenant-id="87654321-4321-4321-4321-210987654321" \\
    --service-url="https://smba.trafficmanager.net/amer/" \\
    --conversation-id="a:1AbCdEfGhIjKlMnOpQrStUvWxYz" \\
    --message="Hello from proactive message!"

  # Send message to a channel thread
  python app.py sendChannelThread \\
    --app-id="12345678-1234-1234-1234-123456789012" \\
    --app-password="your-password" \\
    --tenant-id="87654321-4321-4321-4321-210987654321" \\
    --service-url="https://smba.trafficmanager.net/amer/" \\
    --conversation-id="19:channel-id@thread.tacv2" \\
    --message="Hello from channel proactive message!"
        """
    )
    
    subparsers = parser.add_subparsers(dest='command', help='Command to execute')
    
    # Common arguments for both commands
    def add_common_args(subparser):
        """Add common arguments to a subparser."""
        subparser.add_argument('--app-id', required=True, help='Bot Application (Client) ID')
        subparser.add_argument('--app-password', required=True, help='Bot Application Secret')
        subparser.add_argument('--tenant-id', required=True, help='Tenant ID')
        subparser.add_argument('--service-url', required=True, help='Service URL from coordinate logger')
        subparser.add_argument('--conversation-id', required=True, help='Conversation ID from coordinate logger')
        subparser.add_argument('--message', required=True, help='Message to send')
        subparser.add_argument('--max-retries', type=int, default=3, help='Maximum retry attempts (default: 3)')
        subparser.add_argument('--retry-delay', type=float, default=1.0, help='Initial retry delay in seconds (default: 1.0)')
    
    # sendUserMessage command
    user_parser = subparsers.add_parser(
        'sendUserMessage',
        help='Send a proactive message to a user (1-on-1 conversation)'
    )
    add_common_args(user_parser)
    
    # sendChannelThread command
    channel_parser = subparsers.add_parser(
        'sendChannelThread',
        help='Send a proactive message to a channel thread'
    )
    add_common_args(channel_parser)
    
    return parser


async def send_user_message(args):
    """Send a proactive message to a user."""
    sender = MessageSender(
        app_id=args.app_id,
        app_password=args.app_password,
        tenant_id=args.tenant_id
    )
    
    try:
        result = await sender.send_to_conversation(
            service_url=args.service_url,
            conversation_id=args.conversation_id,
            message=args.message,
            max_retries=args.max_retries,
            retry_delay=args.retry_delay
        )
        
        if result:
            print("Message sent successfully!")
            return 0
        else:
            print("Failed to send message")
            return 1
            
    except Exception as e:
        print(f"Error: {e}")
        return 1
    finally:
        await sender.cleanup()


async def send_channel_thread(args):
    """Send a proactive message to a channel thread."""
    sender = MessageSender(
        app_id=args.app_id,
        app_password=args.app_password,
        tenant_id=args.tenant_id
    )
    
    try:
        result = await sender.send_to_conversation(
            service_url=args.service_url,
            conversation_id=args.conversation_id,
            message=args.message,
            max_retries=args.max_retries,
            retry_delay=args.retry_delay
        )
        
        if result:
            print("Message sent successfully!")
            return 0
        else:
            print("Failed to send message")
            return 1
            
    except Exception as e:
        print(f"Error: {e}")
        return 1
    finally:
        await sender.cleanup()


async def main():
    """Main entry point."""
    parser = create_parser()
    args = parser.parse_args()
    
    if not args.command:
        parser.print_help()
        return 1
    
    if args.command == 'sendUserMessage':
        return await send_user_message(args)
    elif args.command == 'sendChannelThread':
        return await send_channel_thread(args)
    else:
        print(f"Unknown command: {args.command}")
        parser.print_help()
        return 1


if __name__ == "__main__":
    exit_code = asyncio.run(main())
    sys.exit(exit_code)
