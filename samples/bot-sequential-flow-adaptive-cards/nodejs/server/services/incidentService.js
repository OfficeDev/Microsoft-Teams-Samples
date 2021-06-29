const Incident = require('../models/incident');
const storageService = require('../services/storageService');
const incKey = 'incidents_key';
let incidents = [];

const saveInc = async (action, members) => {
    const assignedTo = members.find(m => m.aadObjectId === action.data.inc_assigned_to);
    const { inc_title: title, category, sub_category: subCategory, inc_created_by: createdBy } = action.data;
    if (storageService.storeCheck(incKey)) {
        incidents = storageService.storeFetch(incKey);
    }
    const newInc = new Incident(null, title, category, subCategory, createdBy, assignedTo, null, null);
    incidents.push(newInc);
    storageService.storeSave(incKey, incidents);
    return newInc;
};

const updateInc = async (action, members) => {
    const assignedTo = members.find(m => m.aadObjectId === action.data.inc_assigned_to);
    const oldInc = action.data.incident;
    const { inc_title: title, category, sub_category: subCategory, inc_created_by: createdBy } = action.data;
    if (storageService.storeCheck(incKey)) {
        incidents = storageService.storeFetch(incKey);
    }
    const inc = new Incident();
    inc.fill(oldInc);
    inc.fill({ title, category, subCategory, createdBy, assignedTo });
    const index = incidents.findIndex(i => i.id === inc.id);
    incidents[index] = inc;
    storageService.storeUpdate(incKey, incidents);
    return inc;
};

const updateStatusInc = async (action) => {
    const oldInc = action.data.incident;
    const status = action.data.status;
    if (storageService.storeCheck(incKey)) {
        incidents = storageService.storeFetch(incKey);
    }
    const inc = new Incident();
    inc.fill(oldInc);
    inc.setStatus(status);
    const index = incidents.findIndex(i => i.id === inc.id);
    incidents[index] = inc;
    storageService.storeUpdate(incKey, incidents);
    return inc;
};

const getAllInc = async () => {
    if (storageService.storeCheck(incKey)) {
        incidents = storageService.storeFetch(incKey);
    }
    return incidents;
};

module.exports = {
    saveInc,
    updateInc,
    updateStatusInc,
    getAllInc
};
