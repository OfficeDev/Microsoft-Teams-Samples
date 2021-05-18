function MeetingMetadata(MeetingId, CurrentToken, MaxTokenIssued) {       // Accept CurrentToken and MeetingId in the constructor
    this.CurrentToken = CurrentToken || 0;
    this.MeetingId  = MeetingId  || null;
    this.MaxTokenIssued  = MaxTokenIssued  || 0;
}

MeetingMetadata.prototype.getMeetingId = function() {
    return this.MeetingId;
}

MeetingMetadata.prototype.setMeetingId = function(MeetingId) {
    this.MeetingId = MeetingId;
}

MeetingMetadata.prototype.getCurrentToken = function() {
    return this.CurrentToken;
}

MeetingMetadata.prototype.setCurrentToken = function(CurrentToken) {
    this.CurrentToken = CurrentToken;
}

MeetingMetadata.prototype.getMaxTokenIssued = function() {
    return this.MaxTokenIssued;
}

MeetingMetadata.prototype.setMaxTokenIssued = function(MaxTokenIssued) {
    this.MaxTokenIssued = MaxTokenIssued;
}

MeetingMetadata.prototype.equals = function(otherMeetingMetadata) {
    return otherMeetingMetadata.getCurrentToken() == this.getCurrentToken()
        && otherMeetingMetadata.getMeetingId() == this.getMeetingId()
        && otherMeetingMetadata.getMaxTokenIssued() == this.getMaxTokenIssued();
}

MeetingMetadata.prototype.fill = function(newFields) {
    for (var field in newFields) {
        if (this.hasOwnProperty(field) && newFields.hasOwnProperty(field)) {
            if (this[field] !== 'undefined') {
                this[field] = newFields[field];
            }
        }
    }
};

module.exports = MeetingMetadata; 