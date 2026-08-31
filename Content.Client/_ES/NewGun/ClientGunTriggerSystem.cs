using Content.Shared._ES.NewGun;
using Content.Shared._ES.NewGun.Trigger;
using Content.Shared.CombatMode;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Shared.Input;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Client._ES.NewGun;

/// <summary>
/// Clienside system that listens to input and sets the trigger as pulled;
/// </summary>
public sealed partial class ClientGunTriggerSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private InputSystem _input = default!;
    [Dependency] private EyeManager _eye = default!;
    [Dependency] private IInputManager _inputMan = default!;
    [Dependency] private TransformSystem _transform = default!;
    [Dependency] private GunTriggerSystem _trigger = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        if (_player.LocalEntity is not { } user)
            return;

        if (_trigger.GetGun(user) is not { } gun)
            return;

        if (!TryComp<CombatModeComponent>(user, out var combatModeComp) || !combatModeComp.IsInCombatMode)
            return;

        if (GetTarget() is not { } target)
            return;

        var shootKey = gun.Comp.UseKey ? EngineKeyFunctions.Use : EngineKeyFunctions.UseSecondary;

        if (!gun.Comp.TriggerHeld && _input.CmdStates.GetState(shootKey) == BoundKeyState.Down)
        {
            RaisePredictiveEvent(new AttemptGunTriggerPulledMessage(GetNetCoordinates(target)));
        }
        else if (gun.Comp.TriggerHeld && _input.CmdStates.GetState(shootKey) == BoundKeyState.Up)
        {
            RaisePredictiveEvent(new AttemptGunTriggerReleasedMessage());
        }
    }

    /// <summary>
    /// Find out where this client is aiming their gun at.
    /// </summary>
    public EntityCoordinates? GetTarget()
    {
        var mousePos = _eye.PixelToMap(_inputMan.MouseScreenPosition);
        if (mousePos.MapId == MapId.Nullspace)
            return null;
        if (_player.LocalEntity is not { } user)
            return null;
        return _transform.ToCoordinates(user, mousePos);
    }
}