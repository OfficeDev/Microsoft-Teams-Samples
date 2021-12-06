const { v4: uuidv4 } = require('uuid');
function Task(id, title, description, createdBy, assignedTo, createdAt, status) {
    this.id = id || uuidv4();
    this.title = title || null;
    this.description = description || null;
    this.createdBy = createdBy || null;
    this.assignedTo = assignedTo || null;
    this.createdAt = createdAt || new Date().toLocaleString();
    this.status = status || 'Created';
}

Task.prototype.getId = function() {
    return this.id;
};

Task.prototype.setId = function(id) {
    this.id = id || uuidv4();
};

Task.prototype.getTitle = function() {
    return this.title;
};

Task.prototype.setTitle = function(title) {
    this.title = title;
};

Task.prototype.getDescription = function() {
    return this.description;
};

Task.prototype.setDescription = function(description) {
    this.description = description;
};


Task.prototype.getCreatedBy = function() {
    return this.createdBy;
};

Task.prototype.setCreatedBy = function(createdBy) {
    this.createdBy = createdBy;
};

Task.prototype.getAssignedTo = function() {
    return this.assignedTo;
};

Task.prototype.setAssignedTo = function(assignedTo) {
    this.assignedTo = assignedTo;
};

Task.prototype.getCreatedAt = function() {
    return this.createdAt;
};

Task.prototype.setCreatedAt = function(createdAt) {
    this.createdAt = createdAt || new Date().toLocaleString();
};

Task.prototype.getStatus = function() {
    return this.status;
};

Task.prototype.setStatus = function(status) {
    this.status = status || 'Created';
};

Task.prototype.equals = function(otherTask) {
    return otherTask.getId() === this.getId() &&
    otherTask.getTitle() === this.getTitle() &&
    otherTask.getCreatedBy() === this.getCreatedBy() &&
    otherTask.getAssignedTo() === this.getAssignedTo() &&
    otherTask.getCreatedAt() === this.getCreatedAt() &&
    otherTask.getStatus() === this.getStatus();
};

Task.prototype.fill = function(newFields) {
    for (var field in newFields) {
        // eslint-disable-next-line no-prototype-builtins
        if (this.hasOwnProperty(field) && newFields.hasOwnProperty(field)) {
            if (this[field] !== 'undefined') {
                this[field] = newFields[field];
            }
        }
    }
};

Task.prototype.status = {
    created: 'Created',
    approved: 'Approved',
    rejected: 'Rejected'
};

module.exports = Task;
