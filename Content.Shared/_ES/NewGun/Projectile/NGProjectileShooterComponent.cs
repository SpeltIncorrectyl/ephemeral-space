using Robust.Shared.GameStates;

namespace Content.Shared._ES.NewGun.Projectile;

/// <summary>
/// This gun shoots projectiles.
/// This is the most common thing. Other guns are things like hitscans.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class NGProjectileShooterComponent : Component;