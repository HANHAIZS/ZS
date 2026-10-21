---
id: game-audio
title: Background music and harvest sound effects
status: Verified
owner: Project
created: 2026-10-19
updated: 2026-10-19
---

# Background music and harvest sound effects

## Goal
Integrate the four supplied audio files at their requested gameplay moments, preserving existing UI and gameplay.

## User-visible behavior
Home/Shop/Setting loop 主界面待机音V2; levels 1–5 loop 关卡背景音乐V3. Menu/level navigation changes the single music track; tab changes, dialogs and retries do not restart it. Existing Music and Sound preferences independently control their channels and remain saved through existing Commands. Backgrounding pauses music at its position and stops transient effects; returning resumes only when Music remains enabled. No old effects replay. A successful hive group landing plays 蜂巢进入槽位音效 once after activation commits. Cancelled/rejected hops stay silent. The existing committed pickup event starts 采集搬运V2 when the bee actually acquires cargo. Modal/simulation pause, Home, win and stale sessions cannot start effects.

## Out of scope
No new audio, editing/normalizing supplied files, sliders, crossfades, save keys, SDKs, UI or gameplay changes. No Foundation changes or global persistent singleton. No automatic change to user preferences.

## Ownership and architecture
GameAudioController is project Unity presentation under ViewControllers/Audio, added once by each scene-owned MainMenuNavigationController during initialization. It caches Models/clips/sources and subscriptions; no architecture registration is needed. A new HiveEnteredSlotEvent is emitted by ActivateWaitingHiveCommand only after a successful atomic commit, once for a connected group. Existing HarvestChangedEvent.Picked drives carrying sound. All project state still mutates through existing Commands; audio only reads it. The scene host owns one looping music source and six bounded 2D effect voices; sources/subscriptions stop on disable and die with the scene. No per-frame allocation/loading/dependency resolution.

## State and persistence
Only transient selected clip, pause position and cached revisions/generation. Existing SettingsModel/SettingsPreferences own Music/Sound, no new save schema or setting.

## UI, prefab, scene, and Inspector impact
No serialized scene or prefab change. Original files are copied byte-identically to Assets/Resources/Audio/Music/{HomeMusic,LevelMusic}.mp3 and Assets/Resources/Audio/Sfx/{HiveSlot,BeeCarry}.wav. Resources keys match those paths relative to Resources without extensions. The host loads four clips once per scene; music imports as streaming and short effects as PCM/decompress-on-load. Source files remain untouched. No Inspector work.

## Integrations and failure behavior
Missing clips fail initialization clearly. App pause/disable interrupts sources safely; navigation/generation changes clear old effects; enabling Music/Sound never replays past effects. Duplicate initialization creates no additional voices/subscriptions. No external network/SDK integration.

## Acceptance criteria
- [x] Correct supplied clips load and music loops/switches without overlap or retry/tab restart.
- [x] Landing and pickup effects fire once at actual commits, not every frame or cancelled input.
- [x] Independent saved preferences, background/resume, disable/re-enable and scene teardown work safely.
- [x] Native import/readback, focused/full tests, console, source identity/scope and project validation pass.

## Verification matrix
| Boundary | Evidence | Result |
| --- | --- | --- |
| Runtime music and effects | Five GameAudioTests exercise actual clips/AudioSources, landing cancellation, committed pickup, independent preferences, lifecycle and dense voice bounds | Pass |
| Native imports | Four original files match SHA-256; music streams, effects use PCM/decompress-on-load; native output has nonzero samples for both music tracks | Pass |
| Existing game/UI behavior | 147/147 EditMode + 157/157 PlayMode; zero failures or skips | Pass |
| Conventions/compile | Project validator: zero warnings; final Unity Console: zero errors/warnings | Pass |
| Scope/lifecycle | All existing scenes/prefabs byte-identical; one music + six effect sources, one listener; independent read-only review found no confirmed issues | Pass |

## Definition of Done
- [x] Project presentation/event ownership and asset lifecycle match implementation.
- [x] Acceptance criteria have actual verification evidence.
- [x] All checks pass and limitations are documented.


## Verification evidence (2026-10-19)
- RED: four focused runtime tests failed for the missing audio host before implementation (job c85003a07ab1429f979518d97257041a). Initial focused GREEN: 4/4 (0c5d5db8b5b644ccb247724357c1e4a3). Dense-harvest coverage was then added and passed in the complete suite.
- EditMode job 2c688eb5dd764d1ab862f6b0582df015: 147 passed. PlayMode job 4de7aa488e8943afad388e6cb2511ff9: 157 passed, zero failed/skipped.
- Native import readback: HomeMusic 21.6294 s, mono 44.1 kHz, Streaming; LevelMusic 133.9559 s, stereo 44.1 kHz, Streaming; HiveSlot 0.1451 s, stereo 44.1 kHz, PCM; BeeCarry 0.1787 s, stereo 48 kHz, PCM. Original bytes were preserved.
- Native PlayMode audit: each scene had one host, one listener, seven sources and one playing loop. HomeMusic DSP peak/RMS 0.008787/0.003254; LevelMusic 0.201355/0.066626. Opening Game view enabled the editor audio preview; earlier preview samples were zero. Audit used isolated test settings and memory economy, then restored original services/preferences.
- Baseline hash audit of 213 existing script/scene/prefab files: only HarvestBoosterCommands.cs and MainMenuNavigationController.cs changed. All existing scenes/prefabs are unchanged. Editor setup tests regenerate three menu assets; those were restored through Unity FileUtil before the complete PlayMode suite.
- Independent audio review found no confirmed correctness defect. Connected hive groups emit one landing event outside the activation loop; runtime landing coverage exercises a single hive directly.
- Evidence is in the task workspace work/game-audio/: full-edit-results.json, full-play-results.json, import.json, source-identity.json, final-scope.json, home-output.json, level-output.json and final-console.json.
- Limits: no physical-device/headphone listening session or device profiling was performed. Native DSP output, actual AudioSources and runtime tests verify playback/trigger behavior; final mix is adjustable in GameAudioController (music 0.4, landing 0.8, carrying 0.55).
