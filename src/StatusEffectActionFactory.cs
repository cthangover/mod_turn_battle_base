using System;
using System.Collections.Generic;
using Cthangover.Core.Cards.StatusEffect;
using Cthangover.Core.Utils;

namespace Cthangover.Core.Factories.Impls
{
    /// <summary>
    /// Reflection-based plugin registry for status effect behaviour
    /// classes, mirroring the pattern used by <c>ItemActionFactory</c>.
    /// Each <c>IStatusActions</c> implementation corresponds to a status
    /// effect ID referenced in battle data JSON. The kernel hands every mod's
    /// implementations to <see cref="StatusActionsConsumer"/>, so mod authors
    /// add new status effects (e.g. poison, burn, freeze) by implementing the
    /// interface in their assembly; they leave again when their mod unloads.
    /// </summary>
    public class StatusEffectActionFactory
    {
        private readonly Dictionary<string, IStatusActions> actions;
        
#region Singleton

        private static readonly Lazy<StatusEffectActionFactory> instance = new(() => new StatusEffectActionFactory());
        /// <summary>Lazy singleton, filled by <see cref="StatusActionsConsumer"/>.</summary>
        public static StatusEffectActionFactory Instance => instance.Value;

        public StatusEffectActionFactory()
        {
            actions = new Dictionary<string, IStatusActions>();
        }

#endregion

        /// <summary>Registers a behaviour set under its <c>ID</c>; the first registration of an id wins.</summary>
        public void Register(IStatusActions action)
        {
            if (action != null && !string.IsNullOrEmpty(action.ID) && !actions.ContainsKey(action.ID))
                actions[action.ID] = action;
        }

        /// <summary>Removes exactly <paramref name="action"/> (its mod is being unloaded).</summary>
        public void Unregister(IStatusActions action)
        {
            if (action != null && !string.IsNullOrEmpty(action.ID) && actions.TryGetValue(action.ID, out var current) && ReferenceEquals(current, action))
                actions.Remove(action.ID);
        }

        /// <summary>Resolves the behaviour set registered under the effect's <c>Actions</c> id; unknown ids are logged and return <c>null</c>.</summary>
        public IStatusActions Get(string id)
        {
            if (actions.TryGetValue(id, out var result))
                return result;
            GameLogger.Log("STATUS", "actions '" + id + "' not found!", LogLevel.Error);
            return null;
        }
        
    }
}
