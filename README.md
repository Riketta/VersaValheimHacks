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
dotnet build -c Release                   # optimized build (logging still available)
```

- Default `ValheimDir` is `E:\SteamLibrary\steamapps\common\Valheim`.
- The loader expects `0Harmony.dll` and `Newtonsoft.Json.dll` to be present in
  `Mods\` (they ship there); the mod only references them at compile time.
- Logging is gated by the `Logging` config flag, not by the build
  configuration (see [Logging](#logging)).

## Configuration

Config lives next to `valheim.exe`: `VersaValheimHacks.json`. It is
auto-created with defaults on first run and re-saved on every load (so new
fields appear automatically). Edit it in any text editor, then press the
reload hotkey — no game restart needed. If the file ever becomes unreadable
(broken JSON, unknown key value), it is kept as `VersaValheimHacks.json.broken`
and regenerated with defaults on the next launch.

Durations are in **seconds** (`1800` = 30 min).

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
| `Num *` | Toggle debug mode (`Debug`) — unlocks debug tools + extra hacks |
| `Numpad1`| Apply saved food set: restore the loadout saved with `Numpad2` (natural values, replaces current food) |
| `Numpad2`| Save current food set to config (persistent across sessions); ignored with an empty stomach |
| `Numpad3`| Clear food: remove all eaten food buffs (max HP/stamina/eitr drop back to base) |
| `Numpad4`| Apply rested buff with the base rest duration (`BuffsOptions.RestDurationBase`, 0 = vanilla 300 s) |
| `Numpad5`| Area stack: trigger the vanilla chest stack on every chest within `AreaStackOptions.Radius` (default 15 m) |
| `Numpad6`| Toggle stack protection on the hovered item slot: marked items are skipped by all stack-to-chest operations and outlined in `AreaStackOptions.MarkedColor` |
| `Numpad7`| Auto-plant: mark the top-left corner (nearest planted crop) |
| `Numpad8`| Auto-plant: mark the bottom-left corner |
| `Numpad9`| Auto-plant: mark the bottom-right corner and plant the rectangle |
| `Num -` | Despawn all friendly skeletons you summoned — only your own minions are touched |
| `Num +` | Unlock all crafting recipes (debug mode required; informative, re-press is a no-op) |
| CapsLock | Friendly skeleton weapons: **ON** = sword + shield, **OFF** = bow |

The whole-map reveal is **unbound by default** (it stays available as a
feature): set `HotkeysOptions.RevealWholeMap` to a key in the config to use
it. Any key set to `None` means unbound.

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

### Stamina tuning (`StaminaOptions`, master toggle)
- **Regen delay**: `Player.RPC_UseStamina` postfix: the regen delay applied
  whenever stamina is spent (vanilla 1 s) is scaled by `RegenDelayMultiplier`
  (default 0.25 → 0.25 s; `0` = instant regen, `1` = vanilla).
- Covers every stamina action (attacks, blocking, running, dodging, jumping,
  building, swimming) — they all funnel through `UseStamina`. Note: no regen
  happens during *continuous* drains (e.g. sneaking), so this helps between
  actions, not mid-drain.
- **Sneak drain**: `Player.OnSneaking` prefix/postfix scales
  `m_sneakStaminaDrain` (vanilla 5/s at skill 0) by `SneakDrainMultiplier`
  (default 0.25 → 1.25/s; `0` = free sneaking, `1` = vanilla). Only the
  crouch-walk drain is scaled — attacks and all other stamina spends are
  untouched.

### No mist *(master toggle + `DisableMistlandsMist`)*
- `ParticleMist.Update` is skipped — removes Mistlands mist (and other
  particle mist volumes).

### Better eating (`BetterEatingOptions`)
- **Re-eat any food** *(master toggle + `Enabled`)*:
  `Player.Food.CanEatAgain` → true, so the same food can be eaten again
  immediately instead of waiting until it is half-burned.
- **Food duration** *(feature `Enabled` only)*: after every bite, the
  remaining time of *all* eaten food resets to `ExtendedFoodDuration`
  (default 30 min, ≈ vanilla burn times). Disable the extension with
  `BuffsOptions.OverrideFood: false` or `ExtendedFoodDuration: 0`: foods keep
  vanilla burn times and only food cycling touches them (auto-restart at
  natural end).
- **Food cycling** *(feature `Enabled` + `FoodCycling`)*: when the extended
  timer of a food runs out, it restarts on its **natural** vanilla burn time
  (full stats again) instead of disappearing — no stats are lost at the
  transition. After that natural cycle ends, the food is gone for good.
  `FoodCycling: false` disables the auto-restart entirely —
  food is removed when its timer ends (extended or natural), like vanilla;
  `Numpad1` still works manually.
- **Healing**: `SEMan.ModifyHealthRegen` multiplier × `HealingMultiplier`, applied on top of the vanilla multiplier — zero-regen states (Freezing, etc.) stay at zero
  (default 2) — scales food/tick health regen.
- **Food loadout** (`Numpad2` save / `Numpad1` apply): `Numpad2` records the
  currently eaten foods into `BetterEatingOptions.SavedFood` (persists across
  sessions; saving with an empty stomach keeps the existing set). `Numpad1`
  replaces your current food with that saved set, created exactly like
  vanilla eating — natural burn times and stats, no extensions, and cycling
  never touches applied food.
- `Numpad1` used to cycle extended foods manually — that still exists as a
  feature but is unbound now.

### Better guardian powers (`BetterPowersOptions`, master toggle + `Enabled`)
- **No cooldown**: `StartGuardianPower` / `ActivateGuardianPower` cooldown is
  zeroed for the duration of the call → powers activate with **no cooldown**.
  `BetterPowersOptions.NoCooldown: false` restores the vanilla cooldown between
  activations (extra powers still apply when you do activate).
- `StackAllBossPowers`: activating your power also applies every boss power marked
  `true` in `BuffExtraPowers` (`GP_Eikthyr`, `GP_TheElder`, ...), each with
  TTL `Duration` (default 10 min). `Duration: 0` or `BuffsOptions.OverridePower:
  false` keeps each power's vanilla duration. The game's own behavior of granting the power
  to players within 10 m still applies, so nearby players get the extras too.

### Rested buff (`BuffsOptions`, always on)
- **Apply rested (`Numpad4`)**: adds the vanilla rested status effect on
  demand with the base rest duration — `BuffsOptions.RestDurationBase`
  (0 = vanilla base 300 s), no comfort scaling. Uses the game's own
  status-effect system, purely client-side. Handy after death or when leaving
  base; note this skips the actual resting, so it's a convenience-cheat.
- `SE_Rested.Setup`: rested duration base → `RestDurationBase` (default 5 min,
  vanilla) and per-comfort-level → `RestDurationPerComfort` (default
  1 min/comfort, vanilla). A value of `0` keeps the vanilla duration for that
  part, and `OverrideRest: false` disables both overrides.
- **Auto-reset**: `RestAutoRefresh` (default `true`) — re-resting re-arms the
  buff to its full duration. `false` makes the timer run out from when it was
  first applied; re-resting no longer extends it.

### Skill gain (`SkillsOptions`, master toggle)
- `Skills.Skill.Raise`: skill XP factor × `GainMultiplierBelow50` while the
  skill is level ≤ 50, then × `GainMultiplierAbove50` above 50.

### No death penalties
- `Skills.LowerAllSkills` prefix: the death skill drain is scaled by
  `SkillsOptions.DeathDrainMultiplier` (default 0.25 → keep a quarter of the
  vanilla drain, i.e. ~6% of each skill instead of 25%; `0` = no drain at
  all, `1` = vanilla). Master-toggle gated; the server's death-penalty world
  modifier still applies on top of the vanilla factor. A death notification
  states the effective drain.
- `Player.OnDeath`: eaten food is backed up before death and re-added after →
  you keep your food buffs, **and** beneficial status effects survive —
  rested, guardian powers, wisplight demister, cozy and attribute buffs like
  frost resistance — with their remaining timers intact. Debuffs you died
  with (poison, wet, smoke...) still clear, as in vanilla.
- Each half is separately toggleable in `DeathOptions`:
  `RestoreFoodOnDeath` (default `true`; `false` = vanilla — food is lost on
  death) and `RestoreBuffsOnDeath` (default `true`; `false` = vanilla —
  rested/powers/etc. are lost too). The "food preserved" death notification
  only appears while `RestoreFoodOnDeath` is enabled.

### Skip start cinematic (`SkipIntroCinematic`, always on)
- **Launch cinematic**: `CinematicsManager.Play(Intro)` is blocked, so the
  intro video at game start never plays and the main menu appears immediately.
- **World-entry intro** (first spawn per character per world): the intro
  video, the Valkyrie flight and the intro text are cancelled the moment they
  activate (`Game.SkipIntro`) and you spawn directly on the ground.
- Purely client-side. Set `"SkipIntroCinematic": false` to keep both.

### Shield durability bar (`HudOptions.ShieldDurabilityBar`, default on)
- A clone of the vanilla health bar appears next to the HP bar while a shield
  status effect is active, showing remaining/total absorb damage (e.g.
  `540/700`) in real time. Fixed-length bar; the fill normalizes remaining/max.
- Stays visible in streamer mode; during it the shield takes at least 50%
  damage (see below).

### Health regen countdown (`HudOptions.HealthRegenCountdown`, default on)
- A small label right under the HUD health value counts down the seconds to
  the next food healing tick (vanilla heals every 10 s — the sum of the
  eaten foods' regen scaled by status effects). Hidden while no eaten food
  provides regen; `HealthRegenCountdown: false` removes the label.

### Shield tuning (`GodModeOptions.ShieldDamageMultiplier`, always on)
- `SE_Shield.OnDamaged`: incoming damage against *your* shield is reduced by
  the multiplier (0.5 = shield takes 50% damage, i.e. lasts 2× longer; the
  configured 0.002 ≈ 500× shield). Shows a HUD notification with remaining
  shield value per hit.

### Parry window (`GodModeOptions.ParryWindowMultiplier`, master toggle)
- `Humanoid.BlockAttack`: the perfect-block (parry) timing window is widened
  or narrowed by the multiplier. Vanilla window is a hardcoded 0.25 s — a
  block raised less than that before the hit lands counts as perfect.
  `2` = 0.5 s window, `0.5` = 0.125 s (harder), `1` = vanilla (feature off).
- Implementation: the block timer is scaled down while the original check
  runs and restored right after — the parry *rewards* (bonus damage,
  stagger, stamina, adrenaline) all stay exactly vanilla.
- Purely client-side: the parry result is computed locally, same as normal
  blocking.

### Area pickup (`PickableOptions.AreaPickupRadius`, always on when radius > 0)
- `Pickable.Interact` postfix: picking anything also picks every *identical*
  pickable within the radius (berry bushes, mushrooms, stone/branch piles).
- Recursion-guarded; radius 0 disables the feature.

### Area chest stacking (`Numpad5`, `AreaStackOptions`)
- The AoE version of the vanilla chest stack (hover a chest + use = move every
  item that already has a stack in that chest into it): pressing `Numpad5`
  walks every container within `AreaStackOptions.Radius` (default 15 m) —
  chests and carts — and triggers the game's own `Container.StackAll()` on
  each, nearest first.

### Stack protection (`Numpad6`, `AreaStackOptions`)
- Hover any item slot in the inventory (or an open chest panel) and press
  `Numpad6`: the item is marked untransferable and gets a colored outline
  (`AreaStackOptions.MarkedColor`, default red). Marked items are skipped by
  every stack-to-chest operation — the vanilla chest button and `Numpad5`
  area stack alike. Press `Numpad6` again to unmark.
- The mark lives in the item's vanilla custom data, so it survives saves,
  relogs and moving the item around, and it syncs in multiplayer.
  `ProtectMarkedItems: false` disables the feature entirely.
- The item-dump debug bind that used to live on `Numpad6` is unbound
  (see debug tools).

### Despawn summoned skeletons (`Num -`, always on)
- Press `Num -` to despawn every friendly skeleton **you** summoned —
  instantly, without corpses or death effects.
- Safety is layered: only characters with the `Skeleton_Friendly` prefab
  (player summons — wild skeletons use different prefabs) whose MonsterAI
  follow target resolves to a player whose ID matches yours are destroyed.
  Wild creatures, other players' skeletons and everything else never match.
- Uses the game's own `ZNetView.Destroy`, so it behaves identically on
  multiplayer servers. Shows how many minions were removed.

### Auto-planting (`Numpad7` / `Numpad8` / `Numpad9`, always on)
- Plant three same-type crops as field corners, then mark them in order:
  stand near each and press `Numpad7` (**top-left**), `Numpad8`
  (**bottom-left**), `Numpad9` (**bottom-right**) — planting starts on the
  third mark and fills the rectangle automatically.
- Spacing is the same per-crop distance the snap-point chain planting uses
  (grow-safe); corner offsets are snapped to whole spacing steps, so sloppy
  marking just rounds the field size. All three marks must be the same crop
  type; plants are placed nearest-to-you first, one seed per plant.
- Stops and notifies when the matching seeds run out; already-planted spots
  are skipped, so pressing `Numpad9` again only fills gaps. The seed type is
  matched from the crop; if that fails it falls back to the first seeds in
  your bag.
- The merge logic and its safety rails are 100% vanilla, so this is fully
  server-safe: the container ownership request is RPC-validated, in-use
  chests and other players' private chests are refused by the game itself,
  and equipped items are never moved.
- The game shows its own "stacked N items" message per affected chest; the
  mod adds one summary notification. `AreaStackOptions.Enabled: false`
  disables the hotkey.

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

### Skeleton minions (`GodModeOptions.SkeletonSummonLimit`, always on)
- `Tameable.UnsummonMaxInstances`: the summon cap for staff-summoned friendly
  skeletons is overridden to `SkeletonSummonLimit` (default 9).
- **Weapon forcing** (`Humanoid.GiveDefaultItems`): skeletons that follow you
  get a fixed loadout instead of a random one — CapsLock **ON**: skeleton
  sword + bronze buckler, CapsLock **OFF**: skeleton bow.

### Recipe discovery (`RecipeOptions`, master toggle)
- `Player.HaveRequirementItems` postfix: a recipe is revealed as soon as **any
  single ingredient** has been picked up once, instead of all of them
  (`RevealBySingleIngredient`, default on). Vanilla `requireOnlyOneIngredient`
  recipes behaved this way already; now everything does.
- Only discovery changes: crafting still consumes the full vanilla cost, and
  the crafting-station/DLC gates still apply.
- **Crafting panel sort** (`SortCraftingPanel`, default on): the craftable
  items list is sorted by progression region, then crafting tier (required
  station level), then alphabetically — see the region-colored labels below
  for how the region is determined. `RecipeOptions.SortOrder` switches the
  order: `"Region, Tier, Alphabet"` (default) or `"Tier, Region, Alphabet"`
  for the station-level grouping first. Overrides the vanilla sort modes —
  those are console-only (`sortcraft <mode>`) and always group by craftable
  state and category weights. `false` restores vanilla ordering.
- **Region-colored labels** (`ColorByRegion`, default on): each row's item
  name is colored with its region's color — Meadows, Black Forest, Swamp,
  Mountain, Plains, Mistlands, Ashlands, Deep North. The game carries no
  biome tag on items, so the region is inferred from the recipe's
  ingredients (highest known ingredient's home region — e.g. an iron weapon
  reads as Swamp). Recipes with no mapped ingredient stay gray and sort last
  inside their tier block. Non-craftable rows keep the color dimmed.
  `false` keeps the vanilla white/gray labels.
  The palette is selected via `ColorPalette` and looked up (case-
  insensitively) in `ColorPalettes` — a dictionary of named palettes, each
  a list of hex values (`"#RRGGBB"`, leading `#` optional; index 0 =
  Meadows ... index 7 = Deep North; a shorter list clamps to its last
  color). Built-in keys: `"RegionBright"` (default — biome-matched brights:
  bright green, forest green, orchid, ice blue, gold, fog blue, ember
  orange, glacier cyan), `"Region"` (muted earth tones: green, dark green,
  plum, pale blue, golden yellow, night blue, red-orange, glacier blue)
  and `"Rarity"` (rarity-style: gray, white, green, blue, purple, orange,
  red, cyan). Add your own keys to `ColorPalettes` and
  select them via `ColorPalette` for custom themes; unknown or broken
  palettes fall back to `"Region"`, then to the built-in fallback colors.
  Edits apply on config reload.
- **Colored item tooltips** (`ColorItemTooltips`, default on): the name line
  of item tooltips in the inventory and chest panels uses the same region
  color as the crafting panel — raw items by their own region, crafted items
  by their recipe's hardest ingredient (so an iron sword reads as Swamp).
  Items with no mapped region keep the vanilla color.

### Unlock all recipes (`Num +`, debug mode)
- On demand: pressing `Num +` adds every enabled `ObjectDB` recipe to known
  recipes via the game's own `AddKnownRecipe` — you get the vanilla
  "new recipe" toasts once, then silence (re-presses are silent no-ops).
  If a crafting panel is open it refreshes in place. Requires debug mode
  (`Num *`); knowledge is client-side only and is never sent to the server.
- Automatic session-start unlock *(opt-in: debug mode +
  `RecipeOptions.UnlockAllDebug`)*: `Player.UpdateKnownRecipesList` prefix
  unlocks everything once at world entry. Previously implied by debug mode;
  now must also set `"UnlockAllDebug": true` in the config.

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
- `Numpad6` — *(unbound by default; set `HotkeysOptions.DumpItemDatabase`)* dump the
  whole item database to `VersaValheimHacks.ItemDump.txt` in the game root:
  every ObjectDB item, every recipe with its ingredients and station level,
  plus the sorted list of unique ingredients (used to keep the crafting
  panel's region map complete after game updates).

## Notifications

`NotificationManager` prints HUD messages (center / top-left). The custom
broadcast — `NotificationOptions.CustomMessageToNearbyPlayers` to every player
within `CustomMessageToNearbyPlayersRadius` (default 20 m) — is **unbound by
default**; bind it via `HotkeysOptions.SendCustomNotificationToNearbyPlayers`
if you want it back.

## Logging

With `Logging: true` every patch action, hotkey action and error is
appended, timestamped, to `VersaValheimHacks.log` in the game root, next to
`valheim.exe` and the config file.

Boot messages (config discovery, patch results) are always logged - even
before the config exists or when it is unreadable - so a broken setup can
always be diagnosed from the log alone.

Harmony's built-in `FileLog` is deliberately not used: it buffers every line
until a log listener (BepInEx) attaches, which never happens with this
loader, so anything written through it is silently lost.

## Robustness

- Patches apply **independently**: one broken patch (e.g. after a game update
  renames a target) is skipped and logged while the rest of the mod and all
  hotkeys keep working.
- Hotkey handlers run inside a try/catch — a failing handler is reported and
  skipped, never taking down the game loop.
- A corrupt config self-heals (see Configuration).

## Caveats

- This is a **client-side cheat mod**. On multiplayer servers the server's
  authority still applies, the game can flag crafted items as cheated, and
  power/notifications affect other nearby players.
- A few QoL patches are intentionally always-on (death penalties, rested
  buff, shield tuning, area pickup, plant building, skeletons). Delete the
  corresponding file and rebuild if you don't want one.
