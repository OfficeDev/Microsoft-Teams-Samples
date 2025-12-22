const { v4: uuidv4 } = require('uuid');

class TaskService {
    constructor(storage) {
        this.storage = storage;
    }

    /**
     * Save a new task request
     * @param {Object} action - The action data from the adaptive card
     * @param {Array} allMembers - All members in the conversation
     * @returns {Object} - The created task object
     */
    async saveTask(action, allMembers) {
        const taskId = uuidv4();
        const now = new Date();
        
        // Find the assigned person by ID
        const assignedTo = allMembers.find(m => m.id === action.data.inc_assigned_to || m.aadObjectId === action.data.inc_assigned_to);
        
        const task = {
            id: taskId,
            title: action.data.inc_title,
            description: action.data.inc_description,
            createdBy: action.data.inc_created_by,
            assignedTo: assignedTo,
            status: 'Pending',
            createdAt: now,
            updatedAt: now
        };

        // Store the task
        await this.storage.set(`task_${taskId}`, task);
        
        // Add to tasks list for the conversation
        const conversationId = action.conversationId || 'default';
        let tasks = await this.storage.get(`tasks_${conversationId}`) || [];
        tasks.push(taskId);
        await this.storage.set(`tasks_${conversationId}`, tasks);
        
        return task;
    }

    /**
     * Update an existing task
     * @param {Object} action - The action data from the adaptive card
     * @param {Array} allMembers - All members in the conversation
     * @returns {Object} - The updated task object
     */
    async updateTask(action, allMembers) {
        const task = action.data.task;
        
        // Find the assigned person by ID if changed
        let assignedTo = task.assignedTo;
        if (action.data.inc_assigned_to && action.data.inc_assigned_to !== task.assignedTo.id) {
            assignedTo = allMembers.find(m => m.id === action.data.inc_assigned_to || m.aadObjectId === action.data.inc_assigned_to);
        }
        
        const updatedTask = {
            ...task,
            title: action.data.inc_title || task.title,
            description: action.data.inc_description || task.description,
            assignedTo: assignedTo,
            updatedAt: new Date()
        };

        // Update storage
        await this.storage.set(`task_${task.id}`, updatedTask);
        
        return updatedTask;
    }

    /**
     * Update task status (approve/reject)
     * @param {Object} action - The action data from the adaptive card
     * @returns {Object} - The updated task object
     */
    async updateTaskStatus(action) {
        const task = action.data.task;
        const updatedTask = {
            ...task,
            status: action.data.status,
            updatedAt: new Date()
        };

        // Update storage
        await this.storage.set(`task_${task.id}`, updatedTask);
        
        return updatedTask;
    }

    /**
     * Get a task by ID
     * @param {string} taskId - The task ID
     * @returns {Object} - The task object
     */
    async getTask(taskId) {
        return await this.storage.get(`task_${taskId}`);
    }

    /**
     * Get all tasks for a conversation
     * @param {string} conversationId - The conversation ID
     * @returns {Array} - Array of task objects
     */
    async getAllTasks(conversationId) {
        const taskIds = await this.storage.get(`tasks_${conversationId}`) || [];
        const tasks = [];
        
        for (const taskId of taskIds) {
            const task = await this.getTask(taskId);
            if (task) {
                tasks.push(task);
            }
        }
        
        return tasks;
    }

    /**
     * Cancel a task request
     * @param {Object} action - The action data from the adaptive card
     * @returns {Object} - The cancelled task object
     */
    async cancelTask(action) {
        const task = action.data.task;
        const cancelledTask = {
            ...task,
            status: 'Cancelled',
            updatedAt: new Date()
        };

        // Update storage
        await this.storage.set(`task_${task.id}`, cancelledTask);
        
        return cancelledTask;
    }
}

module.exports = { TaskService };