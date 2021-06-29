const { v4: uuidv4 } = require('uuid');
function Incident(id, title, category, subCategory, createdBy, assignedTo, createdAt, status) {
    this.id = id || uuidv4();
    this.title = title || null;
    this.category = category || null;
    this.subCategory = subCategory || null;
    this.createdBy = createdBy || null;
    this.assignedTo = assignedTo || null;
    this.createdAt = createdAt || new Date().toLocaleString();
    this.status = status || 'Created';
}

Incident.prototype.getId = function() {
    return this.id;
};

Incident.prototype.setId = function(id) {
    this.id = id || uuidv4();
};

Incident.prototype.getTitle = function() {
    return this.title;
};

Incident.prototype.setTitle = function(title) {
    this.title = title;
};

Incident.prototype.getCategory = function() {
    return this.category;
};

Incident.prototype.setCategory = function(category) {
    this.category = category;
};

Incident.prototype.getSubCategory = function() {
    return this.subCategory;
};

Incident.prototype.setSubCategory = function(subCategory) {
    this.subCategory = subCategory;
};

Incident.prototype.getCreatedBy = function() {
    return this.createdBy;
};

Incident.prototype.setCreatedBy = function(createdBy) {
    this.createdBy = createdBy;
};

Incident.prototype.getAssignedTo = function() {
    return this.assignedTo;
};

Incident.prototype.setAssignedTo = function(assignedTo) {
    this.assignedTo = assignedTo;
};

Incident.prototype.getCreatedAt = function() {
    return this.createdAt;
};

Incident.prototype.setCreatedAt = function(createdAt) {
    this.createdAt = createdAt || new Date().toLocaleString();
};

Incident.prototype.getStatus = function() {
    return this.status;
};

Incident.prototype.setStatus = function(status) {
    this.status = status || 'Created';
};

Incident.prototype.equals = function(otherIncident) {
    return otherIncident.getId() === this.getId() &&
        otherIncident.getTitle() === this.getTitle() &&
        otherIncident.getCategory() === this.getCategory() &&
        otherIncident.getSubCategory() === this.getSubCategory() &&
        otherIncident.getCreatedBy() === this.getCreatedBy() &&
        otherIncident.getAssignedTo() === this.getAssignedTo() &&
        otherIncident.getCreatedAt() === this.getCreatedAt() &&
        otherIncident.getStatus() === this.getStatus();
};

Incident.prototype.fill = function(newFields) {
    for (var field in newFields) {
        // eslint-disable-next-line no-prototype-builtins
        if (this.hasOwnProperty(field) && newFields.hasOwnProperty(field)) {
            if (this[field] !== 'undefined') {
                this[field] = newFields[field];
            }
        }
    }
};

Incident.prototype.status = {
    created: 'Created',
    approved: 'Approved',
    rejected: 'Rejected'
};

module.exports = Incident;
