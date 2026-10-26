using DragonEmu.Game.Help;
using DragonEmu.Protocol.Messages.Outgoing.Help;

namespace DragonEmu.Protocol.Messages.Incoming.Help;

public sealed class GetFaqCategoryMessageComposer : IMessageComposer
{
    private readonly HelpFaqCache _faq;

    private int _categoryId;

    public GetFaqCategoryMessageComposer(HelpFaqCache faq)
    {
        _faq = faq;
    }

    public void Parse(ClientPacket packet)
    {
        _categoryId = packet.PopInt32();
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (_faq.GetCategory(_categoryId) is { } category)
        {
            session.Send(new FaqCategoryMessageEvent(category).Compose());
        }

        return ValueTask.CompletedTask;
    }
}
