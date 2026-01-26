using Content.Shared.Cargo.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Content.Shared.Roles;

namespace Content.Shared.Utopia.Economy;

[RegisterComponent, NetworkedComponent]
public sealed partial class BankCardComponent : Component
{
    [DataField]
    public int? AccountId;

    [DataField]
    public int StartingBalance;

    [DataField]
    public bool CommandBudgetCard;

    [DataField]
    public ProtoId<CargoAccountPrototype>? CommandBudgetType;

    [DataField]
    public List<ProtoId<JobPrototype>> Jobs;
}
