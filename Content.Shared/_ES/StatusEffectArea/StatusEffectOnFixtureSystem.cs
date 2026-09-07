using Content.Shared.StatusEffectNew;
using Content.Shared.Whitelist;
using Robust.Shared.Physics.Events;
using Robust.Shared.Utility;

namespace Content.Shared._ES.StatusEffectArea;

public sealed partial class StatusEffectOnFixtureSystem : EntitySystem
{
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private StatusEffectsSystem _status = default!;

    [SubscribeLocalEvent]
    private void OnCollisionStart(Entity<StatusEffectOnFixtureComponent> ent, ref StartCollideEvent args)
    {
        if (args.OurFixtureId != ent.Comp.Fixture || !args.OtherFixture.Hard)
            return;

        if (_whitelist.IsWhitelistFail(ent.Comp.Whitelist, args.OtherEntity))
            return;

        foreach (var effect in ent.Comp.Effects)
        {
            _status.TryAddIndefiniteStatusEffect(args.OtherEntity, effect, out _);
        }
    }

    [SubscribeLocalEvent]
    private void OnCollisionEnd(Entity<StatusEffectOnFixtureComponent> ent, ref EndCollideEvent args)
    {
        if (args.OurFixtureId != ent.Comp.Fixture || !args.OtherFixture.Hard)
            return;

        if (_whitelist.IsWhitelistFail(ent.Comp.Whitelist, args.OtherEntity))
            return;

        foreach (var effect in ent.Comp.Effects)
        {
            _status.TryRemoveStatusEffect(args.OtherEntity, effect);
        }
    }
}