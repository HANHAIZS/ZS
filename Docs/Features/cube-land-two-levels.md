---
id: cube-land-two-levels
title: Cube Land花朵苹果关卡与连接蜂巢
status: Verified
owner: Project
created: 2026-10-15
updated: 2026-10-15
---

# Cube Land花朵苹果关卡与连接蜂巢

## Goal
按用户提供的0.5.3资料包替换现有Level01/Level02的模型、颜色身份、额度和队列，同时增加连接蜂巢玩法。仅接入plant_flower和Apple，不扩展787关或推断关序。

## User-visible behavior
- Level01来自plant_flower：1784方块、30蜂巢、3队列列；Level02来自Apple：4710方块、75蜂巢、5列。保留五工作槽、三等待行、现有道具与两关导航。
- 各方块坐标、原始色号、深度、蜂巢额度、列内顺序和原始64位引用身份可追溯；保留两份完整来源JSON。
- ConnectedBlasters合为无向连接组。用户确认：整组到各自列首位后才允许同时入槽，各占一槽，任一成员不在首位或槽不足时整组不移动、不消耗任意选取、不开始动画。
- 各成员保留自己的额度与三只蜜蜂并发，额度耗尽按原规则释放自己的槽；组内点击、连续点击不能重复激活。整组跳跃同时开始/落地，暂停/切后台/重开取消全部临时运动。
- 金色连线与链环标记表示连接关系。原任意选取继续用于独立后排蜂巢；连接组遵守用户指定的全员首位规则。洗牌按连接组整体排序并保持成员分处不同列，避免拆散后进入无法首位对齐的状态。
- Home开始按钮显示/进入同关，第一关胜利进第二关，第二关胜利回Home；其他UI、经济、设置规则保持。

## Ownership and architecture
- Commands：StartLevelCommand原子验证/创建连接组；ActivateWaitingHiveCommand原子提交整组；ShuffleWaitingHivesCommand保持组约束。
- Model：LevelData/CubeData/HiveData承载可追溯来源与连接索引；HiveState缓存不可变成员列表；HarvestModel缓存列数。
- System：HarvestSystem判断整组首位/容量并生成入槽计划，不在View里写状态。
- Utility：LevelOneAssets缓存已有加载器与新增色板/连接材质；LinkedHiveGroups为无状态验证辅助。离线导入脚本转换JSON且验证坐标/引用/额度。
- Views：HiveHopController/HiveModelView同步整组呈现，LevelOneHud显示实际列数，HarvestBoardView一次字典绑定模型身份并保持现有采集/旋转/视线规则。
- Editor：CubeLandLevelSetup通过Unity原生API生成共享倒角网格、12种源色材质、现有蜜蜂/蜂巢造型的染色版本与Flower/Apple模型prefab。不编辑Foundation。

## State and persistence
连接与来源信息仅属于关卡配置与当前会话，不新增玩家存档。模型坐标用固定正交坐标转换适配Unity竖向，保留原始坐标以便审计。金币/体力/偏好不改，测试使用MemorySaveStore。

## UI, prefab, scene, and Inspector impact
- 维持Resources键Levels/Level01、Levels/Level02、Prefabs/Gameplay/Flower、Apple及原三场景路径。
- 新PaletteXX蜜蜂/蜂巢prefab复用现有造型与固定白/黑部件，只替换Tint材质；原九色资源不改。
- 直接引用网格/材质在Assets/Sprites/Gameplay/CubeLand；新连接材质通过Resources/Materials/HarvestLink缓存加载。来源JSON位于Assets/LevelSources/CubeLand，不按运行时路径加载。
- 原始数据只给颜色编号，未给原材质色板。显示色板依据包内原始Icon纹理的全库采样对照，保持12种使用到的颜色身份独立；采样色不冒称原材质精确RGB。
- 两模型根与CubeData.name一一对应，各体素独立Renderer共享网格与材质，保留原采集拾取接口及页面生命周期。

## Out of scope
其他785对象、未出现的冰层/包装/大方块等机制、联网/热更、支付、关卡解锁存档、Foundation和现有UI重新设计。

## Integrations and failure behavior
缺失色板/来源引用、重复坐标、额度不匹配、无效连接或同列连接组在导入/启动前明确拒绝，避免半重置。异步跳跃提交时再验证代次、首位和所有槽；取消无模型修改。来源JSON没有通关解，完整可玩性以本次实测为证，不把资料包完整性等同于已验证可解。

## Performance considerations
12种共享材质、一个共享倒角Mesh；颜色资源按需缓存。体素绑定由全表重复搜索改为一次字典；无每帧GetModel/GetUtility或Resources.Load。组成员在开始关卡时缓存，常规可选性判断不分配新组。大模型需检查Editor运行表现，真机性能另测。

## Acceptance criteria
- [x] 两关体素/原色号/额度/队列/连接与选中来源完整对应。
- [x] 连接组首位/容量/指定槽原子性、重复点击、任意选取与洗牌约束正确。
- [x] 整组跳跃、取消/后台/重开、可见连接标记与逐成员释放槽位正常。
- [x] 两关布局、拾取/回巢、Home/Retry/Continue与经济保持。
- [x] 导入可复现，编译/规则及场景测试、原生读回、截图、规范和审查通过。

## Verification matrix
| Boundary | Evidence |
| --- | --- |
| 来源完整性 | 787索引SHA全部匹配；选中2份逐条对照 |
| 导入转换 | 最小数据错误分支与两关全量对照测试 |
| 连接玩法 | 新EditMode原子性/首位/容量/洗牌测试及PlayMode整组动画/取消 |
| 原行为 | 完整EditMode/PlayMode测试，更新内容变化导致的旧计数与队列预期 |
| 模型/呈现 | 原生prefab数量/身份/材质、两关实拍与实际采集 |
| 完成门槛 | Project/prefab validator、Console及独立审查 |

## Definition of Done
- [x] 以上证据齐全后Verified；报告未完成的设备或解谜验证。

## Verification evidence (2026-10-15)
- Import: `python3 Scripts/tests/test_cube_land_import.py` — 8/8 passed. `Scripts/import_cube_land_levels.py` reproduces Level01/02 byte-for-byte from `Assets/LevelSources/CubeLand`.
- EditMode: job `23701adc429241deb14967eb4e0c7f73` — 66/66 passed. New linked rules ran red before implementation; the planned-slot-release regression also ran red before its fix.
- PlayMode: job `d51ab8e991e9496d9dfb8e1e1d4580cd` — 92/92 passed, 69.50 seconds test duration. Covers linked group synchronous animation, duplicate clicks, pause/background/generation cancellation, first-frame three-column alignment, source model layout, boosters, Home/Retry/Continue, transport/collision and settlement.
- Native asset readback: Flower1784 and Apple4710 unique cube names, all names match LevelData, no missing mesh/material, each model uses one shared voxel mesh. All12 source palette Bee/Hive pairs load; each bee retains four grasp limbs.
- Visual checks: `flower-runtime.png`, `apple-runtime.png` in the workspace `outputs/cube-land-import` folder. Models sit between top collector and work slots; three rows retain existing toolbar layering. Source metadata/checksums and link pairs in `source-verification.json`.
- Convention gate: `python3 Scripts/validate_game_foundation_project.py --project .` passed with0 warnings. Clean preview Console:0 errors after loading and capturing both production scenes.
- Independent review verified complete source mapping and found twoP2 issues (initial column invalidation/zero-width text; landing slot replanning), both fixed and re-reviewed with no remaining findings.
- Existing MainMenuSetupTests regenerates MainMenu scene and two UI prefabs via the unchanged native setup pipeline; the complete menu/settings/navigation suite passed afterward.
- Harness recovery: two PlayMode jobs were aborted after an unexpected assembly reload lost cached test state; no completion was claimed for those jobs. Final run used a temporary reload lock after its bootstrap compilation. The lock is released and original Enter Play Mode options restored. Unity Test Framework1.1.33 emitted an exit-only BackgroundWatcher exception; clean gameplay preview did not reproduce it. No framework/package edits were made.

## Verification limits
- The supplied source levels have no verified solution path. New-source tests establish exact import, legal moves, presentation, bounded transport and conservation; they do **not** establish that a particular sequence solves each full puzzle. A naive front-first simulation can fill slots and stall. The archived original522-cube fixture retains its independent full-win simulation; settlement and next-level rules retain separate runtime coverage.
- Source material RGB values were not included. Independent original color IDs are exact; display tones are representative samples of the package Icon previews.
- Real-device profiling and complete human puzzle playthrough remain to be done. Each cube remains a separate Renderer to preserve individual pickup; shared meshes/materials and linear identity binding reduce asset and initialization overhead.
