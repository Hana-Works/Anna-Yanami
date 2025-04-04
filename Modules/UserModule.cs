namespace AnnaYanami.Modules;

using AnnaYanami.Repositories;
using Discord;
using Discord.Interactions;

[Group("user", "User related commands")]
public class UserModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly UserRepository _userRepository;

    public UserModule(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [SlashCommand("profile", "View your profile or another user's profile")]
    public async Task ProfileAsync(
        [Summary("user", "The user whose profile you want to view")] IUser user)
    {
        try
        {
            var userProfile = await _userRepository.GetOrCreateUserAsync(user.Id);

            var embed = new EmbedBuilder()
                .WithAuthor(user)
                .WithTitle("✨ User Profile ✨")
                .WithColor(new Color(255, 192, 203))
                .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                .AddField("🎫 Quota Remaining", userProfile.Quota.ToString(), true)
                .WithFooter($"Requested by {Context.User.Username}", Context.User.GetAvatarUrl())
                .WithCurrentTimestamp()
                .Build();

            await RespondAsync(embed: embed, ephemeral: true);
        }
        catch (Exception ex)
        {
            var errorEmbed = new EmbedBuilder()
                .WithTitle("💫 Oopsie! Something went wrong~ 💫")
                .WithDescription($"*Nya~* We had a little problem: {ex.Message}")
                .WithColor(new Color(255, 182, 193)) 
                .WithFooter("🌸 Don't worry! Please try again later! 🌸")
                .WithCurrentTimestamp()
                .Build();

            await RespondAsync(embed: errorEmbed, ephemeral: true);
        }
    }

    [SlashCommand("quota", "Set user quota")]
    [RequireOwner]
    public async Task QuotaAsync() 
    {
        await RespondAsync("This command is not implemented yet.", ephemeral: true);
    }
}