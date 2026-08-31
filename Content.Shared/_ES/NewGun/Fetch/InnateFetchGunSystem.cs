namespace Content.Shared._ES.NewGun.Fetch;

/// <summary>
/// First priority for finding the gun. Check if the entity itself IS the gun.
/// </summary>
public sealed partial class InnateFetchGunSystem : EntitySystem
{
    [SubscribeLocalEvent]
    private void OnGetGun(Entity<NGGunComponent> ent, ref GetGunEvent args)
    {
        args.Gun ??= ent;
    }
}