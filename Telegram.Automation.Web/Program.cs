using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Telegram.Automation;
using Telegram.Automation.Web.Facades;

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
        RegisterMaintenanceEndpoints(app);
    }

    private static void RegisterMaintenanceEndpoints(WebApplication app)
    {
        app.MapGet("/_maintenance/rewrite", async (HttpContext context) =>
          await context.Response.WriteAsJsonAsync(
              await context.RequestServices.GetRequiredService<MaintenanceFacade>().RewriteAccountsIds()));

    }

    private static void RegisterAccountsEndpoints(WebApplication app)
    {
        int concurrency = GetConcurrency(app); 
        app.MapPost("/account/schedule", (HttpContext context, AccountScheduleRequest scheduleRequest) =>
            context.RequestServices.GetRequiredService<IScheduleExecutor>().AddToScheduleAsync(scheduleRequest, concurrency));
        app.MapPost("/account/unschedule", (HttpContext context, AccountScheduleRequest scheduleRequest) =>
             context.RequestServices.GetRequiredService<IScheduleExecutor>().RemoveFromSchedule(scheduleRequest, concurrency));
    }

    private static void RegisterScheduleEndpoints(WebApplication app)
    {
        int concurrency = GetConcurrency(app);

        app.MapGet("/schedule/get/{id}", async (HttpContext context, [FromRoute] int id) =>
                    await context.Response.WriteAsJsonAsync(
                        await context.RequestServices.GetRequiredService<IScheduleExecutor>().GetPlan(id)));

        app.MapPost("/schedule/createSchedule", (HttpContext context, string name) =>
                         context.RequestServices.GetRequiredService<IScheduleExecutor>().CreateSchedule(name, concurrency));
    }

    private static int GetConcurrency(WebApplication app)
    {
        return int.TryParse(app.Configuration["ScheduleConcurrency"], out var value) ? value : 1;
    }

    private static void RegisterServices(IServiceCollection services, string? mode, IConfiguration config)
    {
        // Add services to the container.
        services.AddRazorPages();
        services.AddOptions<TelegramConnectorOptions>().Bind(config.GetSection("TelegramConnector"));
        services.AddHostedService<ScheduleHostedService>();
        services.AddSingleton<IScheduleExecutor, ScheduleExecutor>();
        services.AddSingleton<ScheduleStore>();
        services.AddSingleton<AccountsManager>();
        services.AddSingleton<MaintenanceFacade>();
        services.AddSingleton<IDateTimeProvider, DefaultDateTimeProvider>();
        
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