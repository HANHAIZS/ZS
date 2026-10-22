---
id: shop-header
title: Shop紫金顶部标题栏
status: Verified
owner: Project
created: 2026-10-22
updated: 2026-10-22
---

## Goal
移除商城旧Shop文字，采用用户图一横向紫金背景及图二Shop标题牌，叠放关系参考图三和既有Setting顶部。保持原顶部金币/体力位置，用户补充明确经济栏叠放在新顶部栏内部，可遮挡顶部栏的背景/边框，但不能遮挡Shop字样。仅将商城滚动可视区顶缘移至标题牌下方，避免商品覆盖标题；原商品内容、数量、价格、顺序、购买按钮、滚动逻辑、导航及设置保持。

## Ownership and architecture
仅Editor ShopHeaderSetup组装和MainMenuPage.prefab静态UI改变，无运行时代码、Model/Command/System、存档或Foundation改动。通过Unity原生Prefab API更新。MainMenuTabsSetup重建链在ShopOffersSetup之后调用相同Build，不会重建回旧Shop文字。

资源Assets/Sprites/UI/Shop/Header20261022/Background.png为图一原字节；Plaque.png由内置image_gen从图二仅提取透明背景，保留紫金标题牌及Shop字样，源图不改。原生SpriteRect裁去外侧透明留白，生成PNG不做脚本像素编辑。Prefab直接引用这两张Sprite，Resources加载键仍为Prefabs/UI/MainMenuPage，随ShopContent显隐及页面生命周期持有；无新加载键或每帧加载。完整提示词与来源记录work/shop-header-20261022/art-manifest.json。

## User-visible behavior
保留EconomyHeader的位置、绘制顺序及按钮，新标题栏贴齐SafeArea顶部，经济栏在其前方显示且位于Shop字样上方。横向背景宽度铺满ShopContent，高225，与SettingsHeader高度一致；Shop牌宽370、按原生Sprite比例高度约235，居中并略伸出背景底边。商品ScrollRect顶缘在标题下16，内容及各Button实例、卡片布局不变。新图片raycastTarget=false，不拦截经济栏或滚动输入。旧ShopTitle对象移除，新增ShopHeader/Background与Plaque。当前用户Game.unity及BeePalette19/HivePalette11原字节保持，不提交。

## Acceptance criteria
- [x] 旧ShopTitle移除，图一背景及图二标题牌只在商城显示，横向底板+居中下凸牌，尺寸参照Setting225高标题栏。
- [x] 短/长竖屏经济栏在顶部栏内部、无Shop字样/商品遮挡，正常滚动、Home/Shop/Setting切换和弹窗输入保持。
- [x] native引用、重复Build及Apply、Prefab/项目校验、聚焦现有PlayMode测试及Console通过。
- [x] 原商品/价格Button、导航/设置/经济/原图/场景和用户已有修改保持；记录证据与限制。

## Failure, performance and limits
停止Play才能Apply，素材缺失时明确报错。只有两张共享静态图片，无新MonoBehaviour、Update、事件订阅、遮罩或业务操作。贴图无mipmap、ReadWrite关闭。只做Editor原生几何/层级与运行测试，不截图录屏；不上传GitHub，不做真机观感或性能采样。


## Out of scope
不更改商品卡片、美术或价格内容、购买行为、货币与体力逻辑、主界面/Setting布局、导航栏、玩法/关卡、设置与存档、支付SDK或Foundation。

## Verification matrix
| Boundary | Evidence |
| --- | --- |
| 标题叠放/尺寸/间距 | 两种竖屏原生Rect几何、Sprite引用、透明alpha |
| 商品/经济/导航保持 | 原Prefab序列化对象比对、基线文件哈希、现有PlayMode菜单/经济/动画 |
| 滚动/显隐/输入 | 真正MainMenu Shop展示及滚动读回、现有弹窗输入测试 |
| 重复更新/规范 | Apply/Build幂等、UI及项目validator、Console |

## Definition of Done
- [x] 验收项完成、证据记录、无待绑定Inspector字段，原用户修改保留。


## Final verification — 2026-10-22
用户最后补充：体力和金币位于顶部栏内，允许遮挡背景/边框，只要不遮挡Shop。最终TopInset=0，旧ShopTitle移除，背景225高、Shop牌370×234.5934、列表顶缘距牌底16；经济栏仍用原坐标及前景绘制顺序。

两种竖屏750×1333.333与750×1625原生矩形读回：经济栏（含加号）最低边位于顶部下114.2，生成素材Shop白色字形顶缘为顶部下121.7545，至少7.55间隔（无经济栏盖字），滚动列表与牌底间距16。两新Image均非Raycast目标。真实运行MainMenu→Shop显示头图及原经济栏，滚轮后NormalizedPosition由1到0.54474且标题位置不动，切Setting后Shop头图隐藏；原有11个MainMenuTabs/EconomyUi/NavigationBarAnimationUi测试在最终TopInset=0后全部通过（job `64650aad5a7340d78788b588b51534cc`，零失败/跳过），覆盖页面切换/经济栏/动画/弹窗输入。

针对性Apply连续两次Prefab哈希稳定；不保存的临时Prefab连续Build两次仍一个ShopHeader、两个Image、一个ScrollRect、10个商品Button。原生Prefab规范Valid=True、Issues=0，项目规范零警告，最终Console零错误/警告。独立审查无剩余具体问题。

起始基线哈希：既有文件只MainMenuTabsSetup.cs和MainMenuPage.prefab改变；新建ShopHeaderSetup.cs及meta、Header20261022两个PNG及meta/目录meta、此说明。原商品/全部既有Button组件、经济/Setting/导航相关对象保持；唯一非Shop序列化差异是Unity重存将GoldRim Y从−7.0181813舍入到−7.0181885（0.0000072，无视觉或行为变化）。用户Game和两材质已有修改与起始字节相同，所有原PNG/字体/其他场景不变。

图一原PNG与项目Background.png逐字节相同；图二内置image_gen提取透明背景，生成图、工作区及项目Plaque.png逐字节相同，原用户素材不变。最终资产 `/Users/zhangshuai/BeeHarvest/Assets/Sprites/UI/Shop/Header20261022/{Background,Plaque}.png`；来源、完整提示词及工具见[art-manifest.json](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/shop-header-20261022/art-manifest.json)，验证见[final-audit.json](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/shop-header-20261022/final-audit.json)。无Inspector待绑定字段。没有截图录屏或真机目测、性能profiling；仅原生素材检查/几何和交互回归。不新增运行时脚本/Update或绘制遮罩，仅两张静态图及Prefab引用，纹理max2048、无mipmap、ReadWrite关闭。
