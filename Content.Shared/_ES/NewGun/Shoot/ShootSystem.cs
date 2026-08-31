using Content.Shared._ES.NewGun.Targeting;
using Content.Shared._ES.NewGun.Trigger;
using Robust.Shared.Map;

namespace Content.Shared._ES.NewGun.Shoot;

/// <summary>
/// Handles guns shooting when their triggers are held.
/// </summary>
public sealed partial class ShootSystem : EntitySystem
{
    [Dependency] private SharedTargetingSystem _targeting = default!;

    /// <summary>
    /// Try to get a gun to shoot, if possible.
    /// Systems like <see cref="SemiAutoSystem"/> listen to <see cref="TriggerPulledEvent"/ > and call this method.
    /// </summary>
    public bool TryShoot(EntityUid gun, EntityUid user, string? warnOnTargetingFail = null)
    {
        if (!TryComp<NGTriggerComponent>(gun, out var triggerComp))
        {
            Log.Warning("Called TryShoot on entity without NGTriggerComponent.");
            return false;
        }

        var ev1 = new AttemptShootEvent(user);
        RaiseLocalEvent(gun, ref ev1);
        if (ev1.Cancelled)
            return false;

        if (_targeting.GetTarget((gun, triggerComp)) is not { } target)
        {
            if (warnOnTargetingFail is string message)
                Log.Warning(message);
            return false;
        }

        Log.Debug("shoot!");
        var ev2 = new ShootEvent(user, target);
        RaiseLocalEvent(gun, ref ev2);
        return true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<NGAutoComponent>();
        while (query.MoveNext(out var gun, out _))
        {
            
        }
    }

    [SubscribeLocalEvent]
    private void OnTriggerPulled(Entity<NGSemiAutoComponent> ent, ref TriggerPulledEvent args)
    {
        if (args.Handled)
            return;
        TryShoot(ent.Owner, args.User, warnOnTargetingFail: "Semi-auto fire failed to aquire target. This should be impossible since the system only tries to shoot on the tick the trigger is pulled and this should mean the targeting doesn't fail.");
        args.Handled = true;
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
public record struct ShootEvent(EntityUid User, EntityCoordinates Target);