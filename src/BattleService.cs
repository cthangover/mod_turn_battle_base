using System;
using System.Collections.Generic;
using Cthangover.Core.Factories.Impls;
using Cthangover.Core.Quests;
using Cthangover.Core.Scenes;
using Cthangover.Core.Settings;
using Cthangover.Core.UI.Lights;
using Cthangover.Core.Utils;

namespace Cthangover.Core.Battle
{
    /// <summary>
    /// Entry point for starting a battle encounter. Constructs <see cref="BattleData"/>
    /// from scenario parameters and stores it in <see cref="BattleRequest"/> for the
    /// battle scene to pick up. Captures the current background texture (via
    /// <see cref="BackgroundFactory"/> using <see cref="SceneContextNode.LastBackgroundID"/>)
    /// so the battle scene has the correct visual context, and snapshots the
    /// depth/albedo lighting maps from <see cref="UiLightController"/> for visual
    /// consistency. If a quest id is provided, the quest is bound to the encounter
    /// and an optional new tag triggers a quest notification. The active battle core
    /// is resolved from <see cref="BattleCoreRegistry"/> — wrapped in try/catch
    /// because the registry may not be initialized when called from non-battle
    /// contexts (e.g. preloading).
    /// </summary>
    public static class BattleService
    {
        /// <summary>
        /// Constructs and stores <see cref="BattleRequest.Current"/> for the upcoming
        /// encounter. Must be called after a background has been set by a prior
        /// scenario background action, otherwise the backdrop is null. The enemies
        /// string is a comma-separated list resolved by <see cref="BattleData.InitBattle"/>.
        /// </summary>
        public static void Init(string sceneType, string enemies, string questId = null, string newTag = null)
        {
            var enemyList = enemies.Split(',');
            var background = BackgroundFactory.Instance.Get(SceneContextNode.LastBackgroundID);
            GameLogger.Log("BATTLE", $"BattleService.Init: lastBgId='{SceneContextNode.LastBackgroundID}', bgTexture={(background != null ? "loaded" : "NULL")}", background == null ? LogLevel.Error : LogLevel.Debug);
            var data = BattleData.InitBattle(background, sceneType, enemyList);

            data.DepthMap = UiLightController.Instance?.CurrentDepthMap;
            data.AlbedoMap = UiLightController.Instance?.CurrentAlbedoMap;

            GameLogger.Log("BATTLE", $"BattleService.Init: depthMap from LightsCtrl = {(data.DepthMap != null ? "captured" : "NULL")}, albedoMap = {(data.AlbedoMap != null ? "captured" : "NULL")}", data.DepthMap == null && data.AlbedoMap == null ? LogLevel.Error : LogLevel.Debug);

            try
            {
                data.ActiveBattleCore = BattleCoreRegistry.Instance.GetActive()?.Id;
            }
            catch (Exception ex)
            {
                GameLogger.Log("BATTLE", $"BattleService.Init: {ex.Message}\n{ex.StackTrace}", LogLevel.Error);
            }

            if (!string.IsNullOrEmpty(questId))
            {
                try
                {
                    var quest = QuestFactory.Instance.Get(questId);
                    data.Quest = quest;
                    if (!string.IsNullOrEmpty(newTag))
                    {
                        data.NewTag = newTag;
                        quest.SendNotification();
                    }
                }
                catch (KeyNotFoundException)
                {
                    GameLogger.Log("BATTLE", $"BattleService.Init: quest '{questId}' not found, battle will proceed without quest", LogLevel.Error);
                }
            }

            BattleRequest.Current = data;
        }
    }
}
