using System.Numerics;
using Content.Shared._ES.NewGun.Projectile;
using Robust.Client.Physics;
using Robust.Client.Player;

namespace Content.Client._ES.NewGun;

/// <summary>
/// Clientside system that listens to input and sets the trigger as pulled;
/// </summary>
public sealed partial class ProjectileSystem : SharedProjectileSystem
{
    [Dependency] private IPlayerManager _player = default!;

    protected override void AfterProjectileSpawned(EntityUid projectile)
    {
        AddComp<NGPredictedProjectileComponent>(projectile);
        Physics.UpdateIsPredicted(projectile);
    }

    [SubscribeLocalEvent]
    private void OnUpdatePhysics(Entity<NGPredictedProjectileComponent> ent, ref UpdateIsPredictedEvent args)
    {
        args.IsPredicted = true;
    }

    [SubscribeLocalEvent]
    private void OnReplicatedProjectileInit(Entity<NGReplicatedProjectileComponent> ent, ref ComponentStartup args)
    {
        if (_player.LocalEntity is not { } player)
            return;

        if (!TryComp<NGProjectileComponent>(ent, out var projComp))
        {
            Log.Warning("Replicated projectile lacks NGProjectileComponent.");
            return;
        }

        if (GetEntity(projComp.Shooter) != player)
            return;

        AddComp<NGPredictedProjectileComponent>(ent);
        Physics.UpdateIsPredicted(ent);
    }
}