using DragonEmu.Game.Help;
using DragonEmu.Protocol.Messages.Outgoing.Help;

namespace DragonEmu.Protocol.Messages.Incoming.Help;

public sealed class GetFaqCategoriesMessageComposer : IMessageComposer
{
    private readonly HelpFaqCache _faq;

    public GetFaqCategoriesMessageComposer(HelpFaqCache faq)
    {
        _faq = faq;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        session.Send(new FaqCategoriesMessageEvent(_faq.Categories).Compose());
        return ValueTask.CompletedTask;
    }
}
