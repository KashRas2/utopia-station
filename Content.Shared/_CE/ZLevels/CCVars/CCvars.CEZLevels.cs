using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    public static readonly CVarDef<float>
        CEBaseFallingDamage = CVarDef.Create("zlevels.ce_base_falling_damage", 2.5f, CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<float>
        CEBaseFallingOtherDamage = CVarDef.Create("zlevels.ce_base_falling_other_damage", 0.5f, CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<float>
        CEBaseFallingStunTime = CVarDef.Create("zlevels.ce_base_falling_stun_time", 0.2f, CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<float>
        CEBaseFallingOtherStunTime = CVarDef.Create("zlevels.ce_base_falling_other_stun_time", 0.05f, CVar.SERVER | CVar.REPLICATED);
}
