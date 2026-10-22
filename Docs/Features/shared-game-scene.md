---
id: shared-game-scene
title: MainMenu与Game双场景数据驱动关卡
status: Verified
owner: Project
created: 2026-10-22
updated: 2026-10-22
---

# MainMenu与Game双场景数据驱动关卡

## Goal
用户批准仅保留MainMenu和Game两个场景，由关号加载现有114关原模型/JSON/显示配置。保留原有玩法、视觉和进入/退出/重开/结算流程；新增关卡不再复制Scene。

## User-visible behavior
- Assets/Scenes与BuildSettings最终仅MainMenu.unity和Game.unity；旧114个Level场景先验收后由Unity原生删除，可通过Git回退。
- MainMenu启动关卡和胜利下一关都进入Game，根据导航CurrentLevel创建该关模型/蜂巢；同名Game重载必须真正切换模型、标题、灯光与所有效果。
- 原有Resources/Levels/LevelXX.json及模型Prefab不改；新增Resources/Levels/PresentationCatalog.json保存114份模型键/边色开关/间隙/缺口阴影/源位交接/环境光/主光设置。4%整体放大、7%间隙等沿用既有Board实现。
- 第6/112–114关保留90ms真实源位交接；106和107–111保持原货块附着方式。边色、隐藏/锁链、额度、音频、UI、经济均保持。
- 原有游戏内Home页面缓存和返回/继续暂停语义保持；退出确认仍扣体力并重置该关，重开不跳错关，末关结算仍返回Home第1关。没有新增永久进度存档语义。
- 直接从Game进入Play时，Inspector PreviewLevel决定预览关号；从主页进入时导航所选关号优先，Inspector不覆盖。冷启动和旧导航尚未初始化时明确定义为关1或Inspector有效预览号，非法关号报错。

## Out of scope
不改变关卡几何/队列/配额、机关规则、描边风格、素材、UI布局、音频、奖励和存档schema；不改Foundation，不推GitHub、不截图录屏、不打包真机。

## Ownership and architecture
| Responsibility | Owner | Planned type/path | Notes |
| --- | --- | --- | --- |
| State mutation | Command | InitializeMenuNavigationCommand/原Harvest Commands | 导航初始化标记仅经Command写入 |
| Observable or persistent state | Model | MenuNavigationModel/LevelPresentationData | 初始化标记仅内存；展示数据可序列化 |
| Shared project rules | System | 既有HarvestSystem | 不改 |
| One-shot notification | Event | MenuNavigationChangedEvent等 | 不新增 |
| Unity presentation | ViewController | LevelOneController/MainMenuNavigationController/LevelPresentationView | 按导航加载Game，初始化标题和灯光 |
| Adapter or repository | Utility/Editor | LevelOneAssets/SharedGameSceneSetup/原生成工具 | 展示目录一次缓存，原生迁移/预览 |
| Reusable infrastructure | None | None | Foundation不变 |

## State and persistence
导航Initialized仅进程内；不增加PlayerPrefs/Save字段或版本。关号来源：已初始化导航优先，否则PreviewLevel；StartLevel/Retry/Leave/Continue继续通过QFramework Commands。

## UI, prefab, scene, and Inspector impact
- Game原生复制现有Level02骨架，完整保留相机/UI/音频/弹窗/Inspector引用，绑定LevelPresentationView.KeyLight与Title；PreviewLevel默认1。
- PresentationCatalog键Levels/PresentationCatalog，JSON通过Resources缓存；模型仍Prefabs/Gameplay/{原key}，关卡数据仍Levels/LevelXX；LevelKey明确为数据键，物理场景常量GameSceneName=Game。
- 场景重载释放旧Board/材质/纹理/飞行池/音频/监听，复用既有生命周期。数据缓存与原资源路径不迁移。
- 所有维护工具改为仅编辑Game，SourceLevelSceneSetup.Apply仅构建所选模型并预览，不再复制场景。旧初始脚手架禁止覆盖现有Game。
- 旧测试加载方式改为测试辅助器设置关号再加载真实Game；原测试关卡身份判断读取当前Controller，新增验收直接核对真实SceneManager场景和BuildSettings，不能仅通过伪造旧场景名称验收。

## Integrations and failure behavior
离线Resources；目录缺失、重复关号、不完整记录或模型键非法显式报错。导航与Inspector优先级、重复/旧代次Continue、同Scene下一关、暂停/应用前后台/退出重置、末关Home1、切换释放均需覆盖。场景选择器在加载期间保持输入阻挡。

## Performance considerations
运行时只加载一个Game骨架和所选模型，目录一次缓存；没有逐帧Resources查找、新增逐VoxelUpdate或逐Voxel材质。Game重载沿用原场景重载清理机制；不声称启动或真机帧率必然提高。

## Acceptance criteria
- [x] 114关原内容与原Prefab哈希不变，展示/光照配置逐关与迁移前一致。
- [x] 物理目录和BuildSettings只有MainMenu与Game，旧场景引用不残留于运行路径/维护工具。
- [x] 114关均通过同一个Game正确加载数据/模型/标题/配置且旧实例释放。
- [x] Home进入、同名Game下一关、重开/退出确认、末关Home1和冷启动/Preview优先级通过。
- [x] 原有蜂巢、搬运、材质/阴影/音频/UI焦点运行回归通过，无新增编译/规范错误。

## Verification matrix
| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | validate_game_foundation_project.py | Passed，0 warnings |
| Unity compilation | C#/shader/Console | Passed，无新增错误 |
| Focused tests | EditMode/PlayMode/Python | Passed，256 EditMode、214不同运行检查最终通过、27 Python |
| Runtime or asset behavior | 114关共用Game、菜单/继续/退出/重开/资源释放、源哈希 | Passed，114原Scene已移除，807份原内容资产不变 |
| Performance | 单次缓存、场景卸载与无资源泄漏检查 | Passed，旧模型/缓存释放；真机未测 |

## Definition of Done
- [x] 角色和Command所有权保持。
- [x] 原内容/显示效果/恢复路径验证通过。
- [x] 两场景与原生引用检查通过。
- [x] 规范/编译/针对性回归通过。
- [x] 独立复核与证据记录完成，真机限制说明。

## Decision log
- 2026-10-22: 用户在双场景方案后明确“那你改吧”，按已批准目标连续实施，不重复请求概念批准；使用当前Unity工作副本，工作前Git干净。原生修改Scene/Prefab，删除旧Scene在验收后执行。
- 迁移前文本只读比对114场景共3组，其中1关差异为等价对象ID/排布序列化，2–6/106–114组和7–105组只有环境光/阴影不同；原生核对相机/布局/119对象/三行HUD一致。


## Implementation and verification evidence — 2026-10-22
- 原生Game从Level02复制，差异仅LevelOne对象名、PreviewLevel1/标题、LevelPresentationView及Title/KeyLight绑定。所有原镜头/119对象/HUD三行/Inspector绑定保留。独立逐114对比PresentationCatalog与工作前旧Scene光照/阴影、旧选择器的边色/间隙/cavity/sourceHandoff，0差异。
- LevelKey只表示LevelXX资源身份，GameSceneName/GameScenePath是唯一物理游戏路线。MainMenuNavigationController在QFramework事件派发后下一帧加载Game；导航Initialized仅经InitializeMenuNavigationCommand写入，LevelOneController优先导航、否则PreviewLevel，并在Board建立前应用配置。既有Home缓存/暂停/离开重置语义不改。
- SourceLevelSceneSetup/各UI维护工具迁移为共享Game预览/单场景更新。原初始LevelOneSceneSetup禁止覆盖现有Game；SharedGameSceneSetup仅配置现有Game，不再依赖旧Level02模板。使用说明见Docs/SharedGameScene.md。
- 配置解析使用项目已安装的Unity官方Json.NET3.2.1，新增显式manifest依赖及Game/EditorTest预编译引用，防止依赖Editor插件的传递安装；锁文件仅depth1→0，无版本升级。验证所有必填字段/类型、RGBA/阴影0–1有限值、模型键、version/数量/关号及重复；全部成功后才发布缓存。运行时加载一次，编辑器导入/预览明确刷新。
- RED Edit58225abc6c404aa59ecf002b795b216d：2/2按预期失败（Game/catalog缺失，BuildSettings旧115项）；RED Play077e1c16148e452bb7e10d0ec63a26b0：预期物理Game未出现。新测试最初需额外等待Home35页面输入绑定，修正后按真实UI进入。
- c57e720945654c4e97a3e6f1daaab04b：4项3通过，已完整逐一加载全部114关至同一真实Game、核对模型/方块数/三列/标题/着色器/边色/整体缩放/环境光/主光、旧模型销毁；Home35和同名Game的1/6/111/113/114结算推进均通过。唯一失败为预览测试在LoadScene真正完成前改到了旧Controller，修正为sceneLoaded后、Start前设置Inspector值；生产预览逻辑不改。
- 全量BeeHarvest.PlayTests运行81627ca7b3ad4e8a86e7aac29cc3d1b0：214项中212通过，耗时799秒。只有MainMenuFlowTests.FirstLaunchHeldTouchCannotActivateNewLevelHive与VictorySettlementTests.HeldTouchAfterContinueCannotActivateAHive因sceneLoaded仍过滤旧Level01/Level02未注入手指而失败；过滤器已改真实Game。其余搬运/间隙/阴影/蜂巢/隐藏/锁链/音效/设置/商店/离开重开/暂停/前后台/触摸/UI回归通过，包含另一次114关和旧95关连续加载。
- 5d47bed97fae4902b2299b7cba77f508：14/14通过，复测上述两手指、冷Game预览106与导航6优先、主页35、1/6/111/113/114真实继续按钮、6关源位交接6项、新112–114效果/盲盒2项、107–111显示1项。至此214项不同运行检查均最终通过；没有把212/214的原始运行表述为一次全绿。
- 配置验证RED bec1f8b6a4ef4393845ac5dc4c908055：11/11按预期缺少验证API；GREEN4b0520da14b04edd8a6aecfa4ea40b65：11/11，覆盖缺ambientLight/sourceHandoff/edges/shadowBias/modelKey、负/超范围/NaN、重复关号、不安全路径、缺颜色通道/错误bool类型及完整合法目录。
- 通过验收后，Unity AssetDatabase.DeleteAsset移除114个旧Scene及114个meta；原生读回Scenes只有Game/MainMenu、BuildSettings仅MainMenu→Game两条且启用，Game无缺失脚本，PreviewLevel1/titleLevel1，非Play/非编译/dirty=false。独立GUID扫描没有其他资产依赖旧SceneGUID。
- 删除后最终全BeeHarvest.Tests EditMode cc04d62f0f3c46d5a7f4850d9148d79c：256/256通过，包含真实目录两Scene、双构建项、完整114配置、异常字段和原目录/规则/导航边界。Python完整Scripts/tests27/27，原114JSON确定性可重建；Foundation0warnings。
- 删除后a20bb27923fc4fa08298077baccce5ab：3/3，真实Home35→Game、冷Game106/导航6覆盖预览、同名下一关与末114→Home1→Game复测通过。
- 独立复核初见两触摸测试过滤器和严格配置验证缺口，均已处理；同一复核者确认无Critical/Important残留。当前原始114份JSON、所有Gameplay prefab和原始来源等807份内容资产SHA全部不变。
- 测试原生序列化变化MainMenuScene/Page/SettingsPopup及BeePalette14/HivePalette11已通过Unity FileUtil从工作前Git字节恢复。最终所有仍存在的旧非脚本/非asmdef Assets逐字节保持；唯一旧资源删除是228个Scene/meta。范围报告共享工作区outputs/two-scenes/scope.json。旧C#测试改动只为按关号加载真实Game和识别逻辑关号；新验收直接检查物理场景，未伪造Unity物理结果。
- 不截图录屏、不推GitHub，不新增Save schema或网络运行依赖；未进行iPhone打包/真机性能测试。只做本地Git保存。
