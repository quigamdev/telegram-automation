using Microsoft.AspNetCore.Http.HttpResults;
using System.Reflection.Metadata.Ecma335;
using Telegram.Automation;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddEnvironmentVariables();
        var mode = builder.Configuration["mode"];

        RegisterServices(builder.Services, mode, builder.Configuration);

        var app = builder.Build();

        ApplyDevelopmentSettings(app);

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        RegisterMinimalApi(app);

        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();
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
        app.MapGet("/schedule/get", async (context) =>
            await context.Response.WriteAsJsonAsync(
                await context.RequestServices.GetRequiredService<IScheduleExecutor>().GetPlan()));
    }

    private static void RegisterServices(IServiceCollection services, string? mode, IConfiguration config)
    {
        // Add services to the container.
        services.AddRazorPages();
        services.AddOptions<TelegramConnectorOptions>().Bind(config.GetSection("TelegramConnector"));
        services.AddHostedService<ScheduleHostedService>();
        services.AddSingleton<IScheduleExecutor, ScheduleExecutor>();
        services.AddSingleton<AccountsManager>();
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