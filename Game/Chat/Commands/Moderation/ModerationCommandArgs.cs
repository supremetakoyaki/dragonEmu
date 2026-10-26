namespace DragonEmu.Game.Chat.Commands;

public static class ModerationCommandArgs
{
    public static bool TryDurationAndText(string[] args, int index, out int? duration, out string? text)
    {
        duration = null;
        text = null;
        if (args.Length > index && int.TryParse(args[index], out int number))
        {
            if (number < 1)
            {
                return false;
            }

            duration = number;
            index++;
        }

        text = args.Length > index ? string.Join(' ', args[index..]) : null;
        return true;
    }
}
