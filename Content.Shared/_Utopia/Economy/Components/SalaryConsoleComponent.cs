using Content.Shared.StationRecords;
using Robust.Shared.Serialization;
using Robust.Shared.Audio;

namespace Content.Shared._Utopia.Economy;

[RegisterComponent]
public sealed partial class SalaryConsoleComponent : Component
{
    public const string BudgetCardSlotId = "BankCardSlot";

    [ViewVariables]
    public uint? ActiveKey;

    [ViewVariables]
    public StationRecordsFilter? Filter;

    [DataField]
    public SoundSpecifier SoundDeny = new SoundPathSpecifier("/Audio/_Utopia/Machines/buzz-sigh.ogg");
}

[Serializable, NetSerializable]
public enum SalaryConsoleUiKey
{
    Key
}
