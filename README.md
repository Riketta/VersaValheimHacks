# VersaValheimHacks

Personal client-side trainer / QoL mod for Valheim. A collection of Harmony
patches (no BepInEx) loaded by a custom native loader that injects assemblies
from the game's `Mods\` folder at startup.

## Install & build

The build deploys straight into the game's mod folder:

```sh
cd Mods/VersaValheimHacks
dotnet build                              # -> <ValheimDir>\Mods\VersaValheimHacks.dll
dotnet build -p:ValheimDir="D:\Games\Valheim"   # custom install location
dotnet build -c Release                   # strips all logging (see Logging)
```

- Default `ValheimDir` is `E:\SteamLibrary\steamapps\common\Valheim`.
- The loader expects `0Harmony.dll` and `Newtonsoft.Json.dll` to be present in
  `Mods\` (they ship there); the mod only references them at compile time.
- **Debug** builds (the default) contain logging code; **Release** builds
  compile it out entirely.

## Configuration

Config lives next to `valheim.exe`: `VersaValheimHacks.json`. It is
auto-created with defaults on first run and re-saved on every load (so new
fields appear automatically). Edit it in any text editor, then press the
reload hotkey — no game restart needed.

Durations are in **seconds** (`86400` = 24 h).

## Hotkeys

Key handling runs on a WinAPI polling thread (5 ms tick) and only fires while
the Valheim window has focus. Because Unity APIs are main-thread-only, each
key handler is queued and executed on the game's main thread (drained every
frame by postfixes on `Player.Update` / `FejdStartup.Update`) — this is what
makes the map reveal and in-game notifications safe. Defaults (configurable
under `HotkeysOptions`):

| Key      | Action                                                          |
|----------|-----------------------------------------------------------------|
| `Home`   | Reload config from disk                                         |
| `End`    | Toggle streamer mode (see below)                                 |
| `Numpad0`| Toggle master hack switch (`Enabled`) — see gating below        |
| `Numpad7`| Toggle debug mode (`Debug`) — unlocks debug tools + extra hacks |
| `Numpad1`| Cycle food: end extended durations, restart all foods on their natural burn time |
| `Numpad3`| Send a custom notification to all players within a radius       |
| `Numpad8`| Dump debug info to log (global keys, window handles) *(debug)*  |
| `Numpad9`| Dump all loaded game objects within 5 m of the player *(debug)* |
| CapsLock | Friendly skeleton weapons: **ON** = sword + shield, **OFF** = bow |

The whole-map reveal is **unbound by default** (it stays available as a
feature): set `HotkeysOptions.RevealWholeMap` to a key in the config to use
it. Any key set to `None` means unbound.

`Numpad4`/`Numpad5`/`Numpad6` (key-press test logging, unregister-all) are
only registered when debug mode was on at game start.

## Feature gating

Features fall into three groups:

1. **Master-toggle gated** — need `Enabled = true` *and* their own option.
2. **Debug gated** — need `Debug = true` (recipe unlock, map reveal, dumps).
3. **Always on** — QoL tweaks that run unconditionally (each listed below).

## Streamer mode (`End`, config `StreamerMode`)

Toggles the mod into a viewer-safe state **without disabling the hacks**:

- **All mod HUD messages are hidden** (toggles, power/death/shield/cycle
  statuses, placement angles) — the streamer-mode toggle itself is the only
  message that still shows.
- **Food** stays on natural vanilla timers: each food silently restarts on its
  natural burn time when it runs out ("default + refresh on end"), so the HUD
  always shows vanilla-looking depletion instead of a static 24 h bar.
- **Rested buff** uses vanilla durations (5 min + 1 min/comfort).
- **Guardian powers** use the vanilla cooldown and no extra power buff icons
  are added.
- **Map reveal radius** returns to vanilla (100).
- **Shield compensation** stays on but is clamped in streamer mode: the shield
  takes **at least 50%** of incoming damage (config below that is raised to
  0.5), and the durability bar stays visible.
- Everything else *mechanically* invisible keeps working: free crafting, carry
  weight, no mist, skill XP multiplier, reduced death skill drain, skeleton
  loadouts/limit.

The state is saved to the config and survives restarts.

## Features

### Free crafting *(master toggle + `FreeCraftingEnabled`)*
- `Player.NoCostCheat` → always true, and
  `ZoneSystem.GetGlobalKey(NoCraftCost)` → always true.
- Crafting, upgrading and repairing need no resources, no crafting station,
  no roof/fire; piece placement consumes nothing; the upgrade tab is
  force-enabled in the crafting panel (`InventoryGui.UpdateCraftingPanel`).
- Crafted and upgraded items **never carry the internal "cheated" tag**, and
  neither do placed pieces (walls, stations, chests) — `Inventory.AddItem` /
  `Player.PlacePiece` prefixes strip the flag before creation. Clean stations
  keep cooked/smelted/fermented output clean too. Already-placed pieces keep
  their stored flag (it's baked into the server's world data).

### Never encumbered *(master toggle + `NeverEncumbered`)*
- `Player.IsEncumbered` → false (no slow-walk, no stamina drain).
- `Player.GetMaxCarryWeight` × `CarryWeightMultiplier` (default 5).
- Also raises the auto-pickup weight threshold.

### Stamina regen delay (`StaminaOptions`, master toggle)
- `Player.RPC_UseStamina` postfix: the regen delay applied whenever stamina is
  spent (vanilla 1 s) is scaled by `RegenDelayMultiplier` (default 0.25 →
  0.25 s; `0` = instant regen, `1` = vanilla).
- Covers every stamina action (attacks, blocking, running, dodging, jumping,
  building, swimming) — they all funnel through `UseStamina`.

### No mist *(master toggle + `DisableMistlandsMist`)*
- `ParticleMist.Update` is skipped — removes Mistlands mist (and other
  particle mist volumes).

### Better eating (`BetterEatingOptions`)
- **Re-eat any food** *(master toggle + `Enabled`)*:
  `Player.Food.CanEatAgain` → true, so the same food can be eaten again
  immediately instead of waiting until it is half-burned.
- **Food duration** *(feature `Enabled` only)*: after every bite, the
  remaining time of *all* eaten food resets to `FoodBuffDuration`
  (default 24 h).
- **Food cycling** *(feature `Enabled` + `FoodCycling`)*: when the extended
  timer of a food runs out, it restarts on its **natural** vanilla burn time
  (full stats again) instead of disappearing — no stats are lost at the
  transition. After that natural cycle ends, the food is gone for good.
- **Healing**: `SEMan.ModifyHealthRegen` multiplier × `HealingMultiplier`
  (default 2.5) — scales food/tick health regen.
- `Numpad1` cycles all extended foods to their natural duration immediately
  (works regardless of `Enabled`).

### Better guardian powers (`BetterPowersOptions`, master toggle + `Enabled`)
- `Player.StartGuardianPower` / `ActivateGuardianPower` cooldown is zeroed
  for the duration of the call → powers activate with **no cooldown**.
- `ApplyAllBuffs`: activating your power also applies every boss power marked
  `true` in `BuffExtraPowers` (`GP_Eikthyr`, `GP_TheElder`, ...), each with
  TTL `Duration` (default 3 h). The game's own behavior of granting the power
  to players within 10 m still applies, so nearby players get the extras too.

### Rested buff (`BuffsOptions`, always on)
- `SE_Rested.Setup`: rested duration base → `RestDurationBase` (default 5 h)
  and per-comfort-level → `RestDurationPerComfort` (default 1 h).

### Skill gain (`SkillsOptions`, master toggle)
- `Skills.Skill.Raise`: skill XP factor × `PreFiftyMultiplier` while the
  skill is level ≤ 50, then × `PostFiftyMultiplier` above 50.

### No death penalties (always on)
- `Skills.LowerAllSkills` prefix: the death skill drain is scaled by
  `SkillsOptions.DeathDrainMultiplier` (default 0.25 → keep a quarter of the
  vanilla drain, i.e. ~6% of each skill instead of 25%; `0` = no drain at
  all, `1` = vanilla). Master-toggle gated; the server's death-penalty world
  modifier still applies on top of the vanilla factor. A death notification
  states the effective drain.
- `Player.OnDeath`: eaten food is backed up before death and re-added after →
  you keep your food buffs (always on, independent of the master toggle).

### Skip start cinematic (`SkipIntroCinematic`, always on)
- Auto-skips the first-spawn intro when entering a world (once per character
  per world): the intro video, the Valkyrie flight and the intro text are all
  cancelled the moment they activate, and you spawn directly on the ground.
- Uses the game's own skip path (`Game.SkipIntro`), purely client-side.
- Set `"SkipIntroCinematic": false` to keep the cinematic.

### Shield durability bar *(always on)*
- A clone of the vanilla health bar appears next to the HP bar while a shield
  status effect is active, showing remaining/total absorb damage (e.g.
  `540/700`) in real time. Fixed-length bar; the fill normalizes remaining/max.
- Stays visible in streamer mode; during it the shield takes at least 50%
  damage (see below).

### Shield tuning (`GodModeOptions.ShieldDamageMultiplier`, always on)
- `SE_Shield.OnDamaged`: incoming damage against *your* shield is reduced by
  the multiplier (0.5 = shield takes 50% damage, i.e. lasts 2× longer; the
  configured 0.002 ≈ 500× shield). Shows a HUD notification with remaining
  shield value per hit.

### Area pickup (`PickableOptions.AreaPickupRadius`, always on when radius > 0)
- `Pickable.Interact` postfix: picking anything also picks every *identical*
  pickable within the radius (berry bushes, mushrooms, stone/branch piles).
- Recursion-guarded; radius 0 disables the feature.

### Building: plants & rotation (always on)
- **Plant snap points** (`Piece.GetSnapPoints`): plant pieces get two extra
  snap points — one at the center ("Inner") and one a grow-radius away
  ("Outer") — computed from `Plant.m_growRadius` / `m_growRadiusVines`,
  collider bounds, ×1.1 × `PlantExtraRadiusMultiplier`. Lets you chain-place
  crops/trees at proper spacing.
- **No random rotation** (`PieceTable.GetSelectedPrefab`): pieces listed in
  `PiecesOptions.PlantPieces` (`"$piece_..." → prefab name`) place upright
  instead of with random initial rotation.
- **Placement angle HUD** (`Player.UpdatePlacement`): whenever you rotate a
  piece, a notification shows the exact angle (0–337.5° in 22.5° steps).

### Skeleton minions (`GodModeOptions.SummonsLimit`, always on)
- `Tameable.UnsummonMaxInstances`: the summon cap for staff-summoned friendly
  skeletons is overridden to `SummonsLimit` (default 9).
- **Weapon forcing** (`Humanoid.GiveDefaultItems`): skeletons that follow you
  get a fixed loadout instead of a random one — CapsLock **ON**: skeleton
  sword + bronze buckler, CapsLock **OFF**: skeleton bow.

### Unlock all recipes *(debug mode)*
- `Player.UpdateKnownRecipesList` prefix: every enabled recipe in `ObjectDB`
  is added to known recipes once per session.

### Map reveal radius *(always on)*
- `Minimap.Start`: exploration fog radius × `MapRevealRadiusMultiplier`
  (default ×3 → 300, vanilla 100). Fog recedes three times farther around you
  as you explore. Set the multiplier to `1` for vanilla.
- Reverts to vanilla while streamer mode is on (the wider fog is visible).

### Reveal whole map *(debug mode, unbound by default)*
- `Minimap.ExploreAll()` — bind it via `HotkeysOptions.RevealWholeMap` to use
  it.

## Debug tools *(debug mode)*

- `ZoneSystem` and `World` instances are captured on construction and dumped
  to the log (global keys, key values, starting keys).
- `Numpad8` — dump current global keys/values, window handles.
- `Numpad9` — dump every loaded GameObject within 5 m of the player with its
  components (discovery helper).
- `Numpad4`/`Numpad5` — echo key presses to the log; `Numpad6` — remove all
  registered hotkey handlers.

## Notifications

`NotificationManager` prints HUD messages (center / top-left). `Numpad3`
broadcasts `NotificationOptions.CustomMessageToNearbyPlayers` to every player
within `CustomMessageToNearbyPlayersRadius` (default 20 m) via the game's
`Player.GetPlayersInRange`.

## Logging

With `Logging: true` (and a Debug build) every patch action is appended to
`harmony.log.txt` in the game root, with timestamps and optional stack
traces. Release builds compile all of this out (`#if DEBUG`).

## Caveats

- This is a **client-side cheat mod**. On multiplayer servers the server's
  authority still applies, the game can flag crafted items as cheated, and
  power/notifications affect other nearby players.
- A few QoL patches are intentionally always-on (death penalties, rested
  buff, shield tuning, area pickup, plant building, skeletons). Delete the
  corresponding file and rebuild if you don't want one.
