---
id: mystery-hives
title: 五关隐藏蜂巢与揭晓动画
status: Verified
owner: Project
created: 2026-10-20
updated: 2026-10-21
---

# 五关隐藏蜂巢与揭晓动画

## Goal
用户确认灰色蜂巢/问号隐藏标记；隐藏时不显示额度，问号居中，到真实队首时约0.3秒揭晓额度与预设颜色。第6/8关标记移到更后方并错开行数。原始额度仍保留在Model。

## User-visible behavior
- 选择第6关Chick、第8关Shark、第12关CatHead、第15关DodoBird、第18关Rat。第6/8关左中右列分别第3/4/5只，第12/15/18关各列第2只，共3只/关、15只；开场各列队首正常。
- 隐藏时仅蜂巢主体呈中性灰，不透露预设颜色；非交互问号位于实际蜂巢渲染中心；额度数据保留但文字隐藏，其他蜂巢正常。
- 到真实队首时开始0.3秒轻微放大/左右抖动、灰色向原色渐变、问号淡出、额度淡入。揭晓一次后本轮不再隐藏，洗牌/翻页不会重新遮盖。
- 任意选择直接选后排时，跳跃期间保留隐藏，成功入槽提交后再揭晓；取消跳跃不能提前窥色或消耗额外库存。
- 暂停/设置/后台/主界面时停止揭晓时钟，恢复后继续。重开关卡按配置恢复隐藏，不修改原数据。完成或换场景不留问号/属性块。

## Out of scope
随机颜色/奖励抽取、付费盲盒、其他90关配置、关卡几何/配额/队列/锁链拓扑、现有UI/音效/存档/Foundation。此批五关均无锁链，暂不编排隐藏+锁链组合关卡。

## Ownership and architecture
| Responsibility | Owner | Planned type/path | Notes |
| --- | --- | --- | --- |
| State mutation | Existing Command | Existing Start/Activate/Shuffle unchanged | Presentation reads committed state |
| Config | Model | HiveData.isHidden | Optional false; no save schema change |
| Shared project rules | Existing System | HarvestSystem unchanged | Front/selection rules unchanged |
| One-shot notification | None | None | Existing state revision |
| Unity presentation | ViewController | HiveModelView, HiveMysteryVisual, LevelOneController | Generation-local revealed state and clock |
| Authoring | Project script | import_cube_land_levels.py | Explicit five sourceRid recipes; imports source isHidden while retaining existing unsupported-state checks |
| Foundation | None | None | No shared package changes |

## State and persistence
Optional bool isHidden in HiveData; immutable input. Reveal progress and already-revealed state are local view state, per actual hive identity and generation. No model writes from views, no persistent revealed state, no economy/schema change. Existing commands remain sole gameplay mutation path.

## UI, prefab, scene, and Inspector impact
- Only five existing Resources Levels/Level06,08,12,15,18 JSON add flags. All names, IDs, source metadata, geometry, colors, quotas, queue order and connected arrays unchanged. Other90 JSON byte-identical.
- No new serialized prefab, scene, material, texture or font. HiveMysteryVisual reuses each renderer's shared material, cached per-slot property blocks and original colors. Gray affects tint slots only; dark mouth/interior remains.
- Question Text instances are runtime children of existing quota Text, same font, white with dark outline, raycastTarget=false. Existing controls and toolbar order preserved. Loaded by existing scene component; owned and destroyed with generation/view.
- Inspector references and resource loading paths unchanged.

## Integrations and failure behavior
No SDK/network/hotupdate. Actual front means rules.Front(column), not first visible page. Rear selection only reveals after Started=true; stale/cancelled hops leave it hidden. Zero/invalid animation deltas ignored; disabled, paused or settled views do not advance. Dispose hides markers immediately, restores original material property blocks, releases generated children on generation/scene destruction.

## Performance considerations
Only flagged hives allocate one helper, Text and cached material-slot blocks on first visibility per generation. Reuse shared materials; no Renderer.material copies or new resource loads. Existing controller drives reveal ticks, no additional Update component/coroutine. Force view layout only while an active reveal advances; never re-place ordinary hives solely for an idle hidden marker. Cache flags to avoid new per-frame work in the other90 levels.

## Acceptance criteria
- [x] Exactly five listed levels,3 hidden hives each; Level06/08 depths3/4/5, others depth2; other content and quotas/colors unchanged and import reproducible.
- [x] Gray only intended hives; hidden quota invisible, centered question readable; original quota data and pointer inputs unaffected.
- [x] Front promotion triggers0.3s reveal with intermediate frame; pause freezes; completion stable after shuffle/page.
- [x] Pick Any cancellation cannot leak color; successful commit reveals once without changing inventory/rules.
- [x] Retry resets, scene exit cleans runtime UI/blocks, no shared material changes.
- [x] Compile, importer tests, runtime regressions, native screenshots and project validator pass.

## Verification matrix
| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project | Convention validator, clean Unity Console | Passed; see evidence below |
| Import/data | Python reproducibility and flags/unchanged-baseline audit | Passed; see evidence below |
| Runtime | MysteryHiveTests production scenes, front/pause/Retry/PickAny/shuffle | Passed; see evidence below |
| Regression | Existing linked/layout/booster/modal/audio and editor tests | Passed; see evidence below |
| Visual | Portrait hidden/intermediate/revealed screenshots | Passed; see evidence below |
| Lifecycle/performance | Original material identity, no leaked markers, idle no repeated lookups | Passed; see evidence below |

## Definition of Done
- [x] Model/config/view ownership correct; no gameplay rule mutation.
- [x] User behavior and failure/lifecycle cases verified.
- [x] Existing scene/UI/resource refs preserved; only authorized five data files changed.
- [x] Required checks and exact native captures pass; limitations stated.
- [x] Spec Verified after evidence only.

## Decision log
- 2026-10-20: Bounded approved design; user delegates five-level choice. Five animal levels without links selected to introduce mechanic clearly.

## Verification evidence (2026-10-20)
- Red runtime job0a77d97e281e4767b0f514a9beedb73c:4/4 expected failures because question markers were absent. Source hidden and authored flags tests also failed before implementation; explicit hidden-source reauthoring rejection was independently seen red before its guard.
- Python import/content suite25/25 passed, including all95 deterministic imports. Final rerun25/25 passed in4.66s.
- First combined runtime5c1e8bf4c9e84755ba12d00d04df9762:10/11 passed, including all4new tests. Existing ColoredHiveChainTests emitted an intermittent destroyed-Image log during next-scene load because its preceding test left an unfinished hop. An isolated2-test retry passed; fixture-only TearDown now cancels its own hop. No production cancellation behavior was changed.
- Combined runtime8996eae783c44df59f35a750a2150820:11/11 passed after fixture cleanup.
- Full EditMode0a48fa92479646f58ee24fc1a45d2ca7:256/256 passed,17.93s.
- Full PlayModec43477f9101f43f08a2fdbba8cc88410:177/178 passed in408.30s. The only failure was MysteryHiveTests.RearPickStaysHiddenUntilSuccessfulCommitAndCancelledHopDoesNotPeek line112: its fixture assumed unused Pick Any inventory, but earlier tests correctly consumed it. Fixed only test preparation to refill required items through existing Commands, and assert Pick Any/Shuffle actually succeeded.
- Reproduced empty inventory before final rerun using real commands (PickAny=0,Shuffle=0,pending=false). Final combined runtimeae8bf6e36ad2474fb90e5f976e9f9227:11/11 passed, including the previously failing test. Production implementation was unchanged after the full run; all other177 checks had already passed. Do not describe this as a single clean178/178 run.
- Native Level06 and Level12 hidden/intermediate/revealed portrait captures inspected, all6PNG1080x1920: gray body, original quotas, white outlined question, intermediate tint/fade/light scale, final original color. Level12 specifically reveals red. Preview isolation restored original settings/economy afterward; Play Mode stopped and MainMenu reopened.
- data-audit.json: five old JSONs identical after stripping exactly15true flags; all other90levels byte-identical. No source JSON, geometry, quota, original color, queue or connection edits.
- scope.json: all existing scenes/prefabs/materials/art/audio byte-identical to baseline. Three serialized UI assets regenerated by pre-existing tests and two material float normalizations restored through native FileUtil. Changed existing Assets are only3production C# files,1old test fixture and5levelJSON. Added HiveMysteryVisual, MysteryHiveTests, Python test and this spec; importer and one existing Python test updated.
- Native performance readback:1000warmed idle mystery ticks allocated0managed bytes in Editor. One reused helper/Text per flagged visible identity, shared materials preserved, no extra Resources asset or Inspector setup.
- Independent read-only review found no Critical/Important issues and independently confirmed all25Python tests and the exact15flag diff.
- Project validator passed with0warnings. Last compile/read_console before final tests reported0errors, and final11runtime tests produced no unhandled logs. After the final native asset restore, additional Console requests returned a Unity transport ping timeout; this final extra readback is unavailable (not treated as zero errors). No code changed after the passing final tests.
- Full evidence directory: /Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/mystery-hives.

## Verification limits
Editor verification is on Unity2022.3.62f3c1. No mobile-device profiling or GPU claim. Hidden+linked combinations are not authored in this five-level batch; existing24locked levels are unchanged. Original source hidden states that would be lost by surface queue reauthoring explicitly fail instead of silently dropping the flag.


## 2026-10-21 盲盒后移错行、隐藏额度与居中问号

用户要求第6/8关盲盒往队列后移且不在同一行；隐藏时不显示额度、问号居中，到队首再显示额度和恢复原色。此前“第二行/保留可见额度”的记录为历史行为，由本节覆盖。

### Scope and ownership
- 第6、8关各保留3只盲盒，左中右列分别选择第3/4/5只（零基深度2/3/4），仅迁移isHidden标记；导入器按稳定sourceRid复现。第12/15/18关位置不改，其他90关与几何/顺序/配额/颜色/连接不改。
- 已有全部盲盒统一隐藏额度，问号位于实际蜂巢渲染包围盒投影中心。额度仍存在Model，保留射线点击和原位跃动；到真实队首才启动原0.3s揭晓，额度同步淡入，恢复原色。Pick Any沿用成功入槽后揭晓，未提交/取消不提前泄露。
- HiveMysteryVisual负责显示与清理，复用原Text、字体与CanvasRenderer透明度；不禁用原按钮/射线，不修改Model或Command。HUD仅刷新输入时也不能泄露额度；换行/翻页/普通蜂巢复用同一Text需复原透明度。
- HiveModelView负责最终布局/原位跃动后重新居中问号，灰色状态和数字位置不累积偏移。重开/卸载复原透明度、材质属性块并清理问号。
- 无新美术/资源键/场景/Prefab/Inspector/存档/Foundation修改；复用每个盲盒缓存的Renderer[]，按需计算bounds，无逐帧对象实例化。

### Acceptance and verification
- [x] 第6/8关明确后移错行且仅isHidden迁移，五关共15标记，确定性导入一致。
- [x] 隐藏额度不可见，问号居中；输入锁刷新/原地跃动/翻页仍不泄露额度、普通标签不被隐藏。
- [x] 队首原0.3s渐变恢复颜色和额度；暂停/重开/Pick Any取消及提交/洗牌/场景退出保留。
- [x] Python配置与导入测试、针对性Unity运行测试、编译/Console/规范检查、原生截图及资产哈希证据通过。

### Evidence
- 配置Red：test_mystery_hives.py精确位置断言先失败（期望零基2，实际1）；修改后全部Python25/25通过，含95关确定性导入。data-audit.json核对第6/8关只有isHidden迁移，其他93份JSON字节不变。
- 初始Runtime Red：5b7b032eea594f8abaa4ef1ce1cbeab1，4/4按预期失败，表现为旧位置和隐藏额度alpha=1。首轮e227d3f46717499d8377a2c2f5988f7c，29/29通过。
- 独立复核发现洗牌标签互换的额度泄露，增加Level12 seed27测试；58f8644015c9473085a573b7780c91ce中3/4通过，新增测试真实复现隐藏额度alpha=1。Hide释放标签绑定后修复；最终42202513ae0a4dd28ffe8cdba3adeef5，30/30通过，包括新位置/中心/隐藏额度/输入刷新/原地跃动/翻页/洗牌交换与后续揭晓，以及原队首渐变/暂停/重开/PickAny取消与提交/材质/链组/点击入槽/队列晋升。
- EditMode c78b22ff65c843b6a178b2ecdf5fa9ca，256/256通过（在最后一行标签解绑修复前）；最后修复经最终30项相关PlayMode验证。没有重跑全部191项PlayMode，不作全量通过声明。
- 单独运行洗牌测试97ac7f...曾因Unity测试初始化中断而卡住，场景组件没有正常Awake并产生空引用；原生停止、清除孤立MCP测试任务后重跑四项，获得上述真正的Red。中断留下一个DontSave临时Code-based tests runner和两份InitTestScene，首次原生预览退出时触发测试框架BackgroundWatcher空引用。仅清理这些测试产物后重新完整预览第6/8关，最终Console0errors。没有为测试工具异常修改产品逻辑或Framework包。
- 原生6张1080x1920截图：第6/8关开场、PickAny翻页查看的错行盲盒；第6关队首揭晓中间/结束。已查看问号中心、不显示额度、灰色主体、正常显示普通额度，队首alpha中间0.5/最终1并恢复原色。原生JSON读回见initial6/8、page6/8、middle6、revealed6。页面翻阅仅用于测试呈现，未改产品流程。
- 预览后恢复原偏好/经济、停止Play、返回MainMenu；恢复5份测试生成的UI/材质，删除测试临时场景和孤立runner。scope.json确认原有Assets仅2份生产C#、2份测试与Level06/08 JSON变化；新增4项测试文件与meta。Script导入器及其测试另行更新。场景/Prefab/美术/音频原字节保持。
- 最后编译无错误；最终项目validator通过0warnings，Console0errors。复核确认洗牌问题已关闭、无剩余重要问题。
- 性能：复用Renderer数组和Text/CanvasRenderer，无每帧新对象、无资源加载或架构查找；问号只在展示刷新和跃动时重新对齐。没有iPhone实机触控/帧率验收或GPU性能声明。
- 证据目录：/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/mystery-refinement。
