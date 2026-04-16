namespace Content.Server._Utopia.Genetics.Mutations.Components;

[RegisterComponent]
public sealed partial class MutationBloodRegenerationComponent : Component
{
    /// <summary>
    ///     How much blood (in units) to regenerate per second.
    /// </summary>
    [DataField]
    public float RegenRatePerSecond = 2.0f;

    /// <summary>
    ///     The target blood level percentage (0.0 to 1.0) to passively regenerate up to.
    ///     e.g. 0.8 = regenerate until blood is at 80% of max.
    /// </summary>
    [DataField]
    public float TargetPercentage = 1.0f;
}
