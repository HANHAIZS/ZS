---
id: level-one-hive-click-hop
title: 点击小蜂巢自动入槽及跃动反馈
status: Verified
owner: Project
created: 2026-10-10
updated: 2026-10-21
---

# 点击小蜂巢自动入槽及跃动反馈

## Goal

按用户最新要求取消拖放，恢复点击列前端小蜂巢自动进入空槽，加入模型和数字一起跃起、落稳的反馈；保留白色圆角槽位图标。

## User-visible behavior

- 点击可用的小蜂巢，选择点击时从左到右第一个空槽。
- 0.24秒弧线跃入，随后0.10秒轻微落地回弹。模型轻微伸缩、额度数字跟随移动，移动途中显示在其他场景模型前方。落稳后执行入槽命令、出蜂时扣额度。
- 一次入槽动画期间不接受重复或其他蜂巢入槽，避免误派后排；动画结束即可再次点击。
- 五个白色底座中心不变，模型覆盖底座，底座高度为槽内蜂巢高度的1.18倍。空槽无文字。
- 满槽、空列、暂停、已胜利／死锁点击不生效；不支持拖动或手动指定槽位。
- 重开、暂停、应用进入后台、对象禁用／场景退出取消未完成动画，恢复模型与数字，不扣额度。新一局不继承旧动画。
- 入槽后仍不可撤回。出巢扣额、额度归零释放槽、尾批独立来源、交付计进度和458块通关保持。

## Out of scope

其他关卡、主界面、商城、体力、复活、新蜂巢／蜜蜂模型、联网及存档。

## Ownership and architecture

| Responsibility | Owner | Planned type/path | Notes |
| --- | --- | --- | --- |
| State mutation | Command | Commands/Harvest/HarvestCommands.cs | 复用ActivateHiveCommand；落稳才提交，校验目标空槽 |
| Observable or persistent state | Model | 现有HarvestModel | 不新增模型／存档字段 |
| Shared project rules | System | 现有HarvestSystem | 复用列前端与外域采集规则 |
| One-shot notification | Event | 现有HarvestChangedEvent | 不新增事件 |
| Unity presentation | ViewController | HiveHopController、HiveModelView、LevelOneHud、LevelOneController | 缓存依赖，临时动画状态由ViewController拥有 |
| Adapter or repository | Utility | 现有LevelOneAssets | 原Blender资源路径不变 |
| Reusable infrastructure | None | Foundation不改 | 唯一BeeHarvestApp |

## State and persistence

- Runtime state: ViewController持有本次蜂巢ID、列、目标槽、局代数及动画时间；游戏状态仍只由Command写入。
- Save schema/version/migration: None。
- State mutation path: Button.onClick → HiveHopController → 视觉跃动 → ActivateHiveCommand → HarvestModel。

## UI, prefab, scene, and Inspector impact

- Affected assets: Assets/Scenes/Level01.unity；原3个小蜂巢Prefab不修改。
- Loading mode and final paths: 复用直接引用Assets/Sprites/UI/Gameplay/HiveSlotTile.png及HiveSlotTile.mat；不重新生成图片。
- Resources keys or direct reference owners: HiveModelView.SlotMaterial → Material → PNG。原蜂巢由LevelOneAssets缓存Resources加载。
- Load/cache/instance cleanup responsibility: HiveModelView复用已有蜂巢实例、5个共用材质底座；随场景销毁。不在点击／Update加载资源。
- Hierarchy or binding changes: 移除HiveDragController和5个HiveDragHandle，新增根HiveHopController；保留HUD.SlotTargets位置缓存及白色图标。队列Button恢复点击回调。
- Inspector confirmation: 原标题、花朵、大蜂巢、数字位置、背景引用不变；无拖动Handler残留。750×1334、750×1625预览。

## Integrations and failure behavior

- SDK/consent/lifecycle: 无新增SDK；后台／暂停／重开清理临时动画。
- Content update/offline/rollback: 完全离线；变更前源文件和场景已备份。
- Cancellation, timeout, retry: 入槽动画只持有视觉状态，不预扣额度；结束前校验局代数、列前端和目标槽。取消后可再次正常点击。

## Performance considerations

保留5个共用材质的平面和原蜂巢模型；单次动画移动现有Transform与数字，无协程或逐帧实例化，无额外架构查找。最多1个动画同时运行；未测iPhone实机帧率。

## Acceptance criteria

- [x] 单击即启动跃动并自动进入第一个空槽，数字跟随，落稳后出蜂扣额。
- [x] 白色图标中心不变、略大于蜂巢，模型覆盖图标；无拖动交互。
- [x] 连点／满槽／暂停不重复入槽，重开／后台取消不扣额或误派新一局。
- [x] 原出蜂、尾批来源、回巢交付、重开及458块通关通过回归检查。

## Verification matrix

| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | validate_game_foundation_project.py | Passed，0 warnings |
| Unity compilation | 无新增编译错误 | Passed；最终Console 0 errors |
| Focused tests | 点击自动选槽、跃动轨迹、连点、满槽、取消、原展示及通关 | EditMode 13/13、PlayMode 8/8，均无跳过 |
| Runtime or asset behavior | 图标引用、组件移除、新组件绑定、两种竖屏及跃动截图 | MCP读回Hop=1、Drag Handler=0、Missing scripts=0；750×1334及750×1625预览通过 |
| Performance | 模型复用、仅1个临时动画、依赖缓存 | Source review passed；5个原有图标Quad，共用1材质，纹理max512，无Mipmap／CPU可读副本 |

## Definition of Done

- [x] Ownership and QFramework roles match implementation.
- [x] Public behavior and acceptance criteria implemented.
- [x] UI, scene and Inspector impact verified through Editor APIs.
- [x] Convention validation and Unity compilation pass.
- [x] Focused tests and exact runtime checks pass.
- [x] Review and platform limits recorded.
- [x] Status updated to Verified after evidence exists.

## Decision log

- 2026-10-10: 用户取消拖放，明确恢复点击自动进入槽位并增加跃动。此文档替代level-one-hive-drag-slots的输入要求；保留该文档中的白色槽位底座。
- 2026-10-10: 采用0.34秒串行视觉反馈，落稳后提交Command，避免蜜蜂从尚在移动的蜂巢／空槽提前出现。

- 2026-10-10: 动画期间禁用所有待选蜂巢按钮及其射线命中；Hop组件禁用后缓存点击／Tick回调仍必须拒绝执行。

## Verification evidence

- 通过MCP for Unity 10.3.0操作Unity2022.3.62f3c1，场景Assets/Scenes/Level01.unity已保存，最终dirty=false、PlayMode=false；恢复750×1334预览。
- EditMode 13/13：job `8a543e97f0be49c691f1452f791454d4`。规则、出巢扣额、指定空槽校验通过。
- PlayMode 8/8：job `4067f50808bf4ec0bc5f3c5e9c90159a`。ClickHopsIntoFirstEmptySlotAndKeepsTheTile、RepeatedClicksDoNotActivateTheRearHiveOrAnotherColumn、RestartPauseAndBackgroundCancelAnUnfinishedHop、DisabledHopRejectsInputAndCanResumeWithANewClick、FullSlotsRejectAClickWithoutMovingOrConsumingQuota、BlenderHiveClickDepartureAndRestart、ImmediateSlotReuseKeepsBothExitOrigins、ClickingCarriesDeliversAndRestarts。
- Red→Green：替换交互前job `9b5f805b529240adb57a0079351d36aa` 有5项因缺失点击／跃动失败；禁用组件回调job `4ca2465f53b742a9bc1a94487035e48e`、动画按钮锁job `8e5f4ea70f0e476fa5f1ac68b421ca1e` 均先复现失败，再修复通过。
- Foundation validator最后运行Passed，0 warnings。最终Console 0 errors；编译域重载时MCP WebSocket有一条恢复连接的警告，未影响最终连接和测试。
- 场景读回：HiveHopController=1，5个QueueButton无IBeginDragHandler／IDragHandler；MissingScript=0；5个SlotTargets中心依次(0.110,0.220)、(0.305,0.220)、(0.500,0.220)、(0.695,0.220)、(0.890,0.220)。
- 材质直接引用原HiveSlotTile.png，alpha=true、maxTextureSize=512、mipmap=false、readable=false。图标材质／原三色蜂巢Prefab和背景GUID保留。
- 运行截图存于工作区outputs/level-one/hive-click-hop：HiveClick-idle-750x1334.png、HiveClick-hop-750x1334.png、HiveClick-landed-750x1334.png、HiveClick-landed-750x1625.png。截图采样暂停Editor，不改变产品动画。跃动中quota=108、Started=false、Flights=0；落稳后进入槽0、quota=105、Flights=3。
- 最终只读代码复核：已修复禁用组件仍接受缓存点击／Tick、动画时按下落地后误派后排两项问题；未发现重要遗留问题，尾批Source路径保持。
- 平台限制：尚未iPhone实机验证触控手感、进入后台及GPU帧率。Editor事件／模拟后台、组件禁用、重开、暂停已覆盖；不声明已完成iOS打包或性能验收。

## Changed files and assets

- ViewControllers/Harvest：新增HiveHopController.cs，修改HiveModelView.cs、LevelOneHud.cs、LevelOneController.cs；删除HiveDragController.cs、HiveDragHandle.cs。
- Editor：新增LevelOneHiveClickSetup.cs，修改LevelOneSceneSetup.cs，删除LevelOneHiveDragSetup.cs。
- Tests/Runtime：HiveClickTests.cs替代HiveDragTests.cs；修改HivePresentationTests.cs和LevelOneSceneTests.cs。原EditMode13项保留。
- Scene：Level01.unity移除旧6个拖动组件，新增1个Hop组件；保留原HUD、背景、标题、花朵和收集蜂巢的绑定。
- Art：复用用户图标派生的HiveSlotTile.png及材质，不新增美术；原始生成模式为内置imagegen透明背景编辑，prompt留存于工作区outputs/level-one/hive-drag-slots/HiveSlotTile-prompt.txt。


## 2026-10-21 点击跃动强化（已获用户确认）

### Goal and scope
所有95关沿用的点击入槽动画加强为“立即轻压→向上弹起→回弹落位”，用户确认总时长仍约0.34秒。只修改HiveModelView.RenderHop视觉曲线；HiveHopController原0.24秒Travel（含约0.045秒轻压）+0.10秒Landing时钟、Command提交、额度/飞行、输入锁、锁链和隐藏蜂巢规则保持。

### Presentation and lifecycle
点击合法可选蜂巢的第一帧即轻压；前0.045秒基本留在原地压缩，随后0.195秒弧线飞行并轻微拉伸，末0.10秒落地回弹。Quota标签仍随实际模型锚点移动。两端锁链使用实际根位置更新，整组同步；隐藏标记与灰色状态遵循既有规则。无效点击不启动；暂停/后台/Restart/禁用仍走既有Cancel复原，没有新增状态或协程。

### Ownership and resource impact
纯项目ViewController表现；无Model/System/Command、Foundation、持久化或加载路径变更。无新美术/材质/预制体/场景/Inspector修改；继续复用既有模型Transform、显示数字和缓存数组。只在活动跳跃中计算几项数学曲线，无新增每帧分配或依赖查找。原文2026-10-10资源/关卡范围是历史记录，本次适用于当前共享组件的全部95关。

### Acceptance and checks
- [x] 点击第一帧有可见轻压，前0.045秒不过早飞走；中段上弹拉伸，落地轻压回弹。
- [x] 总0.34秒内未提交、结束只提交一次；额度/蜜蜂/输入锁保持原规则。
- [x] 取消、暂停/后台/重开、链组同步及盲盒标记兼容。
- [x] 新测试先失败再通过；既有点击/链组/隐藏/相关生命周期回归、编译、原生预览、资产哈希与规范检查通过。

### Verification
- Red3/3:52ac3a1e771240d7a76c7361eac81a34，原点击第一帧比例1.0、20ms时反而拉伸1.039，证明即时轻压缺失。
- 初次相关PlayMode3796b79a2c4a48be86c4579cc4b94710：20/21通过。唯一失败是新测试冻结了LevelOneController，SendMessage后台恢复又启用了原指针屏障；测试显式驱动原LateUpdate释放输入后再点击，未改产品输入逻辑。
- 最终PlayModea5dc966b9b4c4b5f8a3984036781f2e3：21/21通过，涵盖新轻压/拉伸/落地/精确提交/取消及原点击、锁链、隐藏蜂巢。
- 完整EditModeae184edda8894d8e8a551e44b33f1b90：256/256通过。此曲线局部调整未重复运行全部PlayMode关卡加载测试，不声称全量181项PlayMode通过。
- 原生录制20帧、1080x1920：开始比例1.0，点击即0.941、20ms压缩0.829、120ms上弹1.178、280ms落地0.877，340ms恢复1.0并只入槽一次。340ms之前Started=false、额度40；340ms入槽后额度39。poses.json保留读回数据。
- 录制结束已注销临时Editor回调、恢复经济/偏好测试隔离、停止Play并返回MainMenu。GIF由原生PNG顺序编码，动作按20ms步长，总动作340ms；首尾增加观看停顿，不改游戏时长。
- scope.json逐文件核对：所有95关配置、美术/音频、旧场景/预制体/材质和其他脚本字节相同。既有Editor测试重建的三份UI文件通过Native FileUtil恢复。仅现有HiveModelView.cs的RenderHop曲线改变，新增3项测试及本记录。
- 独立只读复核未发现重要问题；曲线在45ms与240ms边界连续，原时间/标签/链组/盲盒路径保持。
- Project validator通过，0warnings。Console及最终原生预览检查保存在工作区outputs/hive-click-bounce。
- 性能：无新对象/数组/材质/资源/协程、无每帧架构查找，只在既有活动跳跃路径增加少量数学运算。未在iPhone实机评估触控手感或帧率；这是后续设备检查。
- 动图：/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/hive-click-bounce/蜂巢点击跃动.gif


## 2026-10-21 等待蜂巢原地跃动

### User steering and scope
用户补充“处于等待状态的蜂巢也加一下跃动效果，点一下会原地跃动”。沿用已确认的轻压/弹起/回弹风格：实际队首以外、尚不能激活的可见等待蜂巢点击只原地跃动约0.34秒，水平位置不变、最大抬起约屏高1.25%。普通前排和Pick Any有效选择仍按原规则入槽；不把工作蜂巢改为可点击，不改变满槽前排点击无效规则。

### Ownership, input and presentation
- HiveHopController区分合法激活与后排展示反馈，仍只合法落稳才发送Command；Tick驱动视图等待反馈。跳跃状态、队列、额度、槽位和道具不因等待反馈修改。
- HiveWaitingClickFeedback为运行时绑定的项目BeeHarvestController/IPointerClickHandler，仅在原Button不可交互、等待输入实际开放时转发点击。原Button.interactable及原有效入槽事件保留；后排仍非可选，仅可展示反馈。保持原Graphic射线和工具栏前后关系，禁止覆盖按钮穿透。
- LevelOneHud在现有15个列/行控件上幂等绑定反馈组件，复用Inspector控件和回调；不编辑Prefab/Scene。
- HiveModelView在缓存Visual中保存等待反馈时间/代次/行列/页面，按当前布局重画并让原数字、问号和可见锁链跟随。连点不叠加；可同时点不同等待蜂巢。开始真正入槽时先清理所有等待反馈；暂停/模态/后台/重开/翻页/队列移动/禁用/场景卸载取消，不重播。

### Resources and performance
无新美术、材质、字体或Resources键；仅运行时轻量输入组件随原HUD销毁，Visual复用无逐帧实例化或架构查找。只在等待跃动活跃时迭代缓存视图。其余95关数据/锁链拓扑、UI、音效、存档和Foundation均不改。

### Acceptance
- [x] 通过实际EventSystem点击禁用后排按钮，可原地压缩、上跳、落回；不入槽、不改配额/队列/库存。
- [x] 现有可选前排和Pick Any正常入槽，不重复转发；入槽时输入锁仍生效。
- [x] 第三排覆盖区域仍由工具栏接收，原非可选状态不变。
- [x] 暂停/模态/后台/重开/页面或队列位置变化/禁用/场景卸载清理，不延后重播。
- [x] 隐藏蜂巢保持灰色问号，无揭晓/道具消耗；锁链使用实际位置显示。
- [x] 编译、规则/输入/生命周期回归、原生预览、哈希及规范验证通过。

### Verification
- Red：PlayMode ff3fa15f5b64460e912b917abb694030，新增6项中4项按预期失败（后排无压缩/上跳）；其余2项为原Pick Any与场景退出路径。
- Green：新增6/6通过。最终相关PlayMode 9f4ce609880b4e638e5711aedb4b027d，26/26通过，覆盖真实EventSystem后排/第三排射线、入槽、Pick Any、暂停/重开/卸载、链组、盲盒和队列晋升。完整EditMode 3cb2fdb860b3472b9c0506d5aae3aa55，256/256通过；未重跑全部关卡PlayMode。
- 新脚本首次仅scripts刷新未导入新类，出现CS0246；force all导入后编译和Console均0 errors。没有遗留编译问题。
- 原生1080×1920录制20帧，动作340ms；后排id1额度始终80，Started始终false，X变化小于1px，最高抬起超过10px，末帧位置误差小于1px、比例回到1。数字跟随，画面无水平漂移。GIF由原生PNG编码，首尾仅增加观看停顿。
- 完成后移除Recorder回调、恢复原经济/偏好，退出Play并返回MainMenu。原生API恢复3份Editor测试重建的UI，删除本次测试遗留InitTestScene；scope.json确认既有Assets仅3份C#变化，95份关卡/美术/音频/场景/预制体均字节相同。
- 新增HiveWaitingClickFeedback.cs与WaitingHiveBounceTests.cs及Unity生成meta；修改HiveModelView.cs、HiveHopController.cs、LevelOneHud.cs。本次无Inspector或美术资源变更。
- 独立只读复核无重要问题；等待表现复用缓存、无逐帧实例化或架构查找。设备触控手感与GPU帧率未在iPhone实机验收。
- 最终Foundation validator Passed，0warnings；Editor MainMenu、非Play、非编译、Console0errors。证据路径：工作区outputs/waiting-hive-bounce/{focused,editmode,poses,scope}.json。
- 动图：/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/waiting-hive-bounce/等待蜂巢原地跃动.gif
