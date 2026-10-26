namespace DragonEmu.Domain;

public sealed record UserGift(int RefItem, int? SenderUserId, IReadOnlyList<GiftContent> Contents);
