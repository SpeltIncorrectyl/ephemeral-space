using Content.Shared._ES.CCVar;
using Content.Shared._ES.NewGun.Projectile;
using Robust.Server.Player;
using Robust.Shared.Configuration;

namespace Content.Server._ES.NewGun;

public sealed partial class ProjectileSystem : SharedProjectileSystem
{
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IConfigurationManager _cfg = default!;

    private float _projectileLagCompRange;

    public override void Initialize()
    {
        base.Initialize();
        Subs.CVar(_cfg, ESCVars.ESProjectileLagCompRange, x => _projectileLagCompRange = x);
    }

    protected override void AfterProjectileSpawned(EntityUid projectile)
    {
        AddComp(projectile, new NGReplicatedProjectileComponent());
    }

    [SubscribeNetworkEvent]
    private void OnRequestImpact(ProjectileRequestImpactMessage msg, EntitySessionEventArgs args)
    {
        Log.Debug("client claims impact");
        var projectile = GetEntity(msg.Projectile);
        var victim = GetEntity(msg.Victim);

        if (Deleted(projectile) || Deleted(victim))
        {
            Log.Debug("client impact discarded due to deleted entity");
            return;
        }

        if (!TryComp<NGProjectileComponent>(projectile, out var projComp))
            return;

        if (projComp.Shooter is not { } shooter)
            return;

        if (args.SenderSession.AttachedEntity != GetEntity(shooter))
            return;

        if (Transform(projectile).Coordinates.TryDistance(EntityManager, Transform(victim).Coordinates, out var distance))
            return;

        if (distance < _projectileLagCompRange)
        {
            DoImpact((projectile, projComp), victim);
            Log.Debug("client impact successful SUCCEESS!!!!!!!!!!!!!!");
        }
    }
}