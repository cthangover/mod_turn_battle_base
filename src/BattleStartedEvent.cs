namespace Cthangover.Core.Events
{
    /// <summary>
    /// Published when a battle encounter is initialised (from
    /// <see cref="Cthangover.Core.Battle.BattleService.Init"/>, before the battle
    /// scene loads). Lets mods observe or prepare for the fight — e.g. pre-buff
    /// party members or record the encounter for analytics. The
    /// <c>[LifecycleEvent]</c> marker keeps it out of condition-cached flows.
    /// </summary>
    [LifecycleEvent]
    [EventKey("battle_started")]
    public class BattleStartedEvent : ModEvent
    {
        /// <summary>Logical name of the battle scene route being entered (e.g. <c>"Battle"</c>).</summary>
        public string BattleType { get; init; }
        /// <summary>Template ids of the enemies participating in the encounter.</summary>
        public string[] Enemies { get; init; }
    }
}
