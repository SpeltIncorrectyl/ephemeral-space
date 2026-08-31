using System.Numerics;
using Content.Shared._ES.NewGun;
using Content.Shared.Projectiles;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._ES.NewGun.Projectile;

/// <summary>
/// By default, a gun will shoot as much as possible.
/// This system only allows it to shoot at a certain rate.
/// </summary>
public abstract partial class SharedProjectileSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] protected SharedTransformSystem TransformSystem = default!;
    [Dependency] protected SharedPhysicsSystem Physics = default!;

    protected const string ProjectileFixture = "projectile";

    /// <summary>
    /// Get the prototype of the projectile that is meant to be shot.
    /// </summary>
    public EntProtoId? GetProjectileProto(EntityUid gun)
    {
        var ev = new GetProjectileEvent();
        RaiseLocalEvent(gun, ref ev);
        return ev.ProjectileProto;
    }

    [SubscribeLocalEvent]
    private void OnShoot(Entity<NGProjectileShooterComponent> ent, ref ShootEvent args)
    {
        if (GetProjectileProto(ent) is not { } proto)
        {
            Log.Warning("Gun shoots projectiles but cannot find projectile proto.");
            return;
        }

        var projectile = PredictedSpawnAtPosition(proto, Transform(args.User).Coordinates);
        if (!TryComp<NGProjectileComponent>(projectile, out var projComp))
        {
            Log.Warning("Projectile lacks NGProjectileComponent.");
            return;
        }

        projComp.Shooter = GetNetEntity(args.User);
        DirtyField(projectile, projComp, nameof(NGProjectileComponent.Shooter));

        var fromMap = TransformSystem.ToMapCoordinates(Transform(args.User).Coordinates);
        var toMap = TransformSystem.ToMapCoordinates(args.Target);
        var offset = toMap.Position - fromMap.Position;
        var velocity = offset.Normalized() * projComp.Speed;

        var physics = EnsureComp<PhysicsComponent>(projectile);
        Physics.SetBodyStatus(projectile, physics, BodyStatus.InAir);
        Physics.SetLinearVelocity(projectile, velocity);
        TransformSystem.SetWorldRotation(projectile, velocity.ToAngle() + Angle.FromDegrees(90f));
        AfterProjectileSpawned(projectile);
    }

    protected virtual void AfterProjectileSpawned(EntityUid projectile)
    {

    }

    [SubscribeLocalEvent]
    private void OnGetProjectile(Entity<NGInnateProjectileSourceComponent> ent, ref GetProjectileEvent args)
    {
        args.ProjectileProto ??= ent.Comp.Proto;
    }

    [SubscribeLocalEvent]
    private void OnStartCollide(Entity<NGProjectileComponent> ent, ref StartCollideEvent args)
    {
        if (args.OurFixtureId != ProjectileFixture || !args.OtherFixture.Hard || GetEntity(ent.Comp.Shooter) == args.OtherEntity || ent.Comp.Collided)
            return;

        DoImpact(ent, args.OtherEntity);
        // if you are the client and you predict an impact, through the magic of lag compensation you can make it real!
        // send a message to the server and if the projectile is close enough to the victim on the server when it receives the message it believes the client.
        if (_net.IsClient)
            RaiseNetworkEvent(new ProjectileRequestImpactMessage(GetNetEntity(ent), GetNetEntity(args.OtherEntity)));
    }

    protected void DoImpact(Entity<NGProjectileComponent> ent, EntityUid victim)
    {
        // when projectiles impact turn invisible, freeze in space and ignore future collisions instead of being deleted
        // this is so the server-side projectile has time for the client to send messages about things it predicted hitting 
        Physics.SetLinearVelocity(ent, Vector2.Zero);
        // no dirty because field is not networked
        ent.Comp.Collided = true;
        Log.Debug("impact!");
    }
}

/// <summary>
/// Used to fetch the prototype to spawn a projectile from.
/// Some guns have innate prototypes while others are based off physical cartridges.
/// </summary>
[ByRefEvent]
public record struct GetProjectileEvent(EntProtoId? ProjectileProto = null);

[Serializable, NetSerializable]
public sealed partial class ProjectileRequestImpactMessage(NetEntity projectile, NetEntity victim) : EntityEventArgs
{
    public NetEntity Projectile = projectile;
    public NetEntity Victim = victim;
}