# 第一关核心玩法实施计划

执行方式：本会话直接实施，Unity 编辑器操作均通过 MCP for Unity 10.3.0。

目标：在 Foundation 原版架构上，打开 Level01 场景即可玩完 Blender 花朵采集。

约束：仅第一关；不制作其他页面或后续关卡；只一个 Architecture；业务写入经过 Command；生产资源经 Editor API 创建。

1. 官方 Foundation 接入并读回 localReady。使用 MCP 调用官方 Generate 与 Validate API，替代用户不要求的 BatchMode。
2. 在 Models/Harvest、Systems/Harvest、Commands/Harvest 建立最小接口；先运行 HarvestRulesTests，确认缺失取块、交付、外露和队列逻辑导致失败。
3. 实现 StartLevelCommand、ActivateHiveCommand、AdvanceHarvestCommand 及外域规则。测试 PickupReleasesSlotBeforeDelivery、ManyTicksCannotDuplicateOrOverdraw、EnclosedCenterCannotBeReserved、FullSlotsContinueAndPauseDoesNotAdvance 应全部通过。
4. Blender 导出 Flower/Bee/HivePink/HiveYellow/HiveGreen/Collector 六个 FBX，导出 Level01 JSON。保存每块 stable id 与网格坐标；粉324、黄84、绿50总计458。
5. 在 Utilities/Harvest 建资源仓库，ViewControllers/Harvest 建方块、蜜蜂与关内展示。Resources 加载 Prefabs/Gameplay 与 Levels/Level01，缓存引用，控制实例回收；翅膀和载块只影响展示。
6. 用 MCP 设置 ModelImporter、URP 材质、Gameplay Prefab、安全区及 Level01 场景，不文本编辑序列化资源。五个待选按钮只触发 ActivateHiveCommand。
7. 通过 MCP 运行 EditMode/PlayMode，实测激活、出蜂、取块与交付、归零释放、重开和通关；读取最终 Remaining=0、InTransit=0、Deposited=458、Won=true。
8. 跑官方 convention validator，读控制台编译错误，并保存实际关内截图与验收记录。未验证的 iPhone 构建不报告完成。

关注风险：FBX 坐标和尺寸、独立方块名称、排队蜂巢不能越过前端、重开时旧任务不能继续、失焦与暂停不能重复交付。
