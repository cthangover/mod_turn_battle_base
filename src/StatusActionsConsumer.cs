namespace Cthangover.Core.Factories.Impls
{
    using Cthangover.Core.Cards.StatusEffect;
    using Cthangover.Core.Mods.Consumers;

    /// <summary>
    /// Feeds <see cref="StatusEffectActionFactory"/> with every mod's
    /// <see cref="IStatusActions"/> implementations as the mods load, and
    /// removes them when their mod unloads. Replaces the one-time AppDomain
    /// scan, which saw only the mods loaded before the first lookup and kept
    /// a released mod's behaviours after a game switch.
    /// </summary>
    public class StatusActionsConsumer : ModTypeConsumer<IStatusActions>
    {
        /// <inheritdoc />
        public override string Id => "turn_battle_base.status_actions";

        /// <inheritdoc />
        protected override void Add(IStatusActions instance, string modId) => StatusEffectActionFactory.Instance.Register(instance);

        /// <inheritdoc />
        protected override void Remove(IStatusActions instance, string modId) => StatusEffectActionFactory.Instance.Unregister(instance);
    }
}
