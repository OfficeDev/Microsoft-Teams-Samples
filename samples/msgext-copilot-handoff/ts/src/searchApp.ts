import {
  TeamsActivityHandler,
  TurnContext,
  MessagingExtensionQuery,
  MessagingExtensionResponse,
  InvokeResponse,
  AdaptiveCardInvokeResponse
} from "botbuilder";
import productSearchCommand from "./messageExtensions/productSearchCommand";
import discountedSearchCommand from "./messageExtensions/discountSearchCommand";
import actionHandler from "./adaptiveCards/cardHandler";
import { CreateActionErrorResponse } from "./adaptiveCards/utils";

export class SearchApp extends TeamsActivityHandler {

  notifyContinuationActivity: any;
  continuationParameters: any;
  constructor(
    notifyContinuationActivity: any,
    continuationParameters: any = {} /*conversations: any = {}*/
  ) {
    super();
    this.notifyContinuationActivity = notifyContinuationActivity;
    this.continuationParameters = continuationParameters;
  }

  // Handle search message extension
  public async handleTeamsMessagingExtensionQuery(
    context: TurnContext,
    query: MessagingExtensionQuery
  ): Promise<MessagingExtensionResponse> {

    switch (query.commandId) {
      case productSearchCommand.COMMAND_ID: {
        return productSearchCommand.handleTeamsMessagingExtensionQuery(context, query);
      }
      case discountedSearchCommand.COMMAND_ID: {
        return discountedSearchCommand.handleTeamsMessagingExtensionQuery(context, query);
      }
    }

  }

  // Handle adaptive card actions
  public async onAdaptiveCardInvoke(context: TurnContext): Promise<AdaptiveCardInvokeResponse>  {
    try {     
   
        switch (context.activity.value.action.verb) {
          case 'ok': {
            return actionHandler.handleTeamsCardActionUpdateStock(context);
          }
          case 'restock': {
            return actionHandler.handleTeamsCardActionRestock(context);
          }
          case 'cancel': {
            return actionHandler.handleTeamsCardActionCancelRestock(context);
          }
          case "handoff": {
            return actionHandler.handleTeamsCardActionHandOff(context);
          }
          case "refresh": {
            return actionHandler.handleTeamsCardActionRefreshCard(context);
          }
          default:
            return CreateActionErrorResponse(
              400,
              0,
              `ActionVerbNotSupported: ${context.activity.value.action.verb} is not a supported action verb.`
            );
         
        }
     
    } catch (err) {
      return CreateActionErrorResponse(500, 0, err.message);
    } 
  }

  
  
  private addOrUpdateContinuationParameters(context): void {
    console.log(
      `Adding continuation parameters for context: ${JSON.stringify(context)}`
    );
    this.continuationParameters[context.activity.from.id] = {
      claimsIdentity: context.turnState.get(context.adapter.BotIdentityKey),
      conversationReference: TurnContext.getConversationReference(
        context.activity
      ),
      oAuthScope: context.turnState.get(context.adapter.OAuthScopeKey),
      continuationToken: context.activity.value.continuation,
      cardAttachment:
        actionHandler.handleTeamsCardActionHandOffWithContinuation(
          context.activity.value.continuation
        ),
    };
  }
}
