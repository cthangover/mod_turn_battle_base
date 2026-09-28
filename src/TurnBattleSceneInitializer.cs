using System;
using System.Linq;
using System.Reflection;
using Cthangover.Core.Audio;
using Cthangover.Core.Battle;
using Cthangover.Core.Mods;
using Cthangover.Core.Scenes;
using Cthangover.Core.Utils;

namespace Cthangover.TurnBattle
{
    /// <summary>
    /// Lifecycle entry point of the turn-battle mod. Three responsibilities:
    /// <list type="number">
    /// <item>Publishes the logical "Battle" scene route with fade transition and
    /// combat music, so the core can route <c>switch_scene Battle</c> without
    /// hardcoding a battle path.</item>
    /// <item>Registers <see cref="TurnBattleGDScriptHook"/> so GDScript mods can
    /// still supply action executors and winground listeners after the battle
    /// bridge moved out of the core loader.</item>
    /// <item>Discovers <see cref="IBattleCore"/> and <see cref="IWingroundListener"/>
    /// implementations across mod assemblies — this replaces the core-side
    /// registry fan-out in ModAssemblyLoader. Every mod announcement triggers a
    /// scan of that mod's assembly, and the resources-ready pass re-scans the
    /// whole AppDomain as a safety net for assemblies whose name differs from
    /// the mod id (precompiled DLLs).</item>
    /// </list>
    /// </summary>
    public class TurnBattleSceneInitializer : IModInitializer
    {
        private const string BattleScenePath = "scenes/battle/battle.tscn";

        /// <summary>
        /// Registers battle cores and winground listeners from the mod that just
        /// announced itself, and installs the GDScript hook. Assemblies are looked
        /// up by mod id (the compiler names every source-compiled assembly after
        /// its mod), so no event plumbing is needed — the initializer simply
        /// rescans as the mod set grows. The hook must be installed here, not at
        /// resources-ready: ModGDScriptLoader processes <c>.gd</c> sources during
        /// assembly registration finalization, before resources become ready.
        /// </summary>
        public void OnModLoaded(string modId)
        {
            GDScriptHookRegistry.Register(new TurnBattleGDScriptHook());
            RegisterAssembly(FindAssembly(modId));
        }

        /// <summary>
        /// Publishes the scene route and rescans all loaded assemblies for battle
        /// cores and winground listeners.
        /// </summary>
        public void OnModResourcesReady()
        {
            SceneRouteRegistry.Register("Battle", BattleScenePath, fade: true, musicScene: "Battle", music: MusicType.Combat);
            BattleCoreRegistry.Instance.RegisterAll();
            WingroundListenerRegistry.Instance.RegisterAll();
        }

        private static void RegisterAssembly(Assembly assembly)
        {
            if (assembly == null)
                return;

            BattleCoreRegistry.Instance.RegisterAssembly(assembly);
            WingroundListenerRegistry.Instance.RegisterAssembly(assembly);
        }

        private static Assembly FindAssembly(string modId)
        {
            if (string.IsNullOrEmpty(modId))
                return null;

            return AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(assembly => assembly.GetName().Name == modId);
        }
    }
}
