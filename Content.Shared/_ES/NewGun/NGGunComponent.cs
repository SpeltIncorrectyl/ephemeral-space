using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._ES.NewGun;

/// <summary>
/// The most high level aspect of a gun. Tracks the trigger being pulled and released.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class NGGunComponent : Component
{
    /// <summary>
    /// The rate of fire for this gun, in shoots per second.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public float Firerate;

    /// <summary>
    /// If null then this gun is ready to be shot, otherwise it is the next time it can be shot.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan? NextShoot;

    /// <summary>
    /// Is this gun semi-automatic, as in it will only shoot when the mouse is clicked and not again until it is clicked again?
    /// This is only ever actually read by the client as only the client is concerned with mouse clicks.
    /// The concept of semi-auto vs auto doesn't exist on the server, guns just shoot when the client requests it.
    /// Burst mode is handled by setting the mode to auto and then adding the <see cref="NGBurstComponent"/>.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool SemiAutomatic;

    /// <summary>
    /// Is this gun shot by the primary use key (often bound to left click)?
    /// If this is false then the secondary use key (often bound to right click) is used.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool PrimaryUseKey = true;
}