namespace AnnaYanami.Hosting;

using System.Reflection;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Discord;
using Discord.WebSocket;
using Discord.Interactions;

using AnnaYanami.Extensions.Logger;

public class ApplicationHost : IHostedService
{
	private readonly IServiceProvider _services;
	private readonly DiscordSocketClient _client;
	private readonly IConfiguration _configuration;
	private readonly InteractionService _interactions;
	private readonly ILogger<ApplicationHost> _logger;

	public ApplicationHost(
		IServiceProvider services,
		IConfiguration configuration,
		InteractionService interactions,
		ILogger<ApplicationHost> logger,
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
		_interactions.SlashCommandExecuted += OnSlashCommandExecutedAsync;
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

	private async Task OnSlashCommandExecutedAsync(SlashCommandInfo commandInfo, IInteractionContext context, IResult result)
	{
		if (result.IsSuccess)
		{
			_logger.LogInformation("Executed command {CommandName} by {User}", commandInfo.Name, context.User.Username);
		}
		else
		{
			_logger.LogError("Failed to execute command {CommandName} by {User}: {Error}", commandInfo.Name, context.User.Username, result.ErrorReason);
	   
			string errorMessage = result is PreconditionResult ? result.ErrorReason : "Something went wrong while executing the command.";
			
			var errorEmbed = new EmbedBuilder()
				.WithTitle("💫 Oopsie! Something went wrong~ 💫")
				.WithDescription($"*Nya~* {errorMessage}")
				.WithColor(new Color(255, 182, 193)) 
				.WithFooter("🌸 Don't worry! Please try again later! 🌸")
				.WithCurrentTimestamp()
				.Build();
			
			if (context.Interaction.HasResponded)
			{
				await context.Interaction.DeleteOriginalResponseAsync();
				await context.Interaction.FollowupAsync(embed: errorEmbed, ephemeral: true);
			}
			else
				await context.Interaction.RespondAsync(embed: errorEmbed, ephemeral: true);
		}
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