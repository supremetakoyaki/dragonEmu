using DragonEmu.Game.Catalog;
using DragonEmu.Gateway;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Catalog;

namespace DragonEmu.Game.Chat.Commands;

public sealed class UpdateCatalogChatCommand : IChatCommand
{
    private readonly CatalogCache _catalogCache;
    private readonly ClientSessionManager _sessionManager;

    public UpdateCatalogChatCommand(CatalogCache catalogCache, ClientSessionManager sessionManager)
    {
        _catalogCache = catalogCache;
        _sessionManager = sessionManager;
    }

    public string Name => "update_catalog";

    public string RequiredFuse => "fuse_administrator_access";

    public async ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        await _catalogCache.ReloadAsync(cancellationToken);
        _sessionManager.BroadcastToAll(new CatalogPublishedMessageEvent().Compose().ToArray());
    }
}
