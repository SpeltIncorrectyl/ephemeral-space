using Robust.Shared.Containers;

namespace Content.Shared._ES.Car;

/// <summary>
/// System that manages the interior seats within vehicles like cars.
/// </summary>
public sealed partial class InteriorSeatsSystem : EntitySystem
{
    [Dependency] private SharedContainerSystem _container = default!;

    [SubscribeLocalEvent]
    private void OnInit(Entity<InteriorSeatsComponent> ent, ref ComponentInit args)
    {
        foreach (var seat in ent.Comp.Seats)
        {
            ent.Comp.SeatSlots[seat] = _container.EnsureContainer<ContainerSlot>(ent, seat);
        }
    }
}