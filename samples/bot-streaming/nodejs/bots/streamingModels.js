// streamingModels.js

const StreamType = {
    Informative: 'Informative',
    Streaming: 'Streaming',
    Final: 'Final',
};

class ChannelData {
    constructor({ streamId, streamType, streamSequence }) {
        this.streamId = streamId;
        this.streamType = streamType;
        this.streamSequence = streamSequence;
    }
}

module.exports = {
    StreamType,
    ChannelData,
};
