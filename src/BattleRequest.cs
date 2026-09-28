using Cthangover.Core.Settings;

namespace Cthangover.Core.Battle
{
    /// <summary>
    /// Static hand-off slot between the scenario side (which builds an encounter
    /// through <see cref="BattleService"/>) and the battle scene (which consumes it
    /// in <see cref="BattleSceneContext"/>). Lives in the battle mod instead of the
    /// core runtime state so the core does not need to know about battle data;
    /// <see cref="Current"/> is transient — it is never serialized and is cleared
    /// by the victory/defeat screens when the player leaves the encounter.
    /// </summary>
    public static class BattleRequest
    {
        /// <summary>Pending encounter, or <c>null</c> when no battle was requested.</summary>
        public static BattleData Current { get; set; }
    }
}
