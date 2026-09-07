using Content.Shared.StatusEffectNew.Components;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._ES.StatusEffectArea;

/// <summary>
/// Apply status effects to entities while only while they are within a fixture.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StatusEffectOnFixtureComponent : Component
{
    /// <summary>
    /// The id of the specific fixture.
    /// </summary>
    [DataField(required: true)]
    public string Fixture;

    /// <summary>
    /// The effects to be applied on entry and removed on exit.
    /// </summary>
    [DataField]
    public List<EntProtoId<StatusEffectComponent>> Effects = new();

    /// <summary>
    /// A whitelist for what entities to target.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;
}