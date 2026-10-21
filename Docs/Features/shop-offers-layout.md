---
id: shop-offers-layout
title: Shop商品展示布局
status: Verified
owner: Project
created: 2026-10-14
updated: 2026-10-19
---

# Shop商品展示布局

## Goal

仅给Shop增加UI展示：Shop标题下按去广告、小礼包、大礼包纵向排列，三张对应价格按钮位于各卡片右下角，保持间距。

## User-visible behavior

Shop显示三种商品及US$6.99、US$9.99、US$15.99价格按钮。按钮无购买、支付、发奖或其他回调。顶部复用Home既有体力金币栏，继续显示同一状态。Home、Gallery与设置保持。

## Ownership and architecture

项目Editor/预制体UI布置，新增ShopOffersSetup工具并接入MainMenuTabsSetup重建链。新增ShopRoundRectMaskGraphic只绘制静态UI遮罩几何，无玩法逻辑、Command/Model/System/架构或Foundation改动。用户明确此次仅增加布局，不增加商品系统。

## State and persistence

没有新增状态、存档、支付、广告或SDK接入。

## UI, prefab, scene, and Inspector impact

- 共用Assets/Resources/Prefabs/UI/MainMenuPage.prefab的ShopContent下增加ShopOffers与三张卡片和三枚无事件Button，沿用Prefabs/UI/MainMenuPage加载键与原页面生命周期。
- 六张原图直接导入Assets/Sprites/UI/Shop/：RemoveAds.png、RemoveAdsPrice.png、SmallBundle.png、SmallBundlePrice.png、BigBundle.png、BigBundlePrice.png。Sprite原生裁切，仅裁外围空白，源PNG不改；直接引用于该预制体，不新增Resources键。
- 用户源文件均RGB，价格外侧棋盘格并非透明；使用Unity UI向量圆角Mask组合原图显示，保留原字和图案，不生成或编辑图片。
- 商品图不接收射线，价格Button没有事件回调。价格位于卡片右下方空白区；面板在标题与底部导航之间等比显示。
- 既有EconomyHeader已跨Home/Shop/Gallery显示，复用同一对象及绑定，不新增重复数值或按钮。
- 场景、其他素材、按钮位置与绑定保持。

## Out of scope

IAP、广告移除、礼包奖励、金额与本地化动态配置、商店数据与滚动、玩法、经济、Home/Gallery布局不变。

## Integrations and failure behavior

无新增网络/SDK/存档依赖。素材缺失时Editor工具明确报错。所有商品随ShopContent显隐和页面销毁，无独立加载或订阅。

## Performance considerations

六张共用压缩纹理，卡片max2048、价格max1024、mipmap及ReadWrite关闭。新增静态UI和Mask，仅Shop可见；遮罩仅重建Canvas几何时生成顶点，没有Update查询或运行时加载增加。Unity Editor竖屏检查，未做真机性能测试。

## Acceptance criteria

- [x] Shop标题下按指定顺序等比显示三卡，间距明显。
- [x] 三枚价格按钮分别在对应卡片右下角，棋盘底不显示，点击不变更任何状态。
- [x] 原体力金币栏在Shop显示，引用与数值和Home一致。
- [x] 原页面功能、场景、既有运行时代码、存档与其他美术保持。
- [x] 共用预制体引用、重建幂等性、截图、聚焦回归、Console与规范检查通过。

## Verification matrix

| Boundary | Evidence |
| --- | --- |
| 布局和素材 | 原生坐标读回、1080×1920截图 |
| 无功能价格按钮 | 事件数量读回与实际点击前后状态对比 |
| 共用顶部栏、页面显隐 | 实际Shop/Home/Gallery切换与既有聚焦测试 |
| 重建链 | Build两次无重复对象 |
| 修改范围 | PNG原图哈希、原代码/场景/素材哈希对比 |
| Console与规范 | 检查输出 |

## Definition of Done

- [x] 所有验收证据保存后标记Verified。

## Decision log

2026-10-14：沿用用户所给原始图案，UI遮罩隐藏外围底色；不接入任何购买逻辑。不为可逆静态布局新增实现镜像测试，使用原生读回、实点与既有菜单回归。

## Completed verification — 2026-10-14

- 六张源PNG哈希一致；商品图片仅通过Sprite裁切/向量Mask组合。
- 最终1080×1920截图：[shop-final.png](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/shop-offers-layout/shop-final.png)。后续设置页重建后截图与原生点击证据见 `outputs/oka-settings/shop-regression.png` 和 [runtime-proof.json](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/shop-offers-layout/runtime-proof.json)。
- 三个价格Button实际PointerClick各触发一次；persistent callback数量均0；Coins/Stamina/Revision/Harvest.Generation/Remaining/SelectedTab不变。价格宽164，金币1600、体力5，共用headerCount=1，6个Mask有CanvasRenderer。
- 原菜单聚焦10/10通过（job6498d488131e4b33bd5fa26b9ab175b9）；后续设置任务全套PlayMode85/85、EditMode重建3/3及重建后菜单14/14通过。
- 多次重建后卡片/价格无重复；UiPrefabValidator Valid=True、Issues=0，项目规范0 warnings。独立审查通过，无待绑定字段。
- 本功能未新增支付、广告、发奖或存档逻辑；后续第三页和设置弹窗按用户新需求变更，详见 `oka-settings.md`，不将旧Gallery作为当前界面目标。
- 限Editor竖屏UI验收，未做真机性能采样。

## 2026-10-19 Small Bundle分层素材更新

用户指定：小礼包改用新紫色渐变金边底板、紫色金币袋、无限体力心和绿色购买按钮，按参考图排布；四种道具图标复用现有Speed、Shuffle、AddSlot、FreeSelect。价格冲突已询问，用户明确保留US$9.99。

SmallBundleOffer保留原所在单元位置与约3.6宽高比，金币袋在左，右侧按加速、任意选取、随机排序、增加槽位、无限体力排列；四种道具标×1，体力标1h；Small Bundle标题位于左下、价格位于右下。与原卡一致仅展示，未添加购买/支付/发奖或无限体力玩法。去广告、大礼包、顶部角标、Home/设置、五关及经济库存规则均保持。

新图用内置imagegen提取真实透明alpha，按钮去掉烘焙15.99并用现有按钮字体（FredokaBoldSDF）显示US$9.99。保留来源原图；生成图原样存工作区并复制Assets/Sprites/UI/Shop/SmallBundle20261019/，只由现有MainMenuPage预制体直接引用，无新Resources键。四个现成道具图标直接引用OkaRefresh20261018，不复制。标题/数量与价格各使用共享字体材质（同目录）。

新增Editor ShopSmallBundleSetup负责仅SmallBundleOffer子树的幂等样式组装，ShopOffersSetup重建链调用；保留原PriceButton实例/事件和资源加载生命周期。新图通过SpriteRect裁外围透明留白，未通过像素脚本改图。仅Editor建材质/对象，无新增运行时Update、状态或每帧加载。

验证：真实Shop截图/不同竖屏、价格和图标引用/事件数量回读、价格实点前后状态不变、幂等样式两次无重复且其他两卡保持、既有菜单/经济聚焦回归与UI规范。无需新增镜像静态布局测试。不扩展支付逻辑和不测商店SDK/设备性能。

证据目录：`/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/shop-small-bundle-refresh/`；提示词/来源见工作区manifest.json。验证完成。


### Completed verification — 2026-10-19

- [1080×1920截图](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/shop-small-bundle-refresh/shop-1080x1920.png)和[1080×2340截图](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/shop-small-bundle-refresh/shop-1080x2340.png)：无溢出，US$9.99，四道具×1、体力1h。
- `native-audit.json`：样式重复应用两次后18个节点无重复，4个道具Sprite引用正确，FredokaBoldSDF补充标题字符；UiPrefabValidator Valid=True、Issues=0。
- `prefab-scope.json`：63个序列化对象变化均在SmallBundleOffer子树，原PriceButton组件逐字节保持；其他两卡、经济条和导航栏保持。
- `runtime-proof.json`：三个价格按钮各真实PointerClick一次，callback均0，金币/体力/Revision/Generation/Remaining/SelectedTab不变；经济条仅1份。
- `test-result.json`：既有菜单切换2项、经济UI2项，共PlayMode 4/4通过；Console错误/警告0、项目规范0 warnings。无新增镜像布局测试。
- `scope-hashes.json`：旧内容变更仅ShopOffersSetup.cs、MainMenuPage.prefab、FredokaBoldSDF.asset（补字）；新增Editor助手、4PNG、2材质及metadata。运行时代码、关卡场景、原PNG均保持。系统.DS_Store不属于本次交付。
- 独立审查无待修问题；无新增运行时Update、状态、支付或Inspector待绑定字段。4张静态透明纹理无mipmap且不可读，仅商城可见。未做设备性能采样。已恢复MainMenu编辑场景和原1080×1920 GameView。
- 素材提示词和来源：[manifest.json](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/shop-small-bundle-refresh/manifest.json)。


## 2026-10-19 去广告蓝色卡片及购买按钮内缩

按用户新图替换RemoveAdsOffer蓝色金边底图，右下使用已有空白绿色金边按钮，价格保留US$6.99；价格字体与实际Economy.CoinsLabel及StaminaLabel相同。SmallBundle购买按钮向内移动，两卡价格按钮与右/下金框保留清晰间隙。小礼包其余内容、大礼包、顶部栏及所有功能不变。通过Prefab API局部更新，保留原Button实例/事件；Editor组装链也同步。新图通过内置imagegen提取透明alpha，原PNG保持，只配置原生SpriteRect。直接引用资产Assets/Sprites/UI/Shop/RemoveAds20261019/Panel.png，由MainMenuPage.prefab持有，无新Resources键或运行时生命周期；复用SmallBundle20261019/PurchaseButton.png。验收：原生截图、按钮间距/字体/引用/幂等、原有事件/其他对象保持、既有菜单测试、Console及规范。静态UI不新增镜像测试；不新增支付玩法。


### 本次验收完成

- 真实截图：[shop-final.png](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/shop-remove-ads-refresh/shop-final.png)。去广告与小礼包购买按钮右内缩28、下内缩16 Canvas单位，164×50；边框有清晰留白。US$6.99/US$9.99保持。
- runtime-geometry.json：两个PriceLabel.font均与实际CoinsLabel及StaminaLabel引用相同FredokaBoldSDF，非近似字体。runtime-proof.json：三个原按钮分别真实PointerClick一次，0 callbacks且经济/关卡/导航无状态变化。
- native-audit.json：连续应用两次节点数98→98；UI Prefab Valid=True、Issues=0。prefab-scope.json：15个对象变更全部位于RemoveAdsOffer与SmallBundle PriceButton子树；两枚原Button组件逐字节保持，大礼包/顶部栏/导航/小礼包其他内容不变。
- test-result.json：既有菜单2项及经济UI2项PlayMode 4/4通过；Console错误/警告0；Game Foundation规范0 warnings。独立代码复核无待修问题。
- scope-hashes.json：旧文件变更仅ShopOffersSetup.cs、ShopSmallBundleSetup.cs、MainMenuPage.prefab；新建Editor/ShopRemoveAdsSetup.cs及RemoveAds20261019/Panel.png、对应metadata。未改其他旧材质/字体/美术/场景/运行时代码。
- 新增1张静态透明PNG，关闭mipmap/ReadWrite，max2048；按钮复用原图和共享材质，无新增运行时Update/下载/加载。未采样设备性能，无Inspector待绑定字段。
- [内置imagegen提示词与来源](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/shop-remove-ads-refresh/manifest.json)；生成PNG原样保存在工作区work/shop-remove-ads-refresh/RemoveAdsPanel.png和项目Assets/Sprites/UI/Shop/RemoveAds20261019/Panel.png，仅用SpriteRect裁透明留白，源参考图不变。


## 2026-10-19 中礼包与礼包金币数量

用户要求中礼包沿用小礼包布局，四道具×3、无限体力3h、价格18.99（沿用币种为US$18.99），使用所给银色绿宝石金币宝箱和既有紫色金边底板；中礼包金币数字10000、小礼包5000，与价格字体一致。用户已确认：替换第三张Big Bundle视觉卡片，金币文字位于各自礼包图标右下角，避开商品价格按钮。

保留内部BigBundleOffer对象名和原Button组件，视觉名称改Medium Bundle；旧资产路径不迁移。新增直接引用Assets/Sprites/UI/Shop/MediumBundle20261019/Chest.png（内置imagegen透明提取，工作区PNG原样复制）；由MainMenuPage.prefab持有，底板、道具、无限体力及绿色按钮复用现有SmallBundle素材，无新Resources键或运行时加载/订阅。SmallBundle其他参数保持US$9.99、×1、1h，仅加5000标签。

Editor ShopSmallBundleSetup复用统一StyleBundle参数化布局，ShopOffersSetup第三卡调用StyleMedium。保持既有购买功能（仅展示、无支付奖励）；不增加金币库存或无限体力玩法。去广告、顶部经济栏、导航、关卡和玩法不变。所有Prefab/字体/导入参数通过Unity原生API更新；既有原Button事件保持。

验收：截图、标签/图标/价格/字体引用/按钮边距原生回读、重复组装、原Button与其他对象序列化保持、既有菜单/经济聚焦测试、Console、UI和项目规范。静态UI不增加镜像布局测试。新增1静态纹理，既有共享图标/材质复用，无新增每帧逻辑；不做支付SDK或设备性能验证。


### 本次验收完成 — 2026-10-19

- [最终商城截图](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/shop-medium-bundle/shop-final.png)：Small Bundle新增5000；Medium Bundle银色绿宝石宝箱、四道具×3、无限体力3h、US$18.99及10000。数字与价格按钮分开显示。
- native-audit.json：两档金币数字与价格字体、材质引用完全相同；两次完整样式组装后Prefab字节完全一致(stable=true)，Small/Medium各19节点；UiPrefabValidator Valid=True、Issues=0。
- runtime-geometry.json：两枚价格按钮保持164×50、右内缩28/下内缩16，实际价格与金币/体力字体引用一致。
- runtime-proof.json：原三个价格按钮PointerClick逐个触发、callback均0，Coins/Stamina/Revision/Generation/Remaining/SelectedTab保持；顶部经济栏仍1份。
- prefab-scope.json：72个序列化对象变化仅在两档礼包子树；小礼包旧对象只有Card子节点列表变更，新增1个CoinAmount文本对象及其组件（4条记录），原图标、其他标签和Button均保持；全Prefab所有原Button组件逐字节保持。去广告、顶部栏、导航不变。
- test-result.json：既有菜单切换2项、经济UI2项PlayMode4/4通过；console.json错误/警告0，项目规范0 warnings。独立代码审查无待修问题。
- scope-hashes.json：旧文件仅ShopOffersSetup.cs、ShopSmallBundleSetup.cs、MainMenuPage.prefab变更；新增Chest.png及metadata，没有修改字体、旧材质、其他PNG、场景、关卡或运行时代码。
- 新增1张512上限透明纹理，无mipmap/ReadWrite，其余图标和共享材质复用；无新增每帧脚本、Resources键或加载生命周期。未采样设备性能，无Inspector待绑定字段。恢复开始时MainMenu编辑状态。
- 内置imagegen透明提取的完整[提示词与来源](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/shop-medium-bundle/manifest.json)，输出原样保存在work/shop-medium-bundle/MediumBundleChest.png及Assets/Sprites/UI/Shop/MediumBundle20261019/Chest.png。


## 2026-10-19 新增大礼包与金币点分隔格式

在去广告、小礼包、中礼包后新增第四张大礼包，不替换中礼包。复用小礼包布局及现有四道具/无限体力/绿色按钮，使用新银色金币保险箱与青紫底板，显示Big Bundle、四道具×6、12h、US$36.99、25.000。小礼包金币改5.000，中礼包改10.000，位置沿用用户已确认的礼包图标右下角，字体和材质与价格相同。其余两档数量/时长/价格及去广告内容保持。

四卡保持同等礼包宽高比，调整已有卡片单元的纵向间距，使四张在竖屏标题与导航之间完整显示；不缩小单卡图标/文字字号。新增LargeBundleOffer，保留原BigBundleOffer作为中礼包内部对象名及所有旧Button组件/事件。新增Editor ShopLargeBundleSetup负责只增加缺失的新卡、统一布局和幂等更新，ShopOffersSetup重建链同步；ShopSmallBundleSetup增加青紫底板参数和StyleLarge。

新增直接引用Assets/Sprites/UI/Shop/LargeBundle20261019/Safe.png和Panel.png，由MainMenuPage.prefab持有，无新Resources键、运行时加载/订阅。内置imagegen透明提取，完整PNG原样复制，原始用户PNG不修改；原生SpriteRect仅裁外围留白。新增静态美术与UI，无新运行时Update/经济/支付/发奖逻辑。

验收：两种竖屏截图与卡片边界/不重叠/不遮挡标题导航、各档文字/图标/价格/字体材质精确读回；完整重复Apply字节稳定，原Button/去广告内容/顶部栏/导航/关卡保持，现有菜单/经济回归、Console、UI及项目规范。静态UI不增加镜像测试。无支付SDK或设备性能验证。


### 本次验收完成 — 2026-10-19

- [1080×1920商城截图](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/shop-large-bundle/shop-1080x1920.png)、[1080×2340商城截图](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/shop-large-bundle/shop-1080x2340.png)：四卡完整可见，不遮挡标题/导航；大礼包新保险箱及青紫底板，×6、12h、US$36.99、25.000；小/中为5.000/10.000。
- geometry-1920.json与geometry-2340.json为RenderTexture目标分辨率内实际边界回读（不是Editor窗口Screen大小）：1920卡间隙26.46/55.66/55.66px、标题间隙29.52px；2340卡间隙99.66/128.86/128.86px、标题间隙83.56px，均为正数。geometry-editor.json另保留原Editor窗口坐标读回，包括价格右28/下16 Canvas单位和金币/价格字体材质相同。
- native-audit.json：count=4，完整Apply两次Prefab字节完全稳定(stable=true)，新卡仅1份；所有文字/Sprite引用正确，UiPrefabValidator Valid=True、Issues=0。rebuild-proof.json：不保存的临时Prefab内容执行完整ShopOffersSetup.Build也生成4卡、三档金币及四种正确价格。
- runtime-proof.json：四个价格按钮逐个实际PointerClick、callback均0，金币/体力/Revision/Generation/Remaining/SelectedTab不变，顶部栏仅1份。
- prefab-scope.json：80个序列化对象变化均在ShopOffers子树，全Prefab所有原Button组件逐字节保持；去广告仅所在cell的RectTransform变化，内容/价格不变；顶部栏/导航/Home/设置保持。
- test-result.json：既有菜单2项与经济UI2项PlayMode4/4通过。Console错误/警告0，项目规范0 warnings。独立代码审查无待修问题。
- scope-hashes.json：旧文件仅ShopSmallBundleSetup.cs、ShopOffersSetup.cs、MainMenuPage.prefab变更；新增ShopLargeBundleSetup.cs、2PNG及metadata，无运行时代码/关卡/场景/原美术/材质/字体修改。
- 新增2静态纹理（底板max2048、图标max512），无mipmap/ReadWrite；其他素材/字体/材质复用，无新增Update、加载、下载或支付功能。未采样设备性能，无Inspector待绑定字段。已移除临时长屏验证尺寸并恢复原MainMenu编辑状态。
- 内置imagegen的[来源与完整提示词](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/shop-large-bundle/manifest.json)；输出原样保存工作区work/shop-large-bundle/Safe.png、Panel.png，以及项目Assets/Sprites/UI/Shop/LargeBundle20261019/同名文件；源参考PNG不变，只设置原生SpriteRect。


## 2026-10-19 金币购买区及商城滚动

以图九两行三列布局为准（图八误发已更正）。用户确认金币区接在商城礼包下方，上下滚动查看；顶部经济栏/Shop标题与底部导航固定。六档为1.000/$1.99、5.000/$8.99、10.000/$16.99、25.000/$34.99、50.000/$64.99、100.000/$99.99，使用所给紫边奶油色卡片和六种金币堆图标。数量使用现有Fredoka粗体、金色填充与描边；绿色购买按钮及白色价格字复用当前商城样式。原四卡内容/原Button实例与回调保持，无支付/发奖/存档或金币加号行为改动。

使用Unity内置ScrollRect+RectMask2D，ShopContent/ShopScroll/Viewport/ShopOffers为滚动内容；原ShopOffers与卡片整体原生重挂父节点，保持引用，金币区在其末尾。Editor ShopCoinOffersSetup负责幂等生成/布局。ShopOffersSetup新增FindOffers集中兼容直接/嵌套父路径；Small/Large/RemoveAds工具使用它，Large的组装链末尾调用Coin Build，保证后续重建仍包含滚动与六金币档。无新增项目运行时MonoBehaviour，每帧滚动由Unity原生组件负责。

七张透明图内置imagegen从用户源图提取（原图不改、生成PNG不做像素编辑），直接引用Assets/Sprites/UI/Shop/CoinOffers20261019/，由MainMenuPage.prefab持有，无Resources新键；价格按钮/价格材质/字体复用，只新增金额材质。仅Editor资产操作。验收覆盖顶/底实际截图、拖动/滚轮滚动与边界、切换Home/Settings及弹窗输入阻挡、十个价格按钮无经济变化、格式/引用/遮罩、Apply及完整Build幂等和旧Button/卡片内容/顶部导航保持、既有菜单经济回归、Console及UI/项目规范。设备/IAP不在本次范围，静态UI不添加镜像测试。


### 本次验收完成 — 2026-10-19

- [1080×1920金币区](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/shop-coin-offers/shop-bottom.png)、[1080×2340金币区](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/shop-coin-offers/shop-bottom-2340.png)、[商城顶部](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/shop-coin-offers/shop-top.png)：两行三列，数量/价格正确，标题/经济栏/导航固定，RectMask2D正确裁切滚出内容。
- runtime-proof.json：原生UI拖动400px后NormalizedPosition由1降至0.60390，滚轮后0.26167；横滚关闭、纵滚启用、Clamped，RectMask2D存在；10个价格Button实际PointerClick各一次，callback均0，Coins/Stamina/Revision/Generation/Remaining/SelectedTab不变。
- modal-proof.json：弹窗打开后实际EventSystem射线命中SoundToggle（弹窗内），派发滚轮无背景位移；home-proof.json/settings-proof.json确认切换隐藏商城、Home经济栏保留、设置页可用，能够回切Shop。
- native-audit.json：完整Apply两次Prefab字节一致(stable=true)，scrollCount=1、coinCount=6、buttons=10，全部金币数值/价格/字体正确；UiPrefabValidator Valid=True、Issues=0。rebuild-proof.json：临时Prefab完整ShopOffersSetup.Build两次仍1个滚动区、6金币档、10个正确价格，不保存临时重建。
- prefab-scope.json：160个序列化对象变化均在ShopContent子树；旧ShopOffers全部fileID保留，全Prefab原Button组件逐字节保持；旧礼包对象只有5处RectTransform变化（内容根与四卡布局），原图标/标签/按钮不变；ShopTitle逐字节保持，顶部栏/导航/其他页面不变。
- test-result.json：既有菜单2项、经济UI2项PlayMode4/4通过，Console错误/警告0，项目规范0 warnings。独立审查无待修问题。
- scope-hashes.json：旧文件变更为4个Editor组装工具、MainMenuPage.prefab及FredokaBoldSDF.asset（补Coins标题字符）；新增ShopCoinOffersSetup.cs、7PNG、CoinAmount.mat及metadata。未改运行时代码/场景/关卡/原PNG/旧材质。art-integrity.json：7张生成PNG、工作区副本、项目副本SHA-256均一致。
- 素材路径Assets/Sprites/UI/Shop/CoinOffers20261019/{Card,Coins1000,Coins5000,Coins10000,Coins25000,Coins50000,Coins100000}.png；工作区原样副本work/shop-coin-offers/art/。内置imagegen来源及完整提示词见[manifest.json](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/shop-coin-offers/manifest.json)。只配置原生SpriteRect，不通过脚本修改图片像素。
- 7张纹理max512，无mipmap/ReadWrite，7图/1金额材质共享；价格素材和字体复用。新增内置ScrollRect/RectMask2D，无新增项目运行时Update或状态/支付。未采样设备性能，无Inspector待绑定字段。已恢复MainMenu编辑状态。


## 2026-10-19 去广告卡片新图替换（白色 NO-ADS）

用户要求只把商店 RemoveAdsOffer 背景换为 `商城界面之去广告.png`，其他不变。仅更新直接引用的 `Assets/Sprites/UI/Shop/RemoveAds20261019/Panel.png`，保留 GUID、Sprite ID 和现有引用；内置 imagegen 只提取透明背景，不重绘卡片/字体/边框，原参考图保持。Unity 原生导入器调整 SpriteRect，保留卡片布局、US$6.99 按钮、字体、右下内缩以及所有其他商品/主界面/导航/设置/玩法；若新素材比例不同，仅在现有 Artwork Rect 中适配，不调整卡片布局。无新 Resources 键、运行时代码或生命周期。

验收：透明 alpha/文字外观、当前商店实际截图、原生 Sprite/GUID 读回、所有非目标资产/原 Button 和布局精确保持、UI/项目校验和菜单运行期回归。静态素材替换不增加镜像测试，不重建整套菜单。


本次完成证据：仅 `Panel.png` 和其 `.meta`（原生 SpriteRect）发生改变；所有基线 C#、场景和 UI Prefab 逐字节一致，包括全部价格/购买按钮/布局/导航。GUID `85c484de5d69e4b18aac830511067d84`、Sprite fileID `-3351175042549865338` 保持；原生裁切 `(9,35,2005,673.5142)` 与既有比例 `2.976923` 一致。原 PriceButton 为 `164×50`、位置 `(-28,16)`、`US$6.99`、FredokaBoldSDF，未变。

- 内置 imagegen 只做透明背景提取，完整提示词、原图/生成图/工作区及项目保存路径见 [manifest.json](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/shop-remove-ads-white-refresh/manifest.json)。原图未修改；最终 PNG 原样复制，未重采样像素。
- 实际商店截图：[shop-final.png](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/shop-remove-ads-white-refresh/shop-final.png)；已目视核对白色 NO-ADS、金边、透明外围与按钮留白。
- `work/shop-remove-ads-white-refresh/{final-audit,scope-hashes,final-tests-results,final-console}.json`：Prefab Valid=True/Issues=0；最终 PlayMode job `18b304e14ad74a74a0e080bb5bebbab6`，MainMenuTabsTests/EconomyUiTests/NavigationBarAnimationUiTests 共 11/11 通过，无失败/跳过；最终 Console 无错误/警告，项目规范校验零警告。
- 无代码、购买行为或生命周期改变；未进行真机测试，无新增 Inspector 绑定需求。
