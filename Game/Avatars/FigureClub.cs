using DragonEmu.Domain;
using DragonEmu.Game.Fuses;

namespace DragonEmu.Game.Avatars;

public static class FigureClub
{
    public const string ClubOutfitsFuse = "fuse_use_club_outfits";
    public const string VipOutfitsFuse = "fuse_use_vip_outfits";

    public static async Task<int> LevelAsync(FuseService fuses, UserAccount account, CancellationToken cancellationToken = default)
    {
        if (await fuses.HoldsAsync(account, VipOutfitsFuse, cancellationToken: cancellationToken))
        {
            return 2;
        }

        return await fuses.HoldsAsync(account, ClubOutfitsFuse, cancellationToken: cancellationToken) ? 1 : 0;
    }
}
