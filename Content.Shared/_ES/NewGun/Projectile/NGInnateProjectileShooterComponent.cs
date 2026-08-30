using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._ES.NewGun.Projectile;

/// <summary>
/// This prototype for the projectile this gun shoots is found in this component.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NGInnateProjectileShooterComponent : Component
{
    /// <summary>
    /// The prototype to be spawned.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId<NGProjectileComponent> Proto;
}