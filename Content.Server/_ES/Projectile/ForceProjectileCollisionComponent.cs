namespace Content.Server._ES.Projectile;

/// <summary>
/// Forces projectile to collide with any fixture on this entity even if it is not hard.
/// </summary>
[RegisterComponent]
public sealed partial class ForceProjectileCollisionComponent : Component;