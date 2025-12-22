using System.Text;
using System.Text.Json;

namespace TeamsMsgextThirdpartyStorage.Middleware
{
    public class TeamsActivityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TeamsActivityMiddleware> _logger;

        public TeamsActivityMiddleware(RequestDelegate next, ILogger<TeamsActivityMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only process POST requests to /api/messages
            if (context.Request.Method == "POST" && context.Request.Path.StartsWithSegments("/api/messages"))
            {
                context.Request.EnableBuffering();

                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                try
                {
                    var jsonDoc = JsonDocument.Parse(body);
                    var root = jsonDoc.RootElement;

                    // Check if this is a composeExtension invoke (fetchTask or submitAction)
                    if (root.TryGetProperty("type", out var typeElement) &&
                        typeElement.GetString() == "invoke" &&
                        root.TryGetProperty("name", out var nameElement))
                    {
                        var invokeName = nameElement.GetString();
                        
                        // Handle both fetchTask and submitAction
                        if (invokeName == "composeExtension/fetchTask" || invokeName == "composeExtension/submitAction")
                        {
                            // Check if there's a messagePayload without an id
                            if (root.TryGetProperty("value", out var valueElement) &&
                                valueElement.TryGetProperty("messagePayload", out var messagePayloadElement))
                            {
                                // If messagePayload exists but doesn't have an id, add a placeholder
                                if (!messagePayloadElement.TryGetProperty("id", out _))
                                {
                                    _logger.LogInformation($"Adding placeholder id to messagePayload for {invokeName}");

                                    // Reconstruct the JSON with a placeholder id
                                    var modifiedJson = AddPlaceholderIdToMessagePayload(body);
                                    
                                    // Replace the request body with the modified JSON
                                    var bytes = Encoding.UTF8.GetBytes(modifiedJson);
                                    context.Request.Body = new MemoryStream(bytes);
                                    context.Request.ContentLength = bytes.Length;
                                }
                            }
                        }
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning($"Failed to parse JSON body: {ex.Message}");
                }
            }

            await _next(context);
        }

        private string AddPlaceholderIdToMessagePayload(string json)
        {
            try
            {
                using var jsonDoc = JsonDocument.Parse(json);
                var root = jsonDoc.RootElement;

                // Create a new JSON object with the id added to messagePayload
                var options = new JsonWriterOptions { Indented = false };
                using var stream = new MemoryStream();
                using (var writer = new Utf8JsonWriter(stream, options))
                {
                    writer.WriteStartObject();

                    foreach (var property in root.EnumerateObject())
                    {
                        if (property.Name == "value" && property.Value.ValueKind == JsonValueKind.Object)
                        {
                            writer.WritePropertyName("value");
                            writer.WriteStartObject();

                            foreach (var valueProperty in property.Value.EnumerateObject())
                            {
                                if (valueProperty.Name == "messagePayload" && valueProperty.Value.ValueKind == JsonValueKind.Object)
                                {
                                    writer.WritePropertyName("messagePayload");
                                    writer.WriteStartObject();

                                    // Add placeholder id first
                                    writer.WriteString("id", "placeholder-message-id");

                                    // Copy all existing properties
                                    foreach (var msgProperty in valueProperty.Value.EnumerateObject())
                                    {
                                        msgProperty.WriteTo(writer);
                                    }

                                    writer.WriteEndObject();
                                }
                                else
                                {
                                    valueProperty.WriteTo(writer);
                                }
                            }

                            writer.WriteEndObject();
                        }
                        else
                        {
                            property.WriteTo(writer);
                        }
                    }

                    writer.WriteEndObject();
                }

                return Encoding.UTF8.GetString(stream.ToArray());
            }
            catch (Exception ex)
            {
                // If modification fails, return original JSON
                return json;
            }
        }
    }
}
