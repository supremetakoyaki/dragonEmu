using DragonEmu.Domain;
using DragonEmu.Protocol.Messages;

namespace DragonEmu.Game.Chat.Commands;

public interface IChatCommandDispatcher
{
    ValueTask<bool> TryHandleAsync(IGameSession session, string message, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> GetAvailableCommandsAsync(UserAccount account, CancellationToken cancellationToken);
}
