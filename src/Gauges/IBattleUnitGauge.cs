namespace Cthangover.Core.Battle
{
    using System.Collections.Generic;
    using Cthangover.Core.Characters;

    /// <summary>
    /// Supplies extra bars of battle units (found in mod assemblies by
    /// <see cref="BattleUnitGaugeConsumer"/>): a mod that adds a resource — mana of
    /// <c>battle_formulas</c>, a rage meter — shows it in every battle core that draws
    /// <see cref="BattleUnitGauges"/>, without knowing the cores.
    /// </summary>
    public interface IBattleUnitGauge
    {
        /// <summary>Order among the providers (lower first).</summary>
        int Order { get; }

        /// <summary>The bars of <paramref name="unit"/> now; empty when it has none.</summary>
        IEnumerable<BattleGauge> Read(Character unit);
    }
}
