# AnimationManager

Centralized animation controller for Unity.  
Manages Animator state transitions across a target `Animator`, supports JSON-driven animation definitions for modding, and exposes events for cross-module integration via bridge components.


## Features

- **Play / CrossFade / Stop** — unified API for immediate play, smooth crossfade, and explicit stop
- **Duration tracking** — fires `OnAnimationCompleted` when a non-looping animation reaches its natural end
- **JSON / Modding** — define animation entries in `StreamingAssets/animations/`; merged by `id` on top of Inspector data
- **Events** — `OnAnimationStarted`, `OnAnimationStopped`, `OnAnimationCompleted` for reactive integration
- **CutsceneManager integration** — execute animations from cutscene custom events (`anim.play:id`, `anim.fade:id`, `anim.stop`) (activated via `ANIMATIONMANAGER_CSM`)
- **StateManager integration** — auto-play state-linked animations on `AppState` change (activated via `ANIMATIONMANAGER_STM`)
- **MapLoaderFramework integration** — stop current animation on chapter change (activated via `ANIMATIONMANAGER_MLF`)
- **CharacterManager integration** — swap `RuntimeAnimatorController` on active character change (activated via `ANIMATIONMANAGER_CM`)
- **SaveManager integration** — persist and restore active animation id across saves (activated via `ANIMATIONMANAGER_SM`)
- **EventManager integration** — broadcast `animation.started/stopped/completed` events (activated via `ANIMATIONMANAGER_EM` or `EVENTMANAGER_ANM`)
- **Custom Inspector** — live playback controls, current animation display, and registered animation list in Play Mode
- **DOTween Pro integration** — `DOTween.To` and `DOVirtualFloat` drive `CrossFade` weight and animator property tweens for smooth animation blending (activated via `ANIMATIONMANAGER_DOTWEEN`)
- **RealToon Pro integration** — `MaterialPropertyBlock` sets `_SmearIntensity` on the target renderer during high-velocity animations; reset on `Stop()` / `OnAnimationCompleted` (activated via `ANIMATIONMANAGER_REALTOON`)
- **Odin Inspector integration** — `SerializedMonoBehaviour` base for full Inspector serialization of complex types; runtime-display fields marked `[ReadOnly]` in Play Mode (activated via `ODIN_INSPECTOR`)


## Installation

### Option A — Unity Package Manager (Git URL)

1. Open **Window → Package Manager**
2. Click **+** → **Add package from git URL…**
3. Enter:

   ```
   https://github.com/RolandKaechele/AnimationManager.git
   ```

### Option B — Clone into Assets

```bash
git clone https://github.com/RolandKaechele/AnimationManager.git Assets/AnimationManager
```

### Option C — npm / postinstall

```bash
cd Assets/AnimationManager
npm install
```


## Scene Setup

1. Create a persistent manager GameObject (or reuse your existing manager object).
2. Attach `AnimationManager`.
3. Assign `Target Animator` (the `Animator` component to control).
4. Add animation definitions in the Inspector or via JSON files in `StreamingAssets/animations/`.
5. Add any bridge components (see Bridge Components below).


## Quick Start

### Inspector Fields

| Field | Default | Description |
| ----- | ------- | ----------- |
| `targetAnimator` | *(none)* | Animator component to control |
| `animations` | *(empty)* | Built-in animation definitions |
| `loadFromJson` | `false` | Merge definitions from `animations/` |
| `jsonPath` | `"animations/"` | Folder relative to `StreamingAssets/` containing `.json` files to merge. Falls back to single-file mode if the value points to an existing file. |
| `defaultCrossFadeDuration` | `0.25` | Seconds used by `CrossFade()` when no duration is passed |
| `verboseLogging` | `false` | Log all transitions to Console |

### AnimationDefinition fields

| Field | Description |
| ----- | ----------- |
| `id` | Unique id, e.g. `"idle"`, `"walk"`, `"attack"` |
| `displayName` | Human-readable label |
| `stateName` | Animator state name as set in the Animator Controller |
| `controllerPath` | Optional Resources path to a `RuntimeAnimatorController` to swap in |
| `category` | Tag, e.g. `"combat"`, `"cinematic"` |
| `loop` | If `true`, `OnAnimationCompleted` is never fired |
| `duration` | Approximate clip length in seconds (used for completion timer) |

### Code usage

```csharp
var anim = FindFirstObjectByType<AnimationManager.Runtime.AnimationManager>();

anim.Play("idle");
anim.CrossFade("walk");          // uses default crossfade duration
anim.CrossFade("attack", 0.1f);  // custom duration
anim.Stop();

// Subscribe to events
anim.OnAnimationStarted   += id => Debug.Log($"Started: {id}");
anim.OnAnimationCompleted += id => Debug.Log($"Done: {id}");
```


## Bridge Components

Attach these to the same GameObject as `AnimationManager` (or anywhere in the scene).

| Component | Define | Effect |
| --------- | ------ | ------ |
| `CutsceneManagerBridge` | `ANIMATIONMANAGER_CSM` | Responds to `"anim.play:id"`, `"anim.fade:id"`, `"anim.stop"` cutscene custom events |
| `StateManagerBridge` | `ANIMATIONMANAGER_STM` | Plays mapped animation on `AppState` change |
| `MapLoaderBridge` | `ANIMATIONMANAGER_MLF` | Stops current animation on chapter change |
| `CharacterManagerBridge` | `ANIMATIONMANAGER_CM` | Swaps `RuntimeAnimatorController` on character change |
| `SaveManagerBridge` | `ANIMATIONMANAGER_SM` | Persists/restores active animation id |
| `EventManagerBridge` | `ANIMATIONMANAGER_EM` | Fires `animation.started/stopped/completed` via EventManager |

EventManager can also re-broadcast AnimationManager events using `AnimationEventBridge` (define: `EVENTMANAGER_ANM`).


## JSON / Modding

Place one or more `.json` files in `StreamingAssets/animations/` (path is configurable).
All `*.json` files in the folder are loaded and merged by `id` at startup.

**Example:** `StreamingAssets/animations/main.json`

```json
{
  "animations": [
    {
      "id": "victory",
      "displayName": "Victory Pose",
      "stateName": "VictoryPose",
      "category": "cinematic",
      "loop": false,
      "duration": 3.5
    }
  ]
}
```

JSON entries are **merged by id** — mods can add new entries or override Inspector definitions without reimporting.


## Optional Integrations

| Define | Integration |
| ------ | ----------- |
| `ANIMATIONMANAGER_CSM` | AnimationManager ←→ CutsceneManager |
| `ANIMATIONMANAGER_STM` | AnimationManager ←→ StateManager |
| `ANIMATIONMANAGER_MLF` | AnimationManager ←→ MapLoaderFramework |
| `ANIMATIONMANAGER_CM` | AnimationManager ←→ CharacterManager |
| `ANIMATIONMANAGER_SM` | AnimationManager ←→ SaveManager |
| `ANIMATIONMANAGER_EM` | AnimationManager → EventManager (fire events) |
| `EVENTMANAGER_ANM` | EventManager ← AnimationManager (re-broadcast) |
| `ODIN_INSPECTOR` | AnimationManager ↔→ Odin Inspector (`SerializedMonoBehaviour` + `[ReadOnly]`) |


## Editor Tools

Open via **JSON Editors → Animation Manager** in the Unity menu bar, or via the **Open JSON Editor** button in the AnimationManager Inspector.

| Action | Result |
| ------ | ------ |
| **Load** | Reads all `*.json` from `StreamingAssets/animations/`; creates the folder if missing |
| **Edit** | Add / remove / reorder entries using the Inspector list |
| **Save** | Writes each entry as `<id>.json` to `StreamingAssets/animations/`; entries without an `id` are skipped. Calls `AssetDatabase.Refresh()` |

With **ODIN_INSPECTOR** active, the list uses Odin's enhanced drawer (drag-to-sort, collapsible entries).
