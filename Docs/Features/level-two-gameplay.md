---
id: level-two-gameplay
title: 苹果第二关与胜利后进入下一关
status: Verified
owner: Project
created: 2026-10-13
updated: 2026-10-13
---

# 苹果第二关与胜利后进入下一关

## Goal
接入已完成的苹果模型与规划型蜂巢数据，第二关沿用第一关功能；第一关胜利Continue进入第二关，第二关胜利Continue返回主界面。

## User-visible behavior
- 首次Start进入第一关。Level1胜利Continue进入苹果Level2；连续点击只递进一次。
- 第二关五列十八蜂巢，显示前两整排，第三排下半部分延伸到道具栏后，被道具按钮与底栏遮挡；常规只能选列前端，任意选择道具只放行一次。后续蜂巢可通过现有任意选择分页查看。
- 蜂巢额度逐色总和等于1048个方块；保持规划型难度数据，不改任何配色/采集规则。
- Retry重开当前关卡，库存保留；Home后Start恢复当前关卡进度。
- Level2胜利后Continue返回Home，下一次Start从第一关开始新循环；未制作第三关。

## Out of scope
关卡选择/解锁存档、Gallery功能改变、第三关、新道具、新色板、Foundation修改、重新建模、第一关布局/数据调整。

## Ownership and architecture
| Responsibility | Owner | Type/path | Notes |
| --- | --- | --- | --- |
| State mutation | Command | Commands/Navigation/MenuNavigationCommands.cs | Continue验证胜利、代次和关卡，写导航状态 |
| Observable state | Model | Models/Navigation/MenuNavigationModel.cs | 当前关卡仅会话，不新增存档 |
| Rules | Existing System | HarvestSystem | 采集/道具规则保持 |
| Event | Existing Event | MenuNavigationChangedEvent | 通知现有导航 |
| Presentation | ViewController | LevelOneController/HarvestBoardView/LevelOneHud/HiveModelView/MainMenuNavigationController/VictoryPopupController | 缓存依赖，复用现有绑定与组件 |
| Repository | Utility | LevelOneAssets | 按关卡缓存数据、提供固定关卡资源与场景目录 |
| Infrastructure | None | Foundation unchanged | 项目关卡策略 |

## State and persistence
- 当前关卡由InitializeMenuNavigationCommand/ContinueLevelCommand修改。
- StartLevelCommand重置本关状态而保留库存；设置/后台/Home继续暂停游玩时钟。
- Save schema/version/migration: None。

## UI, prefab, scene, and Inspector impact
- Assets/Scenes/Level02.unity由原生API复制第一关，Controller.LevelNumber=2；原绑定复用。
- 现有Assets/Resources/Prefabs/Gameplay/Apple.prefab，键Prefabs/Gameplay/Apple；Assets/Resources/Levels/Level02.json，键Levels/Level02。
- LevelOneAssets缓存资源/数据，场景销毁释放实例；不新增图像，不移动旧资源。
- Level02增加ThirdLabels/ThirdButtons五组，沿用完整蜂巢与原材质；第三排中心为安全区域高度14.5%，额度在露出部分上移1.8%屏幕高度以完整可读，额度层位于道具栏后，道具底栏上边缘约14.6%，遮住下半部分并拦截覆盖区域点击。原前两排绑定保留；晋升复用同一实例。Level01维持两排。
- BuildSettings包含MainMenu、Level01、Level02。标题Level 2沿用原字体，补烘焙字形2。

## Integrations and failure behavior
- 无SDK/网络/内容热更。全部本地Resources。
- 非胜利、后台、过时代次/关卡的Continue无操作；异步场景加载只发起一次。
- 最后一关明确返回Home；新Start从第一关，不请求不存在的Level03。
- 原有旧指针释放输入门控跨场景继续有效。

## Performance considerations
- 苹果1048Renderer相对第一关522增大，沿用已有飞行池和共享材质，不每帧加载Resources。
- 可见蜂巢最多15个，已有字典实例复用；三行显示不增加采集或蜜蜂并发上限。使用原材质与现有UI覆盖，不增加裁切Shader或运行材质分配。

## Acceptance criteria
- [x] Continue从Level01进入Level02，仅一次，库存保留。
- [x] Level02实际模型1048独立方块与18蜂巢额度匹配，三排可见且权限正确。
- [x] 第二关Retry保持关卡、Home后Start保持代次与进度。
- [x] 最后一关Continue返回Home，下一次Start从Level01新开。
- [x] 原玩法和UI回归通过；规划型通关和第一列堵槽可验证。

## Verification matrix
| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | validate_game_foundation_project.py | 通过，0 warnings（2026-10-13） |
| Unity compilation | Console无新错误 | 最后Console 0 Error，当前源码编译通过 |
| Focused tests | VictorySettlementTests/LevelTwoGameplayTests | 连同HiveQueueRowsTests，14/14通过；新增额度可读断言已在最终71项完整运行回归通过 |
| Full suites | EditMode/PlayMode | 最终84/84 EditMode、71/71 PlayMode，0失败/跳过；目标1080x1920竖屏 |
| Runtime/assets | 场景/资源/模型/布局读回、实际截图、正式命令通关 | Level01两排、Level02三排/5绑定、BuildSettings3场景、1048实际Renderer/18蜂巢/逐色额度一致；竖屏原生Game view检查；正常相机/旋转/正式命令通关1048，第一列堵槽951剩余 |
| Performance | 共享材质/缓存/实例与回调审查，设备未测 | 延用原共享材质和字典实例缓存；无半显专用材质/Shader；苹果Renderer约为第一关2倍；移动设备GPU、帧率/内存和发布构建未实测 |

## Definition of Done
- [x] QFramework所有权、资源生命周期与绑定核对。
- [x] 所有验收项、编译、测试和规范校验通过。
- [x] 独立审查与运行画面检查完成。
- [x] 性能和设备检查范围记录；有证据后Verified。

## Decision log
- 2026-10-13: 用户授权制作第二关并改Continue到下一关；已确认第二关获胜后返回主界面。复用1048方块的已验证内容。

- 2026-10-13: 用户通过两张参考图明确半显指道具按钮和底栏覆盖第三排下半部；替换此前网格裁切方案，保留完整蜂巢和原材质。14/14聚焦运行测试通过。

## Final evidence (2026-10-13)
证据目录：/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/level02-gameplay/
- tests-editmode-quota-final.json: 84/84 Passed。
- tests-playmode-quota-final.json: 71/71 Passed。
- tests-focused-final.json: 14/14 Passed；最终完整套件进一步覆盖额度可读断言。
- asset-readback.json: Level01/02布局、绑定、资源、BuildSettings和逐色额度精确读回；专用半显Shader已移除。
- runtime-gameplay-proof.json: 无道具通关1048个方块、第一列连续选取造成5槽占满并剩951；实际Board发布相机视角和旋转，按正式命令推进。它不代表逐帧模拟所有渲染动画。
- level02-layered.png: 1080x1920原生Game view渲染读回，检查第三排自然遮挡、完整额度、道具角标和苹果布局；GPU读回方向校正，未绘制示意图。
- tests-continue-red.json / tests-async-reopen-red.json / tests-toolbar-occlusion-red.json / tests-quota-visibility-red.json: 修复前失败边界。
- review-report.md（工作目录）: 独立审查Important问题修复、范围与验证限制。

## Changed scripts and assets
项目源码仅9个：Utilities/Harvest/LevelOneAssets.cs、Models/Navigation/MenuNavigationModel.cs、Commands/Navigation/MenuNavigationCommands.cs、ViewControllers/Harvest/LevelOneController.cs、HarvestBoardView.cs、LevelOneHud.cs、HiveModelView.cs、ViewControllers/UI/MainMenuNavigationController.cs、VictoryPopupController.cs。
新增Level02.unity并接入既有Apple.prefab/Level02.json，新增5组第三排绑定；BuildSettings增加第二关，原字体补字形2。VictorySettlementTests更新递进测试，LevelTwoGameplayTests新增5个运行场景测试。原始源码与Level01快照在工作目录before保留；项目无Git。

## Remaining checks
无剩余Inspector绑定。设备性能和发布构建检查未执行，当前完成范围是桌面Unity编辑器竖屏功能、资源、画面与测试验证。

## Layout refinement: equal row spacing (2026-10-13)
用户要求第二、三排间隔缩小，将第三排上方组件整体下移保持协调。当前一、二排中心间距8.5%安全区域高度，二、三排12.75%；将上方组件整体下移4.25%后，两处间距均为8.5%。第三排和道具栏不移动，保留自然遮挡、额度可读和原有操作。
- Level02场景：前两排、六组槽位/目标、分页按钮、Level 2标题和设置入口同量下移。
- HarvestBoardView新增默认0的展示偏移LayoutViewportYOffset；Level02设为-0.0425，将苹果与大蜂巢按视口高度同量下移。第一关默认0，位置保持。
- 只修改Unity展示位置，不修改规则、关卡数据、配色、旋转、采集条件或库存。
- [x] 原生场景读回及运行投影证明两处中心间距一致、第三排/道具栏保持。
- [x] 1080x1920实际Game view核对苹果、大蜂巢、槽位和蜂巢队列协调。
- [x] 现有运行回归、编译和规范校验通过。
本次是低影响可逆布局调整，不新增镜像布局常量的测试；使用实际运行投影和既有玩法测试验证。

- 回归修正：第二关末关Continue测试曾固定等待0.65秒，而入场动画按每帧最多0.05秒累积，低帧率时尚未就绪。现等实际按钮可点击（最多3秒）后再验证返回Home和新循环，保留原有功能断言；生产结算逻辑未修改。

### Latest layout verification
- 实际运行投影：五列的两处中心间距均约0.085，误差小于0.000002；苹果下降0.0424987视口高度，大蜂巢中心y=0.8974997；第三排/道具栏保持自然遮挡。
- 1080x1920 Game view画面已检查，额度可读、三排间距一致，苹果与槽位无重叠。预览：/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/level02-spacing/level02.png。
- 原生Scene读回第一关展示偏移0、第二关-0.0425，五列绑定保持。
- 最终PlayMode 71/71、EditMode84/84，0失败/跳过；编译Console0 Error，规范校验0 warnings。
- scene-readback.json、runtime-layout.json、tests-playmode-final.json、tests-editmode-final.json保存在上述outputs/level02-spacing目录；首次固定时间等待测试失败记录另保留。
- 本次改动：HarvestBoardView.cs展示偏移、Level02.unity原生位置；既有LevelTwoGameplayTests.cs改为等待真实Continue就绪。无新资源、渲染器、材质和每帧计算；手机性能/发布构建未实测，原Inspector引用无待补。
