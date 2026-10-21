namespace Cthangover.Mods.TurnBattle.Tests
{
    using Cthangover.Core.Cards.StatusEffect;
    using Cthangover.Core.Characters;
    using Cthangover.Core.Mods.Testing;

    /// <summary>
    /// Lifetime of status effects: the countdown runs once per turn start, a
    /// duration from the effect's data counts like an explicit one, and the
    /// turn-end hook does not shorten an effect a second time.
    /// </summary>
    public class StatusEffectQueueTests
    {
        private const string Stun = "effect/physics/stun";

        [ModTest("status/data_duration_expires")]
        static void DataDurationExpires(ModTestContext ctx)
        {
            var queue = StatusEffectService.Of(new Character { ID = "status_test" });

            ModAssert.True(queue.Add(Stun), "stun applied with its data duration");
            queue.OnTurnStart();
            queue.OnTurnStart();
            ModAssert.True(queue.HasStun(), "stun still active after 2 of 3 turns");
            queue.OnTurnStart();
            ModAssert.False(queue.HasStun(), "stun expired after its 3 turns");
        }

        [ModTest("status/turn_end_does_not_shorten")]
        static void TurnEndDoesNotShorten(ModTestContext ctx)
        {
            var queue = StatusEffectService.Of(new Character { ID = "status_test" });

            queue.Add(Stun, 2);
            queue.OnTurnEnd();
            queue.OnTurnStart();
            ModAssert.True(queue.HasStun(), "one turn of two left after a full turn");
            queue.OnTurnEnd();
            queue.OnTurnStart();
            ModAssert.False(queue.HasStun(), "expired after two turns");
        }
    }
}
