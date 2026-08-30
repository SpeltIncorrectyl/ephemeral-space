using Robust.Shared.GameStates;

namespace Content.Shared._ES.NewGun.Firerate;

/// <summary>
/// By default, a gun will shoot as much as possible.
/// This component only allows it to shoot at a certain rate.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NGFirerateComponent : Component
{
    /// <summary>
    /// The firerate, in bullets per second.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public float Firerate;
}