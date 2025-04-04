using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;

using Discord;
using Discord.WebSocket;
using Discord.Interactions;

using AnnaYanami.Hosting;
using AnnaYanami.Services;
using MongoDB.Driver;
using AnnaYanami.Repositories;
using Nekoyama.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddLogging(log =>
{
    log.ClearProviders();
    log.AddConsole();
    log.AddDebug();
    log.AddSimpleConsole(options =>
    {
        options.IncludeScopes = true;
        options.SingleLine = true;
        options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
    });
});

builder.Services.Configure<DiscordSocketConfig>(config => {
    config.GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers;
    config.MessageCacheSize = 100;
    config.AlwaysDownloadUsers = true;
    config.LogLevel = LogSeverity.Info;
    config.DefaultRetryMode = RetryMode.RetryRatelimit;
});

builder.Services.Configure<InteractionServiceConfig>(config => {
    config.UseCompiledLambda = true;
    config.DefaultRunMode = RunMode.Async;
    config.LogLevel = LogSeverity.Info;
});

builder.Services.AddSingleton(services => {
    var config = services.GetRequiredService<IConfiguration>();

    if (string.IsNullOrEmpty(config["MongoDB:Uri"]) || string.IsNullOrEmpty(config["MongoDB:Database"]))
    {
        throw new ArgumentNullException("MongoDB:Uri or MongoDB:Database is not set in configuration.");
    }

    var mongoClient = new MongoClient(config["MongoDB:Uri"]);
    return mongoClient.GetDatabase(config["MongoDB:Database"]);
});

builder.Services.AddSingleton<MongoDBService>();

builder.Services.AddScoped<UserRepository>();

builder.Services.AddHttpClient<StableDiffusionService>((provider, client) =>
    {
        var config = provider.GetRequiredService<IConfiguration>();
        if (string.IsNullOrEmpty(config["StableDiffusion:Uri"]))
        {
            throw new ArgumentNullException("StableDiffusion:Uri is not set in configuration.");
        }
        client.BaseAddress = new Uri(config["StableDiffusion:Uri"]!);
        client.Timeout = TimeSpan.FromMinutes(5);
    }
);

builder.Services.AddSingleton(services => {
    var config = services.GetRequiredService<IOptions<DiscordSocketConfig>>().Value;
    return new DiscordSocketClient(config);
});

builder.Services.AddSingleton(services => {
    var client = services.GetRequiredService<DiscordSocketClient>();
    var config = services.GetRequiredService<IOptions<InteractionServiceConfig>>().Value;
    return new InteractionService(client, config);
});

builder.Services.AddHostedService<Application>();

var app = builder.Build();
await app.RunAsync();