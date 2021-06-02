const Role = require('./userRole')

function UserInfo(AadObjectId, Name, Email, Role) {       // Accept Name and AadObjectId in the constructor
    this.AadObjectId  = AadObjectId  || null;
    this.Name = Name || null;
    this.Email  = Email  || null;
    this.Role  = Role  || null;
}

UserInfo.prototype.getAadObjectId = function() {
    return this.AadObjectId;
}

UserInfo.prototype.setAadObjectId = function(AadObjectId) {
    this.AadObjectId = AadObjectId;
}

UserInfo.prototype.getName = function() {
    return this.Name;
}

UserInfo.prototype.setName = function(Name) {
    this.Name = Name;
}

UserInfo.prototype.getEmail = function() {
    return this.Email;
}

UserInfo.prototype.setEmail = function(Email) {
    this.Email = Email;
}
UserInfo.prototype.getRole = function() {
    return this.Role;
}

UserInfo.prototype.setRole = function(Role) {
    this.Role = Role;
}

UserInfo.prototype.equals = function(otherUserInfo) {
    return otherUserInfo.getName() == this.getName()
        && otherUserInfo.getAadObjectId() == this.getAadObjectId()
        && otherUserInfo.getEmail() == this.getEmail()
        && otherUserInfo.getRole() == this.getRole();
}

UserInfo.prototype.fill = function(newFields) {
    for (var field in newFields) {
        if (this.hasOwnProperty(field) && newFields.hasOwnProperty(field)) {
            if (this[field] !== 'undefined') {
                this[field] = newFields[field];
            }
        }
    }
};

module.exports = UserInfo; 