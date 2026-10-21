---
id: level-one-hive-drag-slots
title: 拖动小蜂巢到指定槽位
status: Draft
owner: Project
created: 2026-10-10
updated: 2026-10-10
---

# 拖动小蜂巢到指定槽位

## Goal

玩家拖动待选的小蜂巢，放入五个槽中的任意空槽；以用户提供的白色圆角底座替代“空槽”文字。

## User-visible behavior

- 单击不再入槽。仅列前端蜂巢可拖动，真实模型和额度数字跟随手指／鼠标。
- 在任意空槽松手即进入该槽并启动蜜蜂；在槽外、占用槽松手返回原位，不改队列、额度或派遣状态。槽内蜂巢仍不可撤回或换槽。
- 五个底座中心沿用旧槽位中心，入槽模型高度保持原0.06视口高度，底座高度为其1.18倍；底座显示在模型之后，保留可见外沿。
- 空槽文字删除，占用槽显示蜂巢额度；额度继续在每只蜜蜂出巢时减1，归零释放槽，尾批继续回大蜂巢。
- 拖动期间不启动蜜蜂、不扣额度；暂停、重开、场景卸载／应用切后台取消拖动。只允许一个触点持有一次拖动。

## Out of scope

槽位交换／撤回、新关卡、主页、道具、经济、联网、换美术模型、改变数量和外域采集规则。

## Ownership and architecture

| Responsibility | Owner | Planned type/path | Notes |
| --- | --- | --- | --- |
| State mutation | Command | Commands/Harvest/HarvestCommands.cs | ActivateHiveCommand新增指定槽参数，校验前端和空槽后写入 |
| Observable or persistent state | Model | 现有HarvestModel | 队列/槽/额度不增加持久化字段 |
| Shared project rules | System | 现有HarvestSystem | 继续使用Front和可达采集规则 |
| One-shot notification | Event | 现有HarvestChangedEvent | 采集/交付事件不变 |
| Unity presentation | ViewController | ViewControllers/Harvest/HiveDragController.cs、HiveDragHandle.cs、HiveModelView.cs、LevelOneHud.cs、LevelOneController.cs | 本地拖动会话、输入转发、模型/数字/底座投影，不写业务模型 |
| Adapter or repository | Utility | 现有LevelOneAssets | 沿用三色FBX预制体缓存 |
| Reusable infrastructure | None | 无 | 不修改Foundation包 |

## State and persistence

- Runtime state: 一次拖动缓存蜂巢ID、列、指针ID和关卡Generation，仅ViewController持有；成功松手才发送带槽参数的Command。
- Save schema/version/migration: None。
- State mutation path: Pointer事件→HiveDragController→ActivateHiveCommand→HarvestModel。显式无效/被占槽不回退到别的空槽。
- 原单参数Command保留供既有规则调用；玩家按钮不再绑定该调用。

## UI, prefab, scene, and Inspector impact

- Affected assets: Level01场景添加HiveDragController；五列Button各添加HiveDragHandle；HUD添加五个稳定RectTransform目标区域。保持已有Inspector绑定。
- Loading mode and final paths: 透明底座PNG直接引用，Assets/Sprites/UI/Gameplay/HiveSlotTile.png；材质Assets/Sprites/UI/Gameplay/HiveSlotTile.mat，场景HiveModelView直接持有材质，材质持有PNG。Resources小蜂巢路径不变。
- Resources keys or direct reference owners: HiveModelView.SlotMaterial直接引用；无新增Resources键。
- Load/cache/instance cleanup responsibility: 初始化缓存依赖与引用，创建5个共用材质的平面；拖动复用原蜂巢，不新增拖动克隆；场景卸载销毁实例。
- Hierarchy or binding changes: HiveSlotTarget_0..4为稳定HUD放置区域；HiveSlotTile_0..4为运行期背景平面，位于模型之后；SlotLabels空槽为空字符串。
- Inspector confirmation: 位置不变、图标略大于蜂巢且不遮挡模型、数字/命中区域对齐、5槽均可放置、两种竖屏。

## Integrations and failure behavior

- SDK/consent/lifecycle: 无新增SDK；后台／暂停取消拖动。
- Content update/offline/rollback: 全部离线资源；脚本/场景已备份，保留旧资源GUID。
- Cancellation, timeout, retry: 无效放置恢复同一模型/数字，不扣额度；暂停／重开／列前端改变后旧指针结束不得启动新蜂巢；多指输入不抢夺拖动。

## Performance considerations

新增5个共用材质的小平面，不增加蜜蜂或蜂巢数量。拖动只移动已有模型及数字；单次命中最多检查5个缓存RectTransform，无逐回调架构查找或资源加载。不声明iPhone实机性能已验证。

## Acceptance criteria

- [ ] 单击无效，拖动到五个任意空槽可以选择准确槽位并启动采集。
- [ ] 图标替代空槽文字，中心不变、略大于蜂巢，放入模型覆盖图标。
- [ ] 槽外／占用槽／暂停／重开／多指取消不改变额度、队列和槽，不自动放到其他位置。
- [ ] 出巢扣额、尾批来源、回巢交付、重开与458块通关不回归。

## Verification matrix

| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | validate_game_foundation_project.py | Pending |
| Unity compilation | 无新增编译错误 | Pending |
| Focused tests | 指定槽Command、Pointer拖动、失败/取消；既有规则/展示/通关 | Pending |
| Runtime or asset behavior | 透明PNG/引用/位置/尺寸/覆盖；任意槽与两种竖屏截图 | Pending |
| Performance | 5平面、原蜂巢复用、缓存依赖 | Pending |

## Definition of Done

- [ ] Ownership and QFramework roles match implementation.
- [ ] Public behavior and acceptance criteria implemented.
- [ ] UI, scene and Inspector impact verified through Editor APIs.
- [ ] Convention validation and Unity compilation pass.
- [ ] Focused tests and exact runtime checks pass.
- [ ] Review and platform limits recorded.
- [ ] Status updated to Verified after evidence exists.

## Decision log

- 2026-10-10: 用户明确选择拖动放置任意空槽，覆盖旧点击自动归位交互；槽内不可撤回规则保持。
- 2026-10-10: 使用imagegen内置工具从用户图片提取透明底座，保留白色圆角/灰色边沿/软阴影；生成prompt保存在工作区，原图不改。

- 2026-10-10: 用户已取消拖动要求，本输入功能被level-one-hive-click-hop替代；保留白色底座。拖动测试最终13 EditMode及8 PlayMode通过，但不会作为最终交互交付。
