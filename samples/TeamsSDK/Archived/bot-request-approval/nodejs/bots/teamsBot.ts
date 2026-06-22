import { TeamsActivityHandler, CardFactory, TeamsInfo, TurnContext } from 'botbuilder';
import * as adaptiveCards from '../models/adaptiveCard';

export class TeamsBot extends TeamsActivityHandler {
    constructor() {
        super();

        this.onMembersAdded(this.handleMembersAdded.bind(this));
        this.onMessage(this.handleMessage.bind(this));
    }

    async handleMembersAdded(context: TurnContext, next: () => Promise<void>) {
        const membersAdded = context.activity.membersAdded;
        for (const member of membersAdded) {
            if (member.id !== context.activity.recipient.id) {
                await context.sendActivity("Hello and welcome! With this sample you can send task requests to your manager, and your manager can approve/reject the request.");
            }
        }
        await next();
    }

    async handleMessage(context: TurnContext, next: () => Promise<void>) {
        await context.sendActivity({
            attachments: [CardFactory.adaptiveCard(adaptiveCards.optionInc())]
        });
        await next();
    }

    async onInvokeActivity(context: TurnContext) {
        const user = context.activity.from;
        if (context.activity.name === 'adaptiveCard/action') {
            const allMembers = await TeamsInfo.getMembers(context);
            const card = await adaptiveCards.selectResponseCard(context, user, allMembers);
            return adaptiveCards.invokeResponse(card);
        }
    }
}