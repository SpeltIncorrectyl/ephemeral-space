namespace Content.Client._ES.NewGun;

/// <summary>
/// Client-side component that tracks if the shoot button has already been clicked for semi-automatic fire.
/// </summary>
[RegisterComponent]
public sealed partial class NGTriggerComponent : Component
{
    /// <summary>
    /// Is the trigger of the gun currently held down?
    /// As in, is the client currently holding down the shoot button (probably left click)?
    /// </summary>
    [DataField]
    public bool TriggerHeld;
}