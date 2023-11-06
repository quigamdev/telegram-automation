using Telegram.Automation;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        RegisterServices(builder);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();
    }

    private static void RegisterServices(WebApplicationBuilder builder)
    {
        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddOptions<TelegramConnectorOptions>().Bind(builder.Configuration.GetSection("TelegramConnector"));
        builder.Services.AddHostedService<ScheduleHostedService>();
        builder.Services.AddSingleton<IScheduleExecutor, ScheduleExecutor>();
        builder.Services.AddSingleton<AccountsManager>();
        builder.Services.AddLogging(a => a.AddConsole());
        builder.Services.AddLazyCache();

        builder.Services.AddSingleton<ITelegramConnector, TelegramConnector>();

    }
}