---
id: levels-sixteen-ninety-five
title: 新增80个不同模型关卡（第16至95关）
status: Verified
owner: Project
created: 2026-10-20
updated: 2026-10-20
---

# 新增80个不同模型关卡

## Goal
在现有15关后追加80关。用户明确选择“放宽块数上限，优先使用80个不同模型”；优先较小且清楚的主题，不以同一模型变色凑数。

## User-visible behavior
第16–95关使用80份不同形状的原始模型，最终996–3627块、动物/食物/载具/建筑/日用品等。保留来源坐标、颜色身份、每色总额度、2–5列与5槽。已找到完整通关路径的来源队列继续保留；其余新关卡按 surface-v1 编排自上而下的颜色层队列，单巢1–80额度、独立蜂巢，并稳定重排逻辑id（voxel名称和原source字段不变）。15→16，后续逐关递进，95胜利后返回Home并从1开始；退出与Retry重置当前关。沿用现有自动旋转、音乐、道具、暂停/退出弹窗、胜利特效/音效和白色60。

## Out of scope
更改前15关、修改UI设计、加入未支持机关、经济/道具规则变更、永久解锁存档、Foundation或新资源调度体系、发布与真机性能承诺。

## Ownership and architecture
LevelOneAssets显式关卡目录扩展95，现有ContinueLevelCommand按LastLevel推进；不新增架构、Models或Systems。Python离线适配保留来源校验与原15关转换，显式扩充目录，并为49关应用可复现的surface-v1队列编排。Editor复用CubeLandLevelSetup与SourceLevelSceneSetup原生资产/场景创建，批量执行与验证工具属于Editor。CubeLandPalette补16浅紫和18深红，颜色沿用原Icon采样报告，不混淆身份。

## State and persistence
当前关号仍为会话状态。游戏写操作继续使用现有Commands。无存档schema、SDK、网络或热更新变化；验收在隔离经济/偏好下执行，原始源资料只读。

## UI, prefab, scene, and Inspector impact
- 新Assets/Scenes/Level16.unity..Level95.unity，原生加入BuildSettings。
- 新Assets/Resources/Levels/Level16.json..Level95.json，键Levels/Level16..95。
- 新Resources/Prefabs/Gameplay/{80个ModelKey}.prefab，键Prefabs/Gameplay/ModelKey；原始JSON在Assets/LevelSources/CubeLand/同名.json。
- 新Palette16/18共享Voxel/Bee/Hive材质与蜂/巢Prefab；共享现有Voxel网格，不重写原颜色。
- 新场景复制当前Level02结构，仅绑定自身LevelNumber、标题和模型；原胜利Prefab直接引用随当前版本继承。24个动物/人物/带正面细节的新增模型使用原生Orientation子节点绕Y轴180°，其余保持来源朝向；更新Editor生成器使重建一致，手动/自动旋转接口不变。
- 资源由现有LevelOneAssets加载/缓存，场景实例随场景销毁。禁止文本改prefab/unity/asset，保留原场景和Inspector绑定。

## Integrations and failure behavior
拒绝未知机关、重复坐标、颜色额度不符、连接超5或同列连接/循环依赖等当前无法处理的来源。来源队列未找到通关路径时，先为同一模型离线编排 surface-v1 队列并复验；仍未通过则换用另一兼容模型，直到80份验证完成；不修改玩家规则来让测试通过。末关不加载96；重复/旧代次Continue不推进。无离线外部依赖。

## Performance considerations
优先小模型而非无限放大；80关总量约20万块，单场仍只实例化一个模型，继续共享材质/网格与既有飞行池/音效池，不增加每帧资源/依赖查询。编辑器验证分批处理，避免单次工具超时。已有资源缓存可能保留访问过的模型，设备内存/加载耗时另测。

## Acceptance criteria
- [x] 80份来源SHA、颜色/坐标/每色总额度精确保留；来源队列与离线编排队列区别记录，模型形状互不重复。
- [x] 80关规则通关验证通过，实际场景资源/标题/队列/5槽/UI/音频/胜利绑定正确。
- [x] 15→16→..95→Home→1，重复/代次、Home/Retry/暂停正常。
- [x] 原15关和既有UI/音频资源不变，编译/规范/完整测试通过。
- [x] 80模型预览清楚，引用和剩余设备验证范围记录完整。

## Verification matrix
| Boundary | Evidence required | Result |
| --- | --- | --- |
| Source/content | SHA、可复现导入、独立形状/额度/连接检查 | 80份输入SHA匹配生产JSON；14项Python通过 |
| Gameplay | 全80关正式Commands、0.1秒步长、可见面采集和完整交付（场景实际搬运另测） | 80/80完整逻辑交付通过；6代表新关卡真实飞行/重试通过 |
| Native scenes | 80场景、Resources、BuildSettings、标题和绑定读回 | 全80场景PlayMode加载与绑定检查通过 |
| Lifecycle/navigation | 15/16/94/95边界、换关/退出/重置/后台 | 254项EditMode、167项PlayMode均通过；6代表新关卡实际搬运与Retry |
| Existing regression | Python、EditMode、PlayMode、编译/Console/规范 | Python14、EditMode254、PlayMode167通过；规范0警告；最终Console错误/警告0 |
| Visual/performance | 原生预览、共享资源/循环查找审查，真机范围说明 | 80份Unity原生模型渲染已检查，24份正反面比较并调整；真机性能未测、在范围外 |

## Definition of Done
- [x] 项目所有权、资源生命周期和Inspector绑定正确。
- [x] 80关及原有游戏的验收证据完整。
- [x] 通过校验和测试后才标记Verified；无未报告失败。

## Decision log
- 2026-10-20: 用户新增80关；发现2000块内仅27个未用兼容模型，用户选择放宽上限以优先80种不同模型。
- 2026-10-20: 初选80份996–3627块独立模型，排除同列连接等当前玩法冲突；最终目录以通过规则验证和原生画面检查的名单为准。

- 2026-10-20: 初始80份来源队列经多策略仅37份找到通关路径；为剩余模型离线编排层序队列，保留模型形状而不通过修改运行时规则迁就来源队列。旧步长0.5/2秒的结果只作选材线索，最终验证必须以0.1秒重跑并绑定生产JSON SHA256。

## Final implementation snapshot
- 第16–95关：80个独立模型，共205,926块，996–3627块/关。31关来源队列，49关surface-v1；生产JSON的SHA256逐一对应0.1f秒正式Commands完整通关记录。
- 80关总6,424个蜂巢，单关最多199；单场仅显示原HUD的当前队列行/槽位，视觉实例仍沿用原生命周期。
- 全量EditMode 254/254、Python 14/14和Foundation校验0警告已通过；全量PlayMode 167/167亦通过；80份原生预览完成，最终Console错误/警告0；朝向调整后新增PlayMode边界3/3复验通过。

- 2026-10-20: 原生正反面预览发现24个模型从背面展示；仅调整新增模型美术子节点起始朝向，使脸/面板/果核可见。模型数据及逻辑通关证明的输入SHA不变；朝向调整后复跑新增场景/搬运边界。

## Evidence and delivery
- 详细验收：/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/levels16-95/验收报告.md。
- 生产数据/证明SHA对应：同目录 production-manifest.json、final-content-audit.json、final-native-audit.json、final-baseline-diff.json。
- 80份完整通关记录：同目录 real-rules-source/ 与 real-rules-surface-v1/；每份包含输入SHA256、0.1f秒步长、deposited=total、remaining/inTransit=0。
- 全量EditMode 254/254，PlayMode 167/167（381.99秒），Python14/14；24模型仅改美术朝向后，新增场景/实际搬运/重试/15→16边界3/3再通过。
- 原生模型图：同目录 native-models/（80份）及新增80关模型总览.png；新增80关清单.csv列出各关模型、方块/蜂巢/列数及队列设计。
- 原15关和既有UI/音频资源逐文件SHA比对保持不变，测试写入的UI/材质恢复原字节。最终回到MainMenu，Editor停止Play Mode。
- 无需新增Inspector手工绑定。尚未执行真机性能和95关连续人工通关；不将逻辑回放等同于80关完整场景飞行人工游玩。
