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
  constructor() {
    super();
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
          default:
            // TODO: Handle Refresh correctly and set this line back to
            // returning an error
            // return CreateActionErrorResponse(400, 0, `ActionVerbNotSupported: ${context.activity.value.action.verb} is not a supported action verb.`);

            // Workaround to stop choking on refresh activities
            return CreateActionErrorResponse(200, 0, `ActionVerbNotSupported: ${context.activity.value.action.verb} is not a supported action verb.`);
         
        }
     
    } catch (err) {
      return CreateActionErrorResponse(500, 0, err.message);
    } 
  }

  // Handle invoke activities
  
  public async onInvokeActivity(context: TurnContext): Promise<InvokeResponse> {

    try {

      switch (context.activity.name) {

        case "handoff/action": {

          // TODO: Save continuation token and use it to process final response to user later 

          return { status: 200 }; // return just the http status 

        }

        case "composeExtension/query":

          return {

            status: 200,

            body: await this.handleTeamsMessagingExtensionQuery(

              context,

              context.activity.value

            ),

          };

        default:

          return {

            status: 200,

            body: `Unknown invoke activity handled as default- ${context.activity.name}`,

          };

      }

    } catch (err) {

      console.log(`Error in onInvokeActivity: ${err}`);

      return {

        status: 500,

        body: `Invoke activity received- ${context.activity.name}`,

      };

    }

  }
}

