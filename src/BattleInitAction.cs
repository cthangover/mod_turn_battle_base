using Cthangover.Core.Actions;

namespace Cthangover.Core.Battle
{
    /// <summary>
    /// Dispatched by the scenario DSL to initiate battle. Reads "scene",
    /// "enemies", "quest_id" and "new_tag" from dialog variables and calls
    /// <see cref="BattleService.Init"/>. The enemies string is a comma-separated
    /// list embedded in the scenario script. Battle initialization must happen
    /// *after* the background has been set by a prior background action, otherwise
    /// the encounter gets a null backdrop.
    /// <para/>
    /// Registered by the battle mod's assembly scan, so the core scenario action
    /// registry stays free of battle commands.
    /// </summary>
    public class BattleInitAction : IScenarioAction
    {
        /// <summary>Registered as "battle.init" — the scenario DSL's primary battle initiation command.</summary>
        public string Name => "battle.init";

        /// <summary>
        /// Reads battle parameters from dialog variables and initiates battle
        /// construction. Requires both "scene" and "enemies" to be non-empty —
        /// returns early with a log if either is missing.
        /// </summary>
        public void Run(IActionContext ctx)
        {
            var sceneRaw = ctx.GetParam("scene");
            var enemies = ctx.GetParam("enemies");
            var questId = ctx.GetParam("quest_id");
            var newTag = ctx.GetParam("new_tag");

            if (string.IsNullOrEmpty(sceneRaw) || string.IsNullOrEmpty(enemies))
            {
                ctx.Log("EVENT", "BattleInitAction: missing 'scene' or 'enemies' variable");
                return;
            }

            BattleService.Init(sceneRaw, enemies, questId, newTag);
            ctx.Log("EVENT", $"BattleInitAction: battle started at '{sceneRaw}' with enemies '{enemies}'");
        }
    }
}
