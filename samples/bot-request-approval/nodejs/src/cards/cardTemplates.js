/**
 * Adaptive Card Templates for Request Approval Bot
 * Teams AI Library SDK V2 compatible templates
 */

class CardTemplates {
    /**
     * Create common card header
     */
    static getCardHeader() {
        return {
            type: 'TextBlock',
            size: 'Medium',
            weight: 'Bolder',
            text: 'Task Management'
        };
    }

    /**
     * Create field label
     */
    static getFieldLabel(text) {
        return {
            type: 'RichTextBlock',
            separator: true,
            inlines: [
                {
                    type: 'TextRun',
                    text: text,
                    weight: 'Bolder',
                    italic: true,
                    size: 'medium'
                }
            ]
        };
    }
    /**
     * Initial options card - shows when user invokes the bot
     */
    static getOptionsCard() {
        return {
            $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
            type: 'AdaptiveCard',
            version: '1.4',
            body: [
                this.getCardHeader(),
                {
                    type: 'RichTextBlock',
                    inlines: [
                        {
                            type: 'TextRun',
                            text: 'Choose an option'
                        }
                    ]
                }
            ],
            actions: [
                {
                    type: 'Action.Submit',
                    title: 'Create New Task',
                    data: {
                        verb: 'create_task',
                        option: 'Create New Task'
                    }
                }
            ]
        };
    }

    /**
     * Task creation card - allows user to input task details
     */
    static getCreateTaskCard(user, assigneeOptions) {
        // Validate inputs
        if (!user) {
            throw new Error('User information is required to create task card');
        }
        
        if (!assigneeOptions || assigneeOptions.length === 0) {
            throw new Error('At least one assignee option is required');
        }
        
        console.log('Creating task card for user:', user.name, 'with', assigneeOptions.length, 'assignee options');
        
        return {
            $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
            type: 'AdaptiveCard',
            version: '1.4',
            body: [
                this.getCardHeader(),
                this.getFieldLabel('Title'),
                {
                    type: 'Input.Text',
                    placeholder: 'Enter Title',
                    id: 'inc_title'
                },
                this.getFieldLabel('Description'),
                {
                    type: 'Input.Text',
                    placeholder: 'Enter Description',
                    id: 'inc_description'
                },
                this.getFieldLabel('Created By'),
                {
                    type: 'RichTextBlock',
                    id: 'inc_created_by',
                    inlines: [
                        {
                            type: 'TextRun',
                            text: user.name,
                            wrap: true
                        }
                    ]
                },
                this.getFieldLabel('Assigned To'),
                {
                    type: 'Input.ChoiceSet',
                    choices: assigneeOptions,
                    placeholder: 'Select Assignee',
                    id: 'inc_assigned_to'
                }
            ],
            actions: [
                {
                    type: 'Action.Submit',
                    title: 'Save',
                    data: {
                        verb: 'save_new_task',
                        info: 'save',
                        inc_created_by: user
                    }
                },
                {
                    type: 'Action.Submit',
                    title: 'Back',
                    data: {
                        verb: 'back_to_options',
                        info: 'Back'
                    }
                }
            ]
        };
    }

    /**
     * Task saved card - shows after task is created with ALL appropriate options for different users
     */
    static getTaskSavedCard(task, user, allMembers) {
        let actions = [];

        // For pending tasks, show all relevant buttons with clear labeling
        if (task.status === 'Pending') {
            // Always show assignee buttons if there's an assignee
            if (task.assignedTo) {
                actions.push(
                    {
                        type: 'Action.Submit',
                        title: `✅ Approve (${task.assignedTo.name})`,
                        data: {
                            verb: 'approve_task',
                            info: 'approve',
                            status: 'Approved',
                            task: task
                        }
                    },
                    {
                        type: 'Action.Submit',
                        title: `❌ Reject (${task.assignedTo.name})`,
                        data: {
                            verb: 'reject_task',
                            info: 'reject',
                            status: 'Rejected',
                            task: task
                        }
                    }
                );
            }
            
            // Always show creator buttons
            actions.push(
                {
                    type: 'Action.Submit',
                    title: `✏️ Edit (${task.createdBy.name})`,
                    data: {
                        verb: 'edit_task',
                        info: 'edit',
                        task: task
                    }
                },
                {
                    type: 'Action.Submit',
                    title: `🗑️ Cancel (${task.createdBy.name})`,
                    data: {
                        verb: 'cancel_task',
                        info: 'cancel',
                        status: 'Cancelled',
                        task: task
                    }
                }
            );
        }

        return {
            $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
            type: 'AdaptiveCard',
            version: '1.4',
            body: [
                {
                    type: 'TextBlock',
                    size: 'Medium',
                    weight: 'Bolder',
                    text: 'Task Management'
                },
                {
                    type: 'RichTextBlock',
                    separator: true,
                    inlines: [
                        {
                            type: 'TextRun',
                            text: 'Title',
                            weight: 'Bolder',
                            italic: true,
                            size: 'medium'
                        }
                    ]
                },
                {
                    type: 'RichTextBlock',
                    inlines: [
                        {
                            type: 'TextRun',
                            text: task.title
                        }
                    ]
                },
                {
                    type: 'RichTextBlock',
                    separator: true,
                    inlines: [
                        {
                            type: 'TextRun',
                            text: 'Description',
                            weight: 'Bolder',
                            italic: true,
                            size: 'medium'
                        }
                    ]
                },
                {
                    type: 'RichTextBlock',
                    inlines: [
                        {
                            type: 'TextRun',
                            text: task.description
                        }
                    ]
                },
                {
                    type: 'RichTextBlock',
                    inlines: [
                        {
                            type: 'TextRun',
                            text: 'Created By',
                            weight: 'Bolder',
                            italic: true,
                            size: 'medium'
                        }
                    ]
                },
                {
                    type: 'RichTextBlock',
                    inlines: [
                        {
                            type: 'TextRun',
                            text: task.createdBy.name,
                            wrap: true
                        }
                    ]
                },
                {
                    type: 'RichTextBlock',
                    inlines: [
                        {
                            type: 'TextRun',
                            text: 'Assigned To',
                            weight: 'Bolder',
                            italic: true,
                            size: 'medium'
                        }
                    ]
                },
                {
                    type: 'RichTextBlock',
                    inlines: [
                        {
                            type: 'TextRun',
                            text: task.assignedTo.name,
                            wrap: true
                        }
                    ]
                }
            ],
            actions: actions
        };
    }



    /**
     * Task edit card - allows creator to edit task details
     */
    static getTaskEditCard(task, assigneeOptions) {
        return {
            $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
            type: 'AdaptiveCard',
            version: '1.4',
            body: [
                {
                    type: 'TextBlock',
                    size: 'Medium',
                    weight: 'Bolder',
                    text: 'Task Management'
                },
                {
                    type: 'RichTextBlock',
                    separator: true,
                    inlines: [
                        {
                            type: 'TextRun',
                            text: 'Title',
                            weight: 'Bolder',
                            italic: true,
                            size: 'medium'
                        }
                    ]
                },
                {
                    type: 'Input.Text',
                    placeholder: 'Enter Title',
                    id: 'inc_title',
                    value: task.title
                },
                {
                    type: 'RichTextBlock',
                    separator: true,
                    inlines: [
                        {
                            type: 'TextRun',
                            text: 'Description',
                            weight: 'Bolder',
                            italic: true,
                            size: 'medium'
                        }
                    ]
                },
                {
                    type: 'Input.Text',
                    placeholder: 'Enter Description',
                    id: 'inc_description',
                    value: task.description
                },
                {
                    type: 'RichTextBlock',
                    inlines: [
                        {
                            type: 'TextRun',
                            text: 'Created By',
                            weight: 'Bolder',
                            italic: true,
                            size: 'medium'
                        }
                    ]
                },
                {
                    type: 'RichTextBlock',
                    inlines: [
                        {
                            type: 'TextRun',
                            text: task.createdBy.name,
                            wrap: true
                        }
                    ]
                },
                {
                    type: 'RichTextBlock',
                    inlines: [
                        {
                            type: 'TextRun',
                            text: 'Assigned To',
                            weight: 'Bolder',
                            italic: true,
                            size: 'medium'
                        }
                    ]
                },
                {
                    type: 'Input.ChoiceSet',
                    choices: assigneeOptions,
                    placeholder: 'Select Assignee',
                    id: 'inc_assigned_to',
                    value: task.assignedTo.id
                }
            ],
            actions: [
                {
                    type: 'Action.Submit',
                    title: 'Update',
                    data: {
                        verb: 'update_task',
                        info: 'update',
                        task: task
                    }
                },
                {
                    type: 'Action.Submit',
                    title: 'Cancel',
                    data: {
                        verb: 'cancel_edit',
                        info: 'cancel_edit',
                        task: task
                    }
                }
            ]
        };
    }

    /**
     * Final status card - shows after approval/rejection/cancellation
     */
    static getFinalStatusCard(task) {
        const getStatusColor = (status) => {
            switch (status) {
                case 'Approved':
                    return 'Good';
                case 'Rejected':
                    return 'Attention';
                case 'Cancelled':
                    return 'Warning';
                default:
                    return 'Accent';
            }
        };

        return {
            $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
            type: 'AdaptiveCard',
            version: '1.4',
            body: [
                {
                    type: 'TextBlock',
                    size: 'Medium',
                    weight: 'Bolder',
                    text: 'Task Management'
                },
                {
                    type: 'RichTextBlock',
                    separator: true,
                    inlines: [
                        {
                            type: 'TextRun',
                            text: 'Title',
                            weight: 'Bolder',
                            italic: true,
                            size: 'medium'
                        }
                    ]
                },
                {
                    type: 'RichTextBlock',
                    inlines: [
                        {
                            type: 'TextRun',
                            text: task.title
                        }
                    ]
                },
                {
                    type: 'RichTextBlock',
                    inlines: [
                        {
                            type: 'TextRun',
                            text: 'Description',
                            weight: 'Bolder',
                            italic: true,
                            size: 'medium'
                        }
                    ]
                },
                {
                    type: 'RichTextBlock',
                    inlines: [
                        {
                            type: 'TextRun',
                            text: task.description
                        }
                    ]
                },
                {
                    type: 'ColumnSet',
                    columns: [
                        {
                            type: 'Column',
                            width: 'stretch',
                            items: [
                                {
                                    type: 'RichTextBlock',
                                    inlines: [
                                        {
                                            type: 'TextRun',
                                            text: 'Created By',
                                            weight: 'Bolder',
                                            italic: true,
                                            size: 'medium'
                                        }
                                    ]
                                },
                                {
                                    type: 'RichTextBlock',
                                    inlines: [
                                        {
                                            type: 'TextRun',
                                            text: task.createdBy.name,
                                            size: 'Small'
                                        }
                                    ],
                                    spacing: 'Small'
                                }
                            ]
                        },
                        {
                            type: 'Column',
                            width: 'stretch',
                            items: [
                                {
                                    type: 'RichTextBlock',
                                    inlines: [
                                        {
                                            type: 'TextRun',
                                            text: 'Assigned To',
                                            weight: 'Bolder',
                                            italic: true,
                                            size: 'medium'
                                        }
                                    ]
                                },
                                {
                                    type: 'RichTextBlock',
                                    inlines: [
                                        {
                                            type: 'TextRun',
                                            text: task.assignedTo.name,
                                            size: 'Small'
                                        }
                                    ],
                                    spacing: 'Small'
                                }
                            ]
                        }
                    ]
                },
                {
                    type: 'ColumnSet',
                    columns: [
                        {
                            type: 'Column',
                            width: 'stretch',
                            items: [
                                {
                                    type: 'RichTextBlock',
                                    inlines: [
                                        {
                                            type: 'TextRun',
                                            text: 'Status',
                                            weight: 'Bolder',
                                            italic: true,
                                            size: 'medium'
                                        }
                                    ]
                                },
                                {
                                    type: 'RichTextBlock',
                                    inlines: [
                                        {
                                            type: 'TextRun',
                                            text: task.status,
                                            color: getStatusColor(task.status)
                                        }
                                    ]
                                }
                            ]
                        }
                    ]
                }
            ]
        };
    }
}

module.exports = { CardTemplates };