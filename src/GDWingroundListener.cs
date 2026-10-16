using Godot;

namespace Cthangover.Core.Battle
{
	/// <summary>
	/// GDScript bridge for <see cref="IWingroundListener"/>. Holds a reference to a
	/// GDScript object and delegates each hook through
	/// <see cref="GodotObject.HasMethod"/> / <see cref="GodotObject.Call"/>.
	/// <para/>
	/// All three hooks are optional — the GDScript mod implements only the methods
	/// it needs. The context is marshalled into a plain Godot dictionary
	/// (<c>exp_gained</c>, <c>defeated_enemies</c>, <c>generated_loot</c>,
	/// <c>return_scene</c>) before each call and synced back afterward, so
	/// GDScript mutations to exp and loot propagate to the C# victory pipeline.
	/// Dictionaries replace the old GDWingroundContext RefCounted DTO because mod
	/// classes compiled at runtime are not visible to GDScript.
	/// </summary>
	public class GDWingroundListener : IWingroundListener
	{
		private readonly GodotObject _instance;

		/// <summary>Registry key reported by the script's <c>get_winground_listener_id()</c>; deduplicates on rescan.</summary>
		public string Id { get; }

		/// <summary>
		/// Creates a bridge around a live GDScript object. The caller must ensure
		/// the object outlives the registry.
		/// </summary>
		public GDWingroundListener(string id, GodotObject instance)
		{
			Id = id;
			_instance = instance;
		}

		/// <summary>Calls the script's <c>on_winground_before_exp(ctx)</c> if implemented; no-op otherwise.</summary>
		public void OnBeforeExpAward(WingroundContext ctx)
		{
			if (!_instance.HasMethod("on_winground_before_exp"))
				return;

			var gdCtx = BattleGDScriptBridge.WingroundToDictionary(ctx);
			_instance.Call("on_winground_before_exp", gdCtx);
			BattleGDScriptBridge.SyncWingroundBack(gdCtx, ctx);
		}

		/// <summary>Calls the script's <c>on_winground_after_loot(ctx)</c> if implemented; no-op otherwise.</summary>
		public void OnAfterLootGenerated(WingroundContext ctx)
		{
			if (!_instance.HasMethod("on_winground_after_loot"))
				return;

			var gdCtx = BattleGDScriptBridge.WingroundToDictionary(ctx);
			_instance.Call("on_winground_after_loot", gdCtx);
			BattleGDScriptBridge.SyncWingroundBack(gdCtx, ctx);
		}

		/// <summary>Calls the script's <c>on_winground_before_show(ctx)</c> if implemented; no-op otherwise.</summary>
		public void OnBeforeShow(WingroundContext ctx)
		{
			if (!_instance.HasMethod("on_winground_before_show"))
				return;

			var gdCtx = BattleGDScriptBridge.WingroundToDictionary(ctx);
			_instance.Call("on_winground_before_show", gdCtx);
			BattleGDScriptBridge.SyncWingroundBack(gdCtx, ctx);
		}
	}
}
