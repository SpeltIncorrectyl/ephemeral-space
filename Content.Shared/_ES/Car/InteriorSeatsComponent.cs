using Robust.Shared.Containers;
using Robust.Shared.GameStates;

namespace Content.Shared._ES.Car;

/// <summary>
/// Gives an entity interior seats you sit inside of.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class InteriorSeatsComponent : Component
{
    /// <summary>
    /// A list of seats.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<string> Seats = new();

    /// <summary>
    /// Dictionary with the container slots for each seat.
    /// Is not datafield and is not networked but is instead generated on init on both server and client.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<string, ContainerSlot> SeatSlots = new();
}