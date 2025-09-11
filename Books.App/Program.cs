using Books.App;
using Books.Infrastructure.Data;
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

        services.AddTransient<AppRunner>();
    })
    .Build();

using var scope = host.Services.CreateScope();

var app = scope.ServiceProvider.GetRequiredService<AppRunner>();
app.Run();