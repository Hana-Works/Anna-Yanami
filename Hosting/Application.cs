namespace AnnaYanami.Hosting;

using System.Reflection;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.WebSocket;
using Discord.Interactions;

using AnnaYanami.Extensions.Logger;

public class Application : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _configuration;
    private readonly InteractionService _interactions;
    private readonly ILogger<Application> _logger;

    public Application(
        IServiceProvider services,
        IConfiguration configuration,
        InteractionService interactions,
        ILogger<Application> logger,
        DiscordSocketClient client)
    {
        _services = services;
        _configuration = configuration;
        _interactions = interactions;
        _logger = logger;
        _client = client;

        _client.Log += LogAsync;
        _interactions.Log += LogAsync;

        _client.Ready += OnReadyAsync;
        _client.InteractionCreated += OnInteractionCreatedAsync;
    }

    private Task LogAsync(LogMessage logMessage)
    {
        _logger.Log(logMessage.Severity.ToLogLevel(), logMessage.Exception, logMessage.Message);
        return Task.CompletedTask;
    }


    private async Task OnReadyAsync()
    {
        _logger.LogInformation($"Client connected as {_client.CurrentUser.Username}");
        await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        await _interactions.RegisterCommandsGloballyAsync();
        _logger.LogInformation("Registered total of {Count} commands", _interactions.Modules.Count);

    }

    private async Task OnInteractionCreatedAsync(SocketInteraction interaction)
    {
        var context = new SocketInteractionContext(_client, interaction);
        await _interactions.ExecuteCommandAsync(context, _services);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var token = _configuration["Discord:Token"];
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("Discord bot token is not configured.");
        }

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.LogoutAsync();
        await _client.StopAsync();
    }
}