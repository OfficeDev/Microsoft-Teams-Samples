const Task = require('../models/task');
const storageService = require('./storageService');
const incKey = 'incidents_key';
let tasks = [];

const saveInc = async (action, members) => {
    const assignedTo = members.find(m => m.aadObjectId === action.data.inc_assigned_to);
    const { inc_title: title, inc_description: description, inc_created_by: createdBy } = action.data;

    if (storageService.storeCheck(incKey)) {
        tasks = storageService.storeFetch(incKey);
    }

    const newInc = new Task(null, title, description, createdBy, assignedTo, null, null);
    tasks.push(newInc);
    storageService.storeSave(incKey, tasks);
    return newInc;
};

const updateInc = async (action, members) => {
    const assignedTo = members.find(m => m.aadObjectId === action.data.inc_assigned_to);
    const oldTask = action.data.task;
    const { inc_title: title, inc_description: description, inc_created_by: createdBy } = action.data;

    if (storageService.storeCheck(incKey)) {
        tasks = storageService.storeFetch(incKey);
    }

    const inc = new Task();
    inc.fill(oldTask);
    inc.fill({ title, description, createdBy, assignedTo });
    const index = tasks.findIndex(i => i.id === inc.id);
    tasks[index] = inc;
    storageService.storeUpdate(incKey, tasks);
    return inc;
};

const updateStatusInc = async (action) => {
    const oldTask = action.data.task;
    const status = action.data.status;

    if (storageService.storeCheck(incKey)) {
        tasks = storageService.storeFetch(incKey);
    }
    
    const inc = new Task();
    inc.fill(oldTask);
    inc.setStatus(status);
    const index = tasks.findIndex(i => i.id === inc.id);
    tasks[index] = inc;
    storageService.storeUpdate(incKey, tasks);
    return inc;
};

const getAllInc = async () => {
    if (storageService.storeCheck(incKey)) {
        tasks = storageService.storeFetch(incKey);
    }
    return tasks;
};

module.exports = {
    saveInc,
    updateInc,
    updateStatusInc,
    getAllInc
};