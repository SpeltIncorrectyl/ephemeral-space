using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._ES.NewGun;

/// <summary>
/// The most high level aspect of a gun. Tracks the trigger being pulled and released.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class NGTriggerComponent : Component
{
    /// <summary>
    /// Is the trigger currently held down?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool TriggerHeld;

    /// <summary>
    /// If the trigger is held down, what is it shooting at?
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityCoordinates? Target;

    /// <summary>
    /// When was this component's information last updated?
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan? LastUpdateTime;

    /// <summary>
    /// Whether this gun is shot via the use key or the alt-use key.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool UseKey = true;
}