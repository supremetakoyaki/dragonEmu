using DragonEmu.Game.Avatars;
using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Incoming.Avatar;

public sealed class SaveWardrobeOutfitMessageComposer : IMessageComposer
{
    private const int MaxFigureLength = 255;

    private readonly WardrobeService _wardrobe;

    private int _slot;
    private string _figure = string.Empty;
    private char _gender;

    public SaveWardrobeOutfitMessageComposer(WardrobeService wardrobe)
    {
        _wardrobe = wardrobe;
    }

    public void Parse(ClientPacket packet)
    {
        _slot = packet.PopInt32();
        _figure = packet.PopString();
        string gender = packet.PopString();
        _gender = gender.Length > 0 ? char.ToUpperInvariant(gender[0]) : '\0';
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account && _figure.Length is > 0 and <= MaxFigureLength
            && session.TryUseCooldown(ActionCooldowns.WardrobeSlot(_slot), ActionCooldowns.Default))
        {
            await _wardrobe.SaveAsync(account, _slot, _figure, _gender, cancellationToken);
        }
    }
}
