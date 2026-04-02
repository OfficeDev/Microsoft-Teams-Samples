import { Task } from '../models/task';
import { storeSave, storeFetch, storeUpdate, storeCheck } from './storageService';

const incKey = 'incidents_key';
let tasks: Task[] = [];

export const saveInc = async (action: any, members: any[]): Promise<Task> => {
    const assignedTo = members.find((m: any) => m.aadObjectId === action.data.inc_assigned_to);
    const { inc_title: title, inc_description: description, inc_created_by: createdBy } = action.data;

    if (storeCheck(incKey)) {
        tasks = storeFetch(incKey);
    }

    const newInc = new Task(null, title, description, createdBy, assignedTo, null, null);
    tasks.push(newInc);
    storeSave(incKey, tasks);
    return newInc;
};

export const updateInc = async (action: any, members: any[]): Promise<Task> => {
    const assignedTo = members.find((m: any) => m.aadObjectId === action.data.inc_assigned_to);
    const oldTask = action.data.task;
    const { inc_title: title, inc_description: description, inc_created_by: createdBy } = action.data;

    if (storeCheck(incKey)) {
        tasks = storeFetch(incKey);
    }

    const inc = new Task();
    inc.fill(oldTask);
    inc.fill({ title, description, createdBy, assignedTo });
    const index = tasks.findIndex((i: Task) => i.id === inc.id);
    tasks[index] = inc;
    storeUpdate(incKey, tasks);
    return inc;
};

export const updateStatusInc = async (action: any): Promise<Task> => {
    const oldTask = action.data.task;
    const status = action.data.status;

    if (storeCheck(incKey)) {
        tasks = storeFetch(incKey);
    }

    const inc = new Task();
    inc.fill(oldTask);
    inc.setStatus(status);
    const index = tasks.findIndex((i: Task) => i.id === inc.id);
    tasks[index] = inc;
    storeUpdate(incKey, tasks);
    return inc;
};

export const getAllInc = async (): Promise<Task[]> => {
    if (storeCheck(incKey)) {
        tasks = storeFetch(incKey);
    }
    return tasks;
};