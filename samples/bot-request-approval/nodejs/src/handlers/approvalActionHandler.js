const { CardTemplates } = require('../cards/cardTemplates');
const { TaskService } = require('../services/taskService');
const { TeamsHelper } = require('../services/teamsHelper');

/**
 * Handles all adaptive card actions for the request approval workflow
 * This class replaces the complex switch-case logic from the original Bot Framework sample
 * with a more organized, Teams AI Library SDK V2 approach
 */
class ApprovalActionHandler {
    constructor(storage) {
        this.taskService = new TaskService(storage);
    }

    /**
     * Main handler for adaptive card actions
     * Routes actions based on verb prefix to appropriate handlers
     */
    async handleAction(context, action) {
        // Handle different action formats in Teams AI Library SDK V2
        const verb = action?.verb || action?.data?.verb || '';
        const verbPrefix = verb ? verb.split('_')[0] : '';

        console.log(`Handling action: ${verb}, prefix: ${verbPrefix}, full action:`, JSON.stringify(action, null, 2));

        // Get conversation members for user validation and card refresh
        let members;
        try {
            members = await TeamsHelper.getConversationMembers(context);
            console.log('Successfully retrieved members:', members?.length || 0);
        } catch (error) {
            console.error('Failed to get conversation members:', error);
            throw new Error(`Failed to retrieve team members: ${error.message}`);
        }
        
        const currentUser = context.activity.from;
        console.log('Current user:', currentUser?.name || 'unknown');

        switch (verbPrefix) {
            case 'create':
                console.log('Processing create task action');
                return this.handleCreateTask(currentUser, members);

            case 'save':
                return await this.handleSaveTask(context, action, members);

            case 'edit':
                return this.handleEditTask(action, members);

            case 'update':
                return await this.handleUpdateTask(context, action, members);

            case 'approve':
                return await this.handleApproveTask(context, action, members);

            case 'reject':
                return await this.handleRejectTask(context, action, members);

            case 'cancel':
                if (verb === 'cancel_task') {
                    return await this.handleCancelTask(context, action, members);
                } else if (verb === 'cancel_edit') {
                    return this.handleCancelEdit(action, currentUser, members);
                }
                break;

            case 'refresh':
                return this.handleRefreshCard(action, currentUser, members);

            case 'back':
                return CardTemplates.getOptionsCard();

            default:
                // Default to options card for unknown actions
                return CardTemplates.getOptionsCard();
        }
    }

    /**
     * Handle create task action - shows the task creation form
     */
    handleCreateTask(user, members) {
        try {
            console.log('Creating task card for user:', user?.name || 'unknown');
            console.log('Available members:', members?.length || 0);
            
            if (!members || members.length === 0) {
                console.error('No members available for assignee options');
                throw new Error('No team members found');
            }
            
            const assigneeOptions = TeamsHelper.createAssigneeOptions(members, user.id);
            console.log('Assignee options created:', assigneeOptions.length);
            
            return CardTemplates.getCreateTaskCard(user, assigneeOptions);
        } catch (error) {
            console.error('Error in handleCreateTask:', error);
            throw error;
        }
    }

    /**
     * Handle save new task action - saves the task and shows saved card
     */
    async handleSaveTask(context, action, members) {
        try {
            // Add conversation ID to action data for storage
            action.conversationId = context.activity.conversation.id;
            
            const task = await this.taskService.saveTask(action, members);
            
            // Return the card immediately instead of using updateMessage
            // This provides faster response and avoids the grayed-out delay
            return CardTemplates.getTaskSavedCard(task, action.data.inc_created_by, members);
        } catch (error) {
            console.error('Error saving task:', error);
            throw error;
        }
    }

    /**
     * Handle edit task action - returns edit card (validation will be done by the card display logic)
     */
    handleEditTask(action, members) {
        const task = action.data.task;
        const assigneeOptions = TeamsHelper.createAssigneeOptions(members);
        return CardTemplates.getTaskEditCard(task, assigneeOptions);
    }

    /**
     * Handle update task action - saves changes and shows updated card
     */
    async handleUpdateTask(context, action, members) {
        try {
            const updatedTask = await this.taskService.updateTask(action, members);
            
            // Return the card immediately for faster response
            return CardTemplates.getTaskSavedCard(updatedTask, updatedTask.createdBy, members);
        } catch (error) {
            console.error('Error updating task:', error);
            throw error;
        }
    }

    /**
     * Handle approve task action - approves the task and shows final status
     */
    async handleApproveTask(context, action, members) {
        return this.handleTaskStatusChange(context, action, 'approve', 'Approved');
    }

    /**
     * Handle reject task action - rejects the task and shows final status
     */
    async handleRejectTask(context, action, members) {
        return this.handleTaskStatusChange(context, action, 'reject', 'Rejected');
    }

    /**
     * Common handler for task status changes (approve/reject)
     */
    async handleTaskStatusChange(context, action, actionType, newStatus) {
        try {
            const task = action.data.task;
            const currentUser = context.activity.from;
            
            // Enhanced validation - check both aadObjectId and Teams id for compatibility
            const isAssignee = (currentUser.aadObjectId === task.assignedTo.aadObjectId) || 
                              (currentUser.id === task.assignedTo.id) ||
                              (currentUser.aadObjectId === task.assignedTo.id) ||
                              (currentUser.id === task.assignedTo.aadObjectId);
            
            if (!isAssignee) {
                throw new Error(`Only ${task.assignedTo.name} can ${actionType} this task.`);
            }
            
            // Update the action status
            action.data.status = newStatus;
            const updatedTask = await this.taskService.updateTaskStatus(action);
            
            // Return the final status card immediately for faster response
            return CardTemplates.getFinalStatusCard(updatedTask);
        } catch (error) {
            console.error(`Error ${actionType}ing task:`, error);
            throw error;
        }
    }

    /**
     * Handle cancel task action - cancels the task and shows final status
     */
    async handleCancelTask(context, action, members) {
        try {
            const task = action.data.task;
            const currentUser = context.activity.from;
            
            // Enhanced validation - check both aadObjectId and Teams id for compatibility
            const isCreator = (currentUser.aadObjectId === task.createdBy.aadObjectId) || 
                             (currentUser.id === task.createdBy.id) ||
                             (currentUser.aadObjectId === task.createdBy.id) ||
                             (currentUser.id === task.createdBy.aadObjectId);
            
            if (!isCreator) {
                throw new Error(`Only ${task.createdBy.name} can cancel this task.`);
            }
            
            const cancelledTask = await this.taskService.cancelTask(action);
            
            // Return the final status card immediately for faster response
            return CardTemplates.getFinalStatusCard(cancelledTask);
        } catch (error) {
            console.error('Error cancelling task:', error);
            throw error;
        }
    }

    /**
     * Handle cancel edit action - returns to the saved task view
     */
    handleCancelEdit(action, user, members) {
        const task = action.data.task;
        return CardTemplates.getTaskSavedCard(task, user, members);
    }

    /**
     * Handle refresh card action - returns appropriate card based on task status and user role
     */
    handleRefreshCard(action, user, members) {
        const task = action.data.task;
        
        console.log('🔄 Refreshing card for user:', user?.name, 'Task status:', task.status);
        
        // If task is completed (approved/rejected/cancelled), show final status
        if (task.status !== 'Pending') {
            return CardTemplates.getFinalStatusCard(task);
        }

        // For pending tasks, use the unified getTaskSavedCard which handles role-based buttons
        return CardTemplates.getTaskSavedCard(task, user, members);
    }


}

module.exports = { ApprovalActionHandler };