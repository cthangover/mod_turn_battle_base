using Cthangover.Core.Battle;
using Cthangover.Core.Battle.Actions;
using Cthangover.Core.Mods;
using Cthangover.Core.Utils;
using Godot;

namespace Cthangover.TurnBattle
{
    /// <summary>
    /// GDScript hook of the turn-battle mod. The core's ModGDScriptLoader walks
    /// every <c>.gd</c> file generically and hands objects to registered hooks;
    /// this hook recognizes the two battle-side contracts:
    /// <list type="bullet">
    /// <item><c>get_action_id()</c> + <c>execute(action, user, target)</c> — wraps
    /// the script in a <see cref="GDBattleActionExecutor"/> and registers it in the
    /// global <see cref="ActionExecutorHub"/> pool.</item>
    /// <item><c>get_winground_listener_id()</c> plus any of the three
    /// <c>on_winground_*</c> methods — wraps the script in a
    /// <see cref="GDWingroundListener"/>.</item>
    /// </list>
    /// Keeping the recognition here means the core loader no longer mentions any
    /// battle method name.
    /// </summary>
    public class TurnBattleGDScriptHook : IGDScriptHook
    {
        /// <summary>Stable id used by <see cref="GDScriptHookRegistry"/> for replacement on reload.</summary>
        public string Id => "turn_battle_base.gd_hook";

        /// <summary>Inspects one compiled script and registers an adapter when a contract matches.</summary>
        public void TryRegister(GodotObject obj, string modId, string filePath)
        {
            if (obj == null)
                return;

            if (obj.HasMethod("get_action_id") && obj.HasMethod("execute"))
            {
                var actionId = obj.Call("get_action_id").AsString();
                if (!string.IsNullOrEmpty(actionId))
                {
                    ActionExecutorHub.Instance.RegisterGlobal(new GDBattleActionExecutor(actionId, obj));
                    GameLogger.Log("GDS_MOD",
                        $"Registered GDScript battle executor '{actionId}' from mod '{modId}' ({filePath})");
                }
            }

            var hasWingroundHook = obj.HasMethod("on_winground_before_exp")
                || obj.HasMethod("on_winground_after_loot")
                || obj.HasMethod("on_winground_before_show");

            if (hasWingroundHook && obj.HasMethod("get_winground_listener_id"))
            {
                var listenerId = obj.Call("get_winground_listener_id").AsString();
                if (!string.IsNullOrEmpty(listenerId))
                {
                    WingroundListenerRegistry.Instance.RegisterListener(new GDWingroundListener(listenerId, obj));
                    GameLogger.Log("GDS_MOD",
                        $"Registered GDScript winground listener '{listenerId}' from mod '{modId}' ({filePath})");
                }
            }
        }
    }
}
