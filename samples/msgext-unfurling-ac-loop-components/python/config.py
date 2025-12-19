"""Configuration settings for the Teams bot."""
import os
from pathlib import Path

def load_env_file(file_path):
    """Load environment variables from a file."""
    if file_path.exists():
        with open(file_path, 'r', encoding='utf-8') as f:
            for line in f:
                line = line.strip()
                if line and not line.startswith('#') and '=' in line:
                    key, value = line.split('=', 1)
                    os.environ[key] = value

# Load environment variables from multiple sources
env_files = [
    Path(__file__).parent / '.env',
    Path(__file__).parent / '.localConfigs',
    Path(__file__).parent / 'env' / '.env.local'
]

for env_file in env_files:
    load_env_file(env_file)

class Config:
    """Bot configuration settings."""
    
    def __init__(self):
        self.BOT_ID = os.environ.get("BOT_ID", "")
        self.BOT_PASSWORD = os.environ.get("BOT_PASSWORD", "") or os.environ.get("SECRET_BOT_PASSWORD", "")
        self.PORT = int(os.environ.get("PORT", 3978))
        self.PORT = int(os.environ.get("PORT", 3978))

# Create a global config instance
config = Config()
