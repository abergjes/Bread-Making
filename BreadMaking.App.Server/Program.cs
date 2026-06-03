using BreadMaking.App.Server.Api;
using BreadMaking.App.Server.Data;
using BreadMaking.App.Server.Hubs;
using BreadMaking.App.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddSignalR();
builder.Services.AddSingleton<INotificationService, NotificationService>();
builder.Services.AddHostedService<FoldsReminderService>();

builder.Services.AddScoped<ITimerService, TimerService>();
builder.Services.AddScoped<IBakeSessionService, BakeSessionService>();
builder.Services.AddScoped<IMeasurementService, MeasurementService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

var uploadRoot = Path.Combine(app.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(uploadRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadRoot),
    RequestPath  = "/uploads",
});

app.MapBakeEndpoints();
app.MapStepLogEndpoints();
app.MapGrainEndpoints();
app.MapHub<BakeHub>("/hubs/bake");

app.MapFallbackToFile("index.html");

app.Run();
