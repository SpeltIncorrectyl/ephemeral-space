using Content.Shared._ES.NewGun.Fetch;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Shared._ES.NewGun;

/// <summary>
/// Handles guns shooting when their triggers are held.
/// </summary>
public sealed partial class ShootSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private FetchGunSystem _fetch = default!;

    /// <summary>
    /// Is this gun currently affected by shoot delay?
    /// As in, has it recently been fired and cannot be fired again for a short delay in accordance with its firerate?
    /// </summary>
    public bool HasShotDelay(Entity<NGGunComponent> gun)
    {
        if (gun.Comp.NextShoot is not { } nextShoot)
            return false;

        return _timing.CurTime < nextShoot;
    }

    /// <summary>
    /// Set the next time a gun can shoot based on a specified delay.
    /// </summary>
    public void SetShootDelay(Entity<NGGunComponent> gun, TimeSpan delay)
    {
        gun.Comp.NextShoot = _timing.CurTime + delay;
        DirtyField(gun, gun.Comp, nameof(NGGunComponent.NextShoot));
    }

    /// <summary>
    /// Set the next time a gun can shoot, calculate the delay based on firerate.
    /// </summary>
    public void SetShootDelay(Entity<NGGunComponent> gun)
    {
        var delay = TimeSpan.FromSeconds(1.0 / gun.Comp.Firerate);
        SetShootDelay(gun, delay);
    }

    /// <summary>
    /// See if you can shoot the gun, factoring in shoot delay and also has cancellable events so you can prevent shooting.
    /// </summary>
    public bool CanShoot(Entity<NGGunComponent> gun, EntityUid user)
    {
        if (HasShotDelay(gun))
            return false;

        var ev1 = new AttemptGunShootEvent(user);
        RaiseLocalEvent(gun, ref ev1);
        var ev2 = new AttemptUserShootEvent(gun);
        RaiseLocalEvent(user, ref ev2);

        if (ev1.Cancelled || ev2.Cancelled)
            return false;

        return true;
    }


    public void TryShoot(Entity<NGGunComponent> gun, EntityUid user, EntityCoordinates target)
    {
        if (!CanShoot(gun, user))
            return;

        var ev3 = new ShootEvent(user, target);
        RaiseLocalEvent(gun, ref ev3);

        SetShootDelay(gun);
    }
}

/// <summary>
/// Raised on a gun when attempting to shoot.
/// </summary>
[ByRefEvent]
public record struct AttemptGunShootEvent(EntityUid User, bool Cancelled = false);

/// <summary>
/// Raised on a gun user when attempting to shoot.
/// </summary>
[ByRefEvent]
public record struct AttemptUserShootEvent(EntityUid Gun, bool Cancelled = false);

/// <summary>
/// Raised on a gun when shooting.
/// </summary>
[ByRefEvent]
public record struct ShootEvent(EntityUid User, EntityCoordinates Target);