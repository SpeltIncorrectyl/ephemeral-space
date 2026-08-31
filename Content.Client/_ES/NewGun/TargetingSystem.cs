using Content.Client._ES.NewGun;
using Content.Shared._ES.NewGun;
using Content.Shared._ES.NewGun.Targeting;
using Robust.Shared.Map;

namespace Content.Client._ES.NewGun;

public sealed partial class TargetingSystem : SharedTargetingSystem
{
    [Dependency] private ClientGunTriggerSystem _trigger = default!;

    public override EntityCoordinates? GetTarget(Entity<NGTriggerComponent> gun)
    {
        if (!gun.Comp.TriggerHeld)
            return null;

        if (base.GetTarget(gun) is { } target)
            return target;

        if (_trigger.GetTarget() is not { } target2)
            return null;

        if (Timing.IsFirstTimePredicted)
            RaisePredictiveEvent(new UpdateGunTargetMessage(GetNetCoordinates(target2)));

        return target2;
    }
}