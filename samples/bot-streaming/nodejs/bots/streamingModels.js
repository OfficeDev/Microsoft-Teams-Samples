// streamingModels.js

// StreamType object defines the different types of streaming stages
const StreamType = {
    Informative: 'Informative',  // Initial informative stream (e.g., "getting the information")
    Streaming: 'Streaming',      // Ongoing stream (e.g., content being progressively sent)
    Final: 'Final',              // Final stream (the end of the stream with complete content)
};

// ChannelData class stores the metadata for each streaming session
class ChannelData {
    constructor({ streamId, streamType, streamSequence }) {
        this.streamId = streamId;       // Unique identifier for the streaming session
        this.streamType = streamType;   // Type of stream (Informative, Streaming, Final)
        this.streamSequence = streamSequence; // Sequence number to track the order of chunks
    }
}

// Exporting the StreamType and ChannelData to be used in other files
module.exports = {
    StreamType,
    ChannelData,
};
