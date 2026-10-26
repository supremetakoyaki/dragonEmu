using DragonEmu.Domain;
using DragonEmu.Game.Fuses;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Avatar;

namespace DragonEmu.Game.Avatars;

public sealed class WardrobeService
{
    public const int Slots = 10;

    private const int SmallWardrobeSlots = 5;
    private const string WardrobeFuse = "fuse_use_wardrobe";
    private const string LargerWardrobeFuse = "fuse_larger_wardrobe"; // this could be unused in V48 since it is a VIP fuse, but exists
    private const string ClubOutfitsFuse = "fuse_use_club_outfits";

    private readonly IWardrobeRepository _wardrobeRepository;
    private readonly FuseService _fuses;
    private readonly FigureDataCache _figureData;

    public WardrobeService(IWardrobeRepository wardrobeRepository, FuseService fuses, FigureDataCache figureData)
    {
        _wardrobeRepository = wardrobeRepository;
        _fuses = fuses;
        _figureData = figureData;
    }

    public async Task SendAsync(IGameSession session, UserAccount account, CancellationToken cancellationToken)
    {
        bool small = await _fuses.HoldsAsync(account, WardrobeFuse, cancellationToken: cancellationToken);
        bool larger = await _fuses.HoldsAsync(account, LargerWardrobeFuse, cancellationToken: cancellationToken);
        IReadOnlyList<WardrobeOutfit> outfits = small || larger
            ? [.. (await _wardrobeRepository.GetAsync(account.Id, cancellationToken)).Where(o => o.Slot <= SmallWardrobeSlots ? small : larger)]
            : [];

        session.Send(new WardrobeMessageEvent(small, outfits).Compose());
    }

    public async Task<bool> SaveAsync(UserAccount account, int slot, string figure, char gender, CancellationToken cancellationToken)
    {
        if (slot is < 1 or > Slots || gender is not ('M' or 'F')
            || !await _fuses.HoldsAsync(account, slot <= SmallWardrobeSlots ? WardrobeFuse : LargerWardrobeFuse, cancellationToken: cancellationToken))
        {
            return false;
        }

        bool clubClothes = await _fuses.HoldsAsync(account, ClubOutfitsFuse, cancellationToken: cancellationToken);
        if (FigureSanitizer.Sanitize(_figureData, figure, gender, clubClothes) is not { } filtered)
        {
            return false;
        }

        await _wardrobeRepository.SaveAsync(account.Id, new WardrobeOutfit(slot, filtered, gender), cancellationToken);
        return true;
    }
}
