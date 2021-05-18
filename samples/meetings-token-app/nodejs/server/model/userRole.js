const Conversation = require('./conversation');

function UserRole(MeetingRole, Conversation) {       // Accept Conversation and MeetingRole in the constructor
    this.Conversation = Conversation || null;
    this.MeetingRole  = MeetingRole  || null;
}

UserRole.prototype.getMeetingRole = function() {
    return this.MeetingRole;
}

UserRole.prototype.setMeetingRole = function(MeetingRole) {
    this.MeetingRole = MeetingRole;
}

UserRole.prototype.getConversation = function() {
    return this.Conversation;
}

UserRole.prototype.setConversation = function(Conversation) {
    this.Conversation = Conversation;
}

UserRole.prototype.equals = function(otherUserRole) {
    return otherUserRole.getConversation() == this.getConversation()
        && otherUserRole.getMeetingRole() == this.getMeetingRole();
}

UserRole.prototype.fill = function(newFields) {
    for (var field in newFields) {
        if (this.hasOwnProperty(field) && newFields.hasOwnProperty(field)) {
            if (this[field] !== 'undefined') {
                this[field] = newFields[field];
            }
        }
    }
};

module.exports = UserRole; 