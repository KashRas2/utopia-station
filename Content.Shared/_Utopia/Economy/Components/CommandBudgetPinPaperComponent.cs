using Content.Shared.Cargo.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.Utopia.Economy;

[RegisterComponent]
public sealed partial class CommandBudgetPinPaperComponent : Component
{
    [DataField]
    public ProtoId<CargoAccountPrototype>? CommandBudgetType;
}
