namespace Cthangover.Mods.TurnBattle.Quests
{
    using Cthangover.Core.Actions;
    using Cthangover.Core.Battle;
    using Cthangover.Core.Quests;
    using Cthangover.Core.Utils;

    /// <summary>
    /// Tags the quest of a won battle: <c>battle.init quest_id=… new_tag=…</c> adds the tag to that quest on
    /// victory, on the victory screen before it is shown (<see cref="IWingroundListener.OnBeforeShow"/>).
    /// Only victories reach the victory screen, so a lost battle tags nothing. Quests are reached through the
    /// kernel contract <see cref="QuestServiceRegistry"/>, not through the quest mod's types.
    /// </summary>
    public class QuestBattleListener : IWingroundListener
    {
        /// <inheritdoc />
        public string Id => "turn_battle_base__quests.quest_tag";

        /// <inheritdoc />
        public void OnBeforeExpAward(WingroundContext ctx)
        {
        }

        /// <inheritdoc />
        public void OnAfterLootGenerated(WingroundContext ctx)
        {
        }

        /// <inheritdoc />
        public void OnBeforeShow(WingroundContext ctx)
        {
            var data = ctx?.BattleData;
            if (string.IsNullOrEmpty(data?.QuestId) || string.IsNullOrEmpty(data.NewTag))
                return;

            IQuestService quests = QuestServiceRegistry.Provider;
            if (quests == null || !quests.Exists(data.QuestId))
                return; // the start of the battle already logged the missing quest

            quests.AddTag(data.QuestId, data.NewTag);
            GameLogger.Log("BATTLE", $"QuestBattleListener: won battle tagged quest '{data.QuestId}' with '{data.NewTag}'");
        }
    }
}
