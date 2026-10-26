using DragonEmu.Game.Help;
using DragonEmu.Protocol.Messages.Outgoing.Help;

namespace DragonEmu.Protocol.Messages.Incoming.Help;

public sealed class SearchFaqsMessageComposer : IMessageComposer
{
    private const int MinLength = 3;
    private const int MaxResults = 20;

    private readonly HelpFaqCache _faq;

    private string _text = string.Empty;

    public SearchFaqsMessageComposer(HelpFaqCache faq)
    {
        _faq = faq;
    }

    public void Parse(ClientPacket packet)
    {
        _text = packet.PopString().Trim();
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (_text.Length >= MinLength)
        {
            session.Send(new FaqSearchResultsMessageEvent(_faq.Search(_text, MaxResults)).Compose());
        }

        return ValueTask.CompletedTask;
    }
}
