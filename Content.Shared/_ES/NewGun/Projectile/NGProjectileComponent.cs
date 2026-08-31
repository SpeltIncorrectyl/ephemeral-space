using Robust.Shared.GameStates;
using Robust.Shared.Player;

namespace Content.Shared._ES.NewGun.Projectile;

/// <summary>
/// This entity is a projectile that is fired from a gun.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class NGProjectileComponent : Component
{
    /// <summary>
    /// The speed this projectile is fired at when it is created.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public float Speed;

    /// <summary>
    /// The session that shot this projectile.
    /// </summary>

    [DataField, AutoNetworkedField]
    public NetEntity? Shooter;

    /// <summary>
    /// If the projectile has collided already.
    /// This field is purposefully not networked.
    /// </summary>
    [DataField]
    public bool Collided;
}