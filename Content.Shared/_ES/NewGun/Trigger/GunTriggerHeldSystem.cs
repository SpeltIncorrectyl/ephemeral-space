using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;

namespace Content.Shared._ES.NewGun.Trigger;

/// <summary>
/// Second priority for finding a gun, after innate guns (entities which ARE guns).
/// </summary>
public sealed partial class GunTriggerHeldSystem : EntitySystem
{
    [Dependency] private GunTriggerSystem _gun = default!;
    [Dependency] private SharedHandsSystem _hands = default!;

    [SubscribeLocalEvent(after: [typeof(GunTriggerInnateSystem)])]
    private void OnGetGun(Entity<HandsComponent> ent, ref GetGunEvent args)
    {
        if (args.Gun is not null)
            return;
        if (!_hands.TryGetActiveItem(ent.AsNullable(), out var item))
            return;
        args.Gun ??= _gun.GetGun(item.Value);
    }
}