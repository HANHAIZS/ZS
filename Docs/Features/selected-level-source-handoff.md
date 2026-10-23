---
id: selected-level-source-handoff
title: 第1–6和96–114关完整视觉、布局与采集升级
status: Verified
owner: Project
created: 2026-10-23
updated: 2026-10-23
---

# 第1–6和96–114关完整视觉、布局与采集升级

## Goal
将115–119关完整视觉和紧凑布局方案应用于1–6、96–114共25关。用户再次澄清全部包含出巢节奏、阴影光照、浅黑边、间距和模型放大，同时要求接管90ms、拔出220ms。

## User-visible behavior
- 补充横向紧凑布局：仅这25关蜂巢列距.30→.24，五个槽位列距约.195→.16，整体居中，额外槽位与最左槽位对齐；保持蜂巢/槽位大小及点击区域。
- 槽位整体下移.048安全区高度，队列中心.274/.204/.134，行距.07，缩小槽位与队首距离。
- 逐关测量16个旋转方向，模型中心.64，安全放大上限1.28，宽/高模型按边界收敛；所有模型方块中心、完整模型与配色保留，方块间7%间隙。
- 25关统一RGB(.2,.2,.2)浅黑描边、环境光(.48,.52,.63)、柔和阴影强度.3和采集缺口局部阴影。模型数据、蜂巢顺序、额度、机关保持。
- 出巢.26s、停稳.18s、抓取.16s，各关原有采集目标分配策略保留；真实源位同帧接管、90ms平滑移向当前Grip，阴影衰减和固定长度爪子跟随；220ms拔出及后续Stage/交付规则保持。
- 暂停、加速、重试、旋转、对象池和视图重建复用既有实现。

## Out of scope
不修改25关以外配置、115–119的120ms/260ms参数、模型内容、蜂巢顺序、机关、预制体/源资源/材质/Shader/Scene/存档。原玩法Commands不改；横向列距由既有每关表现配置提供。不上传GitHub。

## Ownership and architecture
- LevelOneAssets读取既有目录，LevelOneController传既有sourceHandoff标志。
- BeeFlightView/HarvestBoardView复用已验证实现及资源缓存/actor池，无新玩法代码或依赖查找。
- LevelPresentationData新增可选queueColumnPitch/slotColumnPitch，LevelOneAssets校验安全范围，LevelOneHud消费每关列距；既有目录、Level JSON、导入规则与范围测试同步。无Command/Event/存档变化。

## UI, prefab, scene, and Inspector impact
既有Resources/Levels/PresentationCatalog.json的25个记录及对应Level JSON时序字段；仍由LevelOneAssets缓存并加载。既有Game场景、模型和材质不改，无新资源或绑定；测试后刷新静态目录缓存。保留用户未提交场景及材质改动。

## Failure and performance
复用既有暂停/加速、重试/actor池、视图重建/跨过拔出阶段保护。列距只在初始化/Generation变化时布局，缓存标量不添加逐帧分配。无新材质/纹理，仅活动cargo90ms使用既有PropertyBlock；所有资源由现有Board/BeeFlightView释放。

## Acceptance criteria
- [x] 25关全部实际运行验证真实源姿态首帧、同帧隐藏、30/60/90ms插值和材质恢复；已有阴影同步衰减。
- [x] 原90ms/220ms保持、出巢/停稳/抓取按配置减慢；原第6关暂停/旋转/池复用/腿长/首帧GPU回归通过。
- [x] 范围外关卡继续既有交接方式；7–95与115–119目录项保持，所有Level JSON剔除beeTiming后与基线相同，Prefab逐字节不变。
- [x] 25关实际运行验证浅黑边/照明/间隙、紧凑布局、16方向旋转安全边界。
- [x] Unity编译、针对性测试、项目规范和审查通过。

## Verification matrix
| Boundary | Evidence | Result |
| --- | --- | --- |
| Actual gameplay | 25关真实首帧和90ms/220ms，6关原行为回归 | Passed |
| Scope | 配置对比，Level JSON/Prefab哈希 | Passed |
| Conventions | validate_game_foundation_project.py | Passed |
| Compilation | Unity Console无新错误 | Passed |

## Definition of Done
- [x] 所有验收项有证据。
- [x] 保留既有资源、玩法和用户未提交文件。
- [x] 仅任务文件本地Git保存，记录验证和平台限制。

## Verification evidence (2026-10-23)
- 本次列距最终EditMode 18/18：`4d211010232a425db8e69bba3ecc1893`。非法范围/NaN/Infinity测试先出现6项预期失败，再通过。
- 本次最终PlayMode 4/4：`8cccaf3c0c9741ceb9cbbf1939c1639e`。逐关实际加载25关，检查列中心、原槽宽.17、点击边界、模型16方向边界、浅黑边/光照/源位90ms交接；实际跳跃、额外槽位、槽位间留隙、重试均通过。另检查115–119保持原行为。
- 完整方案此前EditMode39/39+7/7，PlayMode9/9；其余回归19项中18项首轮通过，最后一项颜色精度断言改为1e-5容差并连同同类测试2/2通过。原6关GPU首帧平均RGB差2.24e-6，软阴影334像素变化；第一关实际无道具收集1784/1784胜利。
- 范围核对：119关剔除指定25关时序字段后游戏数据一致；范围外94条表现配置一致；182个预制体逐字节一致。原机关/配色/额度/队列/目标分配策略保留。
- 规范检查0警告，差异空白检查通过。只读审查未发现阻塞问题。
- Python导入器既有全量27测试25通过、2失败：`test_level106_is_an_exact_level_six_variant`和`test_production_is_reproducible`。HEAD190c15e同样存在，源自历史手工关卡调整；本次不覆盖该既有内容。
- 平台限制：Unity Editor原生布局/渲染验证，未宣称手机真机触控体验验证。无新增截图/录像或Inspector绑定需求。
- 变更：PresentationCatalog/25关beeTiming、导入规则、LevelPresentationData/LevelOneAssets/LevelOneHud和范围测试。Game场景及源Prefab不改。列距只初始化/重试布局使用，无新逐帧分配或渲染资源。
