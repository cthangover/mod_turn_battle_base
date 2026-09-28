using System.Runtime.CompilerServices;
using Cthangover.Core.Characters;

namespace Cthangover.Core.Cards.StatusEffect
{
    /// <summary>
    /// Side-table that owns the <see cref="StatusEffectQueue"/> of every character
    /// participating in battle. The queue used to live on <see cref="Character"/>
    /// itself, but the core character model must stay free of battle subsystems —
    /// so the battle mod keeps queues keyed by character instance instead.
    /// <para/>
    /// Uses <see cref="ConditionalWeakTable{TKey, TValue}"/>: queues die together
    /// with their character, which matters because each battle constructs fresh
    /// copies via <see cref="Character.Copy"/> and discards them afterwards. All
    /// access happens on the main thread during battle, so no extra locking is
    /// needed beyond what the weak table provides.
    /// </summary>
    public static class StatusEffectService
    {
        private static readonly ConditionalWeakTable<Character, StatusEffectQueue> queues = new();

        /// <summary>
        /// Returns the queue of <paramref name="character"/>, creating an empty one
        /// on first access. Returns <c>null</c> for a null character so call sites
        /// can keep their existing null-conditional patterns.
        /// </summary>
        public static StatusEffectQueue Of(Character character)
        {
            if (character == null)
                return null;

            return queues.GetValue(character, CreateQueue);
        }

        private static StatusEffectQueue CreateQueue(Character owner)
        {
            return new StatusEffectQueue(owner);
        }
    }
}
