function MeetingSummary(MeetingMetadata, UserTokens) {       // Accept UserTokens and MeetingMetadata in the constructor
    this.MeetingMetadata = MeetingMetadata || null;
    this.UserTokens  = UserTokens  || null;
}

MeetingSummary.prototype.getMeetingMetadata = function() {
    return this.MeetingMetadata;
}

MeetingSummary.prototype.setMeetingMetadata = function(MeetingMetadata) {
    this.MeetingMetadata = MeetingMetadata;
}

MeetingSummary.prototype.getUserTokens = function() {
    return this.UserTokens;
}

MeetingSummary.prototype.setUserTokens = function(UserTokens) {
    this.UserTokens = UserTokens;
}

module.exports = MeetingSummary; 