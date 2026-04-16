using Robust.Shared.Prototypes;

namespace Content.Server._Utopia.Genetics.Mutations.Components;

[RegisterComponent]
public sealed partial class MutationAdrenalineRushComponent : Component
{
    [DataField(required: true)]
    public EntProtoId ActionId = "ActionAdrenalineRush";

    public EntityUid? GrantedAction;
}
