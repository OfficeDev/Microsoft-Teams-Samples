/**
 * Teams Helper Utilities for Request Approval Bot
 * Provides Teams-specific functionality for the Teams AI Library SDK V2
 */

class TeamsHelper {
    /**
     * Get conversation members using Teams AI Library SDK V2 patterns
     */
    static async getConversationMembers(context) {
        try {
            console.log('Getting conversation members from Teams API...');
            
            // Try to get real members from the Teams conversation
            if (context.api?.conversations) {
                console.log('Using Teams API to get conversation members');
                const members = await context.api.conversations.members(context.activity.conversation.id).get();
                console.log(`Found ${members.length} real conversation members`);
                
                const formattedMembers = members.map(member => ({
                    id: member.id,
                    aadObjectId: member.aadObjectId || member.id,
                    name: member.name || member.givenName || 'Unknown User',
                    email: member.email || member.userPrincipalName || ''
                }));
                
                return formattedMembers;
            }
            
            // Try alternative approach
            if (context.turnContext?.adapter) {
                console.log('Trying turnContext adapter approach...');
                const members = await context.turnContext.adapter.getConversationMembers(context.turnContext, context.activity.conversation.id);
                
                return members.map(member => ({
                    id: member.id,
                    aadObjectId: member.aadObjectId || member.id,
                    name: member.name || member.givenName || 'Unknown User',
                    email: member.email || member.userPrincipalName || ''
                }));
            }
            
            throw new Error('No API methods available');
            
        } catch (error) {
            console.warn('Error getting conversation members from API:', error.message);
            console.log('Falling back to mock members for development');
            return TeamsHelper.createMockMembers(context);
        }
    }
    
    /**
     * Create mock members for development and testing
     */
    static createMockMembers(context) {
        const currentUser = context.activity.from;
        
        const members = [
            {
                id: currentUser.id,
                aadObjectId: currentUser.aadObjectId || currentUser.id,
                name: currentUser.name || 'Current User',
                email: currentUser.email || 'current@example.com'
            }
        ];
        
        // Add mock team members for testing approval flows
        const mockUsers = [
            { id: 'manager-1', name: 'Alice Manager', email: 'alice@company.com' },
            { id: 'developer-1', name: 'Bob Developer', email: 'bob@company.com' },
            { id: 'designer-1', name: 'Carol Designer', email: 'carol@company.com' },
            { id: 'analyst-1', name: 'David Analyst', email: 'david@company.com' }
        ];
        
        mockUsers.forEach(user => {
            if (user.id !== currentUser.id && user.name !== currentUser.name) {
                members.push({
                    id: user.id,
                    aadObjectId: `${user.id}-aad`,
                    name: user.name,
                    email: user.email
                });
            }
        });
        
        console.log(`Created ${members.length} mock members for testing`);
        return members;
    }
    
    /**
     * Create assignee options for adaptive card choice sets
     */
    static createAssigneeOptions(members, excludeUserId) {
        if (!members || !Array.isArray(members)) {
            console.warn('Invalid members array provided to createAssigneeOptions');
            return [];
        }
        
        const options = members
            .filter(m => m && m.id !== excludeUserId)
            .map(m => ({
                title: m.name || m.givenName || m.displayName || 'Unknown User',
                value: m.id
            }));
            
        console.log('Created assignee options:', options.length, 'options from', members.length, 'members');
        return options;
    }
    
    /**
     * Find a user by ID from the members list
     */
    static findUserById(members, userId) {
        return members.find(m => m.id === userId || m.aadObjectId === userId);
    }
    
    /**
     * Validate user permissions for actions
     */
    static canUserPerformAction(user, task, action) {
        const isCreator = (user.aadObjectId === task.createdBy.aadObjectId) || 
                         (user.id === task.createdBy.id) ||
                         (user.aadObjectId === task.createdBy.id) ||
                         (user.id === task.createdBy.aadObjectId);
                         
        const isAssignee = (user.aadObjectId === task.assignedTo.aadObjectId) || 
                          (user.id === task.assignedTo.id) ||
                          (user.aadObjectId === task.assignedTo.id) ||
                          (user.id === task.assignedTo.aadObjectId);
                         
        switch (action) {
            case 'edit':
            case 'cancel':
                return isCreator;
            case 'approve':
            case 'reject':
                return isAssignee;
            default:
                return false;
        }
    }
}

module.exports = { TeamsHelper };