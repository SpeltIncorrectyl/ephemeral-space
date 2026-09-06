using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

/// <summary>
/// The prototype for the hitscan this gun shoots is provided directly by this component.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NGInnateHitscanComponent : Component
{
    /// <summary>
    /// The prototype of the hitscan.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public EntProtoId<NGHitscanComponent> Proto;
}