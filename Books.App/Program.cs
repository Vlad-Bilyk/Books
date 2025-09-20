using Books.App;
using Books.App.IO;
using Books.Core.Interfaces;
using Books.Infrastructure.Data;
using Books.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            var connString = context.Configuration.GetConnectionString("DefaultConnection")
                 ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            options.UseSqlServer(connString);
        });

        services.AddSingleton<IConsoleWrapper, ConsoleWrapper>();
        services.AddSingleton<IFileReader, FileReader>();
        services.AddSingleton<IBookParser, BookCsvParser>();
        services.AddScoped<IBookImportService, BookImportService>();

        services.AddSingleton<IFilterReader, JsonFilterReader>();
        services.AddScoped<IBookSearchService, BookSearchService>();

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IBookCsvWriter, CsvExportWriter>();

        services.AddScoped<AppRunner>();
    })
    .Build();

using var scope = host.Services.CreateScope();

var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
try
{
    await dbContext.Database.MigrateAsync();
    Console.WriteLine("Database is up to date.");
}
catch (Exception ex)
{
    Console.WriteLine($"DB migration failed: {ex.Message}");
    throw;
}

var app = scope.ServiceProvider.GetRequiredService<AppRunner>();
await app.RunAsync();