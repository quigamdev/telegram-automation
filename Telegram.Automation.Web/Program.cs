using Microsoft.AspNetCore.Http.HttpResults;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using Telegram.Automation;

internal class Program
{
    private static async Task Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddEnvironmentVariables();
        var mode = builder.Configuration["mode"];

        Console.WriteLine($"Mode: {mode}");

        RegisterServices(builder.Services, mode, builder.Configuration);

        var app = builder.Build();

        ApplyDevelopmentSettings(app);

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        RegisterMinimalApi(app);

        app.UseAuthorization();

        app.MapRazorPages();

        await app.RunAsync();
    }

    private static void ApplyDevelopmentSettings(WebApplication app)
    {
        if (app.Environment.IsDevelopment()) return;

        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        //app.UseHsts(); -- handled by proxy server
        app.UseExceptionHandler("/Error");
    }

    private static void RegisterMinimalApi(WebApplication app)
    {
        RegisterScheduleEndpoints(app);
        RegisterAccountsEndpoints(app);
    }

    private static void RegisterAccountsEndpoints(WebApplication app)
    {
        app.MapPost("/account/start/{id}", async (HttpContext context, string id) =>
         await context.Response.WriteAsJsonAsync(
             await context.RequestServices.GetRequiredService<AccountsManager>().StartAccount(id)));

        app.MapPost("/account/stop/{id}", async (HttpContext context, string id) =>
            await context.Response.WriteAsJsonAsync(
                await context.RequestServices.GetRequiredService<AccountsManager>().StopAccount(id)));
    }

    private static void RegisterScheduleEndpoints(WebApplication app)
    {
        app.MapGet("/schedule/get", async (context) =>
                    await context.Response.WriteAsJsonAsync(
                        await context.RequestServices.GetRequiredService<IScheduleExecutor>().GetPlan()));

        app.MapPost("/schedule/add", async (HttpContext context, ScheduleItem data) =>
            await context.RequestServices.GetRequiredService<IScheduleExecutor>().AddToSchedule(data));
    }

    private static void RegisterServices(IServiceCollection services, string? mode, IConfiguration config)
    {
        // Add services to the container.
        services.AddRazorPages();
        services.AddOptions<TelegramConnectorOptions>().Bind(config.GetSection("TelegramConnector"));
        services.AddHostedService<ScheduleHostedService>();
        services.AddSingleton<IScheduleExecutor, ScheduleExecutor>();
        services.AddSingleton<AccountsManager>();
        services.AddSingleton<ScheduleStore>();

        services.AddLogging(a => a.AddConsole());
        services.AddLazyCache();

        if (mode == ServiceModes.Prod)
        {
            services.AddSingleton<ITelegramConnector, TelegramConnector>();
        }
        else
        {
            services.AddSingleton<ITelegramConnector, MockTelegramConnector>();
        }
    }
}