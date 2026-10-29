# Turn Battle Base — nền tảng của mọi trận chiến

Turn Battle Base là phần chung của mọi trận chiến theo lượt trong bộ sưu tập: mở trận từ kịch bản, cảnh chiến đấu lấy nền theo nơi trận nổ ra, màn hình thắng và thua, kinh nghiệm và chiến lợi phẩm, các hiệu ứng trạng thái như choáng, và cả việc chọn **hệ thống chiến đấu**. Còn người chơi đánh thế nào, nó không quyết định — việc đó thuộc về mod hệ thống chiến đấu: **FF Battle** (menu lệnh) hoặc **Card Battle** (kéo thẻ vào mục tiêu).

## Hai hệ thống chiến đấu, một nền tảng

Cùng một trận đánh trong RPG Plus — chiến binh đấu với con sói trong làng — dưới hai hệ thống:

![Bên trái: FF Battle — sói ở trên, thẻ nhỏ của anh hùng ở dưới bên trái, nhật ký chiến đấu bên phải. Bên phải: Card Battle — thẻ nhân vật cỡ lớn, bảng hành động và nút End turn.](assets/two-cores.jpg)

Mọi thứ xung quanh trận đánh đều giống nhau: nền làng, các con số của con sói, các nút tốc độ **x1 / x4 / x8** ở góc trên bên phải (chúng tăng tốc hiệu ứng, lựa chọn được ghi nhớ) và những gì diễn ra sau trận đánh.

Game chọn hệ thống bằng thiết lập `battle.core` của bộ sưu tập; RPG Plus dùng `ff_battle`. Không có thiết lập thì hệ thống được cài đầu tiên sẽ được dùng. Trong trình soạn thảo bộ sưu tập, thiết lập này có tên "Hệ thống chiến đấu".

## Một trận chiến diễn ra thế nào

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

## Thắng và thua

Khi kẻ địch cuối cùng gục xuống, màn hình thắng sẽ hiển thị kinh nghiệm của từng thành viên đội còn sống và phần chiến lợi phẩm:

![Màn hình thắng: chữ "Victory", nút Continue, anh hùng được +24, và chiến lợi phẩm — da sói x2, xu x8 và một viên hổ phách.](assets/victory.jpg)

- **Kinh nghiệm.** Mỗi kẻ địch bị hạ mang lại `Level × Exp` của nó, chia đều cho những thành viên đội còn trụ. Con sói cấp 2 với `Exp` 12 — **+24**.
- **Chiến lợi phẩm.** Với mỗi kẻ địch bị hạ, trò chơi quay số theo danh sách `Loot` của nó; vật phẩm vào thẳng túi đồ.
- **Continue** đưa bạn về cảnh mà trận chiến bắt đầu. Máu đã mất trong trận chiến vẫn mất.

Cả đội gục ngã thì màn hình thua chỉ còn **Main menu**: không kinh nghiệm, không chiến lợi phẩm.

![Màn hình thua: chữ "Defeat" trên vũng máu đỏ sẫm và nút Main menu.](assets/defeat.jpg)

## Mở trận chiến từ kịch bản

```scenario
action battle.init scene=town_entry enemies=wolf_1,wolf_2 quest_id=MeetingMurakamiQuest new_tag=wolf_battle
switch_scene Battle
end
```

- `enemies` — id nhân vật, phân tách bằng dấu phẩy; cùng một id xuất hiện hai lần nghĩa là hai kẻ địch.
- `scene` — cảnh sẽ quay về sau khi thắng.
- `quest_id`, `new_tag` (tùy chọn) — gắn trận chiến với nhiệm vụ và gán cho nó một thẻ (thông báo trên nhật ký).
- Nền trận chiến là nền đang hiển thị tại thời điểm đó, nên hãy đặt nền trước khi gọi `battle.init`.

Trên bản đồ tile, hãy dùng `action rpg.battle enemies=rpg_wolf win_flag=rpg_wolf_dead` của mod RPG thay thế: đội sẽ về đúng ô cũ và cờ được đặt khi thắng.

Hành động `action battle.set_core core=card_battle` đổi hệ thống chiến đấu cho các trận chiến tiếp theo.

## Kẻ địch, chiến lợi phẩm và hiệu ứng trạng thái trong dữ liệu

Kẻ địch là nhân vật bình thường (xem Characters Engine). Chiến lợi phẩm của con sói trong RPG Plus:

```json
{
  "Id": "rpg_wolf",
  "Loot": [
    { "ItemId": "rpg_plus/wolf_pelt", "MinCount": 2, "MaxCount": 3, "Probability": 100 },
    { "ItemId": "money/coin", "MinCount": 8, "MaxCount": 14, "Probability": 100 }
  ]
}
```

`Probability` là phần trăm; số lượng là một số ngẫu nhiên từ `MinCount` đến `MaxCount` (bao gồm cả hai đầu).

**Hiệu ứng trạng thái** là thực thể loại `status_effect`. Nền tảng có sẵn choáng — nhân vật bị bỏ qua các lượt của mình:

```json
{ "Id": "effect/physics/stun", "Icon": "characters/effects/stun", "Type": "Stun", "Duration": 3, "Actions": "basic/stun" }
```

Hành động chiến đấu gây hiệu ứng này có thể tự đặt số lượt; thời gian giảm dần vào đầu mỗi lượt.

## Dành cho tác giả mod

Hệ thống chiến đấu là một mod đăng ký `IBattleCore` (cùng `IBattleAutoPlayable` cho bộ kiểm thử tự động). Các hành động của nó chạy qua bộ thực thi của nền tảng, nên một mod như Battle Formulas có thể thêm loại hành động mới cho tất cả các hệ thống cùng lúc. Mã nguồn có thể phản ứng khi trận chiến kết thúc qua `BattleEndedEvent` và khi thắng qua `IWingroundListener`; Recruiting dùng đúng cơ chế này để mời kẻ địch bị hạ gia nhập đội.

## Khi có sự cố

1. **Nền trận chiến trống trơn.** `battle.init` chạy trước khi nền của cảnh được đặt.
2. **Mở nhầm hệ thống chiến đấu.** Kiểm tra thiết lập `battle.core` của game và xem có kịch bản nào đã đổi nó bằng `battle.set_core` không.
3. **Thắng rồi mà không có kinh nghiệm.** Kẻ địch có `Exp` 0 không cho kinh nghiệm; thành viên đội đã gục cũng không nhận.
