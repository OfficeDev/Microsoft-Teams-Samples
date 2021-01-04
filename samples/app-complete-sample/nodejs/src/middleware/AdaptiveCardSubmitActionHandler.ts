import * as builder from "botbuilder";

// Set the text field on message events to "adaptive card", if request is from an adaptive card
export class AdaptiveCardSubmitActionHandler implements builder.IMiddlewareMap {

    public readonly receive = (event: builder.IEvent, next: Function): void => {
        if (event.type === "message")
        {
            let currEvent = (event as builder.IMessage);

            // Check event text is blank, replyToId is not null, event value has isFromAdaptiveCard and messageText in incoming payload to check if the incoming
            // payload is from a Submit action button click from an AdaptiveCard (this is set in …\src\dialogs\examples\basic\AdaptiveCardDialog.ts in the Submit action
            // data field). If so, then set the text field of the incoming payload so the BotFramework regex recognizers will route the message to the desired dialog.
            if (currEvent.text === "" && currEvent.replyToId && currEvent.value && currEvent.value.isFromAdaptiveCard && currEvent.value.messageText)
            {
                currEvent.text = currEvent.value.messageText;
            }
        }

        next();
    }
}
