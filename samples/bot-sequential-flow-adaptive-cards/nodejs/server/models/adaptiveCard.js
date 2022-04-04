const { StatusCodes, MessageFactory, CardFactory } = require('botbuilder');
const { MicrosoftAppCredentials, ConnectorClient } = require('botframework-connector');
const incidentService = require('../services/incidentService');

const categories = [
    {
        name: 'Software',
        subcategory: ['Email', 'OS', 'Application']
    },
    {
        name: 'Hardware',
        subcategory: ['CPU', 'Disk', 'Keyboard', 'Memory', 'Monitor', 'Mouse']
    }

];

let allMembers = [];
let otherMembers = [];

const incidentManagementCard = (profileName) => ({
    version: '1.0.0',
    type: 'AdaptiveCard',
    body: [
        {
            type: 'TextBlock',
            text: `Hello ${profileName}`
        },
        {
            type: 'TextBlock',
            text: 'Starting Incident Management Workflow'
        }
    ]
});

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

const invokeIncidentTaskResponse = (title, card) => {
    const task = {
        status: StatusCodes.OK,
        body: {
            task: {
                type: 'continue',
                value: {
                    card: card,
                    heigth: 460,
                    width: 600,
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
    if (verb && prefix === 'choose') {
        return await chooseCategory();
    } else if (verb && prefix === 'category') {
        return await chooseSubCategory(verb);
    } else if (verb && prefix === 'subcategory') {
        return createInc(verb, user);
    } else if (verb && prefix === 'save') {
        const card = await saveInc(action);
        await updateCardInTeams(context, card);
        return card;
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
            text: 'Incident Management'
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
                    verb: 'choose_category',
                    title: 'Create New Incident',
                    data: {
                        option: 'Create New Incident'
                    }
                },
                {
                    type: 'Action.Execute',
                    verb: 'list_inc',
                    title: 'List Incidents',
                    data: {
                        option: 'List Incidents'
                    }
                }
            ]
        }
    ],
    type: 'AdaptiveCard',
    version: '1.4'
});

const incidentListCard = (choiceset) => ({
    $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
    version: '1.0.0',
    type: 'AdaptiveCard',
    body: [
        {
            type: 'TextBlock',
            text: 'Select a incident to send in chat',
            size: "large",
            weight: "bolder"
        },
        {
            type: "Input.ChoiceSet",
            id: "incidentId",
            style: "expanded",
            isMultiSelect: false,
            value: "",
            choices: choiceset,
            wrap: true
        }
    ],
    actions: [
        {
            type: "Action.Submit",
            id: "submit",
            title: "Send",
            data: {
                action: "incidentSelector"
            }
        }
    ]
});

const chooseCategory = async () => {
    const actionsPromise = categories.map(c => ({
        type: 'Action.Execute',
        verb: `category_${c.name}`,
        title: c.name,
        data: {
            option: c.name
        }
    }));
    const actions = await Promise.all(actionsPromise);
    const card = {
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        appId: process.env.MicrosoftAppId,
        body: [
            {
                type: 'TextBlock',
                size: 'Medium',
                weight: 'Bolder',
                text: 'Incident Management'
            },
            {
                type: 'RichTextBlock',
                inlines: [
                    {
                        type: 'TextRun',
                        text: 'Choose a Category'
                    }
                ]
            },
            {
                type: 'ActionSet',
                actions: actions
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
        ],
        type: 'AdaptiveCard',
        version: '1.4'
    };
    return card;
};

const chooseSubCategory = async (verb) => {
    const category = verb.split('_')[1];
    const subCategories = categories.filter(c => c.name === category);
    const actionsPromise = subCategories[0].subcategory.map(sc => ({
        type: 'Action.Execute',
        verb: `subcategory_${category}_${sc}`,
        title: sc,
        data: {
            option: sc
        }
    }));
    const actions = await Promise.all(actionsPromise);
    const card = {
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        appId: process.env.MicrosoftAppId,
        body: [
            {
                type: 'TextBlock',
                size: 'Medium',
                weight: 'Bolder',
                text: 'Incident Management'
            },
            {
                type: 'RichTextBlock',
                inlines: [
                    {
                        type: 'TextRun',
                        text: 'Choose a Sub-Category'
                    }
                ]
            },
            {
                type: 'ActionSet',
                actions: actions
            }
        ],
        actions: [
            {
                type: 'Action.Execute',
                verb: 'choose_category',
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

const createInc = async (verb, user) => {
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
                text: 'Incident Management'
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
                type: 'ColumnSet',
                columns: [
                    {
                        type: 'Column',
                        width: 'auto',
                        items: [
                            {
                                type: 'RichTextBlock',
                                inlines: [
                                    {
                                        type: 'TextRun',
                                        text: 'Category',
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
                                        text: verb.split('_')[1]
                                    }
                                ]
                            }
                        ]
                    },
                    {
                        type: 'Column',
                        width: 'auto',
                        items: [
                            {
                                type: 'RichTextBlock',
                                inlines: [
                                    {
                                        type: 'TextRun',
                                        text: 'Sub-Category',
                                        weight: 'Bolder',
                                        italic: true,
                                        size: 'medium',
                                        horizontalAlignment: 'Center'
                                    }
                                ]
                            },
                            {
                                type: 'RichTextBlock',
                                inlines: [
                                    {
                                        type: 'TextRun',
                                        text: verb.split('_')[2],
                                        horizontalAlignment: 'Center'
                                    }
                                ]
                            }
                        ]
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
                    category: verb.split('_')[1],
                    sub_category: verb.split('_')[2],
                    inc_created_by: user
                }
            },
            {
                type: 'Action.Execute',
                verb: `category_${verb.split('_')[1]}`,
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
    const inc = await incidentService.saveInc(action, allMembers);
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
                    incident: inc
                }
            },
            userIds: allMembers.map(m => m.id)
        },
        body: [
            {
                type: 'TextBlock',
                size: 'Medium',
                weight: 'Bolder',
                text: 'Incident Management'
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
                type: 'ColumnSet',
                columns: [
                    {
                        type: 'Column',
                        width: 'auto',
                        items: [
                            {
                                type: 'RichTextBlock',
                                inlines: [
                                    {
                                        type: 'TextRun',
                                        text: 'Category',
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
                                        text: inc.category
                                    }
                                ]
                            }
                        ]
                    },
                    {
                        type: 'Column',
                        width: 'auto',
                        items: [
                            {
                                type: 'RichTextBlock',
                                inlines: [
                                    {
                                        type: 'TextRun',
                                        text: 'Sub-Category',
                                        weight: 'Bolder',
                                        italic: true,
                                        size: 'medium',
                                        horizontalAlignment: 'Center'
                                    }
                                ]
                            },
                            {
                                type: 'RichTextBlock',
                                inlines: [
                                    {
                                        type: 'TextRun',
                                        text: inc.subCategory,
                                        horizontalAlignment: 'Center'
                                    }
                                ]
                            }
                        ]
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
    const inc = action.data.incident;
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
                text: 'Incident Management'
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
                type: 'ColumnSet',
                columns: [
                    {
                        type: 'Column',
                        width: 'auto',
                        items: [
                            {
                                type: 'RichTextBlock',
                                inlines: [
                                    {
                                        type: 'TextRun',
                                        text: 'Category',
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
                                        text: inc.category
                                    }
                                ]
                            }
                        ]
                    },
                    {
                        type: 'Column',
                        width: 'auto',
                        items: [
                            {
                                type: 'RichTextBlock',
                                inlines: [
                                    {
                                        type: 'TextRun',
                                        text: 'Sub-Category',
                                        weight: 'Bolder',
                                        italic: true,
                                        size: 'medium',
                                        horizontalAlignment: 'Center'
                                    }
                                ]
                            },
                            {
                                type: 'RichTextBlock',
                                inlines: [
                                    {
                                        type: 'TextRun',
                                        text: inc.subCategory,
                                        horizontalAlignment: 'Center'
                                    }
                                ]
                            }
                        ]
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
                    category: inc.category,
                    sub_category: inc.subCategory,
                    inc_created_by: inc.createdBy,
                    incident: inc
                }
            },
            {
                type: 'Action.Execute',
                verb: 'view_inc',
                title: 'Back',
                data: {
                    info: 'Back',
                    incident: inc
                }
            }
        ],
        type: 'AdaptiveCard',
        version: '1.4'
    };
    return card;
};

const updateInc = async (action) => {
    const inc = await incidentService.updateInc(action, allMembers);
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
                    incident: inc
                }
            },
            userIds: allMembers.map(m => m.id)
        },
        body: [
            {
                type: 'TextBlock',
                size: 'Medium',
                weight: 'Bolder',
                text: 'Incident Management'
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
                type: 'ColumnSet',
                columns: [
                    {
                        type: 'Column',
                        width: 'auto',
                        items: [
                            {
                                type: 'RichTextBlock',
                                inlines: [
                                    {
                                        type: 'TextRun',
                                        text: 'Category',
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
                                        text: inc.category
                                    }
                                ]
                            }
                        ]
                    },
                    {
                        type: 'Column',
                        width: 'auto',
                        items: [
                            {
                                type: 'RichTextBlock',
                                inlines: [
                                    {
                                        type: 'TextRun',
                                        text: 'Sub-Category',
                                        weight: 'Bolder',
                                        italic: true,
                                        size: 'medium',
                                        horizontalAlignment: 'Center'
                                    }
                                ]
                            },
                            {
                                type: 'RichTextBlock',
                                inlines: [
                                    {
                                        type: 'TextRun',
                                        text: inc.subCategory,
                                        horizontalAlignment: 'Center'
                                    }
                                ]
                            }
                        ]
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
    const inc = action.data.incident;
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
                    incident: inc
                }
            },
            userIds: allMembers.map(m => m.id)
        },
        body: [
            {
                type: 'TextBlock',
                size: 'Medium',
                weight: 'Bolder',
                text: 'Incident Management'
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
                type: 'ColumnSet',
                columns: [
                    {
                        type: 'Column',
                        width: 'auto',
                        items: [
                            {
                                type: 'RichTextBlock',
                                inlines: [
                                    {
                                        type: 'TextRun',
                                        text: 'Category',
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
                                        text: inc.category
                                    }
                                ]
                            }
                        ]
                    },
                    {
                        type: 'Column',
                        width: 'auto',
                        items: [
                            {
                                type: 'RichTextBlock',
                                inlines: [
                                    {
                                        type: 'TextRun',
                                        text: 'Sub-Category',
                                        weight: 'Bolder',
                                        italic: true,
                                        size: 'medium',
                                        horizontalAlignment: 'Center'
                                    }
                                ]
                            },
                            {
                                type: 'RichTextBlock',
                                inlines: [
                                    {
                                        type: 'TextRun',
                                        text: inc.subCategory,
                                        horizontalAlignment: 'Center'
                                    }
                                ]
                            }
                        ]
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
    const inc = await incidentService.updateStatusInc(action);
    const statusIcon = inc.status === 'Approved' ? 'https://spsharewithme.azurewebsites.net/Images/approve1.png' : (inc.status === 'Rejected' ? 'https://spsharewithme.azurewebsites.net/Images/reject.png' : '');
    const card = {
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        appId: process.env.MicrosoftAppId,
        body: [
            {
                type: 'TextBlock',
                size: 'Medium',
                weight: 'Bolder',
                text: 'Incident Management'
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
                type: 'ColumnSet',
                columns: [
                    {
                        type: 'Column',
                        width: 'auto',
                        items: [
                            {
                                type: 'RichTextBlock',
                                inlines: [
                                    {
                                        type: 'TextRun',
                                        text: 'Category',
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
                                        text: inc.category
                                    }
                                ]
                            }
                        ]
                    },
                    {
                        type: 'Column',
                        width: 'auto',
                        items: [
                            {
                                type: 'RichTextBlock',
                                inlines: [
                                    {
                                        type: 'TextRun',
                                        text: 'Sub-Category',
                                        weight: 'Bolder',
                                        italic: true,
                                        size: 'medium',
                                        horizontalAlignment: 'Center'
                                    }
                                ]
                            },
                            {
                                type: 'RichTextBlock',
                                inlines: [
                                    {
                                        type: 'TextRun',
                                        text: inc.subCategory,
                                        horizontalAlignment: 'Center'
                                    }
                                ]
                            }
                        ]
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
                type: 'Image',
                id: 'inc_status',
                url: statusIcon,
                altText: inc.status,
                height: '30px'
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
    const inc = action.data.incident;
    let actions = [];
    if (user.aadObjectId === inc.createdBy.aadObjectId) {
        actions = [
            {
                type: 'Action.Execute',
                verb: 'edit_inc',
                title: 'Edit',
                data: {
                    info: 'edit',
                    incident: inc
                }
            }
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
                    incident: inc
                }
            },
            {
                type: 'Action.Execute',
                verb: 'status_reject_inc',
                title: 'Reject',
                data: {
                    info: 'reject',
                    status: 'Rejected',
                    incident: inc
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
                text: 'Incident Management'
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
                type: 'ColumnSet',
                columns: [
                    {
                        type: 'Column',
                        width: 'auto',
                        items: [
                            {
                                type: 'RichTextBlock',
                                inlines: [
                                    {
                                        type: 'TextRun',
                                        text: 'Category',
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
                                        text: inc.category
                                    }
                                ]
                            }
                        ]
                    },
                    {
                        type: 'Column',
                        width: 'auto',
                        items: [
                            {
                                type: 'RichTextBlock',
                                inlines: [
                                    {
                                        type: 'TextRun',
                                        text: 'Sub-Category',
                                        weight: 'Bolder',
                                        italic: true,
                                        size: 'medium',
                                        horizontalAlignment: 'Center'
                                    }
                                ]
                            },
                            {
                                type: 'RichTextBlock',
                                inlines: [
                                    {
                                        type: 'TextRun',
                                        text: inc.subCategory,
                                        horizontalAlignment: 'Center'
                                    }
                                ]
                            }
                        ]
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

const refreshBotCard = async (inc) => {
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
                    incident: inc
                }
            },
            userIds: allMembers.map(m => m.id)
        },
        body: [
            {
                type: 'TextBlock',
                size: 'Medium',
                weight: 'Bolder',
                text: 'Incident Management'
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
                type: 'ColumnSet',
                columns: [
                    {
                        type: 'Column',
                        width: 'auto',
                        items: [
                            {
                                type: 'RichTextBlock',
                                inlines: [
                                    {
                                        type: 'TextRun',
                                        text: 'Category',
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
                                        text: inc.category
                                    }
                                ]
                            }
                        ]
                    },
                    {
                        type: 'Column',
                        width: 'auto',
                        items: [
                            {
                                type: 'RichTextBlock',
                                inlines: [
                                    {
                                        type: 'TextRun',
                                        text: 'Sub-Category',
                                        weight: 'Bolder',
                                        italic: true,
                                        size: 'medium',
                                        horizontalAlignment: 'Center'
                                    }
                                ]
                            },
                            {
                                type: 'RichTextBlock',
                                inlines: [
                                    {
                                        type: 'TextRun',
                                        text: inc.subCategory,
                                        horizontalAlignment: 'Center'
                                    }
                                ]
                            }
                        ]
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

const viewAllInc = async () => {
    const incs = await incidentService.getAllInc();
    let incCards = [];
    if (!incs.length) {
        incCards = [
            {
                type: 'RichTextBlock',
                inlines: [
                    {
                        type: 'TextRun',
                        text: 'No Incidents Available',
                        italic: true,
                        size: 'medium',
                        color: 'Attention'
                    }
                ]
            }
        ];
    } else {
        incCards = incs.map(inc => (
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
                                                text: 'Category',
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
                                                text: inc.category
                                            }
                                        ]
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
                                                text: 'Sub Category',
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
                                                text: inc.subCategory
                                            }
                                        ]
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
                text: 'Incidents List'
            },
            {
                type: 'Container',
                separator: true,
                items: incCards
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
    return str.replace(/\b\w/g, function (txt) { return txt.toUpperCase(); });
};

module.exports = {
    incidentManagementCard,
    invokeResponse,
    invokeTaskResponse,
    invokeIncidentTaskResponse,
    selectResponseCard,
    optionInc,
    chooseCategory,
    chooseSubCategory,
    createInc,
    saveInc,
    refreshBotCard,
    incidentListCard,
    toTitleCase
};