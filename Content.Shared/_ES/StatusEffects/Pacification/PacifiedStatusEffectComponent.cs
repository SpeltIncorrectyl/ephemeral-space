using Robust.Shared.GameStates;

namespace Content.Shared._ES.StatusEffects.Pacification;

/// <summary>
/// This component makes a status effect pacify who it is applied to.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class PacifiedStatusEffectComponent : Component
{
    /// <summary>
    /// If true, this will prevent you from disarming opponents in combat.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool DisallowDisarm = false;

    /// <summary>
    /// If true, this will disable combat entirely instead of only disallowing attacking living creatures and harmful things.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool DisallowAllCombat = false;

    /// <summary>
    /// When attempting attack against the same entity multiple times,
    /// don't spam popups every frame and instead have a cooldown.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan PopupCooldown = TimeSpan.FromSeconds(3.0);

    /// <summary>
    /// Time at which the next popup can be shown.
    /// </summary>
    [DataField, AutoNetworkedField, AutoPausedField]
    public TimeSpan? NextPopupTime = null;

    /// <summary>
    /// The last entity attacked, used for popup purposes (avoid spam)
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? LastAttackedEntity = null;

    // Prevent cheat clients from using this to identify thieves and players that cannot fight back.
    // This should not matter for prediction reasons since it only blocks user input.
    public override bool SendOnlyToOwner => true;
}