using Content.Shared._ES.NewGun.Shoot;
using Robust.Shared.Timing;

namespace Content.Shared._ES.NewGun.Firerate;

/// <summary>
/// By default, a gun will shoot as much as possible.
/// This system only allows it to shoot at a certain rate.
/// </summary>
public sealed partial class FirerateSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;

    /// <summary>
    /// Get the time between shots.
    /// This is the reciprocal of the firerate.
    /// </summary>
    public TimeSpan GetShootDelay(Entity<NGFirerateComponent> ent)
    {
        return TimeSpan.FromSeconds(1.0 / ent.Comp.Firerate);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<NGShootDelayComponent>();
        while (query.MoveNext(out var entity, out var comp))
        {
            if (_timing.CurTime >= comp.NextShootTime)
                RemComp<NGShootDelayComponent>(entity);
        }
    }

    [SubscribeLocalEvent]
    private void OnShoot(Entity<NGFirerateComponent> ent, ref ShootEvent args)
    {
        AddComp(ent, new NGShootDelayComponent
        {
            NextShootTime = _timing.CurTime + GetShootDelay(ent)
        });
    }

    [SubscribeLocalEvent]
    private void OnAttemptShoot(Entity<NGShootDelayComponent> ent, ref AttemptShootEvent args)
    {
        args.Cancelled = true;
    }
}