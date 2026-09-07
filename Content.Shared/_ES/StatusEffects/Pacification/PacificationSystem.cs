using System.Diagnostics.CodeAnalysis;
using Content.Shared._ES.StatusEffects.Pacification;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.CombatMode;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.FixedPoint;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Events;
using Content.Shared.Light.Components;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Timing;

namespace Content.Shared._ES.StatusEffects.Pacification;

public sealed partial class PacificationSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actionsSystem = default!;
    [Dependency] private SharedCombatModeSystem _combatSystem = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IGameTiming _timing = default!;

    private bool PacifiedCanAttack(EntityUid user, EntityUid target, [NotNullWhen(false)] out string? reason)
    {
        var ev = new AttemptPacifiedAttackEvent(user);

        RaiseLocalEvent(target, ref ev);

        if (ev.Cancelled)
        {
            reason = ev.Reason;
            return false;
        }

        reason = null;
        return true;
    }

    private void ShowPopup(EntityUid user, Entity<PacifiedStatusEffectComponent> status, EntityUid target, string reason)
    {
        // Popup logic.
        // Cooldown is needed because the input events for melee/shooting etc. will fire continuously
        if (target == status.Comp.LastAttackedEntity
            && !(_timing.CurTime > status.Comp.NextPopupTime))
            return;

        var targetName = Identity.Entity(target, EntityManager);
        _popup.PopupEntity(Loc.GetString(reason, ("entity", targetName)), user, user);
        status.Comp.NextPopupTime = _timing.CurTime + status.Comp.PopupCooldown;
        status.Comp.LastAttackedEntity = target;
    }

    [SubscribeLocalEvent]
    private void OnShootAttempt(Entity<PacifiedStatusEffectComponent> ent, ref StatusEffectRelayedEvent<ShotAttemptedEvent> args)
    {
        if (HasComp<PacifismAllowedGunComponent>(args.Args.Used))
            return;

        if (TryComp<BatteryWeaponFireModesComponent>(args.Args.Used, out var component))
            if (component.FireModes[component.CurrentFireMode].PacifismAllowedMode)
                return;

        if (!TryComp<StatusEffectComponent>(ent, out var statusEffectComp) || statusEffectComp.AppliedTo is not { } user)
            return;

        // Disallow firing guns in all cases.
        ShowPopup(user, ent, args.Args.Used, "pacified-cannot-fire-gun");
        var newArgs = args.Args;
        newArgs.Cancel();
        args.Args = newArgs;
    }

    [SubscribeLocalEvent]
    private void OnAttackAttempt(Entity<PacifiedStatusEffectComponent> ent, ref StatusEffectRelayedEvent<AttackAttemptEvent> args)
    {
        if (ent.Comp.DisallowAllCombat || args.Args.Disarm && ent.Comp.DisallowDisarm)
        {
            args.Args.Cancel();
            return;
        }

        // If it's a disarm, let it go through (unless we disallow them, which is handled earlier)
        if (args.Args.Disarm)
            return;

        // Allow attacking with no target. This should be fine.
        // If it's a wide swing, that will be handled with a later AttackAttemptEvent raise.
        if (args.Args.Target == null)
            return;

        // If we would do zero damage, it should be fine.
        if (args.Args.Weapon != null && args.Args.Weapon.Value.Comp.Damage.GetTotal() == FixedPoint2.Zero)
            return;

        if (!TryComp<StatusEffectComponent>(ent, out var statusEffectComp) || statusEffectComp.AppliedTo is not { } user)
            return;

        if (PacifiedCanAttack(user, args.Args.Target.Value, out var reason))
            return;

        ShowPopup(user, ent, args.Args.Target.Value, reason);
        args.Args.Cancel();
    }

    [SubscribeLocalEvent]
    private void OnApply(Entity<PacifiedStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        if (!TryComp<CombatModeComponent>(args.Target, out var combatMode))
            return;

        if (ent.Comp.DisallowDisarm && combatMode.CanDisarm != null)
            _combatSystem.SetCanDisarm(args.Target, false, combatMode);

        if (ent.Comp.DisallowAllCombat)
        {
            _combatSystem.SetInCombatMode(args.Target, false, combatMode);
            _actionsSystem.SetEnabled(combatMode.CombatToggleActionEntity, false);
        }
    }

    [SubscribeLocalEvent]
    private void OnRemove(Entity<PacifiedStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (!TryComp<CombatModeComponent>(args.Target, out var combatMode))
            return;

        if (combatMode.CanDisarm != null)
            _combatSystem.SetCanDisarm(args.Target, true, combatMode);

        _actionsSystem.SetEnabled(combatMode.CombatToggleActionEntity, true);
    }

    [SubscribeLocalEvent]
    private void OnBeforeThrow(Entity<PacifiedStatusEffectComponent> ent, ref StatusEffectRelayedEvent<BeforeThrowEvent> args)
    {
        var thrownItem = args.Args.ItemUid;
        var itemName = Identity.Entity(thrownItem, EntityManager);

        // Raise an AttemptPacifiedThrow event and rely on other systems to check
        // whether the candidate item is OK to throw:
        var ev = new AttemptPacifiedThrowEvent(thrownItem, ent);
        RaiseLocalEvent(thrownItem, ref ev);
        if (!ev.Cancelled)
            return;

        args.Args = args.Args with { Cancelled = true };

        if (!TryComp<StatusEffectComponent>(ent, out var statusEffectComp) || statusEffectComp.AppliedTo is not { } user)
            return;

        // Tell the player why they can’t throw stuff:
        var cannotThrowMessage = ev.CancelReasonMessageId ?? "pacified-cannot-throw";
        _popup.PopupEntity(Loc.GetString(cannotThrowMessage, ("projectile", itemName)), user, user);
    }

    [SubscribeLocalEvent]
    private void OnPacifiedDangerousAttack(Entity<PacifismDangerousAttackComponent> ent, ref AttemptPacifiedAttackEvent args)
    {
        args.Cancelled = true;
        args.Reason = "pacified-cannot-harm-indirect";
    }
}


/// <summary>
/// Raised when a Pacified entity attempts to throw something.
/// The throw is only permitted if this event is not cancelled.
/// </summary>
[ByRefEvent]
public struct AttemptPacifiedThrowEvent
{
    public EntityUid ItemUid;
    public EntityUid PlayerUid;

    public AttemptPacifiedThrowEvent(EntityUid itemUid,  EntityUid playerUid)
    {
        ItemUid = itemUid;
        PlayerUid = playerUid;
    }

    public bool Cancelled { get; private set; } = false;
    public string? CancelReasonMessageId { get; private set; }

    /// <param name="reasonMessageId">
    /// Localization string ID for the reason this event has been cancelled.
    /// If null, a generic message will be shown to the player.
    /// Note that any supplied localization string MUST accept a '$projectile'
    /// parameter specifying the name of the thrown entity.
    /// </param>
    public void Cancel(string? reasonMessageId = null)
    {
        Cancelled = true;
        CancelReasonMessageId = reasonMessageId;
    }
}

/// <summary>
///     Raised ref directed on an entity when a pacified user is attempting to attack it.
///     If <see cref="Cancelled"/> is true, don't allow attacking.
///     <see cref="Reason"/> should be a loc string, if there needs to be special text for why the user isn't able to attack this.
/// </summary>
[ByRefEvent]
public record struct AttemptPacifiedAttackEvent(EntityUid User, bool Cancelled = false, string Reason = "pacified-cannot-harm-directly");
