var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseFileServer();
app.UseRouting();

app.MapControllerRoute(
    name: "Default",
    pattern: "{controller=Home}/{action=Index}/{Id?}");

app.Run();
