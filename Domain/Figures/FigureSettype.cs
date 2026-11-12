namespace DragonEmu.Domain;

public sealed record FigureSettype(string Type, int PaletteId, bool MandatoryMale, bool MandatoryFemale, bool MandatoryMaleClub, bool MandatoryFemaleClub)
{
    public FigureSettype(string Type, int PaletteId, bool Mandatory)
        : this(Type, PaletteId, Mandatory, Mandatory, Mandatory, Mandatory)
    {
    }

    public bool IsMandatory(char gender, int clubLevel) => (char.ToUpperInvariant(gender), clubLevel > 0) switch
    {
        ('M', false) => MandatoryMale,
        ('M', true) => MandatoryMaleClub,
        (_, false) => MandatoryFemale,
        _ => MandatoryFemaleClub,
    };
}
