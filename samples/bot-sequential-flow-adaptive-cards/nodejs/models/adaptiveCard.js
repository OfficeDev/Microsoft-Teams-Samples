// Adaptive Cards module for Teams SDK v2
// Handles all card templates and logic for incident management

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

// Invoke response helper for Teams SDK
const invokeResponse = (card) => {
    const cardRes = {
        statusCode: 200,
        type: 'application/vnd.microsoft.card.adaptive',
        value: card
    };
    const res = {
        status: 200,
        body: cardRes
    };
    return res;
};

// Task response for message extensions
const invokeTaskResponse = (title, card) => {
    const task = {
        status: 200,
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

// Incident task response for message extensions
const invokeIncidentTaskResponse = (title, card) => {
    const task = {
        status: 200,
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
        return await saveInc(action);
    } else if (verb && prefix === 'edit') {
        return editInc(action);
    } else if (verb && prefix === 'update') {
        return await updateInc(action);
    } else if (verb && prefix === 'view') {
        return await viewInc(action);
    } else if (verb && prefix === 'status') {
        return await updateStatusInc(action);
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
        type: 'AdaptiveCard',
        version: '1.4',
        appId: process.env.CLIENT_ID,
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
        type: 'AdaptiveCard',
        version: '1.4',
        appId: process.env.CLIENT_ID,
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
        type: 'AdaptiveCard',
        version: '1.4',
        appId: process.env.CLIENT_ID,
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
        type: 'AdaptiveCard',
        version: '1.4',
        appId: process.env.CLIENT_ID,
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
                        text: inc.assignedTo ? inc.assignedTo.name : 'Not assigned',
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
        type: 'AdaptiveCard',
        version: '1.4',
        appId: process.env.CLIENT_ID,
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
        type: 'AdaptiveCard',
        version: '1.4',
        appId: process.env.CLIENT_ID,
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
        type: 'AdaptiveCard',
        version: '1.4',
        appId: process.env.CLIENT_ID,
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
    const statusColor = inc.status === 'Approved' ? 'Good' : (inc.status === 'Rejected' ? 'Attention' : 'Default');
    const card = {
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        type: 'AdaptiveCard',
        version: '1.4',
        appId: process.env.CLIENT_ID,
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
                type: 'RichTextBlock',
                id: 'inc_status',
                inlines: [
                    {
                        type: 'TextRun',
                        text: inc.status,
                        weight: 'Bolder',
                        size: 'Large',
                        color: statusColor
                    }
                ]
            }
        ],
        type: 'AdaptiveCard',
        version: '1.4'
    };
    return card;
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
        type: 'AdaptiveCard',
        version: '1.4',
        appId: process.env.CLIENT_ID,
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
        type: 'AdaptiveCard',
        version: '1.4',
        appId: process.env.CLIENT_ID,
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

const createAssigneeNotificationCard = async (incident, members) => {
    // Store members for later use in refresh actions
    allMembers = members;
    otherMembers = allMembers.filter(tm => tm.aadObjectId !== incident.assignedTo.aadObjectId);
    
    const card = {
        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
        type: 'AdaptiveCard',
        version: '1.4',
        body: [
            {
                type: 'TextBlock',
                size: 'Large',
                weight: 'Bolder',
                text: '🔔 New Incident Assigned',
                color: 'Accent'
            },
            {
                type: 'TextBlock',
                text: `You have been assigned to work on this incident.`,
                wrap: true,
                spacing: 'Small'
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
                        text: incident.title
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
                                        text: incident.category
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
                                        size: 'medium'
                                    }
                                ]
                            },
                            {
                                type: 'RichTextBlock',
                                inlines: [
                                    {
                                        type: 'TextRun',
                                        text: incident.subCategory
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
                inlines: [
                    {
                        type: 'TextRun',
                        text: incident.createdBy.name,
                        wrap: true
                    }
                ]
            }
        ],
        actions: [
            {
                type: 'Action.Execute',
                verb: 'status_approve_inc',
                title: 'Approve',
                style: 'positive',
                data: {
                    info: 'approve',
                    status: 'Approved',
                    incident: incident
                }
            },
            {
                type: 'Action.Execute',
                verb: 'status_reject_inc',
                title: 'Reject',
                style: 'destructive',
                data: {
                    info: 'reject',
                    status: 'Rejected',
                    incident: incident
                }
            }
        ]
    };
    return card;
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
    toTitleCase,
    createAssigneeNotificationCard
};