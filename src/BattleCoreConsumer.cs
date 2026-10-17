namespace Cthangover.Core.Battle
{
    using Cthangover.Core.Mods.Consumers;

    /// <summary>
    /// Feeds <see cref="BattleCoreRegistry"/> from the kernel's assembly
    /// fan-out: every mod's <see cref="IBattleCore"/> implementations are
    /// registered as the mod loads and removed when it unloads, so a game
    /// switch or hot reload never leaves a core of a released mod behind.
    /// </summary>
    public class BattleCoreConsumer : ModTypeConsumer<IBattleCore>
    {
        /// <inheritdoc />
        public override string Id => "turn_battle_base.battle_cores";

        /// <inheritdoc />
        protected override void Add(IBattleCore instance, string modId) => BattleCoreRegistry.Instance.Register(instance);

        /// <inheritdoc />
        protected override void Remove(IBattleCore instance, string modId) => BattleCoreRegistry.Instance.Unregister(instance);
    }
}
