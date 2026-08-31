using System.Diagnostics;
using Content.Shared._ES.NewGun.Fetch;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._ES.NewGun;

/// <summary>
/// Handles guns shooting when their triggers are held.
/// </summary>
public sealed partial class ShootSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private FetchGunSystem _fetch = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeAllEvent<RequestShootMessage>(OnRequestShoot);
    }

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

    private void OnRequestShoot(RequestShootMessage msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } user)
            return;

        if (_fetch.GetGun(user) is not { } gun)
            return;

        if (HasShotDelay(gun))
            return;

        // set shoot delay now before we even know if the gun can fire
        // otherwise if the shooting is not possible it would spam failures
        SetShootDelay(gun);

        var ev1 = new AttemptUserShootEvent(gun);
        RaiseLocalEvent(user, ref ev1);
        if (ev1.Cancelled)
            return;

        var ev2 = new AttemptGunShootEvent(user);
        RaiseLocalEvent(gun, ref ev2);
        if (ev2.Cancelled)
            return;

        var ev3 = new ShootEvent(user, GetCoordinates(msg.Target));
        RaiseLocalEvent(gun, ref ev3);
    }
}

/// <summary>
/// A message sent by the client when it wants to shoot a gun.
/// Only the target coordinates are sent, everything else can be worked out from the SenderSession.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class RequestShootMessage(NetCoordinates target) : EntityEventArgs
{
    public readonly NetCoordinates Target = target;
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