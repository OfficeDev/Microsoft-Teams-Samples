const { StatusCodes, MessageFactory, CardFactory } = require('botbuilder');
const { MicrosoftAppCredentials, ConnectorClient } = require('botframework-connector');
const taskService = require('../services/taskService');

let allMembers = [];
let otherMembers = [];

const invokeResponse = (card) => {
    const cardRes = {
        statusCode: StatusCodes.OK,
        type: 'application/vnd.microsoft.card.adaptive',
        value: card
    };
    const res = {
        status: StatusCodes.OK,
        body: cardRes
    };
    return res;
};

const invokeTaskResponse = (title, card) => {
    const task = {
        status: StatusCodes.OK,
        body: {
            task: {
                type: 'continue',
                value: {
                    card: card,
                    heigth: 250,
                    width: 400,
                    title: title
                }
            }
        }
    };
    return task;
};

const selectResponseCard = async (context, user, members) => {
    allMembers = members;
    otherMembers = allMembers.filter(tm => tm.aadObjectId !== user.aadObjectId);
    const action = context.activity.value.action;
    const verb = action.verb;
    const prefix = verb.split('_')[0];
    if (verb && prefix === 'create') {
        return await createTask(verb, user);
    } else if (verb && prefix === 'save') {
        const card = await saveInc(action);
        await updateCardInTeams(context, card);
        return card;
    } else if (verb && prefix === 'cancel') {
        return cancelInc(action);
    } else if (verb && prefix === 'edit') {
        return editInc(action);
    } else if (verb && prefix === 'update') {
        const card = await updateInc(action);
        await updateCardInTeams(context, card);
        return card;
    } else if (verb && prefix === 'view') {
        const card = await viewInc(action);
        await updateCardInTeams(context, card);
        return card;
    } else if (verb && prefix === 'status') {
        const card = await updateStatusInc(action);
        await updateCardInTeams(context, card);
        return card;
    } else if (verb && prefix === 'refresh') {
        return await refreshInc(user, action);
    } else if (verb && prefix === 'cancelledreq') {
        const card = await requestCancelledInc(user, action);
        await updateCardInTeams(context, card);
        return card;
    } else if (verb && prefix === 'list') {
        return await viewAllInc();
    } else {
        return optionInc();
    }
};

const optionInc = () => ({
    $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
    appId: process.env.MicrosoftAppId,
    body: [
        {
            type: 'TextBlock',
            size: 'Medium',
            weight: 'Bolder',
            text: 'Task Management'
        },
        {
            type: 'RichTextBlock',
            inlines: [
                {
                    type: 'TextRun',
                    text: 'Choose a Option'
                }
            ]
        },
        {
            type: 'ActionSet',
            actions: [
                {
                    type: 'Action.Execute',
                    verb: 'create_task',
                    title: 'Create New Task',
                    data: {
                        option: 'Create New Task'
                    }
                }
            ]
        }
    ],
    type: 'AdaptiveCard',
    version: '1.4'
});

const createTask = async (verb, user) => {
    const assignees = otherMembers.map(m => ({
        title: m.name,
        value: m.aadObjectId
    }));
    const card = {
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        appId: process.env.MicrosoftAppId,
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
                id: 'inc_title'
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
                type: 'Input.Text',
                placeholder: 'Enter Description',
                id: 'inc_description'
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
                id: 'inc_created_by',
                inlines: [
                    {
                        type: 'TextRun',
                        text: user.name,
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
                choices: assignees,
                placeholder: 'Select Assignee',
                id: 'inc_assigned_to'
            }
        ],
        actions: [
            {
                type: 'Action.Execute',
                verb: 'save_new_inc',
                title: 'Save',
                data: {
                    info: 'save',
                    inc_created_by: user
                }
            },
            {
                type: 'Action.Execute',
                verb: `other`,
                title: 'Back',
                data: {
                    info: 'Back'
                }
            }
        ],
        type: 'AdaptiveCard',
        version: '1.4'
    };
    return card;
};

const saveInc = async (action) => {
    const inc = await taskService.saveInc(action, allMembers);
    const card = {
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        appId: process.env.MicrosoftAppId,
        refresh: {
            action: {
                type: 'Action.Execute',
                title: 'Refresh',
                verb: 'refresh_edit_status',
                data: {
                    info: 'refresh',
                    task: inc
                }
            },
            userIds: allMembers.map(m => m.id)
        },
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
                id: 'inc_title',
                type: 'RichTextBlock',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.title
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
                id: 'inc_description',
                type: 'RichTextBlock',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.description
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
                id: 'inc_created_by',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.createdBy.name,
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
                id: 'inc_assigned_to',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.assignedTo.name,
                        wrap: true
                    }
                ]
            }
        ],
        type: 'AdaptiveCard',
        version: '1.4'
    };
    return card;
};

const cancelInc = async (action) => {
    const inc = action.data.task;
    const card = {
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        appId: process.env.MicrosoftAppId,
        refresh: {
            action: {
                type: 'Action.Execute',
                title: 'Refresh',
                verb: 'cancelledreq_edit_status',
                data: {
                    info: 'refresh',
                    status: 'Cancelled',
                    task: inc
                }
            },
            userIds: allMembers.map(m => m.id)
        },
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
                id: 'inc_title',
                type: 'RichTextBlock',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.title
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
                id: 'inc_description',
                type: 'RichTextBlock',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.description
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
                id: 'inc_created_by',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.createdBy.name,
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
                id: 'inc_assigned_to',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.assignedTo.name,
                        wrap: true
                    }
                ]
            }
        ],
        type: 'AdaptiveCard',
        version: '1.4'
    };
    return card;
};

const editInc = async (action) => {
    const inc = action.data.task;
    const assignees = otherMembers.map(m => ({
        title: m.name,
        value: m.aadObjectId
    }));
    const assignee = assignees.find(m => m.value === inc.assignedTo.aadObjectId);
    const card = {
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        appId: process.env.MicrosoftAppId,
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
                value: inc.title
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
                placeholder: 'Enter Title',
                id: 'inc_description',
                value: inc.description
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
                id: 'inc_created_by',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.createdBy.name,
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
                choices: assignees,
                placeholder: 'Select Assignee',
                id: 'inc_assigned_to',
                value: assignee.value
            }
        ],
        actions: [
            {
                type: 'Action.Execute',
                verb: 'update_inc',
                title: 'Update',
                data: {
                    info: 'update',
                    inc_created_by: inc.createdBy,
                    task: inc
                }
            },
            {
                type: 'Action.Execute',
                verb: 'view_inc',
                title: 'Back',
                data: {
                    info: 'Back',
                    task: inc
                }
            }
        ],
        type: 'AdaptiveCard',
        version: '1.4'
    };
    return card;
};

const updateInc = async (action) => {
    const inc = await taskService.updateInc(action, allMembers);
    const card = {
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        appId: process.env.MicrosoftAppId,
        refresh: {
            action: {
                type: 'Action.Execute',
                title: 'Refresh',
                verb: 'refresh_edit_status',
                data: {
                    info: 'refresh',
                    task: inc
                }
            },
            userIds: allMembers.map(m => m.id)
        },
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
                id: 'inc_title',
                type: 'RichTextBlock',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.title
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
                id: 'inc_description',
                type: 'RichTextBlock',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.description
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
                id: 'inc_created_by',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.createdBy.name,
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
                id: 'inc_assigned_to',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.assignedTo.name,
                        wrap: true
                    }
                ]
            }
        ],
        type: 'AdaptiveCard',
        version: '1.4'
    };
    return card;
};

const viewInc = async (action) => {
    const inc = action.data.task;
    const card = {
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        appId: process.env.MicrosoftAppId,
        refresh: {
            action: {
                type: 'Action.Execute',
                title: 'Refresh',
                verb: 'refresh_edit_status',
                data: {
                    info: 'refresh',
                    task: inc
                }
            },
            userIds: allMembers.map(m => m.id)
        },
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
                id: 'inc_title',
                type: 'RichTextBlock',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.title
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
                id: 'inc_created_by',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.createdBy.name,
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
                id: 'inc_assigned_to',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.assignedTo.name,
                        wrap: true
                    }
                ]
            }
        ],
        type: 'AdaptiveCard',
        version: '1.4'
    };
    return card;
};

const updateStatusInc = async (action) => {
    const inc = await taskService.updateStatusInc(action);
    const card = {
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        appId: process.env.MicrosoftAppId,
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
                id: 'inc_title',
                type: 'RichTextBlock',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.title
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
                id: 'inc_description',
                type: 'RichTextBlock',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.description
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
                id: 'inc_created_by',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.createdBy.name,
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
                id: 'inc_assigned_to',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.assignedTo.name,
                        wrap: true
                    }
                ]
            },
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
                        text: inc.status,
                        color: inc.status === 'Approved' ? 'Good' : (inc.status === 'Rejected' ? 'Attention' : 'Accent')
                    }
                ]
            }
        ],
        type: 'AdaptiveCard',
        version: '1.4'
    };
    return card;
};

const updateCardInTeams = async (context, card) => {
    const serviceUrl = context.activity.serviceUrl;
    const credentials = new MicrosoftAppCredentials(process.env.MicrosoftAppId, process.env.MicrosoftAppPassword);
    const connectorClient = new ConnectorClient(credentials, { baseUri: serviceUrl });
    const conversationId = context.activity.conversation.id;
    const activityId = context.activity.replyToId;
    const replyActivity = MessageFactory.attachment(CardFactory.adaptiveCard(card));
    replyActivity.id = activityId;
    replyActivity.attachments = [CardFactory.adaptiveCard(card)];
    await connectorClient.conversations.updateActivity(conversationId, activityId, replyActivity);
};

const refreshInc = async (user, action) => {
    const inc = action.data.task;
    let actions = [];
    if (user.aadObjectId === inc.createdBy.aadObjectId) {
        actions = [
            {
                type: 'Action.Execute',
                verb: 'edit_inc',
                title: 'Edit',
                data: {
                    info: 'edit',
                    task: inc
                }
            },
            {
                type: 'Action.Execute',
                verb: 'cancelledreq',
                title: 'Cancel',
                data: {
                    info: 'cancel',
                    status: 'Cancelled',
                    task: inc
                }
            },

        ];
    }
    if (user.aadObjectId === inc.assignedTo.aadObjectId) {
        actions = [
            {
                type: 'Action.Execute',
                verb: 'status_approve_inc',
                title: 'Approve',
                data: {
                    info: 'approve',
                    status: 'Approved',
                    task: inc
                }
            },
            {
                type: 'Action.Execute',
                verb: 'status_reject_inc',
                title: 'Reject',
                data: {
                    info: 'reject',
                    status: 'Rejected',
                    task: inc
                }
            }
        ];
    }
    const card = {
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        appId: process.env.MicrosoftAppId,
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
                id: 'inc_title',
                type: 'RichTextBlock',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.title
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
                id: 'inc_description',
                type: 'RichTextBlock',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.description
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
                id: 'inc_created_by',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.createdBy.name,
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
                id: 'inc_assigned_to',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.assignedTo.name,
                        wrap: true
                    }
                ]
            }
        ],
        actions: actions,
        type: 'AdaptiveCard',
        version: '1.4'
    };
    return card;
};

const requestCancelledInc = async (user, action) => {
    const inc = await taskService.updateStatusInc(action);
    const card = {
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        appId: process.env.MicrosoftAppId,
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
                id: 'inc_title',
                type: 'RichTextBlock',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.title
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
                id: 'inc_description',
                type: 'RichTextBlock',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.description
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
                id: 'inc_created_by',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.createdBy.name,
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
                id: 'inc_assigned_to',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.assignedTo.name,
                        wrap: true
                    }
                ]
            },
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
                        text: inc.status,
                        color: inc.status === 'Approved' ? 'Good' : (inc.status === 'Rejected' ? 'Attention' : 'Accent')
                    }
                ]
            }
        ],
        type: 'AdaptiveCard',
        version: '1.4'
    };
    return card;
};

const viewAllInc = async () => {
    const tasks = await taskService.getAllInc();
    let taskCards = [];
    if (!tasks.length) {
        taskCards = [
            {
                type: 'RichTextBlock',
                inlines: [
                    {
                        type: 'TextRun',
                        text: 'No Tasks Available',
                        italic: true,
                        size: 'medium',
                        color: 'Attention'
                    }
                ]
            }
        ];
    } else {
        taskCards = tasks.map(inc => (
            {
                type: 'Container',
                separator: true,
                items: [
                    {
                        type: 'RichTextBlock',
                        inlines: [
                            {
                                type: 'TextRun',
                                text: inc.title,
                                weight: 'Bolder',
                                italic: true,
                                size: 'medium'
                            }
                        ],
                        separator: true
                    },
                    {
                        type: 'RichTextBlock',
                        inlines: [
                            {
                                type: 'TextRun',
                                text: inc.description,
                                weight: 'Bolder',
                                italic: true,
                                size: 'medium'
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
                                                text: inc.createdBy.name,
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
                                                text: inc.assignedTo.name,
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
                                                text: inc.status,
                                                color: inc.status === 'Approved' ? 'Good' : (inc.status === 'Rejected' ? 'Attention' : 'Accent')
                                            }
                                        ]
                                    }
                                ]
                            }
                        ]
                    }
                ]
            }
        ));
    }

    const card = {
        type: 'AdaptiveCard',
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        version: '1.4',
        body: [
            {
                type: 'TextBlock',
                size: 'Medium',
                weight: 'Bolder',
                text: 'Tasks List'
            },
            {
                type: 'Container',
                separator: true,
                items: taskCards
            }
        ],
        actions: [
            {
                type: 'Action.Execute',
                verb: 'option_inc',
                title: 'Back',
                data: {
                    info: 'Back'
                }
            }
        ]
    };
    return card;
};

const toTitleCase = (str) => {
    return str.replace(/\b\w/g, function(txt) { return txt.toUpperCase(); });
};

module.exports = {
    invokeResponse,
    invokeTaskResponse,
    selectResponseCard,
    optionInc,
    saveInc,
    toTitleCase
};
