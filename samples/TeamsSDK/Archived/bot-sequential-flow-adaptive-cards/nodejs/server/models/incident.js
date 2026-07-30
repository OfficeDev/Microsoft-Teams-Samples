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

Incident.prototype.setStatus = function(status) {
    this.status = status || 'Created';
};

Incident.prototype.fill = function(newFields) {
    for (var field in newFields) {
        if (Object.prototype.hasOwnProperty.call(this, field) && Object.prototype.hasOwnProperty.call(newFields, field)) {
            if (this[field] !== 'undefined') {
                this[field] = newFields[field];
            }
        }
    }
};

module.exports = Incident;
