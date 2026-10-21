---
id: level-one-collector-layout
title: 第一关花朵与大蜂巢居中布局
status: Verified
owner: Project
created: 2026-10-10
updated: 2026-10-10
---

# 第一关花朵与大蜂巢居中布局

## Goal

把大蜂巢放在第三个工作槽位的正上方、花朵模型的正下方，花朵适当上移。

## User-visible behavior

- 花朵与大蜂巢沿第三槽位的画面中心线排列。
- 大蜂巢位于画面纵向31.5%处，保持原有大小与朝向；与花茎和槽位文字留出间距。
- 花朵Y坐标从6.33上移到7.05，大小与旋转方式保持现状。
- 蜜蜂携带方块飞往移动后的蜂巢入口；采集、交付、重开继续有效。

## Out of scope

修改模型、背景、标题、槽位、额度、蜂巢队列、关卡规则、其他页面和后续关卡。

## Ownership and architecture

| Responsibility | Owner | Planned type/path | Notes |
| --- | --- | --- | --- |
| State mutation | None | 无 | 既有Commands不变 |
| Observable or persistent state | None | 无 | 无新增Model或存档 |
| Shared project rules | None | 无 | 不改变采集规则 |
| One-shot notification | None | 无 | 不新增事件 |
| Unity presentation | ViewController | ViewControllers/Harvest/HarvestBoardView.cs | 初始化时调整两个模型位置，重新解析大蜂巢DepositPoint；继承BeeHarvestController |
| Adapter or repository | None | 无 | 沿用LevelOneAssets缓存 |
| Reusable infrastructure | None | 无 | 不修改Foundation |

## State and persistence

- Runtime state: 无新增业务状态，仅模型Transform和已存在的交付位置。
- Save schema/version/migration: None。
- State mutation path: 既有Command保持现状，视图不写Model。

## UI, prefab, scene, and Inspector impact

- Affected assets: HarvestBoardView.cs；Level01场景中的运行期Flower与Collector实例布局。
- Loading mode and final paths: 沿用Assets/Resources/Prefabs/Gameplay/Flower.prefab和Collector.prefab。
- Resources keys or direct reference owners: Prefabs/Gameplay/Flower、Prefabs/Gameplay/Collector，LevelOneAssets缓存加载，HarvestBoardView持有实例。
- Load/cache/instance cleanup responsibility: 仍由关卡场景管理实例；无新增加载，场景卸载时销毁。
- Hierarchy or binding changes: 不改变现有场景／Prefab绑定；模型在初始化时生成，位置由脚本持久化，不只移动临时运行态对象。
- Inspector confirmation: 验证花朵与蜂巢的屏幕中心、上下间距，蜂巢入口和Board.Deposit一致，五槽UI绑定完整。

## Integrations and failure behavior

- SDK/consent/lifecycle: None。
- Content update/offline/rollback: 无网络；本次脚本和场景已备份。
- Cancellation, timeout, retry: 当前生产场景必须包含MainCamera；交付布局平面不可与相机射线平行，否则初始化明确报错。

## Performance considerations

仅初始化时获取主相机并计算一次视口射线与Z=-0.45平面交点；不增加逐帧查找或分配。仍通过移动后的DepositPoint更新回程目标。

## Acceptance criteria

- [x] 大蜂巢中心与第三槽位中心水平对齐，位于花朵下方。
- [x] 9:16和长竖屏中花朵、大蜂巢和槽位没有遮挡，标题不受影响。
- [x] 蜂巢入口对应交付点，真实采集、交付和重开验证通过。
- [x] 停止并重新运行／重载关卡后布局保持。

## Verification matrix

| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | validate_game_foundation_project.py | 通过，0 warnings |
| Unity compilation | Error控制台 | 编译成功，最终0 errors、0 warnings |
| Focused tests | 既有ButtonsCarryDeliverAndRestart；低影响位置修改不新增实现镜像测试 | PlayMode 1/1通过，job 4bb41b1a6039463da307eb2b31b66570 |
| Runtime or asset behavior | 两比例截图、屏幕包围盒、中心线、入口读回、重启 | 两比例对齐无遮挡；重载运行布局保持；交付点误差0 |
| Performance | 初始化一次，无新增每帧计算 | 一个相机查找和射线／平面计算，仅初始化执行 |

## Definition of Done

- [x] Ownership and QFramework roles match implementation.
- [x] Public behavior and acceptance criteria implemented.
- [x] UI, scene and Inspector impact verified.
- [x] Convention validation and Unity compilation pass.
- [x] Focused tests and exact runtime checks pass.
- [x] Performance and platform limits reported.
- [x] Status updated to Verified after evidence exists.

## Decision log

- 2026-10-10: 按用户明确要求，采用“花朵—大蜂巢—第三槽位”居中布局；只移动模型，不修改玩法或比例。
- 2026-10-10: 当前第三槽位中心为视口X=0.5，Collector使用该中心线定位；通过相机射线计算位置，避免斜视角下仅设置世界X=0仍有视觉偏移。

## Verified evidence

仅修改HarvestBoardView.cs：花朵Y=7.05，Collector按照视口(0.5,0.315)与Z=-0.45平面的交点定位，缩放／旋转不变；交付目标从移动后的DepositPoint读取。未修改Prefab或模型、玩法、UI或Foundation包。现有Level01在运行时实例化模型，脚本位置配置会在每次初始化时应用。

9:16读回：第三槽位X=0.5，蜂巢X=0.499995768、Y=0.314999223。花朵屏幕包围盒Y=0.426967353..0.8537415，蜂巢Y=0.260236..0.36976248；两模型之间间距为视口高度的5.72%，蜂巢与槽位文字上沿间距2.52%。蜂巢入口与Board.Deposit误差0。长屏比例0.461538464时蜂巢X=0.499994844、Y=0.314999223，花朵X=0.5000001，未裁切／遮挡。重载关卡再次运行后蜂巢／花朵位置保持、交付点误差0。

ButtonsCarryDeliverAndRestart验证真实点击、取块、交付、旋转后的回程、458块完成和重开均通过。场景恢复为任务开始时的停止状态，Game View恢复为9:16。未进行本次请求范围外的iOS构建或真机测试。

预览：/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/level-one/collector-layout-change/preview.png，以及preview-long.png。原场景和脚本已备份，工作源码镜像同步更新。
