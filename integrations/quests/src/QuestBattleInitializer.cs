namespace Cthangover.Mods.TurnBattle.Quests
{
    using Cthangover.Core.Actions;
    using Cthangover.Core.Battle;
    using Cthangover.Core.Events;
    using Cthangover.Core.Mods;
    using Cthangover.Core.Quests;
    using Cthangover.Core.Utils;

    /// <summary>
    /// When a battle with <c>quest_id</c> starts, checks that the quest exists (an error in the log if not)
    /// and notifies the player about its new tag, as the battle used to do when it was initialised.
    /// </summary>
    public class QuestBattleInitializer : IModInitializer
    {
        /// <inheritdoc />
        public void OnModLoaded(string modId)
        {
            ModEventBus.Subscribe<BattleStartedEvent>(OnBattleStarted, modId: modId);
        }

        /// <inheritdoc />
        public void OnModResourcesReady()
        {
        }

        private static void OnBattleStarted(BattleStartedEvent evt)
        {
            var data = BattleRequest.Current;
            if (string.IsNullOrEmpty(data?.QuestId))
                return;

            IQuestService quests = QuestServiceRegistry.Provider;
            if (quests == null || !quests.Exists(data.QuestId))
            {
                GameLogger.Log("BATTLE", $"QuestBattleInitializer: quest '{data.QuestId}' not found, battle proceeds without it", LogLevel.Error);
                return;
            }

            if (!string.IsNullOrEmpty(data.NewTag))
                quests.SendNotification(data.QuestId);
        }
    }
}
