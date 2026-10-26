using DragonEmu.Game.Avatars;
using DragonEmu.Protocol.Messages.Outgoing.Avatar;

namespace DragonEmu.Protocol.Messages.Incoming.Avatar;

public sealed class CheckUserNameMessageComposer : IMessageComposer
{
    private readonly UsernameChangeService _usernameChange;

    private string _name = string.Empty;

    public CheckUserNameMessageComposer(UsernameChangeService usernameChange)
    {
        _usernameChange = usernameChange;
    }

    public void Parse(ClientPacket packet)
    {
        _name = packet.PopString();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account || !session.NameChangeDialogOpen
            || !session.TryUseCooldown("name_check", TimeSpan.FromMilliseconds(500)))
        {
            return;
        }

        UsernameVerdict verdict = await _usernameChange.CheckAsync(account, _name, cancellationToken);
        session.Send(new CheckUserNameResultMessageEvent((int)verdict.Check, verdict.Name, verdict.Suggestions).Compose());
    }
}
