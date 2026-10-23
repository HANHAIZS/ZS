---
id: lightweight-levels115-119
title: Lightweight levels 115–119 with slower collection and compact layout
status: Verified
owner: Project
created: 2026-10-23
updated: 2026-10-23
---

# Lightweight levels 115–119

## Goal

Add five lightweight levels with deliberate collection motion and a larger, clearer model area.

## User-visible behavior

Add Lute (1369), BustStatue (1476), Sundae (1508), GoldenBox (1331), HiddenBox (1331). Preserve complete source geometry, color quotas, hidden hive identities and linked groups. Use three queue columns and the level114 light outlines, separated voxels, ambient lighting, soft shadows and cavity shading. Increase whole-model display size after measuring the available space. Move slots and queue down, reduce row pitch and slot-to-queue gap, keep hives usable above the booster toolbar. The five models may use different measured scale multipliers.

Only these five levels use a .26s departure interval, .18s settle, .16s reach, .26s lift and .12s source-to-grip handoff. Cargo replaces the source at its exact position and rotation, blends to the moving grip, then departs. Existing levels retain their timing, model scales, layout, content and saved progress behavior.

## Ownership and architecture

- Model: optional serialized BeeTimingData and immutable BeeTiming snapshot in HarvestModel; LevelPresentationData owns optional per-level display parameters.
- Commands: StartLevelCommand validates timing before resetting state; AdvanceHarvestCommand consumes the snapshot for departure, pickup and lift.
- Views: BeeFlightView uses the same timing snapshot; LevelOneHud lays out controls from the presentation profile; HarvestBoardView scales and centers the art.
- Utility: LevelOneAssets validates presentation catalog defaults and ranges, loads new levels with existing Resources lifecycle.
- Authoring: importer recipes preserve source identities and linked groups when repacking columns. CubeLandLevelSetup.ApplyLevel builds only the five new prefabs through Unity APIs.
- No Foundation package changes, new services, persistence/schema migration or new gameplay mechanisms.

## Assets, scenes and lifecycle

- New Assets/LevelSources/CubeLand/{Lute,BustStatue,Sundae,GoldenBox,HiddenBox}.json, authoring only.
- New Assets/Resources/Levels/Level115.json through Level119.json, keys Levels/Level115…Level119; cached by LevelOneAssets.
- New Assets/Resources/Prefabs/Gameplay/{Lute,BustStatue,Sundae,GoldenBox,HiddenBox}.prefab, keys Prefabs/Gameplay/{name}; instantiated by the board and destroyed on scene unload.
- PresentationCatalog.json appends five entries; LastLevel becomes119, continue from114 reaches115 and119 returns Home.
- Shared Game and MainMenu remain the only scenes. Preserve existing scene bindings, user scene/material changes and all old model prefabs. No UI art replacement.

## Failure, cancellation and performance

Malformed timing or presentation values fail before a partial profile is published or session reset. Missing timing (including Unity’s all-zero nested record) uses legacy defaults; partial timing records reject invalid durations. Pause, retry and scene switching keep the source handoff continuous and release temporary cavity materials. New models remain under2000 voxels; layouts/timings are cached per initialization and add no per-frame loading or dependency lookups. Stop testing cleanly and restore editor scene without overwriting user changes. No network dependency.

## Acceptance criteria

- [x] All five full original models, quotas, links and hidden identities preserved in three columns.
- [x] All five complete through real production commands with five slots and no boosters.
- [x] New timing changes observable dispatch/pickup/lift; legacy reset retains old timing; invalid timing atomic rejection.
- [x] Native runtime checks cover source pose replacement, .12s blend, pause/retry, shadows/outline/gaps, compact layout and safe model bounds across all five.
- [x] Navigation crosses114–119 and ends at Home; old level data/prefabs remain byte-identical.
- [x] Unity compilation, focused EditMode/PlayMode tests and project convention validator pass.

## Evidence

Native geometry and runtime readback used without screenshots or recording. No GitHub push.

- EditMode:42/42 passed, job edf3d689d82e4af889d0c785b111f0a3. Covers timing/legacy round-trip defaults, atomic invalid rejection, navigation114→119→Home, catalog ranges, complete resources and presentation defaults.
- PlayMode:3/3 passed, job fa5c4418f0bb4ea59f200b9970dfcc2e. All five load through the shared Game scene with correct light edges, voxel gaps, ambient and soft shadows; true source replacement and120ms grip interpolation; pause/retry; unchanged114 layout/timing; unchanged level6 90ms source transfer; navigation through new end range.
- Full real-command replay at dt=.1s with default five slots, no booster activation:1151369/1369;1161476/1476;1171508/1508;1181331/1331;1191331/1331. Reports match final production JSON SHA256; workspace evidence work/levels115-119/rules-summary.json.
- Source cube geometry, color quotas, hidden flags, hive identities and linked pairs retained; generated new data exactly matches importer output. Legacy1–114 JSON and every existing model prefab match pre-change SHA256.
- Convention validator passes with0 warnings. Focused code review found the JsonUtility omitted-record compatibility issue; corrected and verified with serialized legacy/default fixtures and actual old scenes. No remaining important review findings.
- Python import suite:25/27 pass. Existing test_level106_is_an_exact_level_six_variant and test_production_is_reproducible fail because prior level1/6 manual queue/dispersion edits already differ from historical importer output. Confirmed against HEAD5e7cc1f; not changed or masked here. Do not regenerate existing production JSON from the all-level CLI.

| Level | Model/count | Scale relative to114 | Runtime rotating X range | Runtime rotating Y range |
| --- | --- | --- | --- | --- |
|115|Lute1369|1.28|.3327–.6673|.4399–.8386|
|116|BustStatue1476|1.28|.2717–.7283|.4388–.8370|
|117|Sundae1508|1.28|.3564–.6436|.4404–.8348|
|118|GoldenBox1331|1.18|.0781–.9219|.4352–.8448|
|119|HiddenBox1331|1.18|.0781–.9219|.4352–.8448|

Measured16 yaw angles with native renderer bounds at portrait9:16. Whole-model center=.64; slots move down.048 of safe-area height (first five centers .3975→.3495); queue centers .274/.204/.134, pitch .07 vs original .085. Slot-to-front distance .0825→.0755. Queue hit regions do not overlap; retry does not accumulate layout offsets. Lower row remains partially behind the unchanged booster toolbar as in the existing design. New GoldenBox color21 uses an independent warm orange tone sampled from its supplied icon, with new voxel/hive/bee materials and prefabs; old palette assets are preserved.

Performance: no new per-frame allocations/lookups introduced by profile access; larger models retain source renderer counts under2000. Physical phone ergonomics remain a device follow-up; verified desktop portrait framing.

## Out of scope

No changes to existing level content, navigation art, economy rules, asset loaders or shared scene bindings. No GitHub upload.

## Verification matrix

| Boundary | Evidence required | Result |
| --- | --- | --- |
| Conventions | Project validator | Pass,0 warnings |
| Simulation | New timing tests and all five complete real-command replays | Pass |
| Runtime | All five native layout, render material, source handoff checks | Pass |
| Content | Source identity/quota/link preservation and legacy hashes | Pass |

## Definition of Done

- [x] Acceptance criteria and QFramework ownership verified.
- [x] Unity compiles, focused tests and convention validator pass.
- [x] New data/prefabs load at runtime; old data/prefabs preserved.
- [x] Native layout/model boundaries read back and results recorded.
- [x] Local commit includes only task-owned files; no GitHub push.
