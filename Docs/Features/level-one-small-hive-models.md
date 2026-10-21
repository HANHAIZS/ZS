---
id: level-one-small-hive-models
title: 小蜂巢模型与出巢额度
status: Verified
owner: Project
created: 2026-10-10
updated: 2026-10-10
---

# 小蜂巢模型与出巢额度

## Goal

使用原有Blender小蜂巢模型代替额度色块；每只蜜蜂出巢时消耗1额度，数字同步减少。

## User-visible behavior

- 待选列显示对应颜色的小蜂巢模型，额度数字显示在蜂巢正面；原有色块不再绘制，点击区域保留。
- 点击列前端蜂巢后，模型显示在工作槽中，后排成为新前端；仍为五列／五槽、不能撤回。
- 蜜蜂从实际小蜂巢出口飞出，每启动一趟扣1额度；取到方块不会重复扣额度。每蜂巢仍最多3趟同时工作。
- 剩余额度为0时立即释放槽位；正在飞出的蜜蜂继续取块并回大蜂巢，交付才计入已交付数量。
- 没有可达同色方块时等待，不启动蜜蜂，不消耗额度。暂停、重开和最终458块交付保持正确。
- 花朵、大蜂巢、背景和Level 1标题保持当前布局。

## Out of scope

新模型、其他关卡／页面、额外道具、币值／体力、广告、联网、改变每蜂巢并发数或方块外域可达判定。

## Ownership and architecture

| Responsibility | Owner | Planned type/path | Notes |
| --- | --- | --- | --- |
| State mutation | Command | Commands/Harvest/HarvestCommands.cs | 在启动运输任务时扣额度和释放槽位；取块仅改变方块与运输计数 |
| Observable or persistent state | Model | Models/Harvest/LevelData.cs现有HiveState | Remaining变为尚未派遣额度；Reserved为尚未取块的出巢任务数；无需新存档 |
| Shared project rules | System | 现有HarvestSystem | 可达判定和队列前端不变 |
| One-shot notification | Event | 现有HarvestChangedEvent | 保持取块／交付事件 |
| Unity presentation | ViewController | ViewControllers/Harvest/HiveModelView.cs、HarvestBoardView.cs、BeeFlightView.cs、LevelOneHud.cs、LevelOneController.cs | 项目拥有模型实例、数字与出巢口绑定；依赖初始化缓存，不写业务状态 |
| Adapter or repository | Utility | 现有LevelOneAssets | 沿用Resources原型缓存 |
| Reusable infrastructure | None | 无 | 不修改Foundation包 |

## State and persistence

- Runtime state: 每蜂巢Remaining + 已启动趟数 = 初始额度；Reserved <= Busy <= 3；Remaining不为负；全局Remaining + InTransit + Deposited = 458。
- Save schema/version/migration: None。
- State mutation path: UI／Controller→Command→Model，View只读取Model。最后出巢释放槽位后，旧任务取块不可清空新占用者。

## UI, prefab, scene, and Inspector impact

- Affected assets: Level01添加HiveModelView组件，按钮Image透明、额度字体加粗并描边；场景重建工具同步。
- Loading mode and final paths: 沿用Assets/Resources/Prefabs/Gameplay/HivePink.prefab、HiveYellow.prefab、HiveGreen.prefab，直接引用既有Assets/Sprites/Gameplay三色FBX和材质；不新增贴图／模型资源。
- Resources keys or direct reference owners: Prefabs/Gameplay/HivePink、HiveYellow、HiveGreen，LevelOneAssets缓存原型，HiveModelView按蜂巢ID持有实例。
- Load/cache/instance cleanup responsibility: 每蜂巢实例至多创建一次；在待选和工作位置之间复用，额度耗尽隐藏，重开复用，卸载场景时清理。
- Hierarchy or binding changes: LevelOne/HiveModelView；运行期SmallHive_<id>；原五列Button和五槽／额度Label绑定保留。粉／黄数字跟随Count_Plaque；绿色原模型无数字牌，使用相同模型正面坐标(0, 0.22, 0.37)。出巢读取SpawnPoint并按蜂巢ID缓存；去程随视口重新投影，返程取块位置固定。槽中心独立缓存，不依赖已移动的数字标签。
- Inspector confirmation: 三色模型、透明按钮仍可射线点击、工作槽数字、出口绑定、竖屏与长屏无重叠，保持现有大蜂巢居中和标题配置。

## Integrations and failure behavior

- SDK/consent/lifecycle: 无新增SDK；暂停不启动或推进任务。
- Content update/offline/rollback: 全部本地资源；脚本与场景修改前备份。
- Cancellation, timeout, retry: 无可达目标时不扣额度；最后出巢后的任务独立完成；缺少SpawnPoint／相机时明确报错，Controller初始化失败时不继续推进；绿色缺少数字牌使用明确正面锚点。

## Performance considerations

第一关实际5个小蜂巢，粉／黄各8个Renderer、绿7个，共39个Renderer，按前端／工作状态隐藏复用。位置和尺寸只在状态／视口尺寸变化时重新布局；没有逐帧架构查找或加载。每蜂巢最多3只蜜蜂，包含已释放槽位的尾批任务时本关最多15只；不在本次任务声明iPhone性能达标。

## Acceptance criteria

- [x] 额度色块由原有三色Blender模型替代，数字可读，透明按钮可点击。
- [x] 出巢前后额度与趟数对应，取块不重复扣，无目标不扣，额度不会负数。
- [x] 最后出巢即释放槽位，旧任务取块不影响同槽新蜂巢。
- [x] 实际出口、数字和模型对齐；两种竖屏、暂停、重开、458块交付验证通过。

## Verification matrix

| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | validate_game_foundation_project.py | 通过，0 warnings |
| Unity compilation | 无新增脚本错误／警告 | 通过；截图时出现2条Metal memoryless depth提示，无运行错误 |
| Focused tests | 出巢扣额、旧任务／槽复用Red→Green；完整HarvestRules与Scene用例 | EditMode 11/11；PlayMode 3/3（最终任务250c76260bda428282ffc3e2a3ecd605） |
| Runtime or asset behavior | 模型／数字／实际入口／点击射线；静止和出巢截图；长屏和重载 | 750×1334、750×1625；额度108→105，取块前仍458；长屏数字误差0.000031px、出口误差0；重载后组件/透明按钮保留 |
| Performance | 模型复用、实际实例数量、依赖缓存 | 原模型复用，5实例/39Renderer，重开保留实例ID；没有iPhone实机性能结论 |

## Definition of Done

- [x] Ownership and QFramework roles match implementation.
- [x] Public behavior and acceptance criteria implemented.
- [x] UI, scene and Inspector impact verified through Editor APIs.
- [x] Convention validation and Unity compilation pass.
- [x] Focused tests and exact runtime checks pass.
- [x] Review findings handled; performance and platform limits recorded.
- [x] Status updated to Verified after evidence exists.

## Decision log

- 2026-10-10: 用户将原先“取块扣额度”调整为“蜜蜂出巢扣额度”；以本次指示为准，保留交付计数和外域可达规则。
- 2026-10-10: 用真实Blender网格替换色块，数字保留UI绘制并绑定模型计数牌；透明按钮负责点击。

## Verification evidence

- 出巢扣额Red: f8367aa3a07642688b50edc0233e1a4f，2项因旧版取块扣额失败；规则Green: afbd10c725644092b2baf5b217df4b7d，11/11。
- 模型展示Red: 9aa2b11117814900ab75d29051101e30，旧版无SmallHive_0；最终PlayMode: 250c76260bda428282ffc3e2a3ecd605，3/3，包括真实透明按钮点击、出口、数字、快速复用、视口变化、重开和458块交付。
- 视口边界Red: 5865b08e54ac44c88a0e200b635d9fac，旧尾批Source偏移0.5045；修复后随视口重投影，通过最终PlayMode。
- 审查P2已修复：稳定槽中心替代数字标签作为位置锚点；测试fixture保留完整颜色额度/模型索引。最后复核无新增问题。
- 预览位于本工作区outputs/level-one/small-hive-models，场景备份Level01-before-small-hives.unity。Editor最后恢复停止运行、Level01、750×1334。
