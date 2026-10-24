namespace Cthangover.Core.Battle
{
    using Cthangover.Core.Mods.Consumers;

    /// <summary>Registers every <see cref="IBattleUnitGauge"/> of a loaded mod assembly in <see cref="BattleUnitGauges"/>.</summary>
    public class BattleUnitGaugeConsumer : ModTypeConsumer<IBattleUnitGauge>
    {
        /// <inheritdoc />
        public override string Id => "turn_battle_base.unit_gauges";

        /// <inheritdoc />
        protected override void Add(IBattleUnitGauge instance, string modId) => BattleUnitGauges.Add(instance);

        /// <inheritdoc />
        protected override void Remove(IBattleUnitGauge instance, string modId) => BattleUnitGauges.Remove(instance);
    }
}
