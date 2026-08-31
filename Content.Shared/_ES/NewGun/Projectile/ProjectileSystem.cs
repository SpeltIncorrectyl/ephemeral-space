using Content.Shared._ES.NewGun.Shoot;
using Content.Shared.Projectiles;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._ES.NewGun.Projectile;

/// <summary>
/// By default, a gun will shoot as much as possible.
/// This system only allows it to shoot at a certain rate.
/// </summary>
public sealed partial class ProjectileSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;

    /// <summary>
    /// Get the prototype of the projectile that is meant to be shot.
    /// </summary>
    public EntProtoId? GetProjectileProto(EntityUid gun)
    {
        var ev = new GetProjectileEvent();
        RaiseLocalEvent(gun, ref ev);
        return ev.ProjectileProto;
    }

    [SubscribeLocalEvent]
    private void OnShoot(Entity<NGProjectileShooterComponent> ent, ref ShootEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (GetProjectileProto(ent) is not { } proto)
        {
            Log.Warning("Gun shoots projectiles but cannot find projectile proto.");
            return;
        }

        // Using Spawn instead of PredictedSpawn in purpose
        // projectile is short-lived and with PredictedSpawn it will die before the server replicates it.
        // Or worse it will be replicated mid flight and replace the client-side version and jitter and mess things up.
        var projectile = SpawnAtPosition(proto, Transform(args.User).Coordinates);
    }

    [SubscribeLocalEvent]
    private void OnGetProjectile(Entity<NGInnateProjectileSource> ent, ref GetProjectileEvent args)
    {
        args.ProjectileProto ??= ent.Comp.Proto;
    }
}

/// <summary>
/// Used to fetch the prototype to spawn a projectile from.
/// Some guns have innate prototypes while others are based off physical cartridges.
/// </summary>
[ByRefEvent]
public record struct GetProjectileEvent(EntProtoId? ProjectileProto = null);