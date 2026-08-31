using Robust.Shared.Map;

namespace Content.Shared._ES.NewGun.Fetch;

/// <summary>
/// Handles guns shooting when their triggers are held.
/// </summary>
public sealed partial class FetchGunSystem : EntitySystem
{
    /// <summary>
    /// Find the gun an entity is currently using, if any.
    /// This is what you would shoot with if you were in combat mode and clicked.
    /// 
    /// The priorities are as follows:
    /// innate gun -> held gun
    /// 
    /// The reason the handlers for <see cref="GetGunEvent"/> are in their own tiny systems is so system ordering can be used to enforce the priority.
    /// </summary>
    public Entity<NGGunComponent>? GetGun(EntityUid user)
    {
        var ev = new GetGunEvent(user);
        RaiseLocalEvent(user, ref ev);
        return ev.Gun;
    }
}

/// <summary>
/// Event raised on a user to find their gun.
/// It is done this way to handle all the many different possible cases.
/// User is gun, user holding gun, user has automatic gun implant, user is inside mech with gun, e.t.c.
/// </summary>
[ByRefEvent]
public record struct GetGunEvent(EntityUid User, Entity<NGGunComponent>? Gun = null);