function Conversation(Id) { 
    this.Id = Id  || null;
}

Conversation.prototype.getId = function() {
    return this.Id;
}

Conversation.prototype.setId = function(Id) {
    this.Id = Id;
}

Conversation.prototype.equals = function(otherConversation) {
    return otherConversation.getId() == this.getId();
}

Conversation.prototype.fill = function(newFields) {
    for (var field in newFields) {
        if (this.hasOwnProperty(field) && newFields.hasOwnProperty(field)) {
            if (this[field] !== 'undefined') {
                this[field] = newFields[field];
            }
        }
    }
};

module.exports = Conversation; 