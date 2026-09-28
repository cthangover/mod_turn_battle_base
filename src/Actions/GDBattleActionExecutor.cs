using Cthangover.Core.Characters;
using Godot;

namespace Cthangover.Core.Battle.Actions
{
	/// <summary>
	/// Adapter that lets a GDScript mod act as a battle action executor. The
	/// script is detected by <see cref="TurnBattleGDScriptHook"/> when it exposes
	/// <c>get_action_id()</c> and <c>execute(action, user, target)</c>; this adapter
	/// flattens the C# arguments into Godot dictionaries via
	/// <see cref="BattleGDScriptBridge"/> and maps the returned dictionary back to
	/// <see cref="ChangedAttributes"/>. Dictionaries are used instead of custom
	/// RefCounted DTOs because runtime-compiled mod classes get no Godot source
	/// generation, so their members would not be visible to GDScript.
	/// </summary>
	public class GDBattleActionExecutor : IActionExecutor
	{
		private readonly GodotObject _instance;

		/// <summary>Creates an adapter around a live GDScript object. The object is retained by reference.</summary>
		public GDBattleActionExecutor(string actionId, GodotObject instance)
		{
			ActionId = actionId;
			_instance = instance;
		}

		/// <summary>Action id reported by the script through <c>get_action_id()</c>.</summary>
		public string ActionId { get; }

		/// <summary>
		/// Calls the script and converts its result. A script that returns
		/// anything other than a dictionary (or throws) yields a failed result so
		/// one broken mod cannot abort the action pipeline.
		/// </summary>
		public ChangedAttributes Execute(ActionCharacter action, Character user, Character target)
		{
			var gdAction = BattleGDScriptBridge.ActionToDictionary(action);
			var gdUser = BattleGDScriptBridge.CharacterToDictionary(user);
			var gdTarget = BattleGDScriptBridge.CharacterToDictionary(target);

			var variant = _instance.Call("execute", gdAction, gdUser, gdTarget);
			if (variant.VariantType != Variant.Type.Dictionary)
				return new ChangedAttributes { Result = false };

			return BattleGDScriptBridge.ResultFromDictionary(variant.AsGodotDictionary());
		}
	}
}
