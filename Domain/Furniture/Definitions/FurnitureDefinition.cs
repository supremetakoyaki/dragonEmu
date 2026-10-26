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
    bool AllowRecycle);
