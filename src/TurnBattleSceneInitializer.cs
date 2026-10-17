using Cthangover.Core.Audio;
using Cthangover.Core.Battle;
using Cthangover.Core.Mods;
using Cthangover.Core.Scenes;

namespace Cthangover.TurnBattle
{
    /// <summary>
    /// Lifecycle entry point of the turn-battle mod. Two responsibilities:
    /// <list type="number">
    /// <item>Publishes the logical "Battle" scene route with fade transition and
    /// combat music, so the core can route <c>switch_scene Battle</c> without
    /// hardcoding a battle path.</item>
    /// <item>Registers <see cref="TurnBattleGDScriptHook"/> so GDScript mods can
    /// still supply action executors and winground listeners after the battle
    /// bridge moved out of the core loader.</item>
    /// </list>
    /// Battle cores, winground listeners and status effect behaviours of all
    /// mods reach their registries through the kernel's assembly consumers
    /// (<see cref="BattleCoreConsumer"/>, <see cref="WingroundListenerConsumer"/>,
    /// <see cref="StatusActionsConsumer"/>), not from here.
    /// </summary>
    public class TurnBattleSceneInitializer : IModInitializer
    {
        private const string BattleScenePath = "scenes/battle/battle.tscn";

        /// <summary>
        /// Installs the GDScript hook. It must happen here, not at
        /// resources-ready: ModGDScriptLoader processes <c>.gd</c> sources during
        /// assembly registration finalization, before resources become ready.
        /// </summary>
        public void OnModLoaded(string modId)
        {
            GDScriptHookRegistry.Register(new TurnBattleGDScriptHook());
        }

        /// <summary>Publishes the scene route.</summary>
        public void OnModResourcesReady()
        {
            SceneRouteRegistry.Register("Battle", BattleScenePath, fade: true, musicScene: "Battle", music: MusicType.Combat);
        }
    }
}
