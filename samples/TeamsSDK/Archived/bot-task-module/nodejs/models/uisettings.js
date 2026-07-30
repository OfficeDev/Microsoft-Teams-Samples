// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

/**
 * UISettings defines the UI settings for task modules in Microsoft Teams.
 */
class UISettings {
    /**
     * Creates an instance of UISettings.
     * @param {number} width - The width of the task module.
     * @param {number} height - The height of the task module.
     * @param {string} title - The title of the task module.
     * @param {string} id - The ID of the task module.
     * @param {string} buttonTitle - The button title for the task module.
     */
    constructor(width, height, title, id, buttonTitle) {
        this.width = width;
        this.height = height;
        this.title = title;
        this.id = id;
        this.buttonTitle = buttonTitle;
    }
}

module.exports.UISettings = UISettings;
