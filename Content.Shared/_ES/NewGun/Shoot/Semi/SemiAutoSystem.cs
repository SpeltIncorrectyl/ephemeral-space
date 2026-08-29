using Content.Shared._ES.NewGun.Trigger;

namespace Content.Shared._ES.NewGun.Shoot.Semi;

/// <summary>
/// The most high level gun system, handles pulling the trigger.
/// </summary>
public sealed partial class SemiAutoSystem : EntitySystem
{
    [Dependency] private ShootSystem _shoot = default!;

    [SubscribeLocalEvent]
    private void OnTriggerPulled(Entity<NGSemiAutoComponent> ent, ref TriggerPulledEvent args)
    {
        if (args.Handled)
            return;
        _shoot.TryShoot(ent.Owner, args.User);
        args.Handled = true;
    }
}