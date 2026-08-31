using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._ES.NewGun.Projectile;

/// <summary>
/// This prototype for the projectile this gun shoots is found on this component.
/// Othertimes it can be found on ammo cartridges, which lets gun shoot different projectiles depending on what ammo is loaded.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NGInnateProjectileSourceComponent : Component
{
    /// <summary>
    /// The prototype to be spawned.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId<NGProjectileComponent> Proto;
}