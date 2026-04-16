using Content.Shared._Utopia.Genetics;

namespace Content.Server._Utopia.Genetics.Mutations.Components;

[RegisterComponent]
public sealed partial class PolymorphGeneticsRestoreDataComponent : Component
{
    public List<MutationEntry> MutationSnapshot = new();
    public HashSet<string> EnabledMutationIds = new();
    public int GeneticInstability;
    public HashSet<string> BaseMutationIds = new();
}
