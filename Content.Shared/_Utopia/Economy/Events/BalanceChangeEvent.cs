using JetBrains.Annotations;

namespace Content.Shared.Utopia.Economy;

[PublicAPI]
public sealed class BalanceChange : EntityEventArgs
{
    public EntityUid? CartridgeUid { get; set; }

    public BalanceChange(EntityUid? cartridgeuid)
    {
        CartridgeUid = cartridgeuid;
    }
}

