namespace Content.Server._Utopia.Economy;

[RegisterComponent]
public sealed partial class BankCartridgeComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public int? AccountId;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Loader;

    public string AccountLinkResult = string.Empty;

    public string TransferResult = string.Empty;
}
