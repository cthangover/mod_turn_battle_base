using System.Collections.Generic;
using System.Linq;
using Cthangover.Core.Battle;
using Cthangover.Core.Factories.Impls;
using Cthangover.Core.Characters;
using Godot;

namespace Cthangover.Core.Settings
{
    /// <summary>
    /// Immutable descriptor for a battle encounter, constructed via the
    /// static factory <see cref="InitBattle"/> overloads. Holds the
    /// background texture, enemy roster, quest reference, card order side,
    /// and the scene to return to when the battle ends. The private
    /// constructor forces creation through the factories so that all
    /// required data is supplied upfront.
    /// </summary>
    public class BattleData
    {
        /// <summary>Id of the quest that triggered this battle, as the scenario
        /// gave it; <c>null</c> for non-quest skirmishes. The battle only carries
        /// the id: the quest mod's integration (<c>turn_battle_base__quests</c>)
        /// resolves it, so this mod does not depend on quests.</summary>
        public string QuestId { get; set; }
        /// <summary>Tag the integration adds to <see cref="QuestId"/> after a won
        /// battle (and notifies about when the battle starts).</summary>
        public string NewTag  { get; set; }

        /// <summary>List of enemy <see cref="Character"/> instances spawned from
        /// the provided IDs or default templates.</summary>
		public List<Character> EnemiesCards { get; private set; }
        /// <summary>Battle background texture (full-resolution).</summary>
		public Texture2D Background { get; private set; }
        /// <summary>Depth map texture for the parallax post-process effect.</summary>
		public Texture2D DepthMap { get; set; }
        /// <summary>Albedo/colour map texture for the parallax post-process effect.</summary>
		public Texture2D AlbedoMap { get; set; }
        /// <summary>Which side of the screen the player's cards are drawn on.</summary>
		public BattleSide BattleSide { get; private set; } = BattleSide.Player;
        /// <summary>Identifier for the active battle core logic module.</summary>
        public string ActiveBattleCore { get; set; }
        /// <summary>Scene name to transition to when the battle finishes or is
        /// fled. Stored as a string to avoid type coupling.</summary>
        public string ReturnScene { get; private set; }

        private BattleData()
        { }

        /// <summary>
        /// Convenience overload that defaults <see cref="BattleSide"/> to
        /// <c>BattleSide.Player</c>. Constructs enemies from their string IDs
        /// via <see cref="Cthangover.Core.Factories.Impls.CharacterFactory"/>,
        /// falling back to a level-1 template when the ID is unknown.
        /// </summary>
        public static BattleData InitBattle(Texture2D background, string returnScene, params string[] enemies)
        {
            return InitBattle(background, returnScene, BattleSide.Player, enemies);
        }
        
        /// <summary>
        /// Creates a battle with a specified <see cref="BattleSide"/> and
        /// enemy IDs. Each ID is resolved through
        /// <see cref="Cthangover.Core.Factories.Impls.CharacterFactory.Get"/>.
        /// Unknown IDs produce a fallback <see cref="Character"/> with 20 HP
        /// and 3 action points.
        /// </summary>
        public static BattleData InitBattle(Texture2D background, string returnScene, BattleSide cardOrder = BattleSide.Player, params string[] enemies)
        {
            var list = enemies.Select(o => CharacterFactory.Instance.Get(o) ?? new Character
            {
                ID = o,
                Attributes = new CharacterAttributes
                {
                    Health = new Attribute { Value = 20, BaseValue = 20 },
                    Point = new Attribute { Value = 3, BaseValue = 3 },
                },
                Level = 1,
                Exp = 10,
            }).ToList();
            return InitBattle(background, returnScene, list, cardOrder);
        }
        
        /// <summary>
        /// Core factory that directly accepts a pre-built
        /// <see cref="List{Character}"/> for the enemy roster. This is the
        /// overload that all other <c>InitBattle</c> variants delegate to.
        /// </summary>
        public static BattleData InitBattle(Texture2D background, string returnScene, List<Character> enemiesCards, BattleSide cardOrder = BattleSide.Player)
        {
            return new BattleData
            {
                Background   = background,
                ReturnScene  = returnScene,
                EnemiesCards = enemiesCards,
                BattleSide    = cardOrder,
            };
        }
    }
}
