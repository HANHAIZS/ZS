---
id: oka-economy-gameplay-ui-refresh
title: 金币体力与关卡功能区欧卡素材替换
status: Verified
owner: Project
created: 2026-10-18
updated: 2026-10-19
---

# 金币体力与关卡功能区欧卡素材替换

## Goal
按用户指定11份图片更新主界面经济条、各界面加号角标、关卡道具底栏/按钮/暂停/槽位，保留原有功能与实时数据。

## User-visible behavior
- 主界面金币和体力采用新金色硬币/红心加桃色条。素材内1240、5、Full是示例，移除后由现有TMP字段显示金币、体力、恢复时间/full。
- Home与Shop共用经济条统一更新；加金币/体力、道具补充等加号角标统一图三。用户已确认保留原数量数字角标，只替换加号。
- 关卡底栏用紫金长条，四按钮用绿金底板，并按原功能顺序显示增加槽位、加速、随机排序、任意选取的新图标；数量和加速倒计时继续实时可读。
- 关卡设置齿轮换紫色圆形图六；槽位显示深蓝蜂窝六边形金边图十一；原位置、数量、增槽规则、点击目标和蜂巢进出不变。
- 原图实底棋盘格经imagegen背景提取为真实透明alpha。导入裁取透明留白，按钮/底栏依实际轮廓适配，保留原第三排蜂巢的遮叠关系及按钮位置，避免白方块和拉伸边框。

## Out of scope
不修改经济/库存/购买/加速/选蜂巢/排序/增槽/暂停业务规则，不更改关卡、蜂飞行、模型、主界面背景、导航栏图标或设置弹窗。

## Ownership and architecture
现有EconomyHeaderView/HarvestBoosterBar/SettingsPopupController/HiveModelView继续展示并转发Commands。Editor导入与样式更新归项目Editor Setup，仅更新Sprite导入、Prefab和场景序列化呈现引用/布局；Commands/Models/Systems不变。资源只直接引用，保留加载与生命周期。

## State and persistence
无运行时新业务状态或存档变更；保留数字字段、Button引用、监听注册和暂停/后台输入门限。

## UI, prefab, scene, and Inspector impact
直接引用PNG统一保存在Assets/Sprites/UI/OkaRefresh20261018/，文件名CoinCounter、StaminaCounter、RefillBadge、ToolbarBase、ButtonDisc、Speed、FreeSelect、Shuffle、AddSlot、SettingsGear、HiveSlotTile。保留旧PNG与GUID，更新引用到新素材；不覆盖原用户图片。生成选中版先存workspace。

相关资产：MainMenuPage.prefab、HarvestBoosterBar.prefab、Level01/Level02场景、HiveSlotTile.mat及新图导入meta。通过Unity API修改，不文本修改Prefab/Scene/Material。共用经济条覆盖Home/Shop；不新增Resources加载键。缓存/实例销毁沿用现有View，新增Inspector绑定由Editor脚本完成并回读。

## Integrations and failure behavior
离线、SDK无变更。加号耗尽切换、数量、计时、背景暂停和设置遮罩输入继续原规则。重开/切关复用样式；Editor重建菜单不能恢复旧图或旧排版。

## Performance considerations
使用导入后的Sprite/材质，无运行时抠图/纹理分配；保留共享材质、池化和UI依赖缓存。贴图按显示尺寸缩放导入，当前为未压缩RGBA、不可读、关闭mipmap、保留alpha。计数条上限1024、图标512、底栏2048；11张纹理在Editor中全部加载的Profiler读数约34.5 MiB，不等同于手机端占用。未新增每帧分配或业务调度。

## Acceptance criteria
- [x] 11项映射正确，未改数量角标，加号统一。
- [x] 金币/体力/倒计时为实时数据，无烘焙示例数字或棋盘格白底。
- [x] 两关底栏/四功能/暂停/六边形槽位可见、比例合适、不遮住交互。
- [x] 点击效果、禁用/暂停/后台、数量耗尽与补充、恢复时间、增槽/排序/选取/加速保持原行为。
- [x] 两关和主界面/商城实际截图及不同竖屏宽高比通过。

## Verification matrix
Unity编译、工程规范、Prefab Validator、目标Sprite/Material/Prefab回读；现有菜单经济与道具/槽位/设置场景测试；真实点击和1080×1920画面；只对有意义的实时数据/交互/重建边界新增回归，不为图片替换写镜像测试。

## Definition of Done
- [x] 素材、导入、引用、Editor重建路径一致且保留功能。
- [x] 相关测试与UI规范通过，控制台无新错误。
- [x] 原生画面检查并记录资源来源、prompt、验证边界，更新Verified。

## Decision log
- 2026-10-18: 用户明确只改UI；异步确认保留数量角标，仅更换加号。原PNG背景不透明，使用内置imagegen逐项提取，经济条清理示例数字；不改变原用户图片。


## Verification evidence (2026-10-18)
- Final EditMode: 121/121 passed; job `e90b6e84f1c14719927a2c78649258f5`.
- Final PlayMode: 126/126 passed; job `5cccc55a39d2451bb57cfe56855eee61`, 146.03 s. Covers economy, real touch refill, inventory/countdown, pause/background locks, slots, hive interaction and existing bee routes.
- Foundation project validator: passed, 0 warnings. MainMenuPage/Page, HarvestBoosterBar/Component, MainMenuSettingsPopup/Popup: valid, 0 issues each.
- Binding audit confirms all 11 mappings. Four quantity badges still reference `Assets/Sprites/UI/Boosters/QuantityBadge.png`; only six live plus badges use the new RefillBadge (two economy, four booster).
- Native screenshots: Home, Shop, Level01, Level02 at1080×1920; Home and Level01 at1080×2340. Additional speed/refill state shows green plus and live `2x 3:00` countdown. No baked sample text or checkerboard remains. Preview restored to1080×1920, temporary size removed.
- Scope audit: no existing runtime C# or gameplay data changed. UI generators reserialized MainMenu.unity/MainMenuSettingsPopup.prefab as part of the existing rebuild test; inspector references remain coherent. Existing pause popup layout and menu navigation retained.
- Editor compile complete; final smoke check and warning/error console recorded in output evidence. No Inspector assignment remains. Physical-device rendering/performance not tested.

## Art provenance and deliverables
Built-in imagegen `background-extraction` edits produced true-alpha cutouts from the 11 supplied local RGB images; Coin/Stamina also removed baked sample values. No source image overwritten. Selected generated PNGs copied byte-for-byte to workspace `work/oka-ui-refresh/art/` and project `Assets/Sprites/UI/OkaRefresh20261018/`.

Exact final prompts, source paths and generated outputs: `/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/oka-ui-refresh/manifest.json`.

Evidence root: `/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/oka-ui-refresh/` (`editmode-results.json`, `playmode-results.json`, `binding-audit.json`, `prefab-validation.json`, `scope-audit.json`, `asset-audit.json`, screenshots, final-state.json, console.json).

## Changes and regeneration
- New Editor helper: `Assets/Scripts/Game/Editor/OkaUiRefreshSetup.cs` (alpha bounds import, serialized art/style application, slot UV mapping).
- Existing Editor hooks: EconomyHeaderSetup, LevelOneBoostersSetup, HarvestInventorySetup, LevelOneSettingsSetup, LevelOneHiveClickSetup. Rebuilding UI retains the new art and live fields.
- Modified assets: MainMenuPage and HarvestBoosterBar prefabs, HiveSlotTile material, Level01/Level02 scenes; coherent menu/settings regeneration batch above. Runtime Commands/Models/Systems/ViewControllers unchanged.
- First candidate moved toolbar/button layout; two existing third-row occlusion tests rejected that change. Restored original positions/layering and removed the speculative new containment test. Existing regression tests unchanged and full final run passed. Historical failed results retained as `initial-layout-regressions.json`; workspace-only candidate test is discarded.

## 2026-10-19 金币与体力角标位置微调

用户要求绿色加号贴在金币/红心图标的右下角。仅调整MainMenuPage共享EconomyHeader中两个既有Button的RectTransform：CoinsPlus从(0.37,0.15)到(0.30,0.20)，StaminaPlus从(0.44,0.15)到(0.35,0.20)，大小44×44与偏移0保留。两个图标轮廓宽度不同，分别对齐右下缘。Home与Shop复用同一头栏；EconomyHeaderSetup的生成参数同步，保持重建一致。

不改图片、数字、点击事件、体力/金币规则、运行脚本或存档。资源键Prefabs/UI/MainMenuPage和实例生命周期不变。验证采用原生Rect回读、Home/Shop实际竖屏截图、已有经济交互检查及工程规范；不新增镜像布局测试。该位置调整没有新增运行成本，无手工Inspector绑定。

位置微调验证：Unity编译完成；两处锚点原生回读正确；Home/Shop 1080×1920画面人工确认；现有2项经济加号/商城交互测试2/2通过（bcb5d5b42c524228a942b8d1b8137395）；控制台0条错误或警告。Prefab保存差异仅四行锚点坐标，既有Button、资源和事件引用保持。结束时恢复Level05编辑场景。证据目录 `/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/economy-badge-position/`。
