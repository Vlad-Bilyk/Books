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
            options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddSingleton<IConsoleWrapper, ConsoleWrapper>();
        services.AddSingleton<IFileReader, StreamFileReader>();
        services.AddSingleton<IBookParser, BookCsvParser>();
        services.AddScoped<IBookImportService, BookImportService>();

        services.AddTransient<AppRunner>();
    })
    .Build();

using var scope = host.Services.CreateScope();

var app = scope.ServiceProvider.GetRequiredService<AppRunner>();
await app.RunAsync();