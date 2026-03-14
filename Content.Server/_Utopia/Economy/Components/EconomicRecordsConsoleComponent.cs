using Content.Shared.StationRecords;

namespace Content.Server._Utopia.Economy;

[RegisterComponent, Access(typeof(EconomicRecordsConsoleSystem))]
public sealed partial class EconomicRecordsConsoleComponent : Component
{
    [DataField]
    public uint? ActiveKey;

    [DataField]
    public StationRecordsFilter? Filter;
}

