using Robust.Shared.GameStates;

namespace Content.Shared._ES.NewGun.Shoot;

/// <summary>
/// This component enables semi-automatic fire.
/// The gun will try to shoot only when the trigger is pulled.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class NGSemiAutoComponent : Component;