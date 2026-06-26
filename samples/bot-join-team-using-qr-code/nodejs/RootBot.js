// bots/rootBot.js
class RootBot {
    constructor(teamsBot, dialogBot) {
        this.teamsBot = teamsBot;
        this.dialogBot = dialogBot;
    }

    async run(context) {
        if (context.activity.channelId === 'msteams') {
            // Delegate to TeamsBot
            await this.teamsBot.run(context);
        } else {
            // Delegate to DialogBot
            await this.dialogBot.run(context);
        }
    }
}

module.exports.RootBot = RootBot;
