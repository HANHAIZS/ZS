---
id: booster-inventory
title: 道具数量角标与限时加速
status: Verified
owner: Project
created: 2026-10-12
updated: 2026-10-19
---

# 道具数量角标与限时加速

> 2026-10-19 更新：玩家点击零库存“+”的行为由 [booster-purchase-popup](booster-purchase-popup.md) 接管：打开确认窗，以 1600 金币购买 3 次库存，不立即使用。此前“+”免费补充一个的描述为历史规则。关卡 Quit 的重置规则以 [leave-level-confirmation](leave-level-confirmation.md) 为准。

## Goal

给关卡现有四个辅助按钮加右下角库存/补充角标，使用道具消费库存；加速改为三分钟。

## User-visible behavior

- 四个道具各自初始 1 个。有库存显示红色角标和 Fredoka Bold 数字；数量 0 换绿色加号。
- 绿色加号是独立按钮，点击只补充该道具一个，不同时使用。只有数量 0 时可以补充；重复、过时请求无效。
- 主按钮只有库存大于 0 且动作有效时才可使用。成功使用原子扣 1，失败不扣。增加槽位仍最多 6 槽，已满不可消费更多；随机排序至少两个等待蜂巢。
- 加速仍为 2 倍，成功使用后持续 180 秒实际游玩时间；结束自动恢复 1 倍。设置、Home、后台暂停时间不计入。再次补充并使用加速将剩余时长刷新为 180 秒，倍率不叠加。
- 任意选择点击成功时扣1个道具，获得一次任意选择等待蜂巢的机会。下一只等待蜂巢成功落入空槽后，机会立即消耗，恢复每列仅可选择最前方蜂巢的规则；前排选择也计为这一次。
- 任意选择没有点击开/关行为，也不显示ON。机会待用时再次点击不关闭、不叠加、不重复扣款；补充仅增加库存。无效选择、槽位已满或取消跃动不消耗机会。
- Retry 清除第六槽位、加速和自由选择等关卡效果，保留已经消耗/补充的库存。Home→Start 保留进度、库存和加速剩余时间。
- 跃动、暂停、设置、Home、后台、胜利和死锁期间拒绝道具使用和补充，角标不捕获被阻断的按下事件。

## Out of scope

- 支付、广告、购买商店或收费流程；虽然素材文件名有“付费”，用户指定点击直接获得一个。
- 应用重启后的库存存档、账户同步、货币和奖励动画。
- 蜜蜂建模、花朵遮挡规则和抓取路径修改。

## Ownership and architecture

| Responsibility | Owner | Planned type/path | Notes |
| --- | --- | --- | --- |
| State mutation | Command | Commands/Harvest/HarvestBoosterCommands.cs; HarvestCommands.cs | 库存消费、补充、时长推进、Retry效果清除 |
| Observable state | Model | Models/Harvest/HarvestModel.cs | 四道具数量初始1、加速剩余秒；会话库存 |
| Shared rules | System | Existing HarvestSystem | 暂停/完成/容量/自由选择合法性 |
| One-shot notification | None | Existing HarvestChangedEvent unchanged | 无新增事件 |
| Unity presentation | ViewController | HarvestBoosterBar | 角标、独立补充按钮、倒计时与输入阻断 |
| Adapter | Utility | Existing cached LevelOneAssets | 无新依赖注册 |
| Reusable infrastructure | None | Foundation unchanged | 道具规则留在项目 |

## State and persistence

- 会话库存初始化各1；StartLevel/Retry不会重新赠送道具。用户确认保留已消耗/补充数量。
- 加速剩余秒按Time.unscaledDeltaTime的实际游玩时长递减，与原来上限0.1秒的动画步长分开；跨180秒边界的单步只将有效时间段乘2，后段按1倍推进；负值、NaN和Infinity不推进。
- FreeSelection表示待用的一次选择机会，由UseFreeHiveSelectionCommand发放，由ActivateWaitingHiveCommand在成功激活时消耗。Retry清除未用机会但不修改库存；设置/Home保留未用机会。
- Save schema/version/migration: None。应用重新启动库存重新初始化。

## UI, prefab, scene, and Inspector impact

- Assets/Sprites/Prefabs/UI/HarvestBoosterBar.prefab、Assets/Scenes/Level01.unity：四个按钮各增加独立 QuantityBadge/Count 与 RefillBadge。
- 新图 Assets/Sprites/UI/Boosters/{QuantityBadge,RefillBadge}.png，直接由角标 Image 引用，无新增 Resources key；内建 imagegen 去源白底，保留高光、加号和白色边框；原始图片不变。
- 角标52×52，锚点(.87,.08)相对现有142×142按钮，参考图右下角位置；数量28号 Fredoka Bold；加速状态显示2x及m:ss。
- Editor/HarvestInventorySetup.cs幂等组装角标；原LevelOneBoostersSetup.Apply会调用它以保留重新组装能力。
- Prefab 修改通过Editor API，保留旧按钮、素材和HUD引用；销毁场景时移除新补充监听。纹理初始化导入与缓存，运行时不加载资源。

## Integrations and failure behavior

- 无 SDK、网络、支付、热更和离线依赖。
- 原子 Command 先验证库存和行为条件，成功后扣款；0库存、已6槽、已有待用选择机会、无可排序等待单位或暂停时不扣。
- 后台暂停关闭所有关卡射线并DeactivateModule清指针缓存；恢复后等待旧触点全部释放，设置/Home门禁的所有权保持独立。
- 补充命令只允许当前库存0，防止连续点击旧加号获取多个。嵌套点击只到补充 Button，不传递到使用 Button。
- 全部输入遵守现有设置旧指针释放锁与跃动锁；晚释放不能触发已重新可用控件。

## Performance considerations

- 新增8个 Image、4个独立补充Button和4个数量TMP；旧模型缓存复用。
- 倒计时文本仅整数秒改变时刷新；库存/角标按Revision刷新。无每帧资源加载、依赖解析和监听重复添加。
- Fredoka图集增加一个512×512 Alpha8页以容纳完整数字/冒号，模式保持Static，运行时不生成新字形。
- 真机触控、Safe Area和帧率仍需要设备；Editor验证两种竖屏。

## Acceptance criteria

- [x] 初始各1，红色角标显示数字；0时只有绿色补充角标。
- [x] 使用仅成功时扣1，0库存不能用，点击补充只获得一个，不同时消费。
- [x] 加速2倍保持180秒，暂停冻结，到期恢复，跨边界计算正确。
- [x] 任意选择点击扣1，成功选择一次后恢复前排规则；无开/关文案与行为。
- [x] 重复点击、补充和无效/取消选择不关闭、叠加或浪费机会；用完可以再次消费新道具。
- [x] 深页选择成功后恢复第一页并隐藏翻页，前排选择也消耗机会。
- [x] 排序和自由选择消费各自库存，已满槽/已有待用机会/无排序单位不消耗。
- [x] Retry保留库存并清效果；Home保留全部当前会话状态。
- [x] 新角标没有设置/跃动/后台/旧指针输入穿透，布局和字体符合图片。

## Verification matrix

| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | validate_game_foundation_project.py | Passed; 0 warnings |
| Unity compilation | Compilation plus Console | Compiled; final error check recorded |
| Focused tests | Current one-use change: booster/inventory rule and runtime UI suites | 15/15 EditMode + 11/11 PlayMode; RED observed for entitlement persistence and ON label |
| Regression | Related booster and inventory suites; historical full-project run retained below | Current 26/26 related tests; previous inventory full run 40/40 EditMode + 48/48 PlayMode |
| Assets/runtime | Exact badge bindings, Component validator, two portrait sizes | 0 issues; initial, empty and mixed states inspected |
| Performance | Cached dependencies, revision/seconds updates, subscription review | Confirmed; no runtime resource loading or repeated dependency lookup; device timing not measured |

## Definition of Done

- [x] QFramework ownership respected; no Foundation edits.
- [x] Acceptance criteria implemented with meaningful regression coverage.
- [x] Sprite, prefab, scene, font and Inspector bindings checked through Editor APIs.
- [x] Convention validator, compilation, focused and full tests pass.
- [x] Fresh read-only review findings addressed, exact runtime evidence saved.
- [x] Remaining device checks reported before status Verified.

## Decision log

- 2026-10-12: User confirmed inventory survives Retry; gameplay clock freezes in settings/Home/background. Latest request supersedes free-selection-until-Retry behavior with one successful hive selection per consumed item.
- 2026-10-12: Successful acceleration refreshes to180s; already-pending free selection and capped extra slot reject further consumption; no external payment inferred from asset name.

## Previous inventory verification evidence

- Full EditMode job `bd3f757cdf2a4fdf89d1a85ddb3cd179`: 40 passed, 0 failed.
- Full PlayMode job `58f334fc45454a119c11acc455681a86`: 48 passed, 0 failed.
- First RED: 7 missing inventory API cases `ce4fab6e7c524f6a827c3dcc42436a57`, 3 missing badge cases `26144a255c5a420194449fc7812bfa4d`.
- Review regression RED: truncated real clock `e177a2444b434db88590560aea832225`, old background pointer consumption `2c3629fc798c4304bf1d35cbea2e3fbc`; idle timer text also observed failing before fix. All corresponding cases included in final GREEN.
- Refills tested through actual touch raycasts and StandaloneInputModule press/release, including a disabled parent Button. Grant changes0→1 without activating/refreshing speed; duplicate grant after1 does nothing.
- Native idempotent rerun of original four-booster assembly succeeded with badges intact. Component validator: valid, 0 issues. Four quantity labels FredokaBoldSDF, non-raycastable; four green refill Buttons with correct art, anchors(.87,.08), size52; stock badge art read back exactly.
- Full numeric glyph/colon availability read back from static Fredoka asset, atlas textures2. Mixed-state screenshot renders second-atlas digit9 correctly.
- [Initial quantities 1080x1920](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/booster-inventory/initial-1080.png), [Empty quantities and countdown 1080x1920](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/booster-inventory/empty-1080.png), [Mixed quantities and countdown 1170x2532](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/booster-inventory/mixed-1170.png), [Final playable state](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/booster-inventory/final-1080.png). Temporary GameView size removed, original1080x1920 restored.
- [Test job records](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/booster-inventory/verification.json), [Review and resolution](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/booster-inventory/review.md), [Original art and extraction prompts](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/booster-inventory/art/manifest.json).

## Final changes

- Models/Harvest/HarvestModel.cs: independent consumable counts and remaining effect seconds.
- Commands/Harvest/{HarvestBoosterCommands,HarvestCommands}.cs: consumption/grant guards, timed use, real/simulation clock split and Retry preserving stock.
- ViewControllers/Harvest/{HarvestBoosterBar,LevelOneController}.cs: independent badges, cached countdown display, real gameplay clock and background touch cancellation.
- Editor/{HarvestInventorySetup,LevelOneBoostersSetup}.cs: native prefab/scene assembly and preserving repeated setup.
- Tests/Editor/{HarvestInventoryTests,HarvestBoosterTests}.cs; Tests/Runtime/{HarvestInventoryUiTests,HarvestBoostersUiTests}.cs: focused regressions and updated superseded free-toggle expectations.
- Assets/Sprites/UI/Boosters/{QuantityBadge,RefillBadge}.png; Assets/Sprites/Prefabs/UI/HarvestBoosterBar.prefab; Assets/Scenes/Level01.unity; FredokaBoldSDF numeric atlas extension. Original source art and old files backed up, no scene/prefab text edits.

## Remaining device checks

Physical touch feel, actual notched safe area, GPU/frame timings and real OS background transitions require a device. Editor actual-input simulations, native bindings and both target-size captures are verified. No remaining Inspector wiring work.

## Single-use selection revision (2026-10-12)

- Goal: remove persistent free-selection presentation and limit each use to one successful waiting-hive activation.
- Ownership: existing Model stores the pending entitlement; existing Commands grant/consume it; existing System and HUD derive selectable hives and paging; no Foundation, save, asset, hierarchy, or Inspector change.
- Verification: core tests for rear/front success, invalid IDs/slots/full capacity/pause, refill/repeated use; PlayMode tests for actual button callbacks, deep-page reset, cancellation/retry and re-use. Compilation and convention validation required.
- Performance: one boolean assignment during successful activation, existing revision refresh; no new runtime allocations or dependency lookup.
- Verification result: Verified. New core RED job `68a5bc761c3e40189ab20042ec7d9e92`: 2/2 fail because entitlement remained after success. UI RED job `2721a630bb914bc5abb319f6425d787e`: 2/2 fail because ON remained.
- Fresh GREEN: `9f973199d0204462bd49500d166f7c5c` 15/15 EditMode; `c7020925ad6f4c618a765a5eb0630934` 11/11 PlayMode. Includes second rear selection rejection, front consumption, invalid destinations, full capacity/pause, repeated use/refill, cancelled hop, deep-page reset and reuse. Existing clock, Retry, Home, background and hop input cases still pass.
- Compile completed; final Console error entries0. Convention validator passed with0warnings. Fresh read-only reviewer found no Critical/Important issues. Optional future coverage: repeated use callback on a deep page.
- Exact runtime readback: Level01, pending false, blank free-selection state label, neutral white button art, page0, all rear buttons disabled. User-ready preview restocked through existing refill Commands to each1, without granting a choice; no model setters used by UI/QA.
- Current changed production scripts: Commands/Harvest/HarvestBoosterCommands.cs (successful activation consumes choice), Models/Harvest/HarvestModel.cs (one-use state documentation), ViewControllers/Harvest/HarvestBoosterBar.cs (no ON/tint toggle presentation, repeated callback no-op).
- Current changed tests: Tests/Editor/HarvestBoosterTests.cs (+2), Tests/Runtime/HarvestInventoryUiTests.cs (+1), Tests/Runtime/HarvestBoostersUiTests.cs (deep page and reuse assertions). No serialized assets, Inspector fields, Foundation packages or dependencies changed. No new recurring allocations or loading.
- [Current test evidence](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/single-use-hive-selection/verification.json), [Current review](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/single-use-hive-selection/review.md), [Exact changes](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/single-use-hive-selection/changes.diff).
- Physical device checks from prior inventory work remain device-only; no new layout/Inspector work.


## 2026-10-19 剩余数量角标底图替换

按用户新红色空白圆底替换所有剩余额度数字角标，其他保持。搜索确认实际共用Assets/Sprites/UI/Boosters/QuantityBadge.png，经HarvestBoosterBar.prefab的四个QuantityBadge引用并覆盖五关；不替换绿色RefillBadge/经济加号、商城商品数量文字或蜂巢正文字数。原资源路径/GUID/Sprite本地ID保留，生成PNG替换原底图，仅用原生TextureImporter更新原QuantityBadge SpriteRect。Prefab、场景、数量数字/字体/位置/52×52尺寸、事件和功能均保持，无新代码/Resources键/加载。

用户原图不改，内置imagegen透明提取后原样复制工作区及项目；备份原PNG与meta。验收：资源GUID/localID、全项目使用覆盖、共享Prefab四处精确引用和绿色加号保持、运行截图、既有库存UI的数量消耗/补充和暂停测试、Console及项目规范。静态图不新增镜像测试；不涉及模型/支付/库存规则修改。


### 本次验收完成 — 2026-10-19

- [关卡原生截图](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/quantity-badge-refresh/level01.png)：四个新红色圆底，原数字清楚、居中，无底色残留或切边。
- persist-import.json：原GUID 0ff8fd7495ef44d9bb003d2d5bc84cd0、localID -2303942535736894706保留，原生SpriteRect实际持久化为130/138/990/988。meta-diff.txt确认metadata只有这四个裁切字段变化。
- coverage.json：原生逐关读回Level01–05，20处数量Image全部匹配新Sprite，52×52、锚点(.87,.08)、原数字保持，绿色RefillBadge仍OkaRefresh20261018/RefillBadge.png。
- scope-hashes.json：全项目旧资源变化只有QuantityBadge.png及其meta，无新增文件；所有代码/Prefab/场景/字体/材质及其他PNG逐字节保持。art-integrity.json确认内置imagegen生成PNG、workspace副本与项目PNG一致。
- test-result.json：既有真实触摸消耗/补充及Home/Settings暂停重试库存PlayMode2/2通过；Console错误/警告0，项目规范0 warnings。独立资产复核无待修问题。
- 提示词与源图记录：[manifest.json](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/quantity-badge-refresh/manifest.json)；输出工作区work/quantity-badge-refresh/QuantityBadge.png，项目原路径Assets/Sprites/UI/Boosters/QuantityBadge.png。
- 复用原单张max512纹理、原导入参数、生命周期，未增加运行时代码或加载，无Inspector待绑定项；未做设备性能采样。已恢复MainMenu编辑场景。
