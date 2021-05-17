function UserToken(UserInfo, TokenNumber, Status) {       // Accept TokenNumber and UserInfo in the constructor
    this.TokenNumber = TokenNumber || null;
    this.UserInfo  = UserInfo  || null;
    this.Status  = Status  || null;
}

UserToken.prototype.getUserInfo = function() {
    return this.UserInfo;
}

UserToken.prototype.setUserInfo = function(UserInfo) {
    this.UserInfo = UserInfo;
}

UserToken.prototype.getTokenNumber = function() {
    return this.TokenNumber;
}

UserToken.prototype.setTokenNumber = function(TokenNumber) {
    this.TokenNumber = TokenNumber;
}

UserToken.prototype.getStatus = function() {
    return this.Status;
}

UserToken.prototype.setStatus = function(Status) {
    this.Status = Status;
}

UserToken.prototype.equals = function(otherUserToken) {
    return otherUserToken.getTokenNumber() == this.getTokenNumber()
        && otherUserToken.getUserInfo() == this.getUserInfo()
        && otherUserToken.getStatus() == this.getStatus();
}

UserToken.prototype.fill = function(newFields) {
    for (var field in newFields) {
        if (this.hasOwnProperty(field) && newFields.hasOwnProperty(field)) {
            if (this[field] !== 'undefined') {
                this[field] = newFields[field];
            }
        }
    }
};

module.exports = UserToken; 