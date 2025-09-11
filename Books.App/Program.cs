using Books.App;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddTransient<AppRunner>();

// Register application services
using var serviceProvider = services.BuildServiceProvider();

var app = serviceProvider.GetRequiredService<AppRunner>();
app.Run();