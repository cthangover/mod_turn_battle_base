namespace Cthangover.Core.Events
{
    /// <summary>
    /// Published when a character dies in combat. Payload copies data
    /// as strings — does not pass a live Node (may be freed by the time
    /// deferred subscribers are called).
    /// </summary>
    [EventKey("character_died")]
    public class CharacterDiedEvent : ModEvent
    {
        /// <summary>Template id of the character that died.</summary>
        public string CharacterId { get; init; }
        /// <summary>Display name at time of death (resolved locale string).</summary>
        public string CharacterName { get; init; }
        /// <summary>Free-form id of the reporting battle system.</summary>
        public string Source { get; init; }
    }
}
