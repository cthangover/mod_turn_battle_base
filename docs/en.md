# Turn Battle Base — the platform under every battle

Turn Battle Base is what all turn-based battles in the collection share: starting a battle from a scenario, the battle scene with the backdrop of the place where it began, the victory and defeat screens, experience and loot, status effects such as stun, and the choice of a **battle system**. It does not decide how the player fights. That is the job of a battle system mod: **FF Battle** (a command menu) or **Card Battle** (cards dragged onto targets).

## Two battle systems, one platform

The same fight in RPG Plus — the warrior against a wolf in the village — in the two systems:

![Left: FF Battle — the wolf at the top, the hero's small card at the bottom left, a battle log on the right. Right: Card Battle — big character cards, an actions panel and an End turn button.](assets/two-cores.jpg)

Everything around the fight is the same: the village backdrop, the wolf's numbers, the speed buttons **x1 / x4 / x8** at the top right (they speed up the animations; the choice is remembered), and what happens after it.

A game chooses its system with the collection setting `battle.core`; RPG Plus uses `ff_battle`. Without the setting the first installed system is used. In the collection editor the setting is called *Battle system*.

## How a battle goes

```text
scenario: battle.init / rpg.battle
        |
        v
battle scene: backdrop of the current place, party from the roster, enemies from their files
        |
        v
battle system (ff_battle or card_battle): player turn <-> enemy turn
        |                                          |
   all enemies defeated                      party defeated
        v                                          v
  Victory: experience, loot             Defeat: Main menu
        |
        v
  back to the scene the battle started from
```

## Victory and defeat

When the last enemy falls, the victory screen shows each surviving party member's experience and the loot:

![The victory screen: "Victory", a Continue button, the Hero with +24, and the loot — wolf pelts x2, coins x8 and an amber.](assets/victory.jpg)

- **Experience.** Each defeated enemy gives its `Level × Exp`, shared equally among the party members still standing. The wolf is level 2 with `Exp` 12 — **+24**.
- **Loot.** Each defeated enemy rolls its `Loot` list; the items go straight into the inventory.
- **Continue** returns to the scene the battle came from. Health lost in battle stays lost.

If the whole party falls, the defeat screen offers only **Main menu**: no experience, no loot.

![The defeat screen: "Defeat" over a dark red puddle and a Main menu button.](assets/defeat.jpg)

## Starting a battle from a scenario

```scenario
action battle.init scene=town_entry enemies=wolf_1,wolf_2 quest_id=MeetingMurakamiQuest new_tag=wolf_battle
switch_scene Battle
end
```

- `enemies` — character ids, comma-separated; the same id twice gives two enemies.
- `scene` — the scene to return to after a victory.
- `quest_id`, `new_tag` (optional) — bind the battle to a quest and give it a tag (a journal notification).
- The backdrop is the background shown at that moment, so set it before `battle.init`.

On a tile map, use `action rpg.battle enemies=rpg_wolf win_flag=rpg_wolf_dead` from the RPG mod instead: it returns to the same map cell and sets the flag on victory.

`action battle.set_core core=card_battle` switches the battle system for the following battles.

## Enemies, loot and status effects in data

Enemies are ordinary characters (see Characters Engine). The loot of the RPG Plus wolf:

```json
{
  "Id": "rpg_wolf",
  "Loot": [
    { "ItemId": "rpg_plus/wolf_pelt", "MinCount": 2, "MaxCount": 3, "Probability": 100 },
    { "ItemId": "money/coin", "MinCount": 8, "MaxCount": 14, "Probability": 100 }
  ]
}
```

`Probability` is a percentage; the count is a random number from `MinCount` to `MaxCount` inclusive.

**Status effects** are entities of kind `status_effect`. The platform brings stun — the character skips its turns:

```json
{ "Id": "effect/physics/stun", "Icon": "characters/effects/stun", "Type": "Stun", "Duration": 3, "Actions": "basic/stun" }
```

A battle action that applies it may set its own number of turns; durations count down at the start of each turn.

## For mod authors

A battle system is a mod that registers an `IBattleCore` (with `IBattleAutoPlayable` for the automatic tests). Its actions run through the platform's executors, so a mod such as Battle Formulas adds new kinds of actions to every system at once. Code can react to the end of a battle through `BattleEndedEvent` and to the victory through an `IWingroundListener`; Recruiting uses this to offer defeated enemies a place in the party.

## If something goes wrong

1. **The battle has an empty backdrop.** `battle.init` ran before the scene's background was set.
2. **The wrong battle system opens.** Check the game's `battle.core` setting, and whether a scenario switched it with `battle.set_core`.
3. **No experience after a victory.** Enemies with `Exp` 0 give none; party members who fell get none.
