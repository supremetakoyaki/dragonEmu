using DragonEmu.Protocol.Messages;

namespace DragonEmu.Game.Chat.Commands;

public interface IChatCommand
{
    string Name { get; }

    string? RequiredFuse { get; }

    ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken);
}
