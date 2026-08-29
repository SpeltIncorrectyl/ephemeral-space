using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
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
    /// The gun in the event has to be the gun that is found from the user.
    /// </summary>
    private bool IsFeasible(EntityUid user, EntityUid gun, ICommonSession senderSession)
    {
        if (GetGun(user) is not { } foundGun)
            return false;

        if (foundGun.Owner != gun)
            return false;

        if (_player.TryGetSessionByEntity(user, out var session) && session != senderSession)
            return false;

        return true;
    }

    private void OnAttemptTriggerPulled(AttemptGunTriggerPulledMessage msg, EntitySessionEventArgs args)
    {
        var ev1 = new AttemptTriggerPulledViaGunEvent(GetEntity(msg.User), GetEntity(msg.Gun), args.SenderSession);
        RaiseLocalEvent(GetEntity(msg.Gun), ref ev1);
        var ev2 = new AttemptTriggerPulledViaUserEvent(GetEntity(msg.User), GetEntity(msg.Gun), args.SenderSession);
        RaiseLocalEvent(GetEntity(msg.User), ref ev2);

        if (ev1.Cancelled || ev2.Cancelled || !IsFeasible(GetEntity(msg.User), GetEntity(msg.Gun), args.SenderSession))
            return;

        var gunComp = Comp<NGTriggerComponent>(GetEntity(msg.Gun));
        gunComp.TriggerHeld = true;
        gunComp.TriggerHeldTime = _timing.CurTime;
        DirtyFields(GetEntity(msg.Gun), gunComp, null, [nameof(NGTriggerComponent.TriggerHeld), nameof(NGTriggerComponent.TriggerHeldTime)]);
        var ev3 = new TriggerPulledEvent(GetEntity(msg.User), GetEntity(msg.Gun));
        RaiseLocalEvent(GetEntity(msg.Gun), ref ev3);
    }

    private void OnAttemptTriggerReleased(AttemptGunTriggerReleasedMessage msg, EntitySessionEventArgs args)
    {
        var ev1 = new AttemptTriggerReleasedViaGunEvent(GetEntity(msg.User), GetEntity(msg.Gun), args.SenderSession);
        RaiseLocalEvent(GetEntity(msg.Gun), ref ev1);
        var ev2 = new AttemptTriggerReleasedViaUserEvent(GetEntity(msg.User), GetEntity(msg.Gun), args.SenderSession);
        RaiseLocalEvent(GetEntity(msg.User), ref ev2);

        if (ev1.Cancelled || ev2.Cancelled || !IsFeasible(GetEntity(msg.User), GetEntity(msg.Gun), args.SenderSession))
            return;

        var gunComp = Comp<NGTriggerComponent>(GetEntity(msg.Gun));
        gunComp.TriggerHeld = false;
        gunComp.TriggerHeldTime = null;
        DirtyFields(GetEntity(msg.Gun), gunComp, null, [nameof(NGTriggerComponent.TriggerHeld), nameof(NGTriggerComponent.TriggerHeldTime)]);
        var ev3 = new TriggerReleasedEvent(GetEntity(msg.User), GetEntity(msg.Gun));
        RaiseLocalEvent(GetEntity(msg.Gun), ref ev3);
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
public sealed partial class AttemptGunTriggerPulledMessage(NetEntity user, NetEntity gun) : EntityEventArgs
{
    public readonly NetEntity User = user;
    public readonly NetEntity Gun = gun;
}

/// <summary>
/// A client is attempting to release the trigger on their gun.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class AttemptGunTriggerReleasedMessage(NetEntity user, NetEntity gun) : EntityEventArgs
{
    public readonly NetEntity User = user;
    public readonly NetEntity Gun = gun;
}

/// <summary>
/// Cancellable event raised on the user to see if they can pull the trigger.
/// </summary>
[ByRefEvent]
public record struct AttemptTriggerPulledViaUserEvent(EntityUid User, EntityUid Gun, ICommonSession SenderSession, bool Cancelled = false);

/// <summary>
/// Cancellable event raised on the gun to see if they can pull the trigger.
/// </summary>
[ByRefEvent]
public record struct AttemptTriggerPulledViaGunEvent(EntityUid User, EntityUid Gun, ICommonSession SenderSession, bool Cancelled = false);

/// <summary>
/// Cancellable event raised on the user to see if they can release the trigger.
/// </summary>
[ByRefEvent]
public record struct AttemptTriggerReleasedViaUserEvent(EntityUid User, EntityUid Gun, ICommonSession SenderSession, bool Cancelled = false);

/// <summary>
/// Cancellable event raised on the gun to see if they can release the trigger.
/// </summary>
[ByRefEvent]
public record struct AttemptTriggerReleasedViaGunEvent(EntityUid User, EntityUid Gun, ICommonSession SenderSession, bool Cancelled = false);

/// <summary>
/// Raised on the gun when the trigger is pulled.
/// </summary>
[ByRefEvent]
public record struct TriggerPulledEvent(EntityUid User, EntityUid Gun, bool Handled = false);

/// <summary>
/// Raised on the gun when the trigger is released.
/// </summary>
[ByRefEvent]
public record struct TriggerReleasedEvent(EntityUid User, EntityUid Gun, bool Handled = false);
