using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._ES.NewGun.Trigger;

/// <summary>
/// The most high level gun system, handles pulling the trigger.
/// </summary>
public sealed partial class GunTriggerSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private ISharedPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeAllEvent<AttemptGunTriggerPulledMessage>(OnAttemptTriggerPulled);
        SubscribeAllEvent<AttemptGunTriggerReleasedMessage>(OnAttemptTriggerReleased);
    }

    /// <summary>
    /// Find the gun an entity is currently using, if any.
    /// This is what you would shoot with if you were in combat mode and clicked.
    /// 
    /// The priorities are as follows:
    /// innate gun -> held gun
    /// 
    /// The reason the handlers for <see cref="GetGunEvent"/> are in their own tiny systems is so system ordering can be used to enforce the priority.
    /// </summary>
    public Entity<NGTriggerComponent>? GetGun(EntityUid user)
    {
        var ev = new GetGunEvent(user);
        RaiseLocalEvent(user, ref ev);
        return ev.Gun;
    }

    /// <summary>
    /// Is this trigger pull or release request actually possible?
    /// This is basic anti-cheat.
    /// The client can't pull someone else's gun.
    /// </summary>
    private bool IsFeasible(EntityUid user, ICommonSession senderSession)
    {
        if (_player.TryGetSessionByEntity(user, out var session) && session != senderSession)
            return false;

        return true;
    }

    private void OnAttemptTriggerPulled(AttemptGunTriggerPulledMessage msg, EntitySessionEventArgs args)
    {
        if (GetGun(GetEntity(msg.User)) is not { } gun)
            return;

        var ev1 = new AttemptTriggerPulledViaGunEvent(GetEntity(msg.User), args.SenderSession);
        RaiseLocalEvent(gun, ref ev1);
        var ev2 = new AttemptTriggerPulledViaUserEvent(GetEntity(msg.User), args.SenderSession);
        RaiseLocalEvent(GetEntity(msg.User), ref ev2);

        if (ev1.Cancelled || ev2.Cancelled || !IsFeasible(GetEntity(msg.User), args.SenderSession))
            return;

        var gunComp = Comp<NGTriggerComponent>(gun);
        gunComp.TriggerHeld = true;
        gunComp.TriggerHeldTime = _timing.CurTime;
        gunComp.Target = GetCoordinates(msg.Target);
        DirtyFields(gun, gunComp, null, [nameof(NGTriggerComponent.TriggerHeld), nameof(NGTriggerComponent.TriggerHeldTime), nameof(NGTriggerComponent.Target)]);
        var ev3 = new TriggerPulledEvent(GetEntity(msg.User));
        RaiseLocalEvent(gun, ref ev3);
    }

    private void OnAttemptTriggerReleased(AttemptGunTriggerReleasedMessage msg, EntitySessionEventArgs args)
    {
        if (GetGun(GetEntity(msg.User)) is not { } gun)
            return;

        var ev1 = new AttemptTriggerReleasedViaGunEvent(GetEntity(msg.User), args.SenderSession);
        RaiseLocalEvent(gun, ref ev1);
        var ev2 = new AttemptTriggerReleasedViaUserEvent(GetEntity(msg.User), args.SenderSession);
        RaiseLocalEvent(GetEntity(msg.User), ref ev2);

        if (ev1.Cancelled || ev2.Cancelled || !IsFeasible(GetEntity(msg.User), args.SenderSession))
            return;

        var gunComp = Comp<NGTriggerComponent>(gun);
        gunComp.TriggerHeld = false;
        gunComp.TriggerHeldTime = null;
        gunComp.Target = null;
        DirtyFields(gun, gunComp, null, [nameof(NGTriggerComponent.TriggerHeld), nameof(NGTriggerComponent.TriggerHeldTime), nameof(NGTriggerComponent.Target)]);
        var ev3 = new TriggerReleasedEvent(GetEntity(msg.User));
        RaiseLocalEvent(gun, ref ev3);
    }
}

/// <summary>
/// Event raised on a user to find their gun.
/// It is done this way to handle all the many different possible cases.
/// User is gun, user holding gun, user has automatic gun implant, user is inside mech with gun, e.t.c.
/// </summary>
[ByRefEvent]
public record struct GetGunEvent(EntityUid User, Entity<NGTriggerComponent>? Gun = null);

/// <summary>
/// A client is attempting to pull the trigger on their gun.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class AttemptGunTriggerPulledMessage(NetEntity user, NetCoordinates target) : EntityEventArgs
{
    public readonly NetEntity User = user;
    public readonly NetCoordinates Target = target;
}

/// <summary>
/// A client is attempting to release the trigger on their gun.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class AttemptGunTriggerReleasedMessage(NetEntity user) : EntityEventArgs
{
    public readonly NetEntity User = user;
}

/// <summary>
/// Cancellable event raised on the user to see if they can pull the trigger.
/// </summary>
[ByRefEvent]
public record struct AttemptTriggerPulledViaUserEvent(EntityUid Gun, ICommonSession SenderSession, bool Cancelled = false);

/// <summary>
/// Cancellable event raised on the gun to see if they can pull the trigger.
/// </summary>
[ByRefEvent]
public record struct AttemptTriggerPulledViaGunEvent(EntityUid User, ICommonSession SenderSession, bool Cancelled = false);

/// <summary>
/// Cancellable event raised on the user to see if they can release the trigger.
/// </summary>
[ByRefEvent]
public record struct AttemptTriggerReleasedViaUserEvent(EntityUid Gun, ICommonSession SenderSession, bool Cancelled = false);

/// <summary>
/// Cancellable event raised on the gun to see if they can release the trigger.
/// </summary>
[ByRefEvent]
public record struct AttemptTriggerReleasedViaGunEvent(EntityUid User, ICommonSession SenderSession, bool Cancelled = false);

/// <summary>
/// Raised on the gun when the trigger is pulled.
/// </summary>
[ByRefEvent]
public record struct TriggerPulledEvent(EntityUid User, bool Handled = false);

/// <summary>
/// Raised on the gun when the trigger is released.
/// </summary>
[ByRefEvent]
public record struct TriggerReleasedEvent(EntityUid User, bool Handled = false);
