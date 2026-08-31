using Robust.Shared.GameStates;
using Robust.Shared.Player;

namespace Content.Shared._ES.NewGun.Projectile;

/// <summary>
/// This component is only placed on the server, not the original predicted spawned client-side projectile.
/// The client will only see this component once the server-spawned projectile is replicated.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class NGReplicatedProjectileComponent : Component;