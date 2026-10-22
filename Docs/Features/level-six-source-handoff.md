---
id: level-six-source-handoff
title: 第6关真实源位到蜂爪的90ms交接
status: Verified
owner: Project
created: 2026-10-22
updated: 2026-10-22
---

# 第6关真实源位到蜂爪的90ms交接

## Goal
用户已确认上轮解释的方案，要求只在第6关实现：块仍在洞中→被拔出→暗腔逐渐露出→飞走。取块首帧cargo以真实世界位置/旋转/大小与外观接管源块，源Renderer同帧隐藏，90ms平滑接到Grip。

## User-visible behavior
- 仅Level06由LevelOneController初始化时开启；106虽然共用Chick模型，仍保留原行为。所有关卡资产/数据/几何/灯光/经济规则不修改。
- 取块Stage0→1时冻结源世界姿态，复制原Mesh/尺度；90ms内按SmoothStep从真实源位移至当前Grip，旋转Slerp。既有220ms法线拔出和后续交通/确认逻辑保留，90ms包含其中。
- 明暗接管：初始暂用源cavity共享材质，以cargo自有PropertyBlock固定源局部采样坐标并逐渐将局部遮蔽淡到0；90ms后恢复既有无cavity的黑边货块材质并清除临时属性，避免取块首帧亮度跳变。不创建逐货块新材质/纹理。
- 蜂身留在既有安全飞行路线，腿不拉长。爪子对准cargo沿相机方向投影到当前Grip深度的同屏姿态，随交接位置与角度变化；完成后正常固定握持。
- 使用f.Age的游戏时间，暂停冻结，加速遵循现有游戏时钟；不改LiftSeconds/Stage/配额。视图晚建或重建已在搬运阶段时不重播洞中交接；重开/对象池复用清理状态。

## Out of scope
其他110关行为、源Prefab/Scene/JSON/Shader/材质文件、新Command/Model/System/存档、模型自动旋转规则、其他飞行路线、长腿变形、截图录屏、GitHub推送与真机性能承诺。

## Ownership and architecture
| Responsibility | Owner | Planned type/path | Notes |
| --- | --- | --- | --- |
| State mutation | Command | 既有Harvest Commands | 不改逻辑 |
| Observable or persistent state | Model | 既有HarvestModel | 只读Age/Phase/Stage |
| Shared project rules | System | 既有HarvestSystem | 不改 |
| One-shot notification | Event | 既有采集事件 | 不增 |
| Unity presentation | ViewController | BeeFlightView/HarvestBoardView/LevelOneController | Actor交接状态、只读原位访问、初始化范围 |
| Adapter or repository | None | 既有LevelOneAssets | 无新资源 |
| Reusable infrastructure | None | None | 不改Foundation |

## State and persistence
- Runtime state: Actor冻结源Pose、时间、源采样矩阵与交接标志；复用CargoBlock。
- Save schema/version/migration: None。
- State mutation path: 原Commands隐藏源数据/Stage，Board与BeeView同次Update渲染切换，不反写玩法状态。

## UI, prefab, scene, and Inspector impact
- Affected assets: None；仅脚本与运行测试/Feature Spec。
- Loading mode and final paths: 沿用现有Resources模型、Materials/Level07VoxelCavity与ChickVoxelOutline，无新加载键。
- Resources keys or direct reference owners: 既有Board拥有palette；Actor临时借用，交接后换回原货块材质。
- Load/cache/instance cleanup responsibility: 不新增材质/纹理；重开/回池清标志，Board与BeeView按现有生命周期销毁。
- Hierarchy or binding changes: None。
- Inspector confirmation: None；第6关运行验证后打开该场景供用户查看。

## Integrations and failure behavior
- SDK/consent/lifecycle: 无新集成。
- Content update/offline/rollback: 本地运行，Git可回退；不改资源。
- Cancellation, timeout, retry: 暂停不累计交接时间；相机/模型旋转后源位不追着模型跑；重开清理旧cargo；已过交接阶段的视图重建不回到源位；卡帧跨过Lift阶段直接完成交接，不延长规则时长。

## Performance considerations
只对第6关活动cargo前90ms更新既有PropertyBlock和矩阵，不逐Voxel更新、不克隆材质/纹理、不增Update/协程。保持固定长度腿约束和已有actor池。

## Acceptance criteria
- [x] 原实现真实位置误差的RED信号成立；新首帧源/cargo世界姿态、网格/尺度一致，源隐藏，阴影外观接管。
- [x] 30/60/90ms位置/旋转与当前Grip的平滑插值正确，完成后无局部阴影/临时属性残留。
- [x] 模型旋转/暂停/重开/池复用、视图重建及真实交付正确，腿不伸长。
- [x] 仅第6关改变，1/106等旧交接与全部源资产保持。
- [x] 编译、原生GPU/数值检查、针对性运行回归与规范验证通过。

## Verification matrix
| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | validate_game_foundation_project.py | Passed，0 warnings |
| Unity compilation | C#/Shader无新错误 | Passed，Console 0 errors，shaderMessages=[] |
| Focused tests | 首帧真实位姿、90ms、暂停/重开/视图恢复、范围 | Passed，6项新增运行检查+33项运行回归+15项编辑检查 |
| Runtime or asset behavior | 真实取块/交付、首帧GPU外观、旧资源哈希 | Passed，GPU首帧差异0，1717份旧非脚本资源不变 |
| Performance | 无新材质/纹理，临时Block清理与池复用 | Passed，共享源材质，临时Block完成后清空，固定腿长/池复用检查通过 |

## Definition of Done
- [x] Ownership and QFramework roles match implementation.
- [x] Acceptance criteria and failure/recovery paths pass.
- [x] No source asset/Inspector migration.
- [x] Conventions, compile and focused checks pass.
- [x] Evidence and platform limits documented.
- [x] Verified only after evidence exists.

## Decision log
- 2026-10-22: 用户明确批准仅第6关试做90ms交接。原PickupPose是安全前景平面投影，仅保持屏幕位置，不能作为真实洞中起点；新增只读源Pose/采样访问，原接口不改变。
- 原飞行路线已包含220ms法线拔出，保留并叠加前90ms真实深度交接；使用f.Age以保持暂停/加速行为一致。

## Verification evidence — 2026-10-22
- RED PlayMode37813493fefc4045b5f768c2da291933：真实源位首帧检查按预期失败，旧实现cargo与源块世界位置相差2.2381103。
- 初版修正后1899e7368aa048f582d6a1d035f5064d：首帧误差仅5.960464e-8，30/60/90ms插值及姿态/原网格/原色检查通过；测试发现交接结束后空PropertyBlock仍挂载。已补Clear→恢复原货块材质→空块SetPropertyBlock(null)，未修改玩法逻辑。
- GREEN197079fd3b5c4dc6b284ca0a68ac8c1b：首帧/90ms/实际交付1/1通过。
- PlayModefb6156660bdc4757b440cabf4252944e：新增6/6通过。覆盖真实源Pose首帧与同帧隐藏、30/60/90ms到移动Grip、源局部采样矩阵冻结、旋转70度不追随原块、暂停冻结、半途重开/actor池复用、跨过Lift后的材质清理和视图重建、90ms全段固定腿长与同屏块面接触、1/106/107关维持前景Grip模式。
- 实际GPU首帧对比：相同相机下仅将源块替换为cargo，RGB平均差0、超过0.08差值的像素0。只在内存读取像素，不保存截图/录制。
- PlayModeaef2d388409d41a09e82a20775659699：既有33/33运行回归通过，包括交接/固定握持/取块动画/蜂与蜂及模型间隙/聚合路线、Chick真GPU棱边、第7关局部阴影GPU/暂停/重开/清理、全部新107–111场景搬运，以及106货块浅边/重开/卸载。合计39项不同PlayMode检查最终通过；未运行全部旧PlayMode。
- EditModef8c6f17a93cd46a3af15166176405809：15/15通过，覆盖既有目录/推进边界/完整源内容/cavity范围。
- 差异仅BeeFlightView（Actor交接与临时属性）、HarvestBoardView（3个只读访问）、LevelOneController（LevelNumber==6初始化开关）和新测试/本Spec。不改Command、Model、System、111关数据、Prefab、Shader、场景、光照或原材质。
- Unity自动序列化导致BeePalette14/18/19旧_Color浮点尾数变化；测试后通过Unity原生FileUtil从提交基线恢复，最终1717份旧非脚本Assets逐字节SHA256与工作前一致，无额外资源改动。范围报告在工作区outputs/level06-source-handoff/scope.json。
- 原生最终状态：Level06.unity打开，dirty=false、非Play/非编译、Shader消息空、Console0errors。由用户自行运行查看，不截图录屏。
- 独立只读复核未发现Critical/Important问题。Foundation验证0warnings，C#和文档空白检查通过。没有新增Material/Texture、逐体素更新或帧循环依赖查找，只有活动cargo的短期属性更新；iPhone真机性能未测。
- 本地Git保存，未上传GitHub。
