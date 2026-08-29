using Content.Shared._ES.NewGun.Trigger;
using Content.Shared.CombatMode;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Input;
using Robust.Shared.Timing;

namespace Content.Client._ES.NewGun.Trigger;

/// <summary>
/// Clienside system that listens to input and sets the trigger as pulled;
/// </summary>
public sealed partial class ClientGunTriggerSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private InputSystem _input = default!;
    [Dependency] private GunTriggerSystem _trigger = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        if (_player.LocalEntity is not { } user)
            return;

        if (!TryComp<CombatModeComponent>(user, out var combatModeComp) || !combatModeComp.IsInCombatMode)
            return;

        if (_trigger.GetGun(user) is not { } gun)
            return;

        var shootKey = gun.Comp.UseKey ? EngineKeyFunctions.Use : EngineKeyFunctions.UseSecondary;

        if (!gun.Comp.TriggerHeld && _input.CmdStates.GetState(shootKey) == BoundKeyState.Down)
        {
            RaisePredictiveEvent(new AttemptGunTriggerPulledMessage(GetNetEntity(user), GetNetEntity(gun)));
        }
        else if (gun.Comp.TriggerHeld && _input.CmdStates.GetState(shootKey) == BoundKeyState.Up)
        {
            RaisePredictiveEvent(new AttemptGunTriggerReleasedMessage(GetNetEntity(user), GetNetEntity(gun)));
        }
    }
}