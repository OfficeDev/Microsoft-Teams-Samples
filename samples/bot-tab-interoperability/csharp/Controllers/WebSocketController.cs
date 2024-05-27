using System;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace AnonymousUsers.Controllers;

public class WebSocketController : ControllerBase
{

    [HttpGet("/ws")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await Echo(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    private static async Task Echo(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!receiveResult.CloseStatus.HasValue)
        {
            await webSocket.SendAsync(
                new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                receiveResult.MessageType,
                receiveResult.EndOfMessage,
                CancellationToken.None);

            string receivedMessage = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
            if (receivedMessage != null)
            {
                string inputValue = "";
                string threadId = "";
                string appID = "";
                string appPassword = "";
                using (JsonDocument document = JsonDocument.Parse(receivedMessage))
                {
                    JsonElement root = document.RootElement;
                    if (root.TryGetProperty("threadId", out JsonElement threadIdElement))
                    {
                        threadId = threadIdElement.GetString();
                    }
                    if (root.TryGetProperty("inputValue", out JsonElement inputValueElement))
                    {
                        inputValue = inputValueElement.GetString();
                    }
                    if (root.TryGetProperty("appID", out JsonElement appIDElement))
                    {
                        appID = appIDElement.GetString();
                    }
                    if (root.TryGetProperty("appPassword", out JsonElement appPasswordElement))
                    {
                        appPassword = appPasswordElement.GetString();
                    }
                    // Send the received message to the bot
                    if (threadId != null)
                    {
                        await SendProactiveMessage(inputValue, threadId, appID, appPassword);
                    }
                }
            }

            receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }

   

    private static async Task SendProactiveMessage(string message, string threadId,string appID, string appPassword)
    {
        var connector = new ConnectorClient(new Uri("https://smba.trafficmanager.net/amer/"), appID, appPassword);
        var conversationReference = new ConversationReference
        {
            Conversation = new ConversationAccount(id: threadId)
        };

        var activity = Activity.CreateMessageActivity();
        activity.Text = message;

        await connector.Conversations.SendToConversationAsync(
            conversationReference.Conversation.Id,
            (Activity)activity);
    }

}
