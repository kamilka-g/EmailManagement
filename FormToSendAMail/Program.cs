using FormToSendAMail.Services;
using Serilog;

public class Program
{
    public static void Main(string[] args)
    {
        // Konfiguracja Serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()    // Logowanie do konsoli
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day) // Logowanie do pliku (dzienny plik)
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);

        // Dodaj us³ugi do kontenera
        builder.Services.AddControllersWithViews();

        // Dodaj EmailService jako us³ugê
        builder.Services.AddSingleton<IEmailService, EmailService>();

        // Dodaj Logger do Dependency Injection
        builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());

        var app = builder.Build();

        // Konfiguracja HTTP request pipeline
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
