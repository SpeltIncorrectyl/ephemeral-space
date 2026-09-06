using Content.Shared.Physics;
using Robust.Shared.GameStates;
using Robust.Shared.Physics.Dynamics;

/// <summary>
/// This marks the entity as a hitscan.
/// </summary>
[RegisterComponent]
public sealed partial class NGHitscanComponent : Component
{
    /// <summary>
    /// Collision mask for things the hitscan can interact with.
    /// </summary>
    [DataField]
    public CollisionGroup CollisionMask = CollisionGroup.BulletImpassable;

    /// <summary>
    /// The maximum distance the hitscan will travel.
    /// </summary>
    [DataField]
    public float MaxDistance = 50f;
}