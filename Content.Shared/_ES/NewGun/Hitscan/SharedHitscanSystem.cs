using System.Numerics;
using Content.Shared._ES.NewGun.Fetch;
using Content.Shared.Construction.Conditions;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._ES.NewGun.Hitscan;

public abstract partial class SharedHitscanSystem : EntitySystem
{
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private FetchGunSystem _fetch = default!;

    /// <summary>
    /// See if a gun is a hitscan gun.
    /// If it is, return its component.
    /// </summary>
    public Entity<NGHitscanGunComponent>? GetHitscanGun(Entity<NGGunComponent> gun)
    {
        if (!TryComp<NGHitscanGunComponent>(gun, out var hitscanGunComp))
            return null;

        return (gun, hitscanGunComp);
    }

    /// <summary>
    /// Spawn a hitscan entity to raise events on.
    /// The entity is spawned in nullspace and is not networked.
    /// </summary>
    public Entity<NGHitscanComponent>? SpawnHitscan(Entity<NGHitscanGunComponent> gun)
    {
        var ev = new GetHitscanEvent();
        RaiseLocalEvent(gun, ref ev);
        if (ev.Proto is not { } proto)
            return null;

        var hitscan = Spawn(proto);
        if (!TryComp<NGHitscanComponent>(hitscan, out var hitscanComp))
        {
            Log.Warning("Hitscan spawned without NGHitscanComponent");
            Del(hitscan);
            return null;
        }

        return (hitscan, hitscanComp);
    }

    /// <summary>
    /// Compute a hitscan and find the hit entity.
    /// Has no side effects (the hitscan isn't animated, there is no damage dealt, e.c.t.)
    /// </summary>
    public RayCastResults? ComputeHitscanVictim(Entity<NGHitscanComponent> hitscan, MapCoordinates from, Vector2 direction)
    {
        var ray = new CollisionRay(from.Position, direction, (int)hitscan.Comp.CollisionMask);
        return _physics.IntersectRay(from.MapId, ray, hitscan.Comp.MaxDistance).FirstOrNull();
    }

    /// <summary>
    /// Compute the hitscan and find out who would be hit.
    /// </summary>
    public HitscanResult? ComputeHitscan(Entity<NGHitscanGunComponent> gun, EntityUid user, EntityCoordinates target)
    {
        var fromCoords = Transform(user).Coordinates;
        var from = _transform.ToMapCoordinates(fromCoords);
        var to = _transform.ToMapCoordinates(target);
        if (from.MapId != to.MapId)
        {
            Log.Warning("Tried to hitscan target on another map.");
            return null;
        }
        var direction = to.Position - from.Position;

        if (SpawnHitscan(gun) is not { } hitscan)
        {
            Log.Warning("Failed to spawn hitscan");
            return null;
        }

        if (ComputeHitscanVictim(hitscan, from, direction) is not { } result)
        {
            var hitPosition = from.Position + direction.Normalized() * hitscan.Comp.MaxDistance;
            var hitMapCoords = new MapCoordinates(hitPosition, to.MapId);
            var hitCoords = _transform.ToCoordinates(user, hitMapCoords);;
            Del(hitscan);
            return new HitscanResult(hitCoords, null);
        }

        var hitMapCoords2 = new MapCoordinates(result.HitPos, to.MapId);
        var hitCoords2 = _transform.ToCoordinates(user, hitMapCoords2);
        Del(hitscan);
        return new HitscanResult(hitCoords2, result.HitEntity);
    }

    /// <summary>
    /// Convert HitscanResult to NetHitscanResult.
    /// </summary>
    public NetHitscanResult ToNetHitscan(HitscanResult result)
    {
        return new NetHitscanResult(GetNetCoordinates(result.To), GetNetEntity(result.Victim));
    }

    /// <summary>
    /// Convert NetHitscanResult to HitscanResult with local entities and coordinates.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public HitscanResult ToLocalHitscan(NetHitscanResult result)
    {
        return new HitscanResult(GetCoordinates(result.To), GetEntity(result.Victim));
    }

    /// <summary>
    /// Apply a HitscanResult to the game so it actually affects the world.
    /// </summary>
    public void ApplyHitscanResult(EntityUid user, HitscanResult result)
    {
        if (_fetch.GetGun(user) is not { } gun)
            return;

        if (GetHitscanGun(gun) is not { } hitscanGun)
            return;

        if (SpawnHitscan(hitscanGun) is not { } hitscan)
            return;

        if (result.Victim is { } victim)
        {
            var ev1 = new HitscanHitEvent(victim);
            RaiseLocalEvent(hitscan, ref ev1);
        }

        var ev2 = new HitscanEvent(Transform(user).Coordinates, result.To);
        RaiseLocalEvent(hitscan, ref ev2);

        Del(hitscan);
    }

    [SubscribeLocalEvent]
    private void OnGetHitscan(Entity<NGInnateHitscanComponent> gun, ref GetHitscanEvent args)
    {
        args.Proto ??= gun.Comp.Proto;
    }
}

/// <summary>
/// The results of a hitscan, including the destination and the victim.
/// Does not include user or start position.
/// </summary>
public record struct HitscanResult(EntityCoordinates To, EntityUid? Victim);

/// <summary>
/// HitscanResult with NetCoordinates and NetEntity so it can be understood by both client/server.
/// </summary>
[Serializable, NetSerializable]
public record struct NetHitscanResult(NetCoordinates To, NetEntity? Victim);

/// <summary>
/// A message from cliet to itself/server requesting for a simulated hitscan to be applied to the world.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class RequestHitscanMessage(NetHitscanResult result) : EntityEventArgs
{
    public readonly NetHitscanResult Result = result;
}

/// <summary>
/// Event raised on hitsan guns to find the prototype for the hitscan they want to shoot
/// </summary>
[ByRefEvent]
public record struct GetHitscanEvent(EntProtoId<NGHitscanComponent>? Proto = null);

/// <summary>
/// Raised on the hitscan when it hits an entity.
/// </summary>
[ByRefEvent]
public record struct HitscanHitEvent(EntityUid Victim);

/// <summary>
/// 
/// </summary>
[ByRefEvent]
public record struct HitscanEvent(EntityCoordinates From, EntityCoordinates To);