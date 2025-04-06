namespace AnnaYanami.Modules;

using System.Text.RegularExpressions;

using Discord;
using Discord.Interactions;

using AnnaYanami.Services;
using AnnaYanami.Repositories;
using AnnaYanami.Extensions.Permission;

[Group("wfx", "WFX commands")]
public partial class WfxModule : InteractionModuleBase<SocketInteractionContext>
{
	private readonly UserRepository _userRepository;
	private readonly StableDiffusionService _stableDiffusion;

	public WfxModule(
		UserRepository userRepository, 
		StableDiffusionService stableDiffusion)
	{
		_userRepository = userRepository;
		_stableDiffusion = stableDiffusion;
	}

	[CheckBanned]
	[SlashCommand("dream", "Generate art using our AI models")]
	public async Task DreamAsync(
		[Summary("prompt", "Enter your image generation prompt")] string prompt,
		[Summary("negative", "Enter negative prompts to exclude from generation")] string negativePrompt = "lowres, bad anatomy, bad hands, text, error, missing finger, extra digits, fewer digits, cropped, worst quality, low quality, low score, bad score, average score, signature, watermark, username, blurry",
		[Summary("ratio", "Set image dimensions (e.g., 1024x1024)")] string ratio = "1024x1024",
		[Summary("steps", "Set generation steps (1-50)")] int steps = 25,
		[Summary("cfg", "Set CFG scale value")] double cfg = 6.0,
		[Summary("seeds", "Set generation seed (-1 for random)")] int seeds = -1)
	{
		try
		{
			await DeferAsync();
			var user = await _userRepository.GetOrCreateUserAsync(Context.User.Id);
			
			if (user.Quota <= 0)
			{
				await Context.Interaction.DeleteOriginalResponseAsync();
				var noQuotaEmbed = new EmbedBuilder()
					.WithTitle("❌ Insufficient Quota")
					.WithDescription("You have no quota left, please contact the administrator.")
					.WithColor(Color.Red)
					.WithAuthor(name: "✨ Anna Yanami AI", iconUrl: Context.Client.CurrentUser.GetAvatarUrl())
					.WithCurrentTimestamp();
				
				await FollowupAsync(embed: noQuotaEmbed.Build(), ephemeral: true);
				return;
			}

			var processingEmbed = new EmbedBuilder()
				.WithTitle("🌸 Creating Magic ｡･:*:･ﾟ★")
				.WithDescription("*:･ﾟ✧ Sprinkling fairy dust on your dream... Please wait~ ✧ﾟ･:*")
				.WithColor(new Color(255, 182, 193)) 
				.WithAuthor(name: "⋆｡˚ Anna Yanami AI ⋆｡˚", iconUrl: Context.Client.CurrentUser.GetAvatarUrl())
				.WithFooter(text: "ʚ Channeling magical energies ɞ")
				.WithCurrentTimestamp();

			await FollowupAsync(embed: processingEmbed.Build());

			var dimension = GetDimension(ratio);
			var generationConfig = new StableDiffusionConfig(
				prompt,
				negativePrompt,
				steps,
				cfg,
				dimension.Width,
				dimension.Height,
				seeds == -1 ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() : seeds
			);
			var generationResult = await _stableDiffusion.GenerateAsync(generationConfig);
			
			var successEmbed = new EmbedBuilder()
				.WithTitle("🌟✨ Magic Dream Created! ✨🌟")
				.WithDescription("*:･ﾟ✧ Your magical creation has come to life! ✧ﾟ･:*")
				.WithFields(
					new EmbedFieldBuilder()
						.WithName("🎨 Magical Prompt ୨୧")
						.WithValue(prompt)
						.WithIsInline(false),
					new EmbedFieldBuilder()
						.WithName("🌙 Negative Enchantments ❀")
						.WithValue(negativePrompt)
						.WithIsInline(false),
					new EmbedFieldBuilder()
						.WithName("🎐 Mystic Settings ⋆｡˚")
						.WithValue($"✧ Steps: {steps}\n🌈 CFG Scale: {cfg}\n🎲 Seed: {generationConfig.Seed}\n📐 Canvas: {dimension.Width}x{dimension.Height}")
						.WithIsInline(false),
					new EmbedFieldBuilder()
						.WithName("⭐ Enchanted Model ੭")
						.WithValue($"✦ {generationResult.Info.SdModelName} ({generationResult.Info.SdModelHash}) ✦")
						.WithIsInline(false)
				)
				.WithColor(new Color(255, 182, 193)) 
				.WithImageUrl("attachment://image.png")
				.WithAuthor(name: "⋆｡˚ Anna Yanami AI ⋆｡˚", iconUrl: Context.Client.CurrentUser.GetAvatarUrl())
				.WithFooter(text: "ʚ Generated with love ɞ")
				.WithCurrentTimestamp();

			await ModifyOriginalResponseAsync(prop => {
				prop.Embed = successEmbed.Build();
				prop.Attachments = new[] { new FileAttachment(new MemoryStream(generationResult.Image), "image.png") };
			});

			await _userRepository.DecrementQuotaAsync(Context.User.Id, 1);
		}
		catch (Exception ex)
		{
			var errorEmbed = new EmbedBuilder()
				.WithTitle("⚠️ Oopsie!")
				.WithDescription($"Something went wrong: {ex.Message}")
				.WithColor(Color.Red)
				.WithAuthor(name: "✨ Anna Yanami AI", iconUrl: Context.Client.CurrentUser.GetAvatarUrl())
				.WithCurrentTimestamp();

  
			await Context.Interaction.DeleteOriginalResponseAsync();
			await FollowupAsync(embed: errorEmbed.Build(), ephemeral: true);
		}
	}

	private record Dimension(int Width, int Height);

	[GeneratedRegex(@"(\d+)\s*[xX]\s*(\d+)")]
	private static partial Regex DimensionRegex();

	private static Dimension GetDimension(string ratio)
	{
		Match pattern = DimensionRegex().Match(ratio);

		if (!pattern.Success)
		{
			throw new Exception("Width/Height must be a Int!");
		}

		return new(int.Parse(pattern.Groups[1].Value), int.Parse(pattern.Groups[2].Value));
	}
}