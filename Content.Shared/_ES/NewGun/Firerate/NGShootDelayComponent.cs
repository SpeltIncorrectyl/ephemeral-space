using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._ES.NewGun.Firerate;

/// <summary>
/// By default, a gun will shoot as much as possible.
/// This component stops it from shooing until the current game time reaches the next shoot time.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class NGShootDelayComponent : Component
{
    /// <summary>
    /// The next time the gun can shot.
    /// </summary>
    [DataField(required: true, customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextShootTime;
}