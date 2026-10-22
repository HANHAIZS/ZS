---
id: levels-one-hundred-seven-eleven
title: 新增第107–111关并沿用106关完整显示效果
status: Verified
owner: Project
created: 2026-10-22
updated: 2026-10-22
---

# 新增第107–111关并沿用106关完整显示效果

## Goal
追加5关，严格沿用第106关浅黑描边、7%方块间隙、4%模型整体放大、降低环境光、柔和投影与第7关采集缺口阴影。用户本轮再次明确不能漏掉整体放大和灯光处理。

## User-visible behavior
- 第107–111关依次麦克风(Microphone_2,2434块)、台灯(Lamp_3,2624)、骑乘式割草机(Mower_Ride,2678)、越野车(Car4x4,2949)、外星人(Alien,2963)。选用未使用、原始方块少于3000且现有颜色/玩法兼容的独立源模型，不删减源方块。
- 全部三列蜂巢、原5槽，离线surface-v1队列保留逐色额度与完整源坐标。沿用现有自动旋转/采集/道具/音乐/弹窗/结算/重开。
- 显示与106一致：边色RGBA(0.2,0.2,0.2,1)、边宽0.11；实例中心坐标不变、方块局部缩放0.93/0.975形成7%间隙；模型根0.86×1.04；环境光(0.48,0.52,0.63)、主光Soft/strength0.3/bias0.04/normalBias0.2。
- 模型采集缺口使用同一局部阴影材质/纹理缓存，暂停冻结、重开复用、卸载释放；货块保留浅黑边和原色/尺寸，移除模型空间阴影。
- 106→107→…→111，111胜利后返回Home并回到第1关；旧关内容/美术/UI不变。

## Out of scope
旧106关JSON/源Prefab/场景/美术/音频重写、新机关、新运行规则/存档/经济、Foundation、GitHub推送、截图录屏、iOS打包/真机性能承诺。

## Ownership and architecture
| Responsibility | Owner | Planned type/path | Notes |
| --- | --- | --- | --- |
| State mutation | Command | 既有ContinueLevelCommand/Harvest Commands | 不增加规则，按LastLevel边界 |
| Observable or persistent state | Model | 既有HarvestModel/MenuNavigationModel | 无schema变更 |
| Shared project rules | System | 既有HarvestSystem | 不改 |
| One-shot notification | Event | 既有事件 | 不增 |
| Unity presentation | ViewController | 既有HarvestBoardView/VoxelCavityVisuals | 扩展逐关参数范围，无新Update |
| Adapter or repository | Utility/Editor | LevelOneAssets、导入器、SourceLevelSceneSetup | 目录与106模板复用 |
| Reusable infrastructure | None | None | 不改Foundation |

## State and persistence
- Runtime state: 沿用模型/源资源缓存；逐色共享显示材质和一张局部阴影纹理。
- Save schema/version/migration: None；既有导航系统读取扩展后的末关。
- State mutation path: 既有Commands；导入和关卡回放仅Editor工具。

## UI, prefab, scene, and Inspector impact
- Affected assets: 新增Assets/Scenes/Level107..111.unity；BuildSettings增加5场景；只对新Prefab添加必要美术朝向。
- Loading mode and final paths: Assets/Resources/Levels/Level107..111.json，键Levels/Level107..111；Assets/Resources/Prefabs/Gameplay/{5 ModelKey}.prefab，键Prefabs/Gameplay/{ModelKey}。
- Resources keys or direct reference owners: 复用Materials/ChickVoxelOutline与Materials/Level07VoxelCavity；原始5份来源保留Assets/LevelSources/CubeLand，不运行加载。
- Load/cache/instance cleanup responsibility: 既有LevelOneAssets与Board，先释放cavity再edge；Prefab引用既有共享Voxel.asset与palette。
- Hierarchy or binding changes: 原生Editor API复制106场景设置关号/标题，保留所有既有引用。源Prefab不文本编辑。
- Inspector confirmation: 五新场景/标题/灯光/引用/BuildSettings，实例缩放/7%间隙/原色/浅黑边/真实货块/重开/清理。

## Integrations and failure behavior
- SDK/consent/lifecycle: 无新增。
- Content update/offline/rollback: 本地Resources，无新外部运行依赖；源SHA记录并可确定性重新导入。
- Cancellation, timeout, retry: 拒绝非法关号、重复或旧代次Continue；重开原配置并清理飞行；缺失模型/未知机关/额度异常仍明确报错。新5关必须通过正式Command完整通关回放，失败则改选兼容内容而非修改玩法。

## Performance considerations
沿用每原色共享材质、每场景一张局部缓存、单次移除最多27格更新；方块数2434–2963。不增加逐方块材质、每帧资源加载或新ForwardPass。软阴影增加GPU成本；iPhone未实测。

## Acceptance criteria
- [x] 5个未使用完整模型和确定性JSON、逐色额度守恒、三列；原106关内容保持。
- [x] 全5关按0.1s正式Commands完整通关并绑定生产JSON SHA。
- [x] 全5场景/Resources/标题/现有UI音频和独立货块/采集/重开正常。
- [x] 全5复用106缩放、7%间隙、浅边、环境光与柔和/局部阴影；暂停与清理正常。
- [x] 导航边界、编译、Python/Unity/规范检查、只读复核通过。

## Verification matrix
| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | validate_game_foundation_project.py | Passed，0 warnings |
| Unity compilation | C#/Shader无新增错误 | Passed，Console 0 errors，shaderMessages=[] |
| Focused tests | 5场景/样式/几何/导航/生命周期 | Passed，271 EditMode、11 PlayMode、26 Python |
| Runtime or asset behavior | 5正式Command完整通关、源SHA与旧范围哈希 | Passed，13,648块完整交付，旧Assets文件保持 |
| Performance | 共享材质/纹理，原生读回，不宣称真机帧率 | Passed，逐色共享/每场景一纹理/重开复用/卸载释放 |

## Definition of Done
- [x] Ownership and QFramework roles match implementation.
- [x] Acceptance criteria and failure/recovery checks pass.
- [x] Native assets and references checked with no Inspector todo.
- [x] Conventions, compilation and focused checks pass.
- [x] Performance limits and evidence recorded.
- [x] Verified only after evidence exists.

## Decision log
- 2026-10-22: 属于已有内容流程的追加，用户已明确沿用106完整显示方案，并再次强调4%整体放大与柔和/局部阴影；按现有Editor流程实施，不新增架构。
- 五模型来自已分析的兼容源目录，原始SHA见工作区work/levels107-111/selected.json。保持旧106关文件；仅目录/显示范围与最后一关推进边界扩展。

## Verification evidence — 2026-10-22
- RED EditMode05817446e3c94735aaf23548f294f973：6项中5项按预期失败（目录仍106、106仍回Home、107+越界）；RED PlayMode28376947aae74fc78268dc7b14c714d2按预期失败（Level107未加入构建）。
- 全量EditMode113bd7c09b4c40658c20ad89da30670f：271/271通过，覆盖新目录、逐色额度、106→107/110→111/111→Home1、重复和旧代次Continue、112拒绝、既有内容和样式范围。
- PlayMode1e1d1cd8138d4721b5428bf229538cdf：11/11通过。新测试内部遍历全部5关，核对每块源网格/原色/模型内中心/0.93÷0.975方块缩放、模型0.86×1.04、浅边0.2与宽0.11、全部灯光参数、共享材质/纹理、真实取块/隐藏/货块浅边与尺寸/交付、暂停冻结/重开复用/切关销毁，以及原106回归、原第7关GPU阴影和最后111胜利回Home按钮显示第1关。
- Python unittest discover Scripts/tests：26/26通过，111关可确定性重新生成，全关三列与旧盲盒范围不变，旧6/106内容仍完全相同。
- 正式Commands每步0.1秒、surface-v1 strategy10全5关赢得胜利，remaining=inTransit=0，deposited=total，activation order无遗漏或重复，报告SHA256与生产JSON匹配。步数分别7819、9000、7565、8547、9830；总13648块交付。报告位于工作区outputs/levels107-111/rules/rules-{107..111}-10.json，与content-audit.json/rule-decisions.json一致；脚本首调用清除旧报告并校验RPC、返回值、磁盘报告、SHA与全部蜂巢激活。
- 五份原始来源SHA与兼容目录一致；完整源坐标/颜色/深度集合和运行时逐块集合相等，未删减任何方块或合并颜色。
- 五个新Scene与106逐行对比仅LevelNumber和Level标题不同，所有旧UI/音效/弹窗/胜利/光照引用一致。原生读回5关：Soft/strength0.3/intensity1.5/bias0.04/normalBias0.2，ambient=(0.48,0.52,0.63)，浅边(0.2,0.2,0.2,1)，separation/cavity/edges=true，scene dirty=false。
- Native BuildSettings共112个启用且唯一路径（MainMenu+111关），无漏项或重复；软阴影能力为true，Shader支持且编译消息为空，最终Console0errors、非Play/非编译，编辑器打开Level107。未截图或录屏。
- 全量Editor测试会自动序列化MainMenu场景/页面/设置Popup；已从提交基线取回原始字节并经Unity原生FileUtil恢复。范围哈希最终确认全部旧非脚本Assets字节未改；旧文件改动仅4份C#（目录/生成器/旧边界测试）、4份Python（导入器/回归）、BuildSettings；新增44份Assets文件（5raw/JSON/Prefab/Scene各带meta与2测试带meta）。
- 独立只读复核未发现Critical/Important问题；确认所有新GUID引用有效、完整模型数和正式通关SHA正确。Foundation验证0warnings，源码/Shader/文档/脚本等空白检查通过，Unity原生序列化格式保留。
- 本地Git保存，未推送GitHub；未执行iPhone打包或真机性能测试。

## Final level list
| 关卡 | 主题 | 模型 | 方块 | 蜂巢 |
| --- | --- | --- | --- | --- |
| 107 | 麦克风 | Microphone_2 | 2434 | 95 |
| 108 | 台灯 | Lamp_3 | 2624 | 131 |
| 109 | 骑乘式割草机 | Mower_Ride | 2678 | 86 |
| 110 | 越野车 | Car4x4 | 2949 | 102 |
| 111 | 外星人 | Alien | 2963 | 106 |
