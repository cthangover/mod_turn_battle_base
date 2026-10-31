namespace Cthangover.Mods.TurnBattle.Tests
{
    using Cthangover.Core.Actions;
    using Cthangover.Core.Battle;
    using Cthangover.Core.Mods;
    using Cthangover.Core.Mods.Testing;
    using Cthangover.Core.Quests;

    /// <summary>
    /// The battle does not depend on quests: the quest id and tag travel as text, and the quest integration
    /// (<c>turn_battle_base__quests</c>) tags the quest on victory. The run without quests (<c>--frontend</c>)
    /// checks the plain battle; the run with quests checks the tag through the integration's listener.
    /// </summary>
    public class QuestBattleTests
    {
        private const string QuestId = "MeetingMurakamiQuest";
        private const string TestTag = "turn_battle_base_test_tag";

        [ModTest("quests/battle_start_keeps_the_quest_text")]
        static void BattleStartKeepsTheQuestText(ModTestContext ctx)
        {
            try
            {
                BattleService.Init("town_entry", "wolf_1", QuestId, TestTag);
                var data = BattleRequest.Current;
                ModAssert.NotNull(data, "the encounter is pending");
                ModAssert.Equal(QuestId, data.QuestId, "quest id carried as text");
                ModAssert.Equal(TestTag, data.NewTag, "tag carried as text");
            }
            finally
            {
                BattleRequest.Current = null;
            }
        }

        [ModTest("quests/battle_without_quest_has_no_quest")]
        static void BattleWithoutQuestHasNoQuest(ModTestContext ctx)
        {
            try
            {
                BattleService.Init("town_entry", "wolf_1");
                var data = BattleRequest.Current;
                ModAssert.NotNull(data, "the encounter is pending without a quest");
                ModAssert.Null(data.QuestId, "no quest id");
                ModAssert.Null(data.NewTag, "no tag");
            }
            finally
            {
                BattleRequest.Current = null;
            }
        }

        [ModTest("quests/victory_tags_the_quest_only_with_quests")]
        static void VictoryTagsTheQuestOnlyWithQuests(ModTestContext ctx)
        {
            var quests = QuestServiceRegistry.Provider;
            if (quests == null)
            {
                // Without quests_base the integration is not mounted: nothing listens, nothing is tagged.
                var listeners = ModRegistry.Instance.GetMod("turn_battle_base__quests") == null;
                ModAssert.True(listeners, "the quest integration is absent without quests_base");
                return;
            }

            ModAssert.True(quests.Exists(QuestId), $"the fixture quest {QuestId} is in the game");
            ModAssert.NotNull(ModRegistry.Instance.GetMod("turn_battle_base__quests"), "the integration is mounted next to quests_base");

            try
            {
                BattleService.Init("town_entry", "wolf_1", QuestId, TestTag);
                var ctx2 = new WingroundContext { BattleData = BattleRequest.Current };
                WingroundListenerRegistry.Instance.NotifyBeforeShow(ctx2);
                ModAssert.True(quests.Get(QuestId).ContainsTag(TestTag), "the won battle tagged the quest");
            }
            finally
            {
                quests.RemoveTag(QuestId, TestTag);
                BattleRequest.Current = null;
            }
        }
    }
}
