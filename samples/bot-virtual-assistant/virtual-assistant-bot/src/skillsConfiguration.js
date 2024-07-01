// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
const config = require('./config');

/**
 * A helper class that loads Skills information from configuration.
 */
class SkillsConfiguration {
    constructor() {
        this.skillsData = {};

        // Note: we only have one skill in this sample but we could load more if needed.
        const skills = config.SkillId.split(',');

        for (var ind = 0; ind < skills.length; ind++) {
            const botFrameworkSkill = {
                id: config.SkillId.split(',')[ind],
                appId: config.SkillAppId.split(',')[ind],
                skillEndpoint: config.SkillEndpoint.split(',')[ind]
            };

            this.skillsData[botFrameworkSkill.id] = botFrameworkSkill;
        };

        this.skillHostEndpointValue = config.SkillHostEndpoint;
        if (!this.skillHostEndpointValue) {
            throw new Error('[SkillsConfiguration]: Missing configuration parameter. SkillHostEndpoint is required');
        }
    }

    get skills() {
        return this.skillsData;
    }

    get skillHostEndpoint() {
        return this.skillHostEndpointValue;
    }
}

module.exports.SkillsConfiguration = SkillsConfiguration;
