---
id: bee-transport-readability
title: 取块至交付的动作与路径可读性
status: Verified
owner: Project
created: 2026-10-15
updated: 2026-10-15
---

# 取块至交付的动作与路径可读性

## Goal
让玩家看清哪只蜜蜂从哪个原方块取走货物、缺口如何形成、从模型哪边离开、最后进入哪个大蜂巢入口。落实用户明确提出的停顿拉出、先外移后回巢、轻微倾斜摆动三项要求。

## User-visible behavior
- 去程仍0.60秒；到点先停约0.14秒，再用0.26秒伸手抓取，原方块在抓稳前仍可见且保持遮挡。
- 原块和载货块在同一投影、大小、材质与朝向交接，用约0.42秒平滑拉出并托稳；拉出结束前蜜蜂不开始横向离开，缺口随方块离开逐渐显露。
- 载货先用约0.32秒飞到靠近取块位置的模型左/右外缘，随后沿外侧上升再转向顶部Collector入口。路径在拾取时缓存，不跟随模型旋转；靠近中轴的起点按任务身份分到两边。
- 外飞阶段有小幅、错相的倾斜与摆动，原块交接阶段保持稳定，接近入口时收拢消失。视觉动作使用模拟Progress，暂停完全冻结，不使用墙钟。
- 颜色、方块身份、额度、出巢间隔、每蜂巢三只并发、槽位与连接组、交付计数和两关导航不变。动作耗时适当延长以便看清，速度道具仍整体加速。

## Out of scope
不新增模型、UI、粒子、声音、目标选择规则、连接蜂巢玩法、经济/存档或Foundation改动；不重新安排关卡。

## Ownership and architecture
- Command: AdvanceHarvestCommand继续唯一负责阶段推进/取块/交付；仅调整动作时间边界，不新增计数规则或存档。
- ViewController: BeeFlightView缓存取块姿态、外缘航点与姿态相位，渲染路径/抓取/轻摆；HarvestBoardView从已有模型局部包围盒计算当次取块外缘出口。
- Utility: 只在必要时增加无状态轨迹曲线，角色仍位于项目集中目录，不注册新架构依赖。
- Model/System/Event: 复用原有结构与事件；不修改Foundation。

## State and persistence
无新增存档。Actor路径仅本关表示缓存，Generation或池复用时清除；继续使用已缓存Model/Board/资源。真实玩家经济与偏好不改，测试经济使用MemorySaveStore。

## UI, prefab, scene, and Inspector impact
本需求无UI、Scene、Prefab或素材设计更改。两关现有BeeFlightView/HarvestBoardView消费改变；保留Resources加载键和Inspector引用。沿用Bee、CarriedVoxel、抓握与对象池。

## Failure and lifecycle behavior
遮挡时继续等待可见目标，不提前取块。暂停冻结身体/货物/翅膀和路径进度；Retry回收旧Actor并恢复原方块；首次渲染即载货或大步长跨阶段仍正确初始化。并发避让继续覆盖身体、翅膀及载货，外缘路径限制在相机可见范围，并保留近/远裁剪间距。载货途中视口变窄时将完整包围盒夹回可视区，平面避让无空位时使用所有既有包围盒外的前/后深度层。当前正交相机裁剪范围0.3..1000；不支持小于一只蜜蜂几何尺寸的异常相机。

## Performance considerations
外缘出口每次取块仅计算已有局部包围盒8角投影；路径与摆动为固定少量向量计算。沿用共享Mesh/材质、Actor池与缓存依赖，不引入物理或逐帧Resources加载。需检查最多18只加速并发，真机性能不在本次Editor验证范围。

## Acceptance criteria
- [x] 到点有独立可见停顿，抓取结束前原块保持；交接身份/位置/大小/材质连续。
- [x] 拉出结束前根位置保持，货物可见移出并形成缺口。
- [x] 首段外移、接着外侧上升、最终入口投影正确，减少模型中轴聚集。
- [x] 载货姿态轻摆错相；暂停、重开、池复用和旋转稳定。
- [x] 并发/货物身体间距、速度道具、计数、导航回归通过；核查序列化资产变化和现有引用。

## Verification matrix
- 先行规则与实际场景测试，记录旧版本对应失败；再运行完整EditMode/PlayMode。
- 原生渲染多帧GIF/关键帧与完整两关场景，检查实际取块→外飞→入口路径。
- Assets逐文件SHA限制改动范围；项目规范、Console和独立只读审查。

## Definition of Done
- [x] 验收、运行画面与回归证据齐全后Verified；记录设备验证边界。


## Implementation and verification evidence
- Product changes: `Assets/Scripts/Game/Commands/Harvest/HarvestCommands.cs`, `Assets/Scripts/Game/ViewControllers/Harvest/HarvestBoardView.cs`, `Assets/Scripts/Game/ViewControllers/Harvest/BeeFlightView.cs`. No new runtime Model/System/Event, no save-schema or Foundation change; existing scenes consume the same components and resource keys. No Inspector assignment is required.
- New tests: one Command timing test and seven PlayMode readability tests; older tests update phase-boundary waits to named constants. Exact handoff material/pose/scale, geometry clearance and conservation checks remain; held cargo region allows the explicitly requested tiny sway.
- Red evidence: old Command removes the source during the new arrival/grasp interval (`baff662c2efb4548a2b51fdade2a9a90`); original four view tests fail on hold, extraction, outside lane and bank (`683028e5d47d49d1ad4cea68333feab7`). Actual Apple five-hive visibility fails left-edge x=-.0076 before screen candidate filtering (`9fa5670cbdcd48e6b8c72585dfaec64d`); cached-lane viewport resize fails x=-.137 before projection fallback (`131956017d9147d2a120e89e6fdebff2`).
- Final PlayMode: `4c9345bff2854edbbed2dd5e1335d157`, **99 passed, 0 failed, 0 skipped**, 71.14 sec. Covers both levels, original UI/navigation/economy, source/held geometry, multiple model angles, pause/retry, boosted concurrent carriers and viewport resize.
- Final EditMode: `02b52beb491f4762b019bf1416411966`, **67 passed, 0 failed, 0 skipped**, 3.36 sec.
- An intermediate full run was interrupted by an unexpected Editor script/domain reload at test71; it is not counted as a completed result. The inactive orphaned test service was cancelled and the full suite rerun. Unity Test Framework1.1.33 also emits a known exit-only BackgroundWatcher exception outside the actual test result; this package was not changed.
- Performance: cached local model bounds and per-task exit; fixed curve/sway math and up to eight-corner viewport checks per candidate, no new physics, no path allocations, no Resources loading or material instantiation in the per-frame feature. Existing collision scan remains bounded by active actors. Device/GPU profiling and a full manual puzzle solve remain outside this animation verification.
- Asset audit: pre-task SHA snapshot in `work/bee-readability/baseline-hashes.json`; final changed-existing list in `outputs/bee-readability/changed-assets.json`. Besides the three product scripts and timing tests, native tests serialize MainMenu scene / MainMenuPage and MainMenuSettingsPopup (existing MainMenuSetupTests calls Apply/SaveAssets), and nine previously loaded palette materials have byte changes. No YAML scene/prefab editing was performed. All twelve palette base colors were checked against the imported source colors; art, resource/meta GUIDs, level JSON and both gameplay scenes remain at their original hashes. The report does not claim every serialized asset is byte-identical.
- Read-only review: reviewer found the missing viewport fallback; the added resize red test reproduced it. After clamp/depth fallback, reviewer found no mandatory issue for the current orthographic level camera.

- Final native recordings: `outputs/bee-readability/花朵-取块到交付.gif`, `outputs/bee-readability/苹果-多蜂外侧交付.gif`, plus `flower-stages.png` / `apple-stages.png` and per-frame traces. Each is72 distinct actual Editor repaint frames at0.05 simulation seconds, not repeated screenshots. Preview Console:0 errors/0 warnings. First carrier remains anchored at0.60..0.95 sec, source switches to same-pose cargo at1.00 sec, pull remains anchored through1.40 sec, outside lane reached around1.80 sec, entry projection around2.50 sec, first deposit at2.55 sec. Conservation checked all144 recorded frames: Flower1784 and Apple4710.
- Final project convention validator: passed,0 warnings. Final state: temporary Editor reload lock released, recording callbacks removed, Play stopped, MainMenu open. No remaining Inspector work. Evidence index: `outputs/bee-readability/verification.json`; runtime/geometry checks are Editor-only, device performance remains unprofiled.
