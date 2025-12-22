from enum import Enum
from pydantic import BaseModel, Field
from typing import Optional

# Enum for StreamType
class StreamType(str, Enum):
    INFORMATIVE = "Informative"
    STREAMING = "Streaming"
    FINAL = "Final"

# ChannelData model using Pydantic
class ChannelData(BaseModel):
    streamId: Optional[str] = Field(default=None, alias="streamId")
    streamType: Optional[str] = Field(default=None, alias="streamType")
    streamSequence: Optional[int] = Field(default=None, alias="streamSequence")