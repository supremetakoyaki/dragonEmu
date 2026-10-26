using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace DragonEmu.Logging;

public sealed class DragonConsoleFormatter : ConsoleFormatter
{
    public const string FormatterName = "dragon";

    private const string ResetColor = "\x1B[39m\x1B[22m";

    private static readonly bool Colored = !Console.IsOutputRedirected;

    public DragonConsoleFormatter()
        : base(FormatterName)
    {
    }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        string message = logEntry.Formatter(logEntry.State, logEntry.Exception);
        if (message.Length == 0 && logEntry.Exception is null)
        {
            return;
        }

        (string tag, ConsoleColor color) = Tag(logEntry.LogLevel);
        textWriter.Write(Colored ? $"{Foreground(color)}{tag}{ResetColor} " : $"{tag} ");
        textWriter.Write(message);

        if (logEntry.Exception is { } exception)
        {
            textWriter.Write(Environment.NewLine);
            textWriter.Write(exception);
        }

        textWriter.Write(Environment.NewLine);
    }

    private static (string Tag, ConsoleColor Color) Tag(LogLevel level) => level switch
    {
        LogLevel.Information => ("[INFO]", ConsoleColor.Cyan),
        LogLevel.Warning => ("[WARN]", ConsoleColor.Yellow),
        LogLevel.Error or LogLevel.Critical => ("[ERROR]", ConsoleColor.Red),
        _ => ("[DEBUG]", ConsoleColor.Green),
    };

    private static string Foreground(ConsoleColor color) => color switch
    {
        ConsoleColor.Black => "\x1B[30m",
        ConsoleColor.DarkRed => "\x1B[31m",
        ConsoleColor.DarkGreen => "\x1B[32m",
        ConsoleColor.DarkYellow => "\x1B[33m",
        ConsoleColor.DarkBlue => "\x1B[34m",
        ConsoleColor.DarkMagenta => "\x1B[35m",
        ConsoleColor.DarkCyan => "\x1B[36m",
        ConsoleColor.Gray => "\x1B[37m",
        ConsoleColor.Red => "\x1B[1m\x1B[31m",
        ConsoleColor.Green => "\x1B[1m\x1B[32m",
        ConsoleColor.Yellow => "\x1B[1m\x1B[33m",
        ConsoleColor.Blue => "\x1B[1m\x1B[34m",
        ConsoleColor.Magenta => "\x1B[1m\x1B[35m",
        ConsoleColor.Cyan => "\x1B[1m\x1B[36m",
        ConsoleColor.White => "\x1B[1m\x1B[37m",
        _ => ResetColor,
    };
}
