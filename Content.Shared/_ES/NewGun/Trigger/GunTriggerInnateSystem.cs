using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;

namespace Content.Shared._ES.NewGun.Trigger;

/// <summary>
/// First priority for finding the gun. Check if the entity itself IS the gun.
/// </summary>
public sealed partial class GunTriggerInnateSystem : EntitySystem
{
    [SubscribeLocalEvent]
    private void OnGetGun(Entity<NGTriggerComponent> ent, ref GetGunEvent args)
    {
        args.Gun ??= ent;
    }
}