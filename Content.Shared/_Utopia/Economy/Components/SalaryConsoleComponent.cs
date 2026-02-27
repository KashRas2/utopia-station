using Content.Shared.StationRecords;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Utopia.Economy;

/// <summary>
/// Economy management console: insert a command budget card, select a crew member from station records,
/// and send money from the card to their account.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SalaryConsoleComponent : Component
{
    public const string BudgetCardSlotId = "BankCardSlot";

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public uint? ActiveKey;

    [DataField]
    public StationRecordsFilter? Filter;
}

[Serializable, NetSerializable]
public enum SalaryConsoleUiKey
{
    Key
}
