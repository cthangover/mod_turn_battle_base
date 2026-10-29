# Turn Battle Base — la plataforma común de todos los combates

Turn Battle Base es lo que comparten todos los combates por turnos de la colección: iniciar un combate desde un escenario, la escena de batalla con el fondo del lugar donde empezó, las pantallas de victoria y derrota, la experiencia y el botín, los efectos de estado como el aturdimiento y la elección del **sistema de combate**. Cómo lucha el jugador no lo decide la plataforma: ese es el trabajo de un mod de sistema de combate, **FF Battle** (menú de comandos) o **Card Battle** (cartas que se arrastran sobre los objetivos).

## Dos sistemas de combate, una plataforma

La misma pelea de RPG Plus — el guerrero contra un lobo en la aldea — en los dos sistemas:

![Izquierda: FF Battle — el lobo arriba, la miniatura del héroe abajo a la izquierda y el registro de combate a la derecha. Derecha: Card Battle — cartas de personaje grandes, un panel de acciones y el botón End turn.](assets/two-cores.jpg)

Todo lo que rodea al combate es igual: el fondo de la aldea, los números del lobo, los botones de velocidad **x1 / x4 / x8** arriba a la derecha (aceleran las animaciones, y la elección se recuerda) y lo que ocurre después.

Cada juego elige su sistema con el ajuste de colección `battle.core`; RPG Plus usa `ff_battle`. Sin ese ajuste se toma el primer sistema instalado. En el editor de colecciones el ajuste se llama «Sistema de combate».

## Cómo se desarrolla un combate

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

## Victoria y derrota

Cuando cae el último enemigo, la pantalla de victoria muestra la experiencia de cada miembro del grupo que siga en pie y el botín:

![La pantalla de victoria: «Victory», un botón Continue, el héroe con +24 y el botín — pieles de lobo x2, monedas x8 y un ámbar.](assets/victory.jpg)

- **Experiencia.** Cada enemigo derrotado aporta su `Level × Exp`, que se reparte a partes iguales entre quienes sigan en pie. El lobo es de nivel 2 con `Exp` 12: **+24**.
- **Botín.** Para cada enemigo derrotado se tira su lista de `Loot`; los objetos van directos al inventario.
- **Continue** devuelve a la escena de la que salió el combate. La vida perdida en el combate sigue perdida.

Si cae todo el grupo, la pantalla de derrota solo ofrece **Main menu**: ni experiencia ni botín.

![La pantalla de derrota: «Defeat» sobre un charco rojo oscuro y un botón Main menu.](assets/defeat.jpg)

## Iniciar un combate desde un escenario

```scenario
action battle.init scene=town_entry enemies=wolf_1,wolf_2 quest_id=MeetingMurakamiQuest new_tag=wolf_battle
switch_scene Battle
end
```

- `enemies` — ids de personajes separados por comas; el mismo id dos veces son dos enemigos.
- `scene` — la escena a la que volver tras la victoria.
- `quest_id`, `new_tag` (opcionales) — vinculan el combate a una misión y le ponen una etiqueta (una notificación en el diario).
- El fondo del combate es el que se muestre en ese momento, así que defínelo antes de `battle.init`.

Sobre un mapa de tiles usa en su lugar el `action rpg.battle enemies=rpg_wolf win_flag=rpg_wolf_dead` del mod RPG: el grupo vuelve a la misma casilla y, al ganar, se activa la bandera.

La acción `action battle.set_core core=card_battle` cambia el sistema de combate para los combates siguientes.

## Enemigos, botín y efectos de estado en los datos

Los enemigos son personajes normales (véase Characters Engine). El botín del lobo de RPG Plus:

```json
{
  "Id": "rpg_wolf",
  "Loot": [
    { "ItemId": "rpg_plus/wolf_pelt", "MinCount": 2, "MaxCount": 3, "Probability": 100 },
    { "ItemId": "money/coin", "MinCount": 8, "MaxCount": 14, "Probability": 100 }
  ]
}
```

`Probability` es un porcentaje; la cantidad es un número aleatorio entre `MinCount` y `MaxCount`, ambos incluidos.

Los **efectos de estado** son entidades de tipo `status_effect`. La plataforma trae de serie el aturdimiento: el personaje pierde sus turnos:

```json
{ "Id": "effect/physics/stun", "Icon": "characters/effects/stun", "Type": "Stun", "Duration": 3, "Actions": "basic/stun" }
```

Una acción de combate que lo aplique puede fijar su propio número de turnos; las duraciones se descuentan al comienzo de cada turno.

## Para autores de mods

Un sistema de combate es un mod que registra un `IBattleCore` (y `IBattleAutoPlayable` para las pruebas automáticas). Sus acciones pasan por los ejecutores de la plataforma, así que un mod como Battle Formulas añade nuevos tipos de acciones a todos los sistemas a la vez. El código puede reaccionar al final del combate con `BattleEndedEvent` y a la victoria con un `IWingroundListener`; Recruiting lo aprovecha para ofrecer un puesto en el grupo a los enemigos derrotados.

## Si algo sale mal

1. **El combate sale con el fondo vacío.** `battle.init` se ejecutó antes de definir el fondo de la escena.
2. **Se abre el sistema de combate equivocado.** Revisa el ajuste `battle.core` del juego y si algún escenario lo cambió con `battle.set_core`.
3. **No hay experiencia tras la victoria.** Los enemigos con `Exp` 0 no dan experiencia, y los miembros del grupo caídos tampoco reciben.
