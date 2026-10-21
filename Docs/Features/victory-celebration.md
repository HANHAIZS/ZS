---
id: victory-celebration
title: 胜利结算庆祝特效、通关音效和奖励数字
status: Verified
owner: Project
created: 2026-10-20
updated: 2026-10-21
---

# 胜利结算庆祝特效、通关音效和奖励数字

## Goal
完成胜利界面的轻量庆祝效果、用户提供的通关音效和金币下方白色60。
## User-visible behavior
- 最后一只蜜蜂交付后随现有胜利弹窗播放一次彩纸喷射，金币周围有少量金色闪光；特效位于插画后、遮罩前，不遮挡文字与按钮。
- 同一次胜利只播放一次成功通关音效，沿用Sound开关；后台和关闭停止，恢复前台不补播。
- 金币下方居中显示白色60，使用现有FredokaBoldSDF圆润粗体，匹配插画标题风格；Victory本身为图片，不能直接共享字体文件。
## Out of scope
更改金币发奖规则、存档、关卡、导航、其他弹窗、Foundation、原始素材、重新绘制Victory标题；不导入整套504MB素材或Demo/第三方脚本。
## Ownership and architecture
- State mutation: None，保留现有ContinueLevelCommand和经济规则。
- Model/System: None，新内容为瞬态表现。
- Event: VictoryPresentationEvent，携带本轮Generation，由Popup发布，GameAudio订阅。
- Unity presentation: VictoryPopupController接入；VictoryCelebrationView缓存和管理原生ParticleSystem；UI粒子Graphic适配器只负责将原生粒子绘制到Canvas，避免额外摄像机和RenderTexture。
- Editor: VictoryCelebrationSetup使用原生Prefab API制作直接引用资产，并读回15关实例；不改现有绑定覆盖。
## State and persistence
仅本轮展示和音效已消费Generation；无存档迁移、SDK、网络或热更新。
## UI, prefab, scene, and Inspector impact
- 用户原始Epic Toon FX中的ConfettiBlastRainbow与SparkleAreaYellow及依赖，复制到Assets/Sprites/DesignImports/EpicToonFX（775,646字节，不含meta）。原始源目录不改；Unity导入时自动规范化5个材质版本字段/无效关键字，原Prefab、纹理、音效字节保持。
- 适配的UI特效Prefab放Assets/Sprites/Prefabs/UI/VictoryCelebration.prefab，VictoryPopup直接引用；不包含供应商脚本、灯光、音效；控制粒子上限与持续时间。
- Assets/Sprites/Prefabs/UI/VictoryPopup.prefab新增Celebration与RewardAmount，保留现有按钮、层级和动效绑定。所有Level01–15继承此Prefab。
- Assets/Resources/Audio/Sfx/Victory.wav，key Audio/Sfx/Victory，由GameAudio初始化加载一次，复用现有6路音效池。
- 打开播放、关闭清空；暂停冻结粒子，恢复继续；禁用和销毁清理。所有图形raycastTarget=false。
## Integrations and failure behavior
音效遵循Sound偏好，与Music独立；旧Generation事件无效；隐藏和换关无残留；原生粒子适配仅接受受支持的Billboard，无需改工程渲染管线。
## Performance considerations
仅胜利时运行；复用粒子与顶点缓存，不逐帧Resources.Load/GetModel/场景查找；不新增音频源、相机或RT；限制一次彩纸和稀疏闪光。
## Acceptance criteria
- [x] 15关胜利均显示白色60且位于金币下方、按钮上方。
- [x] 实际原生粒子有随时间变化的可见庆祝效果，开关、前后台和重开无残留。
- [x] 音效只在胜利首次显示时播放，Sound关闭和后台不补播。
- [x] 原有Continue进度、输入门禁、设置和音乐保持。
## Verification matrix
| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | validate_game_foundation_project.py | Passed |
| Compilation | Console no new errors/warnings | Passed |
| Focused tests | victory, audio, lifecycle | Passed |
| Full suite | EditMode + PlayMode | Passed |
| Exact UI/VFX | Native prefab readback + screenshots | Passed |
| Import | GUID dependencies, byte hashes, bounded budgets | Passed |
## Definition of Done
- [x] 项目分层、资源生命周期、Inspector引用经过核验。
- [x] 验收、编译、约定、测试和截图证据完整。
- [x] 性能与未测设备限制记录。
## Decision log
- 2026-10-20: 报告仅作参考；选择实际彩纸/闪光资产的最小依赖，不导入Demo或升级渲染管线。

## Verification evidence

证据目录：[victory-celebration](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/victory-celebration)。

- 首轮行为测试确认奖励数字缺失、通关音效未触发（2项预期失败）；实现后3项通过。旧轮次隐藏事件也经失败断言复现、修复和通过。
- 全套EditMode 169/169，PlayMode 164/164，共333项通过：full-edit-result.json、full-play-progress3.json。随后仅调整彩纸原生限速模块，最终配置针对性3/3通过：final-focused-result.json。
- 原生读回15关：同一VictoryPopup.prefab，Celebration绑定有效；白色60/FredokaBoldSDF、位置在金币下方与按钮上方、3系统总容量152，原VictoryArtwork引用保留。final-all-fifteen-readback.json。
- 最终运行：左右彩纸各54片/216顶点，星光3片/12顶点；全部UI材质支持且不接受点击；音频源仍7个。通关音频4.9954195秒、双声道22,050Hz。final-runtime-audit-result.json。
- 原生渲染：[1080×1920预览](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/victory-celebration/victory-1920.png)、[1080×2340预览](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/victory-celebration/victory-2340.png)。使用隔离的、保留1784块坐标但单一颜色/蜂巢的展示样例，通过正式Start/Activate/Advance命令获得Won；粒子固定采样在0.65秒，不作为原始第一关无辅助通关证据。未改真实关卡、存档或偏好。
- 临时自动操作原第一关的探针未完成（余1055块），不作为通过证据。小样数量与视图不一致的探针曾触发Sync索引错误，最终展示样例保持数量与坐标一致；未改生产玩法代码。
- 规范校验通过，最终真实流程复验3/3通过（final-clean-result.json），Unity Console无错误/警告（final-clean-console.json）；只读审查提出的旧Generation问题已修复并复查通过。测试工具重建的无关主菜单资产和Palette11按基线恢复。
- 原始素材文件不变，import-hashes.json记录源哈希及导入一致性；5个自动升级材质的差异单独说明。
- 无Android/iOS真机帧率或扬声器听感结论；Editor实际音频源播放、开关、生命周期已由测试核验。无待补Inspector字段。

## Final implemented files

- ViewControllers/UI/VictoryPopupController.cs、VictoryCelebrationView.cs、VictoryParticleGraphic.cs；ViewControllers/Audio/GameAudioController.cs。
- Events/Harvest/VictoryPresentationEvent.cs；Editor/VictoryCelebrationSetup.cs；Tests/Runtime/VictoryCelebrationTests.cs。
- VictoryPopup.prefab新增特效引用和RewardAmount；新VictoryCelebration.prefab；DesignImports/EpicToonFX中12个选中原始资产；Resources/Audio/Sfx/Victory.wav。
- Level01–15场景、原VictoryArtwork、其他UI、音频、关卡和工程设置与本次基线一致，见final-changes.json。

- 临时截图采样探针产生的原生Prefab模拟记录已单独清理；随后真实PlayMode复验3/3通过、干净控制台0条错误/警告。最终停在MainMenu编辑场景，临时经济/设置隔离已还原。


## 2026-10-21 奖励数字移至金币右下角
用户要求把金币下方60移到金币右下角。只调整RewardAmount布局，原60文字/白色/FredokaBoldSDF/84字号/材质/非射线与Continue/Victory/粒子/音效/发奖规则保持。用VictoryArtwork内非绘制RewardArea配合原生AspectRatioFitter.FitInParent，按实际Sprite比例对齐绘制区域；奖励数字位于图案归一化(.73,.155)附近，对不同竖屏比例保持金币右下角关系。

所有105关继承原VictoryPopup.prefab；Editor构建方法同步复现，不改Runtime/Scene/JSON。该可逆低影响布局不新增单元测试，以Native prefab精确读回、两种竖屏截图、编译/Console/validator与Git局部diff核验。

- [x] 白色60位于金币右下角，原字体与字号不变。
- [x] 标准和长屏竖屏布局一致，其他UI/效果/玩法保持。
- [x] 原生资产读回、预览/编译/规范检查通过，修改范围限定为本布局与记录。

验证：原生Prefab读回确认父节点RewardArea、锚点(.63,.075)–(.83,.235)，原TMP文字60/FredokaBoldSDF/84/白色与材质保持；Unity脚本编译完成。隔离展示样例通过正式Start/Activate/Advance命令得到Won，截图确认两种分辨率的金币右下角位置，读回归一化中心均(.73,.155)，RewardArea比例均0.9540498。

- [1080×1920预览](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/victory-reward-corner/victory-1920.png)
- [1080×2340预览](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/victory-reward-corner/victory-2340.png)

首次预览因Editor禁用域/场景重载而复用了失效依赖，未采用其截图；改用正常重载后Won与弹窗IsOpen均为true。截图探针粒子固定采样产生2条“Instantiate Particle System Prefabs before trying to play/simulate them”诊断，来自execute_code采样，不作为生产逻辑或音效/粒子测试结论。预览经济/设置隔离与原Editor重载选项已恢复，停在MainMenu编辑模式。未改变生产运行时代码、关卡数据、原图或存档；本次只验证布局。

规范validator通过（0警告）；Git局部diff仅Editor构建方法、VictoryPopup.prefab与本Spec。
