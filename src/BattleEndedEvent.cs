namespace Cthangover.Core.Events
{
    /// <summary>
    /// Published when the battle concludes (victory or defeat), after the
    /// victory/defeat screen resolves. Mods can use it for post-battle cleanup
    /// or story state. Note: the group defeat with loot and recruitment payload
    /// is the separate <see cref="CharactersDefeatedEvent"/>, which fires from
    /// the victory screen.
    /// </summary>
    [LifecycleEvent]
    [EventKey("battle_ended")]
    public class BattleEndedEvent : ModEvent
    {
        /// <summary>Outcome reported by the battle core (e.g. <c>"victory"</c>, <c>"defeat"</c>).</summary>
        public string Result { get; init; }

        /// <summary>Number of turns the battle lasted.</summary>
        public int Turns { get; init; }
    }
}
