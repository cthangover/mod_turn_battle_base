namespace Cthangover.Core.Battle
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cthangover.Core.Characters;
    using Cthangover.Core.Utils;

    /// <summary>
    /// The registered <see cref="IBattleUnitGauge"/>s and the bars they give for a unit. A
    /// provider calls <see cref="NotifyChanged"/> when a value moved (mana spent), so the
    /// cores redraw that unit; cores also read the bars whenever they redraw it themselves.
    /// </summary>
    public static class BattleUnitGauges
    {
        private static readonly List<IBattleUnitGauge> Providers = new();

        /// <summary>Raised with a unit id (null = every unit) when its bars changed.</summary>
        public static event Action<string> Changed;

        /// <summary>Registers a provider.</summary>
        public static void Add(IBattleUnitGauge gauge)
        {
            if (gauge != null && !Providers.Contains(gauge))
                Providers.Add(gauge);
        }

        /// <summary>Removes a provider (its mod is unloaded).</summary>
        public static void Remove(IBattleUnitGauge gauge) => Providers.Remove(gauge);

        /// <summary>Every bar of <paramref name="unit"/> with a positive maximum, providers in order; a failing provider is logged and skipped.</summary>
        public static List<BattleGauge> Read(Character unit)
        {
            var result = new List<BattleGauge>();
            if (unit == null)
                return result;
            foreach (var provider in Providers.OrderBy(p => p.Order).ToList())
            {
                try
                {
                    result.AddRange((provider.Read(unit) ?? Enumerable.Empty<BattleGauge>()).Where(g => g != null && g.Max > 0));
                }
                catch (Exception ex)
                {
                    GameLogger.Log("BATTLE", $"gauge provider {provider.GetType().Name} failed for '{unit.ID}': {ex.Message}", LogLevel.Error);
                }
            }
            return result;
        }

        /// <summary>Tells the cores that the bars of <paramref name="unitId"/> changed.</summary>
        public static void NotifyChanged(string unitId) => Changed?.Invoke(unitId);
    }
}
