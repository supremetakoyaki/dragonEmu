using DragonEmu.Game.Help;
using DragonEmu.Domain;
using DragonEmu.Protocol.Messages.Outgoing.Help;

namespace DragonEmu.Protocol.Messages.Incoming.Help;

public sealed class GetClientFaqsMessageComposer : IMessageComposer
{
    private readonly HelpFaqCache _faq;

    public GetClientFaqsMessageComposer(HelpFaqCache faq)
    {
        _faq = faq;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        (IReadOnlyList<FaqEntry> urgent, IReadOnlyList<FaqEntry> normal) = _faq.FrontPage;
        session.Send(new FaqClientFaqsMessageEvent(urgent, normal).Compose());
        return ValueTask.CompletedTask;
    }
}
