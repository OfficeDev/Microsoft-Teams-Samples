// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { UISettings } = require('./uisettings');
const { TaskModuleIds } = require('./taskmoduleids');

const TaskModuleUIConstants = {
    contentBubble: new UISettings(510, 450, 'Content Bubble', TaskModuleIds.contentBubble, 'Content Bubble'),
    contentBubbleTwo: new UISettings(510, 450, 'Content Bubble Two', TaskModuleIds.contentBubbleTwo, 'Content Bubble'),
    contentBubbleThree: new UISettings(510, 450, 'Content Bubble Three', TaskModuleIds.contentBubbleThree, 'Content Bubble'),
    AdaptiveCard: new UISettings(400, 200, 'Adaptive Card: Inputs', TaskModuleIds.AdaptiveCard, 'Adaptive Card')
};

module.exports.TaskModuleUIConstants = TaskModuleUIConstants;
