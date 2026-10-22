# Shared Game Scene Implementation Plan

> **For agentic workers:** Use executing-plans to implement inline. User approved the two-scene design and requested implementation in this chat.

**Goal:** Preserve all114 levels while replacing114 gameplay scenes with Game.
**Architecture:** One shared Game skeleton resolves level selection from initialized navigation or Inspector PreviewLevel, reads cached Resources data and presentation config, and reuses existing Commands/Views. Same-name Game reload performs next-level lifecycle cleanup.
**Tech Stack:** Unity2022.3/C#/QFramework/Resources/Unity Editor APIs/Python.
**Spec:** Docs/Features/shared-game-scene.md

## Global Constraints
- Assets/Scenes and enabled BuildSettings contain only MainMenu.unity and Game.unity at completion.
- Preserve existing114 JSON/model prefabs and all source mechanisms, styles and economy semantics.
- No Foundation changes, screenshots, GitHub push, or device performance claims.
- Scene/Prefab/asset writes use native Unity APIs; old scenes deleted only after new flow passes.

## Review Focus
- Navigation level must override Inspector preview after Home starts a non-default level.
- Same-name Game reload must not keep previous level/controller/title/palette.
- Next-level/restart/leave callbacks must not fire twice or carry stale input/old generation.
- 6/106 and112–114 source handoff differences plus lighting must survive cold/shared loads.
- Editor tools and tests must not regenerate obsolete per-level scenes.

### Task1: Preserve content and specify shared-scene contract
- [x] Capture git/content hashes and read-only114-scene differences; verify native camera/HUD parity.
- [x] Add EditMode shared BuildSettings/catalog coverage and PlayMode Home→Game/114-load/continue tests. Run RED because Game/catalog are absent and Home uses Level01.

### Task2: Runtime catalog and navigation
Files: LevelPresentationData.cs, LevelPresentationView.cs, LevelOneAssets.cs, LevelOneController.cs, MenuNavigationModel.cs, MenuNavigationCommands.cs, MainMenuNavigationController.cs, PresentationCatalog.json.
Interfaces: LevelOneAssets.LevelKey(int) preserves LevelXX data key; GameSceneName/GameScenePath are physical route; Presentation(int) validates and caches per-level configuration. LevelPresentationView.Apply(data,number) sets title/light before Board initializes. PreviewLevel is used only when navigation is uninitialized.
- [x] Generate114 explicit entries from pre-migration known presentation selectors and native light parameters.
- [x] Implement validated cached catalog, startup priority, configured display flags and shared Game route.
- [x] Native-copy Level02 to Game, bind presentation title/light, PreviewLevel1, temporarily leave legacy scenes available for comparison.
- [x] Run new tests GREEN and inspect first/last and6/106 differences.

### Task3: Editor and tests migration
Files: SharedGameSceneSetup.cs, SourceLevelSceneSetup.cs, MainMenuSetup.cs, LevelOneSceneSetup.cs, LevelPauseSetup.cs, OkaSettingsSetup.cs, OkaUiRefreshSetup.cs, runtime test loader and existing tests.
- [x] Native shared scene setup/preview/build list; Apply(number) builds selected model and previews shared scene.
- [x] All UI maintenance loops target shared Game once; original initial scaffolder refuses overwriting existing Game.
- [x] Test loader sets navigation via Command then loads actual Game; old semantic identity checks read runtime level, new tests assert actual physical scene.
- [x] Run all project EditMode and Python tests and relevant gameplay/UI/flight suites; fix actual compatibility issues with reproduction evidence.

### Task4: Complete two-scene migration
- [x] After114-load and navigation checks pass, use AssetDatabase.DeleteAsset on114 legacy scene/meta pairs; preserve through Git history.
- [x] Re-run exact two-scene/content hash/reference gates and impacted runtime checks if removals expose dependencies.
- [x] Restore unrelated native serialization churn using baseline bytes/Unity FileUtil.
- [x] Independent read-only whole-change review; fix Critical/Important findings, record evidence in spec.
- [x] Final validator/compile/Console/hash checks, open Game or MainMenu clean, save local Git commit and report to user.

## Completion ledger
All tasks completed; evidence is in Docs/Features/shared-game-scene.md. Direct installedJson.NET3.2.1 dependency was added for strict config validation. Whole-change review and the same reviewer fix verification found no remainingCritical/Important issues.
