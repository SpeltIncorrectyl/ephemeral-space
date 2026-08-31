using Content.Shared._ES.NewGun.Trigger;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._ES.NewGun.Targeting;

/// <summary>
/// System that finds out where a gun is being pointed.
/// </summary>
public abstract partial class SharedTargetingSystem : EntitySystem
{
    [Dependency] protected IGameTiming Timing = default!;
    [Dependency] private GunTriggerSystem _trigger = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeAllEvent<UpdateGunTargetMessage>(OnUpdateTarget);
    }

    /// <summary>
    /// Find out where a gun is pointed.
    /// </summary>

    // If the trigger was pulled this tick then the target on the component is accurate.
    // Otherwise, things get a little more complicated...
    // 
    // On the client:
    //  Just fetch the target from ClientGunTriggerSystem.
    //  However we also need to update the server so send a UpdateGunTargetMessage their way (use IsFirstTimePredicted!!!).
    //  Don't forget to dirty.
    // 
    // On the server:
    //  We can't find out. So we return null.
    //  When the UpdateGunTargetMessage arrives update and dirty the component.
    //  The semi-automatic firemode will only try to fire when the trigger is first pulled and the target is accurate and this doesn't matter.
    //  The automatic firemode will try to fire every tick and when the updated target eventually arrives this will return the target that tick.
    // 
    // TryShoot only calls this method once AttemptShootEvent has succeeded so automatic firemode doesn't spam the network with UpdateGunTargetMessage-s every tick. 
    //
    // The old gun system has the client work out when you fire a bullet then send a RequestShootEvent message to the server.
    // You might at first think this system is less efficient but the same thing is happening:
    //   The client predicts the shooting.
    //   The client automatically sends UpdateGunTargetMessage-s to the server as it predicts things.
    //   The server shoots after it receives the messages, just like in the old gun system.
    public virtual EntityCoordinates? GetTarget(Entity<NGTriggerComponent> gun)
    {
        if (!gun.Comp.TriggerHeld)
            return null;

        if (Timing.CurTime == gun.Comp.LastUpdateTime)
            return GetCoordinates(gun.Comp.Target);

        return null;
    }

    private void OnUpdateTarget(UpdateGunTargetMessage msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } player)
            return;

        if (_trigger.GetGun(player) is not { } gun)
            return;

        gun.Comp.Target = msg.Target;
        gun.Comp.LastUpdateTime = Timing.CurTime;
        DirtyFields(gun, gun.Comp, null, [nameof(NGTriggerComponent.Target), nameof(NGTriggerComponent.LastUpdateTime)]);
    }
}

/// <summary>
/// Message sent from client to server to update a <see cref="NGTriggerComponent"/>'s Target.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class UpdateGunTargetMessage(NetCoordinates target) : EntityEventArgs
{
    public readonly NetCoordinates Target = target;
}