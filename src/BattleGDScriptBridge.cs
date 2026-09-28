using Cthangover.Core.Characters;
using Cthangover.Core.Factories.Impls;
using Cthangover.Core.Items;
using Godot;

namespace Cthangover.Core.Battle
{
    /// <summary>
    /// Converts battle data to and from plain Godot collections so GDScript mods
    /// can work with it. The previous bridge exposed <c>[GlobalClass]</c> DTOs
    /// (GDCharacter, GDBattleResult, GDWingroundContext), but classes compiled at
    /// runtime inside mods never go through Godot's C# source generator, so their
    /// properties and methods are invisible to GDScript. Dictionaries and arrays
    /// are native variants and work regardless of which assembly produced them.
    /// <para/>
    /// Keys are snake_case to match GDScript conventions. The same helpers are
    /// used by <see cref="GDBattleActionExecutor"/> (action/character snapshots in,
    /// result deltas out) and <see cref="GDWingroundListener"/> (victory context
    /// in, exp/loot mutations out).
    /// </summary>
    public static class BattleGDScriptBridge
    {
        /// <summary>Flattens a character into a dictionary of primitive stats and identity fields.</summary>
        public static Godot.Collections.Dictionary CharacterToDictionary(Character character)
        {
            var data = new Godot.Collections.Dictionary();
            if (character == null)
                return data;

            data["id"] = character.ID;
            data["name"] = character.Name;
            data["level"] = character.Level;
            data["exp"] = character.Exp;
            data["recruitment_chance"] = character.RecruitmentChance;

            var attrs = character.Attributes;
            if (attrs != null)
            {
                data["health_value"] = attrs.Health?.Value ?? 0;
                data["health_max"] = attrs.Health?.BaseValue ?? 0;
                data["defence_value"] = attrs.Defence?.Value ?? 0;
                data["defence_max"] = attrs.Defence?.BaseValue ?? 0;
                data["attack_value"] = attrs.Attack?.Value ?? 0;
                data["attack_max"] = attrs.Attack?.BaseValue ?? 0;
                data["point_value"] = attrs.Point?.Value ?? 0;
                data["point_max"] = attrs.Point?.BaseValue ?? 0;

                var properties = new Godot.Collections.Dictionary<string, string>();
                if (attrs.Properties?.Values != null)
                {
                    foreach (var kvp in attrs.Properties.Values)
                        properties[kvp.Key] = kvp.Value;
                }
                data["properties"] = properties;
            }

            return data;
        }

        /// <summary>Flattens an action card into a dictionary of identity fields and raw properties.</summary>
        public static Godot.Collections.Dictionary ActionToDictionary(ActionCharacter action)
        {
            var data = new Godot.Collections.Dictionary();
            if (action == null)
                return data;

            data["id"] = action.ID;
            data["name"] = action.Name;
            data["description"] = action.Description;
            data["type"] = (int)action.Type;

            var properties = new Godot.Collections.Dictionary<string, string>();
            if (action.Properties?.Values != null)
            {
                foreach (var kvp in action.Properties.Values)
                    properties[kvp.Key] = kvp.Value;
            }
            data["properties"] = properties;

            return data;
        }

        /// <summary>
        /// Builds <see cref="ChangedAttributes"/> from the dictionary a GDScript
        /// executor returns. Missing keys default to zero/false; <c>result</c>
        /// defaults to <c>true</c> so a script that only reports deltas still
        /// counts as a successful action.
        /// </summary>
        public static ChangedAttributes ResultFromDictionary(Godot.Collections.Dictionary data)
        {
            if (data == null)
                return new ChangedAttributes { Result = false };

            var result = new ChangedAttributes();
            if (data.TryGetValue("source_damage", out var sourceDamage))
                result.Source.Damage = sourceDamage.AsInt32();
            if (data.TryGetValue("source_defence", out var sourceDefence))
                result.Source.Defence = sourceDefence.AsInt32();
            if (data.TryGetValue("target_damage", out var targetDamage))
                result.Target.Damage = targetDamage.AsInt32();
            if (data.TryGetValue("target_defence", out var targetDefence))
                result.Target.Defence = targetDefence.AsInt32();

            result.Result = !data.TryGetValue("result", out var success) || success.AsBool();
            return result;
        }

        /// <summary>Builds the mutable victory context dictionary passed to GDScript winground hooks.</summary>
        public static Godot.Collections.Dictionary WingroundToDictionary(WingroundContext ctx)
        {
            var data = new Godot.Collections.Dictionary();
            if (ctx == null)
                return data;

            var expGained = new Godot.Collections.Dictionary<string, int>();
            if (ctx.PlayerExpGained != null)
            {
                foreach (var kvp in ctx.PlayerExpGained)
                    expGained[kvp.Key] = kvp.Value;
            }
            data["exp_gained"] = expGained;

            var defeated = new Godot.Collections.Array<Godot.Collections.Dictionary>();
            if (ctx.DefeatedEnemies != null)
            {
                foreach (var enemy in ctx.DefeatedEnemies)
                    defeated.Add(CharacterToDictionary(enemy));
            }
            data["defeated_enemies"] = defeated;

            var loot = new Godot.Collections.Array<Godot.Collections.Dictionary>();
            if (ctx.GeneratedLoot != null)
            {
                foreach (var item in ctx.GeneratedLoot)
                {
                    loot.Add(new Godot.Collections.Dictionary
                    {
                        ["item_id"] = item.Item?.ID ?? string.Empty,
                        ["count"] = item.Count,
                    });
                }
            }
            data["generated_loot"] = loot;

            data["return_scene"] = ctx.BattleData?.ReturnScene ?? string.Empty;
            return data;
        }

        /// <summary>
        /// Writes GDScript mutations from a winground context dictionary back to
        /// the C# pipeline. Only <c>exp_gained</c> and <c>generated_loot</c> are
        /// synced — defeated enemies are read-only snapshots, matching the
        /// documented contract of <see cref="IWingroundListener"/>.
        /// </summary>
        public static void SyncWingroundBack(Godot.Collections.Dictionary data, WingroundContext ctx)
        {
            if (data == null || ctx == null)
                return;

            if (data.TryGetValue("exp_gained", out var expVariant)
                && expVariant.AsGodotDictionary() is Godot.Collections.Dictionary expDict
                && ctx.PlayerExpGained != null)
            {
                ctx.PlayerExpGained.Clear();
                foreach (var key in expDict.Keys)
                    ctx.PlayerExpGained[key.AsString()] = expDict[key].AsInt32();
            }

            if (data.TryGetValue("generated_loot", out var lootVariant)
                && lootVariant.AsGodotArray() is Godot.Collections.Array lootArray
                && ctx.GeneratedLoot != null)
            {
                ctx.GeneratedLoot.Clear();
                foreach (var entry in lootArray)
                {
                    if (entry.AsGodotDictionary() is not Godot.Collections.Dictionary itemDict)
                        continue;

                    var itemId = itemDict.TryGetValue("item_id", out var idVariant) ? idVariant.AsString() : null;
                    var count = itemDict.TryGetValue("count", out var countVariant) ? countVariant.AsInt32() : 0;
                    ctx.GeneratedLoot.Add(new ItemContainer
                    {
                        Item = string.IsNullOrEmpty(itemId) ? null : ItemFactory.Instance.Get(itemId),
                        Count = count,
                    });
                }
            }
        }
    }
}
