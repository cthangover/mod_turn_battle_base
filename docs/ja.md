# Turn Battle Base —— すべての戦闘を支えるプラットフォーム

Turn Battle Base は、コレクション内のすべてのターン制戦闘に共通する土台です。シナリオからの戦闘開始、戦闘が始まった場所を背景にした戦闘シーン、勝利・敗北の画面、経験値と戦利品、スタンなどのステータス効果、そして**バトルシステム**の選択を担います。プレイヤーがどう戦うかは、ここでは決まりません。それはバトルシステムのModの仕事です。**FF Battle**（コマンドメニュー）と **Card Battle**（カードをターゲットへドラッグ）がそれに当たります。

## 2つのバトルシステム、1つのプラットフォーム

RPG Plus での同じ戦闘——村で戦士が狼と対峙する場面——を、2つのシステムで並べます：

![左：FF Battle——上部に狼、左下に英雄の小さなカード、右側に戦闘ログ。右：Card Battle——大きなキャラクターカード、アクションパネル、End turn ボタン。](assets/two-cores.jpg)

戦闘を取り巻く部分はどちらも同じです。村の背景、狼の数値、右上の速度ボタン **x1 / x4 / x8**（アニメーションを速めるもので、選択は記憶されます）、そして戦闘後の流れです。

ゲームはコレクション設定 `battle.core` でシステムを選びます。RPG Plus は `ff_battle` を使用します。設定がなければ、最初にインストールされているシステムが使われます。コレクションエディターでは、この設定は「戦闘システム」という名前です。

## 戦闘の流れ

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

## 勝利と敗北

最後の敵が倒れると、勝利画面には生存しているパーティメンバーそれぞれの経験値と戦利品が表示されます：

![勝利画面：「Victory」の表示、Continue ボタン、英雄の +24、そして戦利品——狼の毛皮 x2、コイン x8、琥珀 1 個。](assets/victory.jpg)

- **経験値。** 倒した敵はそれぞれ `Level × Exp` を出し、それが生存しているパーティメンバーに均等に分配されます。狼はレベル 2・`Exp` 12 なので **+24** です。
- **戦利品。** 倒した敵ごとに `Loot` リストが抽選されます。アイテムはそのままインベントリに入ります。
- **Continue** で戦闘が始まったシーンに戻ります。戦闘中に失った体力は戻りません。

パーティが全滅すると、敗北画面は **Main menu** だけを提示します。経験値も戦利品もありません。

![敗北画面：暗い赤の水たまりの上に「Defeat」の表示と Main menu ボタン。](assets/defeat.jpg)

## シナリオから戦闘を開始する

```scenario
action battle.init scene=town_entry enemies=wolf_1,wolf_2 quest_id=MeetingMurakamiQuest new_tag=wolf_battle
switch_scene Battle
end
```

- `enemies`——キャラクター id をカンマ区切りで指定します。同じ id を 2 回書けば敵が 2 体になります。
- `scene`——勝利後に戻るシーンです。
- `quest_id`、`new_tag`（省略可）——戦闘をクエストに結び付け、タグ（ジャーナル通知）を付けます。
- 背景にはその時点で表示されている背景が使われます。`battle.init` の前に設定しておいてください。

タイルマップでは、代わりに RPG Mod の `action rpg.battle enemies=rpg_wolf win_flag=rpg_wolf_dead` を使ってください。同じマップセルに戻り、勝利時にフラグが立ちます。

`action battle.set_core core=card_battle` は、以降の戦闘で使うバトルシステムを切り替えます。

## データとしての敵・戦利品・ステータス効果

敵はごく普通のキャラクターです（Characters Engine を参照）。RPG Plus の狼の戦利品：

```json
{
  "Id": "rpg_wolf",
  "Loot": [
    { "ItemId": "rpg_plus/wolf_pelt", "MinCount": 2, "MaxCount": 3, "Probability": 100 },
    { "ItemId": "money/coin", "MinCount": 8, "MaxCount": 14, "Probability": 100 }
  ]
}
```

`Probability` はパーセント値です。個数は `MinCount` から `MaxCount` まで（両端を含む）の乱数です。

**ステータス効果**は `status_effect` 種別のエンティティです。プラットフォームにはスタンが用意されていて、かかるとキャラクターは自分のターンを飛ばします：

```json
{ "Id": "effect/physics/stun", "Icon": "characters/effects/stun", "Type": "Stun", "Duration": 3, "Actions": "basic/stun" }
```

この効果をかける戦闘アクションは、独自のターン数を指定できます。持続ターン数は毎ターンの開始時に減っていきます。

## Mod作者向け

バトルシステムとは、`IBattleCore` を登録するModです（自動テストのため `IBattleAutoPlayable` も実装します）。そのアクションはプラットフォームのエグゼキューターを通って実行されるため、Battle Formulas のようなModは新しい種類のアクションをすべてのシステムに一度に追加できます。コードは `BattleEndedEvent` で戦闘終了を、`IWingroundListener` で勝利を受け取れます。Recruiting はこれを利用して、倒した敵をパーティに勧誘します。

## うまくいかないときは

1. **戦闘の背景が空白になる。** シーン背景の設定より先に `battle.init` が実行されています。
2. **別のバトルシステムが開く。** ゲームの `battle.core` 設定と、シナリオが `battle.set_core` で切り替えていないかを確認してください。
3. **勝利後に経験値が入らない。** `Exp` が 0 の敵は経験値を出しません。倒れたパーティメンバーも得られません。
