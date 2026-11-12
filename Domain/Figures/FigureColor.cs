namespace DragonEmu.Domain;

public sealed record FigureColor(int Id, int PaletteId, string Hex, int ClubLevel, bool Selectable)
{
    public FigureColor(int Id, int PaletteId, string Hex, bool Club, bool Selectable)
        : this(Id, PaletteId, Hex, Club ? 1 : 0, Selectable)
    {
    }

    public bool Club => ClubLevel > 0;
}
