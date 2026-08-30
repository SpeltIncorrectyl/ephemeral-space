namespace Content.Shared._ES.NewGun.Shoot;

/// <summary>
/// Handles guns shooting when their triggers are held.
/// </summary>
public sealed partial class ShootSystem : EntitySystem
{
    /// <summary>
    /// Try to get a gun to shoot, if possible.
    /// Systems like <see cref="SemiAutoSystem"/> listen to <see cref="TriggerPulledEvent"/ > and call this method.
    /// </summary>
    public bool TryShoot(EntityUid gun, EntityUid user)
    {
        var ev1 = new AttemptShootEvent(user);
        RaiseLocalEvent(gun, ref ev1);
        if (ev1.Cancelled)
            return false;
        Log.Debug("shoot!");
        var ev2 = new ShootEvent(user);
        RaiseLocalEvent(gun, ref ev2);
        return true;
    }
}

/// <summary>
/// Raised on a gun when attempting to shoot.
/// </summary>
[ByRefEvent]
public record struct AttemptShootEvent(EntityUid User, bool Cancelled = false);

/// <summary>
/// Raised on a gun when shooting.
/// </summary>
[ByRefEvent]
public record struct ShootEvent(EntityUid User);