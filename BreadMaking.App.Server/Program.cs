using BreadMaking.App.Server.Api;
using BreadMaking.App.Server.Data;
using BreadMaking.App.Server.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

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

app.MapBakeEndpoints();
app.MapStepLogEndpoints();
app.MapGrainEndpoints();

app.MapFallbackToFile("index.html");

app.Run();
