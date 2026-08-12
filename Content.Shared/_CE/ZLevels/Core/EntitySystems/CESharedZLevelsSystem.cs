/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
*/

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared._Utopia.ZLevels.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Damage.Systems;
using Content.Shared.Gravity;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using JetBrains.Annotations;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._CE.ZLevels.Core.EntitySystems;

public abstract partial class CESharedZLevelsSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] protected readonly SharedMapSystem MapSys = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedPhysicsSystem _physicsSystem = default!;
    [Dependency] private readonly FixtureSystem _fix = default!; // Utopia-Tweak : ZLevels
    [Dependency] private readonly SharedGravitySystem _gravity = default!; // Utopia-Tweak : ZLevels
    [Dependency] private readonly IConfigurationManager _config = default!; // Utopia-Tweak : ZLevels

    private EntityQuery<MapComponent> _mapQuery;
    private EntityQuery<CEZLevelMapComponent> _zMapQuery;
    protected EntityQuery<MapGridComponent> GridQuery;
    protected EntityQuery<CEZPhysicsComponent> ZPhyzQuery;

    public override void Initialize()
    {
        base.Initialize();

        _mapQuery = GetEntityQuery<MapComponent>();
        _zMapQuery = GetEntityQuery<CEZLevelMapComponent>();
        GridQuery = GetEntityQuery<MapGridComponent>();
        ZPhyzQuery = GetEntityQuery<CEZPhysicsComponent>();

        InitMovement();
        InitView();
        InitializeActivation();
    }

    /// <summary>
    /// Checks whether the map is in the zLevels network. If so, returns true and the current depth + Entity of the current zLevels network.
    /// </summary>
    [PublicAPI]
    public bool TryGetZNetwork(EntityUid mapUid, [NotNullWhen(true)] out Entity<CEZLevelsNetworkComponent>? zLevel)
    {
        zLevel = null;
        var query = EntityQueryEnumerator<CEZLevelsNetworkComponent>();
        while (query.MoveNext(out var uid, out var zLevelComp))
        {
            if (!zLevelComp.ZLevels.ContainsValue(mapUid))
                continue;

            zLevel = (uid, zLevelComp);
            return true;
        }

        return false;
    }

    [PublicAPI]
    public bool TryMapOffset(Entity<CEZLevelMapComponent?> inputMapUid,
        int offset,
        [NotNullWhen(true)] out Entity<CEZLevelMapComponent>? outputMapUid)
    {
        outputMapUid = null;
        if (!Resolve(inputMapUid, ref inputMapUid.Comp, false))
            return false;

        var query = EntityQueryEnumerator<CEZLevelsNetworkComponent>();
        while (query.MoveNext(out var network))
        {
            if (!network.ZLevels.ContainsKey(inputMapUid.Comp.Depth)) // Utopia-Tweak : ZLevels
                continue;

            if (!network.ZLevels.TryGetValue(inputMapUid.Comp.Depth + offset, out var targetMapUid))
                continue;

            // Utopia-Tweak : ZLevels
            if (targetMapUid == null)
                continue;
            // Utopia-Tweak : ZLevels

            if (!_zMapQuery.TryComp(targetMapUid.Value, out var targetZLevelComp)) // Utopia-Tweak : ZLevels
                continue;

            outputMapUid = (targetMapUid.Value, targetZLevelComp);
            return true;
        }

        return false;
    }

    [PublicAPI]
    public bool TryMapUp(Entity<CEZLevelMapComponent?> inputMapUid,
        [NotNullWhen(true)] out Entity<CEZLevelMapComponent>? aboveMapUid)
    {
        return TryMapOffset(inputMapUid, 1, out aboveMapUid);
    }

    [PublicAPI]
    public bool TryMapDown(Entity<CEZLevelMapComponent?> inputMapUid,
        [NotNullWhen(true)] out Entity<CEZLevelMapComponent>? belowMapUid)
    {
        return TryMapOffset(inputMapUid, -1, out belowMapUid);
    }

    /// <summary>
    /// Returns a list of all maps above the specified map. The closest map at the top is returned first.
    /// </summary>
    [PublicAPI]
    public List<EntityUid> GetAllMapsAbove(Entity<CEZLevelMapComponent> inputMapUid)
    {
        var result = new List<EntityUid>();

        var inputDepth = inputMapUid.Comp.Depth;
        var query = EntityQueryEnumerator<CEZLevelsNetworkComponent>();
        while (query.MoveNext(out var network))
        {
            if (!network.ZLevels.ContainsValue(inputMapUid))
                continue;

            result.AddRange(
                network.ZLevels
                    .Where(kv => kv.Value.HasValue && kv.Key > inputDepth)
                    .OrderBy(kv => kv.Key)
                    .Select(kv => kv.Value!.Value)
            );
        }
        return result;
    }

    /// <summary>
    /// Returns a list of all maps below the specified map. The closest map at the bottom is returned first.
    /// </summary>
    [PublicAPI]
    public List<EntityUid> GetAllMapsBelow(Entity<CEZLevelMapComponent> inputMapUid)
    {
        var result = new List<EntityUid>();

        var inputDepth = inputMapUid.Comp.Depth;
        var query = EntityQueryEnumerator<CEZLevelsNetworkComponent>();
        while (query.MoveNext(out var network))
        {
            if (!network.ZLevels.ContainsValue(inputMapUid))
                continue;

            foreach (var zLevelEnt in network.ZLevels
                         .Where(kv => kv.Value.HasValue && kv.Key < inputDepth)
                         .OrderByDescending(kv => kv.Key)
                         .Select(kv => kv.Value!.Value))
            {
                result.Add(zLevelEnt);
            }
        }

        return result;
    }

    [PublicAPI]
    public HashSet<EntityUid> GetTargetGrids(EntityUid parentUid)
    {
        var grids = new HashSet<EntityUid> { parentUid };
        if (!TryComp<GridMotionLinkComponent>(parentUid, out var motionLink))
            return grids;

        var targetGroupId = motionLink.GroupId;

        if (!TryComp(parentUid, out TransformComponent? parentXform) || parentXform.MapUid == null)
            return grids;

        if (!TryGetZNetwork(parentXform.MapUid.Value, out var net) || net == null)
            return grids;

        var validMaps = new HashSet<EntityUid>();
        foreach (var level in net.Value.Comp.ZLevels)
        {
            if (level.Value is { Valid: true } map)
            {
                validMaps.Add(map);
            }
        }

        var query = EntityQueryEnumerator<MapGridComponent, TransformComponent, GridMotionLinkComponent>();
        while (query.MoveNext(out var gridUid, out var _, out var gridXform, out var linkComp))
        {
            if (linkComp.GroupId == targetGroupId && gridXform.MapUid != null && validMaps.Contains(gridXform.MapUid.Value))
            {
                grids.Add(gridUid);
            }
        }

        return grids;
    }
}
