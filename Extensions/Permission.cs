namespace AnnaYanami.Extensions.Permission;

using Discord;
using Discord.Interactions;

using AnnaYanami.Repositories;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CheckBannedAttribute : PreconditionAttribute
{

	public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
	{
		var _userRepository = services.GetService(typeof(UserRepository)) as UserRepository;
		if (_userRepository == null)
		{
			return PreconditionResult.FromError("✧◝(⁰▿⁰)◜✧ Oopsie woopsie! The UserRepository service seems to be playing hide and seek~!");
		}

		var user = await _userRepository.GetOrCreateUserAsync(context.User.Id);
		if (user.IsBanned)
		{
			return PreconditionResult.FromError("(╯°□°）╯︵ ┻━┻ Gomen ne~ You've been banned from using this command!");
		}

		return PreconditionResult.FromSuccess();
	}
}