# Turn Battle Base — 모든 전투를 받치는 플랫폼

Turn Battle Base는 컬렉션의 모든 턴제 전투가 공유하는 부분입니다. 시나리오에서 전투를 여는 일, 전투가 시작된 장소를 배경으로 삼는 전투 장면, 승리·패배 화면, 경험치와 전리품, 기절 같은 상태 이상, 그리고 **전투 시스템** 선택이 모두 여기서 이뤄집니다. 다만 플레이어가 어떻게 싸울지는 정하지 않습니다. 그것은 전투 시스템 모드의 몫입니다. 명령 메뉴 방식의 **FF Battle**이나 카드를 대상에 끌어다 놓는 **Card Battle**이 그 예입니다.

## 두 가지 전투 시스템, 하나의 플랫폼

RPG Plus의 같은 전투 — 마을에서 전사가 늑대와 맞서는 장면 — 을 두 시스템으로 보여주면 다음과 같습니다:

![왼쪽: FF Battle — 늑대가 위에, 영웅의 작은 카드가 왼쪽 아래에, 전투 로그가 오른쪽에 있습니다. 오른쪽: Card Battle — 큰 캐릭터 카드, 행동 패널, End turn 버튼.](assets/two-cores.jpg)

전투를 둘러싼 모든 것은 같습니다. 마을 배경, 늑대의 수치, 오른쪽 위의 속도 버튼 **x1 / x4 / x8**(애니메이션을 빠르게 하며 선택은 기억됩니다), 그리고 전투가 끝난 뒤의 흐름까지요.

게임은 컬렉션 설정 `battle.core`로 시스템을 고릅니다. RPG Plus는 `ff_battle`을 사용합니다. 설정이 없으면 설치된 첫 번째 시스템을 씁니다. 컬렉션 편집기에서는 이 설정이 "전투 시스템"이라는 이름으로 표시됩니다.

## 전투의 진행

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

## 승리와 패배

마지막 적이 쓰러지면 승리 화면에 살아남은 파티원 각자의 경험치와 전리품이 표시됩니다:

![승리 화면: "Victory" 표시, Continue 버튼, 영웅의 +24, 그리고 전리품 — 늑대 가죽 x2, 코인 x8, 호박 1개.](assets/victory.jpg)

- **경험치.** 쓰러뜨린 적은 각자 자신의 `Level × Exp`를 주며, 살아 있는 파티원들이 똑같이 나눕니다. 늑대는 2레벨에 `Exp` 12이므로 **+24**입니다.
- **전리품.** 쓰러뜨린 적마다 자신의 `Loot` 목록이 굴려집니다. 아이템은 곧바로 인벤토리에 들어갑니다.
- **Continue**는 전투가 시작됐던 장면으로 돌아갑니다. 전투에서 잃은 체력은 그대로 남습니다.

파티 전원이 쓰러지면 패배 화면은 **Main menu**만 제안합니다. 경험치도 전리품도 없습니다.

![패배 화면: 짙은 붉은 웅덩이 위의 "Defeat" 표시와 Main menu 버튼.](assets/defeat.jpg)

## 시나리오에서 전투 시작하기

```scenario
action battle.init scene=town_entry enemies=wolf_1,wolf_2 quest_id=MeetingMurakamiQuest new_tag=wolf_battle
switch_scene Battle
end
```

- `enemies` — 캐릭터 id를 쉼표로 나열합니다. 같은 id를 두 번 쓰면 적이 둘 나옵니다.
- `scene` — 승리 후 돌아갈 장면입니다.
- `quest_id`, `new_tag`(선택) — 전투를 퀘스트에 묶고 태그(저널 알림)를 붙입니다.
- 전투 배경은 그 시점에 표시된 배경입니다. `battle.init` 전에 배경을 정해 두세요.

타일 맵에서는 대신 RPG 모드의 `action rpg.battle enemies=rpg_wolf win_flag=rpg_wolf_dead`를 사용하세요. 같은 맵 칸으로 돌아오고, 승리 시 플래그가 설정됩니다.

`action battle.set_core core=card_battle` 액션은 이후 전투의 전투 시스템을 바꿉니다.

## 데이터 속의 적·전리품·상태 이상

적은 평범한 캐릭터입니다(Characters Engine 참조). RPG Plus 늑대의 전리품:

```json
{
  "Id": "rpg_wolf",
  "Loot": [
    { "ItemId": "rpg_plus/wolf_pelt", "MinCount": 2, "MaxCount": 3, "Probability": 100 },
    { "ItemId": "money/coin", "MinCount": 8, "MaxCount": 14, "Probability": 100 }
  ]
}
```

`Probability`는 백분율입니다. 개수는 `MinCount`부터 `MaxCount`까지(양 끝 포함)의 무작위 수입니다.

**상태 이상**은 `status_effect` 종류의 엔티티입니다. 플랫폼에는 기절이 기본으로 들어 있으며, 걸린 캐릭터는 자기 턴을 건너뜁니다:

```json
{ "Id": "effect/physics/stun", "Icon": "characters/effects/stun", "Type": "Stun", "Duration": 3, "Actions": "basic/stun" }
```

이 효과를 거는 전투 액션이 자체 턴 수를 정할 수도 있습니다. 지속 시간은 매 턴 시작 때 줄어듭니다.

## 모드 작성자를 위해

전투 시스템은 `IBattleCore`를 등록하는 모드입니다(자동 테스트를 위한 `IBattleAutoPlayable`도). 이 모드의 액션은 플랫폼의 실행기를 거치므로, Battle Formulas 같은 모드가 새 종류의 액션을 모든 시스템에 한꺼번에 추가할 수 있습니다. 코드는 `BattleEndedEvent`로 전투 종료를, `IWingroundListener`로 승리를 받아볼 수 있습니다. Recruiting이 이를 이용해 쓰러뜨린 적에게 파티 자리를 제안합니다.

## 문제가 생겼을 때

1. **전투 배경이 비어 있음.** 장면 배경 설정보다 `battle.init`이 먼저 실행됐습니다.
2. **다른 전투 시스템이 열림.** 게임의 `battle.core` 설정과, 시나리오가 `battle.set_core`로 바꾸지 않았는지 확인하세요.
3. **승리 후 경험치가 없음.** `Exp`가 0인 적은 경험치를 주지 않고, 쓰러진 파티원도 받지 못합니다.
