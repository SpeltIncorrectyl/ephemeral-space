using Content.Shared._ES.NewGun;
using Content.Shared._ES.NewGun.Fetch;
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
    [Dependency] private IEyeManager _eye = default!;
    [Dependency] private IInputManager _inputMan = default!;
    [Dependency] private TransformSystem _transform = default!;
    [Dependency] private FetchGunSystem _fetch = default!;
    [Dependency] private ShootSystem _shoot = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        if (_player.LocalEntity is not { } user)
            return;

        if (_fetch.GetGun(user) is not { } gun)
            return;

        var shootKey = gun.Comp.PrimaryUseKey ? EngineKeyFunctions.Use : EngineKeyFunctions.UseSecondary;
        var isShootKeyHeld = _input.CmdStates.GetState(shootKey) == BoundKeyState.Down;

        var triggerComp = EnsureComp<NGTriggerComponent>(gun);
        var isTriggerAlreadyHeld = triggerComp.TriggerHeld;
        triggerComp.TriggerHeld = isShootKeyHeld;
        if (gun.Comp.SemiAutomatic && isTriggerAlreadyHeld)
            return;

        if (_shoot.HasShotDelay(gun))
            return;

        if (!TryComp<CombatModeComponent>(user, out var combatModeComp) || !combatModeComp.IsInCombatMode)
            return;

        if (!isShootKeyHeld)
            return;

        if (GetTarget() is not { } target)
            return;

        RaisePredictiveEvent(new RequestShootMessage(GetNetCoordinates(target)));
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