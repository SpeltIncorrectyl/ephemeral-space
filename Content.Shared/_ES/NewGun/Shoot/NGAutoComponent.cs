using Robust.Shared.GameStates;

namespace Content.Shared._ES.NewGun.Shoot;

/// <summary>
/// This component enables semi-automatic fire.
/// The gun will try to shoot every tick the trigger is held down.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class NGAutoComponent : Component;