using TabConversation.Controllers;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Extensions;
using Microsoft.Teams.Plugins.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

var appBuilder = App.Builder();

builder.Services.AddSingleton<Controller>();
builder.Services.AddRazorPages();
builder.AddTeams(appBuilder);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.UseTeams();

app.Run();