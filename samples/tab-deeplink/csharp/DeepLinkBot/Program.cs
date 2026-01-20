// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Agents.Hosting.AspNetCore;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Storage;
using Microsoft.AgentSamples.Bots;

var builder = WebApplication.CreateBuilder(args);

// Add controllers and MVC services
builder.Services.AddControllers();
builder.Services.AddMvc();

// Add HttpClient factory (required by Agent SDK)
builder.Services.AddHttpClient();

// Add in-memory storage for state management
builder.Services.AddSingleton<IStorage, MemoryStorage>();

// Add Agent Application Options and register the Agent
builder.AddAgentApplicationOptions();
builder.AddAgent<DeepLinkBot>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Configure static files, default files, and WebSocket support
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseWebSockets();

// Configure routing and authentication
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Map API messages endpoint for the Agent
app.MapPost("/api/messages", async (
    HttpRequest request,
    HttpResponse response,
    IAgentHttpAdapter adapter,
    IAgent agent,
    CancellationToken cancellationToken) =>
{
    await adapter.ProcessAsync(request, response, agent, cancellationToken);
});

// Map controller endpoints
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
