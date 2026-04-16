namespace Content.Server._Utopia.Genetics.Mutations.Components;

[RegisterComponent]
public sealed partial class MutationLungSwapComponent : Component
{
    [DataField(required: true)]
    public string NewLungPrototype = default!;

    public EntityUid? OriginalLung { get; set; }

    public EntityUid? SwappedLung { get; set; }
}
