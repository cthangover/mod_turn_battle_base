using System;
using System.Collections.Generic;
using System.Linq;
using Cthangover.Core.Games;
using Cthangover.Core.Utils;

namespace Cthangover.Core.Battle
{
    /// <summary>
    /// Pluggable battle-core registry. The kernel hands every mod's
    /// IBattleCore implementations to <see cref="BattleCoreConsumer"/>, which
    /// registers them by ID (each core self-identifies) and removes them
    /// again when their mod unloads; one core is activated by ID. GetActive constructs a
    /// fresh instance every call — battle cores are stateless between
    /// battles. Falls back to the first registered core if no active
    /// ID was set, enabling mods to ship drop-in battle engines.
    /// </summary>
    public class BattleCoreRegistry
    {
        /// <summary>Game collection setting naming the battle core of that game.</summary>
        public const string GameSettingKey = "battle.core";

        /// <summary>Singleton registry instance.</summary>
        public static readonly BattleCoreRegistry Instance = new();

        private readonly Dictionary<string, Type> _cores = new();
        private string _activeCoreId;

        /// <summary>
        /// Registers a core by its self-reported <see cref="IBattleCore.Id"/>; a
        /// later core with the same id replaces the earlier one. Fed by
        /// <see cref="BattleCoreConsumer"/> for every mod assembly.
        /// </summary>
        public void Register(IBattleCore core)
        {
            if (core == null)
                return;
            _cores[core.Id] = core.GetType();
            GameLogger.Log("BATTLE", $"BattleCoreRegistry: registered core '{core.Id}'");
        }

        /// <summary>
        /// Removes a core whose mod is being unloaded (only when the id still
        /// maps to that core's type); an active selection of it is dropped.
        /// </summary>
        public void Unregister(IBattleCore core)
        {
            if (core == null || !_cores.TryGetValue(core.Id, out var type) || type != core.GetType())
                return;
            _cores.Remove(core.Id);
            if (_activeCoreId == core.Id)
                _activeCoreId = null;
        }

        /// <summary>
        /// Selects the active core by ID. Throws if the core is not
        /// registered. The selected core will be instantiated fresh on
        /// the next call to <see cref="GetActive"/>.
        /// </summary>
        public void SetActive(string id)
        {
            if (!_cores.ContainsKey(id))
                throw new Exception($"Battle core '{id}' not found");
            _activeCoreId = id;
        }

        /// <summary>
        /// Constructs a fresh instance of the active core type. Without an
        /// explicit <c>battle.set_core</c> the active game's
        /// <c>settings["battle.core"]</c> decides, then the first registered
        /// core. Throws if no cores are registered at all.
        /// </summary>
        public IBattleCore GetActive()
        {
            if (_activeCoreId != null && _cores.ContainsKey(_activeCoreId))
                return (IBattleCore)Activator.CreateInstance(_cores[_activeCoreId]);

            var gameCore = ActiveGame.GetSetting(GameSettingKey);
            if (gameCore != null && _cores.ContainsKey(gameCore))
            {
                _activeCoreId = gameCore;
                return (IBattleCore)Activator.CreateInstance(_cores[gameCore]);
            }

            var firstId = GetFirstCoreId();
            if (firstId != null)
            {
                _activeCoreId = firstId;
                return (IBattleCore)Activator.CreateInstance(_cores[firstId]);
            }

            throw new Exception("No active battle core set. Use battle.set_core first.");
        }

        /// <summary>
        /// Returns the ID of the first registered core, or null if the
        /// registry is empty. Used as a fallback when no active core
        /// has been explicitly set.
        /// </summary>
        public string GetFirstCoreId()
        {
            return _cores.Keys.FirstOrDefault();
        }

        /// <summary>
        /// Checks whether a core with the given <paramref name="id"/>
        /// has been registered.
        /// </summary>
        public bool HasCore(string id)
        {
            return _cores.ContainsKey(id);
        }

        /// <summary>
        /// Registered core IDs, sorted for stable output. Used by the battle
        /// coverage tool as the single source of truth for how many battle
        /// cores ship with the current mod set.
        /// </summary>
        public IReadOnlyList<string> CoreIds
        {
            get
            {
                var ids = new List<string>(_cores.Keys);
                ids.Sort(System.StringComparer.Ordinal);
                return ids;
            }
        }

        /// <summary>
        /// Returns the registered <see cref="Type"/> for a core, or
        /// <c>null</c>. Lets tooling inspect a core's implemented interfaces
        /// (e.g. <see cref="IBattleAutoPlayable"/>) without instantiating it.
        /// </summary>
        public Type GetCoreType(string id)
        {
            return _cores.TryGetValue(id, out var type) ? type : null;
        }
    }
}
