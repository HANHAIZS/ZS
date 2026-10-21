---
id: level-one-core
title: 第一关 Blender 花朵核心采集
status: Verified
owner: Project
created: 2026-10-09
updated: 2026-10-09
---

# 第一关 Blender 花朵核心采集

## Goal

在 BeeHarvest 原项目路径中，按 Unity Game Foundation v0.6.0 的单一 QFramework Architecture，实现可完整通关和重开的第一关。Unity 编辑器操作仅使用 MCP for Unity 10.3.0。

## User-visible behavior

- 打开 Level01 并运行，直接进入关卡，显示 Blender 花朵、458块进度和五槽／五列队列。
- 花朵有粉324、黄84、绿50个独立方块。拖动花朵可绕中心旋转，旋转只改变展示。
- 队列为第一列粉108→粉108、第二列黄84、第三列绿50、第四列粉108、第五列空。点击当前列前端蜂巢进入第一个空槽，后排随即成为新前端；工作蜂巢不能撤回。
- 每蜂巢最多三只蜜蜂同时工作，每趟取一块同色、与外部空域连通的表层方块。预留中的方块仍阻挡内层，取走后才露出新表层。
- 取走时扣工作蜂巢额度，额度归零立即释放槽位；蜂携方块进入大蜂巢后才增加进度。最后一块交付后，关内文字显示采集完成。
- 重开本关恢复全部方块、队列和计数，并回收旧搬运任务。后台暂停不推进游戏数量和飞行进度。

## Out of scope

主页、商城、设置、图鉴、独立结算页面、后续关卡、体力、金币、提示、存档、倒计时、复活、广告、联网和 iOS 发行构建。关内控件仅用于核心玩法验证。

## Ownership and architecture

| Responsibility | Owner | Planned type/path | Notes |
| --- | --- | --- | --- |
| State mutation | Command | Commands/Harvest/HarvestCommands.cs | StartLevel、ActivateHive、AdvanceHarvest、PauseHarvest；视图不写状态 |
| Observable or persistent state | Model | Models/Harvest/HarvestModel.cs、LevelData.cs | 运行期方块、蜂巢、预留、飞行、槽位、数量和关卡结果；不持久化 |
| Shared project rules | System | Systems/Harvest/HarvestSystem.cs | 缓存 Model，队列前端与外域空域洪泛判定 |
| One-shot notification | Event | Events/Harvest/HarvestChangedEvent.cs | 取块与交付瞬时事件 |
| Unity presentation | ViewController | ViewControllers/Harvest/*.cs | 全部继承 BeeHarvestController；缓存依赖；HUD 发命令，视图呈现 Blender 模型 |
| Adapter or repository | Utility | Utilities/Harvest/LevelOneAssets.cs | Resources 加载与缓存；生成实例由视图管理 |
| Reusable infrastructure | Foundation | Packages/com.nn555k.gamefoundation.* | 使用原包，不为玩法修改公共包 |

## State and persistence

- Runtime state: Remaining + InTransit + Deposited = 458；同一方块只能有一项预留任务；蜂巢 Remaining 不为负；最后交付才 Won。
- Save schema/version/migration: None；重开和重新启动都创建本关新会话。
- State mutation path: UI / Controller → QFramework Command → Model；System 返回规则判断；ViewController 读取状态。

## UI, prefab, scene, and Inspector impact

- Affected assets: Assets/Scenes/Level01.unity；六个 Blender FBX；六个 Gameplay Prefab；关卡 JSON；URP Renderer/Pipeline/材质。
- Loading mode and final paths: Assets/Resources/Prefabs/Gameplay/{Flower,Bee,HivePink,HiveYellow,HiveGreen,Collector}.prefab；Assets/Resources/Levels/Level01.json。
- Resources keys or direct reference owners: Prefabs/Gameplay/{模型名}、Levels/Level01。Prefab 直接引用 Assets/Sprites/Gameplay/*.fbx 和 Materials/*.mat。
- Load/cache/instance cleanup responsibility: LevelOneAssets 缓存原型；Board 管理花朵／蜂巢；BeeFlightView 池化并重用蜜蜂。场景卸载销毁视图，HUD 释放按钮事件。
- Hierarchy or binding changes: LevelOneCamera、KeyLight、LevelOne（Controller/Board/BeeFlight）、CoreControls/SafeArea（UiSafeAreaFitter/HUD）、EventSystem。只一个生产场景。
- Inspector confirmation: 通过 MCP Editor API 创建场景和 Prefab，并读取绑定、模型尺寸、入口位置及 BuildSettings。FBX 保留原始单位转换，外层身份 Transform；禁止文本编辑序列化资产。

## Integrations and failure behavior

- SDK/consent/lifecycle: 无供应商 SDK；移动端 OnApplicationPause 经命令暂停与恢复。编辑器允许后台运行以支持 MCP。
- Content update/offline/rollback: Resources 本地加载；本关无网络请求。Foundation 热更新保留 NoOp，不运行下载流程。旧项目可由备份恢复。
- Cancellation, timeout, retry: 满槽或无前端时点击无副作用；工作无可达同色块时等待其他工作；无任务且无法继续时提示重开。不做复活。关卡数据身份／网格／额度不合法时拒绝启动，避免部分重置。

## Performance considerations

458个独立方块 Renderer；蜜蜂池最多15个对象，每只包含 Blender 多部件 Mesh。外域洪泛仅在方块取走后重算，资源和架构依赖不逐帧查找。已完成任务保留在本关列表，数量上限458；Tick/视图扫描成本有界。尚未在 iPhone 测量帧率、温度或功耗；不能据此声明移动端性能达标。

## Acceptance criteria

- [x] 直接运行 Level01；主体为 Blender 模型，五列仅能点前端，五槽按额度占用／释放。
- [x] 同色且外域连通才能预留；封闭空腔和预留阻挡不会提前开放。
- [x] 并发任务唯一，额度不超支，取块与交付分开计数，合计保持458。
- [x] 自动携带回巢；花朵旋转不改变已离开花朵的回程路径。
- [x] 458块交付才通关；重开恢复完整关卡且没有旧任务残留。
- [x] 暂停不推进；其他页面和后续关卡未实现。

## Verification matrix

| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | validate_game_foundation_project.py | 通过，0 warnings |
| Unity compilation | 无新编译错误 | 已通过；MCP error 控制台空 |
| Focused tests | EditMode 规则/资源；PlayMode 真实场景 | 全部 EditMode 46/46；第一关9项。PlayMode 场景1/1；见 Docs/LevelOneVerification.md |
| Runtime or asset behavior | 图片、状态读回、入口绑定 | 458方块正常尺寸；真实按钮取块／交付／重开通过；最终458已交付、0运输、Won=true；见验收记录 |
| Performance | 查找/分配/池化检查 | 无逐帧架构查找；最多15只；iPhone 尚待实测 |

## Definition of Done

- [x] Ownership and QFramework roles match the implemented files.
- [x] Public behavior and acceptance criteria are implemented.
- [x] Save, SDK, hot-update, UI, prefab, and Inspector impacts are handled where relevant.
- [x] Convention validation and Unity compilation pass.
- [x] Focused tests and exact behavior checks pass.
- [x] Performance risks and manual platform checks are reported.
- [x] Status is updated to Verified only after all required evidence exists.

## Decision log

- 2026-10-09: 用户收窄为第一关核心可玩，其他界面和后续关卡不做；使用 Blender 原始资产重新导出。
- 2026-10-09: 现有458块花朵简化为三色；配额粉108×3、黄84、绿50，教学关允许无道具通关。
- 2026-10-09: 对 FBX 单位问题加入尺寸回归；保留导入根节点并增加中心化外层。
- 2026-10-09: 独立审查后补充封闭空腔／预留阻挡和并发唯一性测试；修复回程起点随花朵旋转漂移。

## Completion evidence

完整结果、测试 job、失败复现、最终截图路径及真机边界见 Docs/LevelOneVerification.md。独立审查已处理三项 Important，状态字段封装的小项后续整理。
