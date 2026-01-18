using Robust.Shared.Prototypes;

namespace Content.Shared.Utopia.Economy;

[Prototype]
public sealed partial class SalaryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public Dictionary<string, int> Salaries = new();
}
