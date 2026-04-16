namespace Content.Server._Utopia.Genetics.Components;

[RegisterComponent]
public sealed partial class DnaSequenceInjectorComponent : Component
{
    [DataField]
    public string? MutationId;

    [DataField]
    public bool IsMutator = false;
}
