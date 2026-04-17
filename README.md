<p align="center">
  <img alt="Unity" src="https://img.shields.io/badge/Unity-6.0%20(6000.2.10f1)-black?logo=unity" />
  <img alt="URP" src="https://img.shields.io/badge/URP-17.2.0-6aa84f" />
  <img alt="Photon" src="https://img.shields.io/badge/Photon-PUN2-00a3e0" />
  <img alt="Photon Voice" src="https://img.shields.io/badge/Photon-Voice%20PUN-00a3e0" />
  <img alt="Cinemachine" src="https://img.shields.io/badge/Cinemachine-3.1.5-7b68ee" />
  <img alt="Input System" src="https://img.shields.io/badge/Input%20System-1.14.2-333333" />
  <img alt="VFX Graph" src="https://img.shields.io/badge/VFX%20Graph-17.2.0-ff4d4d" />
</p>

---

## EN — KGAMagicalNetTeam (High‑Performance README)

### What this is
A Unity 6 multiplayer prototype built around **Photon PUN2** + **URP**, with a gameplay core that leans on:
- **State machines for player control**
- **ScriptableObject-driven actions/items**
- **Mediator-style interaction orchestration** (Timeline + Cinemachine handoff)
- **Pooling of transient runtime objects** (projectiles/SFX) to avoid `Instantiate/Destroy` spikes

### Tech stack (source of truth)
- **Unity**: 6000.2.10f1 (`ProjectSettings/ProjectVersion.txt`)
- **URP**: 17.2.0 (`Packages/manifest.json`)
- **Photon PUN2 / Photon Voice (PUN)**: code uses `Photon.Pun`, `Photon.Voice.PUN` (`Assets/Scripts/**`)

### Quick start
- Open the project in **Unity 6.0 (6000.2.10f1)**.
- Load a lobby/room scene under `Assets/Scenes/Hyeonyong/` (e.g. `Room.unity`).
- Optional runtime perf HUD: add `SimpleFPS` to a bootstrap object to display **ms + FPS** (`Assets/Scripts/YHG/SimpleFPS.cs`).

### Key Optimization Performance
> Table below uses **repo-backed** “before vs after” deltas from current implementation patterns (pool hits, redundant network writes). For absolute timings (ms, GC alloc), add Unity Profiler captures to `docs/perf/` and update this table.

| Measurement Metric | Before | After | Result (%) |
|---|---:|---:|---:|
| Transient projectile spawns (`FryingPanLogic`) — `Instantiate` per shot after warm-up | 1 / shot | 0 / shot (pool hit) | 100% fewer Instantiates |
| SFX playback (`SoundManager` + `SoundPool`) — `Instantiate` per SFX after warm-up | 1 / play | 0 / play (pool hit) | 100% fewer Instantiates |
| Photon CustomProperty writes — redundant “same value” sets (`NetworkProperties.SetProps`) | 1 / call | 0 / call when unchanged | 100% fewer redundant writes |

Evidence anchors:
- Pooling projectiles: `Assets/Scripts/Sensei/FryingPanLogic.cs`
- Pooling audio sources: `Assets/Scripts/SHS/System/SoundManager.cs`, `Assets/Scripts/SHS/DesignPattern/ObjectPooling/SoundPool.cs`
- No-op property write suppression: `Assets/Scripts/LSB/Utill/NetworkProperties.cs`

### Architecture (modular system diagram)
This codebase already behaves like a modular mediator/event hub in two places:
- **Interaction flow**: `PlayableCharacter` → network-safe request → `InteractionManager` → per-`InteractionType` system → Timeline bindings + Cinemachine camera mode via `ProjectManager`.
- **Action flow**: `PlayerMagicSystem` selects `ActionBase` from `ActionItemDataSO` / `MagicDataSO` and executes across Photon boundaries (RPC / network instantiate).

```mermaid
flowchart LR
  subgraph Net["Photon PUN2 Boundary"]
    RPC["RPC calls / Room + Player CustomProperties"]
    Inst["PhotonNetwork.Instantiate(...)"]
  end

  subgraph Player["Player Runtime"]
    PC["PlayableCharacter\n(StateMachine + Inventory + MagicSystem)"]
    SM["StateMachine\n(IState: Execute/FixedExecute)"]
    MS["PlayerMagicSystem\n(aim + cooldown + ExecuteAction)"]
  end

  subgraph Interaction["Interaction Orchestration (Mediator)"]
    IM["InteractionManager\n(RequestInteraction)"]
    BIS["BaseInteractSystem\n(per InteractionType)"]
    TL["PlayableDirector + Timeline bindings"]
  end

  subgraph Global["Global Services"]
    PM["ProjectManager (Singleton)\nCinemachineController"]
    AUD["SoundManager\n+ SoundPool"]
    FPS["SimpleFPS HUD\n(ms + fps)"]
  end

  PC --> SM
  PC --> MS
  PC -- "OnTimelinePlay -> RPC_TimelinePlay" --> RPC
  RPC --> IM
  IM --> BIS
  IM --> TL
  TL --> PM

  MS --> Inst
  MS --> RPC
  AUD --> PM
  FPS --> Player
