namespace AnnaYanami.Extensions.Logger;

using Discord;
using Microsoft.Extensions.Logging;

public static class LogSeverityExtensions
{
	public static LogLevel ToLogLevel(this LogSeverity severity)
	{
		return severity switch
		{
			LogSeverity.Critical => LogLevel.Critical,
			LogSeverity.Error => LogLevel.Error,
			LogSeverity.Warning => LogLevel.Warning,
			LogSeverity.Info => LogLevel.Information,
			LogSeverity.Verbose => LogLevel.Debug,
			LogSeverity.Debug => LogLevel.Trace,
			_ => LogLevel.None,
		};
	}
}