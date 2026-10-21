---
id: level-layout-v4
title: 两关三行蜂巢与顶部和底栏布局调整
status: Verified
owner: Project
created: 2026-10-14
updated: 2026-10-14
---

# 两关三行蜂巢与顶部和底栏布局调整

## Goal

第一关改为三行等待蜂巢；两关第三行通过功能按钮上半部分自然遮挡，蓝色底栏位于按钮下半部分。大蜂巢、设置入口位于顶部，花朵/苹果在大蜂巢与槽位之间垂直居中。

## User-visible behavior

- 第一关五列各三蜂巢；保留522个花朵方块及Red316、Yellow78、Green84、Orange44总额度。拆分为15个蜂巢，原前两行身份和颜色顺序保留。
- 常规选择列首蜂巢，后两行可预览；任意选择仍只放行一次后排选择。晋升复用原模型，Retry恢复完整三行。
- 两关统一槽位和三行中心间距，第三行下部由圆形功能按钮自然覆盖，蓝色背景上缘位于按钮中心附近并低于第三行下缘；数额可读。
- 大蜂巢单独固定到顶部，不跟随模型偏移。设置按钮回到安全区域右上角。模型可视包围盒中心位于大蜂巢下缘与槽位上缘的中间部分。

## Out of scope

第二关方块/颜色/额度、道具库存、金币体力、设置功能、通关递进、采集和飞行动画规则不改。无新素材、Shader、裁切蒙版或Foundation修改。

## Ownership and architecture

项目场景和Level01数据负责三行布局/额度；HarvestBoardView负责初始化时模型与大蜂巢展示位置。继续复用LevelOneHud、HiveModelView三行输入与模型复用逻辑；所有游玩状态仍由现有Command修改。无新角色类或注册。

## State and persistence

仅Level01静态蜂巢列表增加5项并重新分配各色总额度；源方块数组不变。无存档格式变化、无本机余额改写。常规列首/任意选择/消耗逻辑保持。

## UI, prefab, scene, and Inspector impact

原生修改Assets/Scenes/Level01.unity和Level02.unity。第一关新增5组ThirdLabels/ThirdButtons，复用既有HUD组件。每个功能按钮下方添加独立透明Graphic指针保护，道具不可用时覆盖区不会穿透到后方蜂巢；保护对象不位于Selectable下，不记录旧指针。功能按钮、角标、设置和槽位素材保持直接引用；原Resources键Levels/Level01、Levels/Level02、Prefabs/Gameplay/Flower、Apple、Collector均保持。模型实例仍由现有Board生命周期销毁，无额外每帧加载。

## Integrations and failure behavior

全部本地资源；后台/设置/Home暂停、Retry恢复、重复点击门控保持。三行额度匹配方块，避免蜂巢额度不足造成无法通关。部分遮挡区域由功能按钮接收指针。

## Performance considerations

第一关同时显示15个蜂巢，增加5个既有低复杂度模型和标签；每关增加4个静态透明指针Graphic，采集并发上限保持。模型中心计算只在初始化执行，复用已计算包围盒，无逐帧查找或新材质。

## Acceptance criteria

- [x] 第一关15蜂巢/三行/逐色522额度一致，列首和单次任意选择、晋升及Retry正确。
- [x] 两关第三行不被蓝底横切，功能按钮居底栏上缘，额度可读、覆盖区域输入正确。
- [x] 大蜂巢和设置顶部，模型位于大蜂巢与槽位之间并留有均衡空隙。
- [x] 编译、规范、玩法回归及目标竖屏实际截图通过。

## Verification matrix

| Boundary | Evidence required | Result |
| --- | --- | --- |
| 数据与三行交互 | 逐色额度、晋升/Retry/单次后排与通关测试 | 15蜂巢逐色守恒，78/78运行测试通过 |
| 布局与遮挡 | 实际模型投影、UI边界与指针、两关1080×1920截图 | 两关截图、运行投影和输入保护通过 |
| 回归 | EditMode/PlayMode既有套件 | 96/96 EditMode、78/78 PlayMode |
| 编译与规范 | Console及Foundation项目验证 | Console零错误、规范零警告 |

## Definition of Done

- [x] 原生绑定、每项验收证据和测试结果保存后Verified。
- [x] 报告改动、性能影响及设备验证范围。

## Decision log

- 2026-10-14: 按用户参考图修正之前蓝底遮挡和第二关顶部偏移；保留第一关内容和逐色总额度，拆分队列而不提高配色复杂度。

## Completion evidence

证据目录：/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/level-layout-v4/；verification.md记录实际坐标、完整测试、首次失败及审查结论。两关大蜂巢中心.94、设置中心.955，模型中心.66，槽位中心.3975。蓝底上缘.09775，低于第三行模型下缘.11125，按钮中心与底栏上缘对齐。新三行、额度、晋升/单次后排、无库存覆盖区指针、重试/暂停/递进/飞行全套回归通过。原生各5组第三排绑定正确，每关4保护Graphic，控制器enabled。

