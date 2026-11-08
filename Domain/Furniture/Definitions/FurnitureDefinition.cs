namespace DragonEmu.Domain;

public sealed record FurnitureDefinition(
    int Id,
    FurnitureType Type,
    int RefFurnidata,
    string Classname,
    int? XDim,
    int? YDim,
    double? ZDim,
    string LocalizationName,
    string LocalizationDesc,
    FurnitureLogicClass LogicClass,
    FurnitureVisualizationClass VisualizationClass,
    FurnitureInteractionLogic InteractionLogic,
    IReadOnlyList<int>? States,
    IReadOnlyList<int>? InteractionIds,
    bool IsSittable,
    bool IsBed,
    bool IsWalkable,
    bool IsStackable,
    bool AllowTrade,
    bool AllowRecycle,
    LocalizedText? Names = null,
    LocalizedText? Descriptions = null)
{
    public string NameIn(Language language) => Names?.For(language) is { Length: > 0 } name ? name : LocalizationName;

    public string DescriptionIn(Language language) => Descriptions?.For(language) is { Length: > 0 } description ? description : LocalizationDesc;
}
