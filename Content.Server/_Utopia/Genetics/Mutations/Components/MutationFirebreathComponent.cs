namespace Content.Server._Utopia.Genetics.Mutations.Components;

[RegisterComponent]
public sealed partial class MutationFirebreathComponent : Component
{
    [DataField]
    public float Cooldown = 25f;

    public TimeSpan NextUse = TimeSpan.Zero;

    public EntityUid? GrantedAction;
}
