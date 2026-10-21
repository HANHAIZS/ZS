# 第一关验收记录

2026-10-09；生产工程：/Users/zhangshuai/BeeHarvest；Unity 2022.3.62f3c1；编辑器操作使用 MCP for Unity 10.3.0。

## 当前交付

直接运行 Assets/Scenes/Level01.unity。点击底部彩色数字按钮派出相应蜂巢；第一列有两个粉色蜂巢，前端激活后才能选择后排。工作区最多五个蜂巢，每蜂巢最多三只蜜蜂；拖动花朵可旋转观察。粉324＋黄84＋绿50，共458个独立可采方块。通关在关内显示文字；重开按钮恢复本关。

模型来自本次 Blender 输出：Flower、Bee、HivePink、HiveYellow、HiveGreen、Collector。FBX 的单位转换保留在内部导入根节点，外部中心化节点控制显示大小和旋转。主体未用 Unity 基础体替代。场景、Prefab、URP 材质均由 MCP 调用 Editor API 创建。

## 架构与旧项目

使用 nn555k/unity-game-foundation v0.6.0，提交 abcc15a1105fe68ec9a1f142db427a1a723de2fb；286个 Foundation 原仓库文件逐一对比，无修改。单一 BeeHarvestApp 注册游戏 Model/System/Utility，视图继承 BeeHarvestController；所有采集状态经 Command 修改。官方 onboarding-report：stage=verified、localReady=true，yooasset/uiProfile/compiledArchitecture 均 ready。

旧游戏 Assets/BeeHarvest、旧场景、模板等已通过 Unity MCP 移除。保留 Unity 工具与编辑器工程配置。恢复包：outputs/backups/BeeHarvest-before-foundation-20261009.tar.gz；SHA-256 为 ab8315fd1a696bdb585e9af590fb6117adc6ed1b8bef0672b6bf228eeb107105。本工程未初始化 Git，未创建提交或 PR。

## 自动验证

| 检查 | 最终结果 | MCP job / 证据 |
| --- | --- | --- |
| 全部 EditMode | 46/46 通过；其中第一关9项，框架37项 | cf489e8c937341888673d3fa09652093 |
| 真实场景 PlayMode | 1/1 通过 | 99d49751cc4a493d819c92525851cf9b |
| Foundation 规范 | 通过，0 warnings | python3 Scripts/validate_game_foundation_project.py --project . |
| Unity 编译/运行错误 | 最终 error 日志0项 | MCP read_console |

9个规则／资源测试：PickupReleasesSlotBeforeDelivery、ActualFlowerCanDeliverAll458、ManyTicksCannotDuplicateOrOverdraw、EnclosedCenterCannotBeReserved、FullSlotsContinueAndCannotSkipFront、PausedTaskRetainsCounts、FlowerPrefabKeepsWorldSizeWhenScaled、SealedCavityStaysClosedUntilCapPickup、ConcurrentHiveReservationsAreUnique。

PlayMode 的 ButtonsCarryDeliverAndRestart 加载生产场景并调用真实按钮事件；实际帧触发取块与交付，检查入口标记、458个渲染器、携带方块尺寸、旋转后的回程位置、通关计数、演员回收和重开。通关段使用显式推进加速验证全部458块，而非等待完整关卡时长。

## 实际状态读回

真实关卡按钮激活五蜂巢后：Remaining=443、InTransit=15、Deposited=0；各蜂巢额度105、105、81、47、105。最终通过明确步进完成：Remaining=0、InTransit=0、Deposited=458、Won=true、Deadlocked=false，活动蜜蜂0、可见方块0。实际重开按钮后：Remaining=458、InTransit=0、Deposited=0、Won=false，五槽均为空，队列前端恢复。

## 问题复现与修复

- 原始 FBX 的100倍根节点转换被运行时尺寸覆盖；尺寸测试先失败（花朵高度0.0593，预期大于5），包装修复后通过（实际高度约5.93）。
- 原回程每帧重取花朵上的位置，旋转70°使已脱离方块的蜜蜂漂移约2.165单位；回归测试确认旧逻辑失败，保存取块位置后通过。失败 job b16da5cff2b24877a0af027fcdd3cf5b；最终恢复并通过，不保留错误版本。
- 修正颜色材质与蜂巢入口方向；封闭空腔、预留阻挡和并发唯一性检查通过。最终画面仅保留核心操作控件。

## 独立审查及裁定

独立只读审查发现三项需要处理的事项：回程漂移、表层规则独立覆盖不足、Feature Spec 模板残留，均已处理。并发唯一性测试也已补充。

审查未判断的部分：模型尺寸／朝向／槽位绑定由本次 Editor 实测和截图验收；iOS 触控、字体、安全区、后台恢复及性能留作真机验证；测试和编译由本次 MCP 最终结果确认。裁定本阶段交付为编辑器可玩第一关，不声称完成 iPhone 发行或真机性能验收。

延后的小项：公开状态字段可进一步封装为只读属性；当前视图遵守 Command 写入边界，未发现视图修改状态。

## 性能与范围

方块458个独立 Renderer，最多15只池化蜜蜂；外域洪泛按 BoardRevision 缓存，未逐帧查找架构依赖。每只蜜蜂使用多部件 Blender 模型，移动端仍需检查 Draw Calls、帧率、发热和功耗。本关无网络调用与存档；资源全部本地加载。主界面、其他页面和后续关卡均不在此次制作范围。

## 预览与试玩状态

最终已恢复第一关初始状态，Unity 保持 Play，Game 视图选择 BeeHarvest Portrait（750×1334）。下方按钮可直接试玩。截图归档在本记录同目录：Level01-ready.png（初始）、Level01-harvest.png（携带中）、Level01-complete.png（完成）。这是关内核心验证画面，未制作其他页面。
