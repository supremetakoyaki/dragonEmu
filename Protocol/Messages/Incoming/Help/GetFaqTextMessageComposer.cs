using DragonEmu.Game.Help;
using DragonEmu.Protocol.Messages.Outgoing.Help;

namespace DragonEmu.Protocol.Messages.Incoming.Help;

public sealed class GetFaqTextMessageComposer : IMessageComposer
{
    private readonly HelpFaqCache _faq;

    private int _entryId;

    public GetFaqTextMessageComposer(HelpFaqCache faq)
    {
        _faq = faq;
    }

    public void Parse(ClientPacket packet)
    {
        _entryId = packet.PopInt32();
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (_faq.GetEntry(_entryId) is { } entry)
        {
            session.Send(new FaqTextMessageEvent(entry).Compose());
        }

        return ValueTask.CompletedTask;
    }
}
