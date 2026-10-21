---
id: colored-hive-chains
title: 双色圆润锁链与弹性跟随
status: Verified
owner: Project
created: 2026-10-20
updated: 2026-10-20
---

# 双色圆润锁链与弹性跟随

## Goal
按用户的锁链演示1.png及已确认方案，替换当前金色细线/链环标记，呈现厚实圆润、亮面高光、暗边描线的双色连接杆。只改变连接组表现。

## User-visible behavior
- 每条连接杆从A到中点使用A蜂巢/对应方块的色板色，从中点到B使用B颜色；同色两端保持一色。
- 圆头连接杆具有高光、暗部与深色边缘，位于额度标签后方，不遮挡数字。
- 跟随现有同步起跳与落位，带轻微弯曲弹性；两端始终贴住实际蜂巢位置，无闲置持续晃动。
- 暂停、切后台、重试、跨关或任一端不可见时，取消运动/隐藏连接，不留悬空杆。

## Out of scope
连接组拓扑、全员队首/槽数/任意选取/洗牌规则、蜂巢造型与额度、关卡内容、UI、存档、音效、Foundation。

## Ownership and architecture
| Responsibility | Owner | Planned type/path | Notes |
| --- | --- | --- | --- |
| State mutation | None | Existing Commands unchanged | Presentation only |
| Observable or persistent state | None | HarvestModel unchanged | No save changes |
| Shared project rules | None | HarvestSystem unchanged | Existing linked rules |
| One-shot notification | None | None | Existing hop time |
| Unity presentation | ViewController | HiveModelView, HiveChainVisual | Cached geometry, colors, motion |
| Adapter or repository | Utility | LevelOneAssets | Cache separate rounded chain material |
| Reusable infrastructure | None | None | Foundation unchanged |

## State and persistence
Only transient meshes, property blocks and existing hop elapsed time. No data migration, new commands or direct model mutation.

## UI, prefab, scene, and Inspector impact
- All existing scenes, prefabs, art, materials and Inspector references preserved.
- New Assets/Resources/Materials/HarvestRoundedChain.mat, Resources key Materials/HarvestRoundedChain; directly references shader Assets/Sprites/Gameplay/Shaders/HiveRoundedChain.shader.
- LevelOneAssets caches material; HiveModelView owns/reuses each edge visual, releases runtime meshes on destroy. Shared resource material is never destroyed.
- Runtime edge roots retain HiveChainLink_A_B names. Old gold double-ring badges are removed from presentation; no serialized hierarchy changes.
- Applies to all 95 gameplay scenes through existing HiveModelView. No Inspector work.

## Integrations and failure behavior
No SDK/network/content update. Existing hop cancellation, source visibility and generation changes determine rendering. Degenerate coincident endpoints hide rather than producing invalid geometry. Missing material explicitly reports via resource loader. Meshes are cleaned on scene destruction; shared loaded material survives.

## Performance considerations
One shared shader/material and one reusable small dynamic mesh/property block per visible connection. No collider, texture, physics, new per-frame controller or repeated dependency/resource lookup. Vertex arrays allocated once per edge; updates only through existing Sync/ShowHop. Cache bounded by level hive edges; clear old edges on new generation.

## Acceptance criteria
- [x] Both halves take exact endpoint palette identity with a hard midpoint split; same colors remain continuous.
- [x] Rounded bright/dark/beveled rod matches the reference style at portrait resolution without hiding quotas.
- [x] Rod follows both roots through hop, land, cancel and layout changes; no stuck bend or dangling rod.
- [x] Reused geometry/material, generation cleanup, scene disposal and degenerate endpoints are safe.
- [x] Existing activation rules/content/UI unchanged; compile, focused regression tests, validator and native visual checks pass.

## Verification matrix
| Boundary | Evidence required | Result |
| --- | --- | --- |
| Conventions | Project validator | Passed (see evidence below) |
| Compile/shader | Clean compile and shader error readback | Passed (see evidence below) |
| PlayMode | LinkedHivePresentationTests plus new ColoredHiveChainTests | Passed (see evidence below) |
| Regression | EditMode and project PlayMode suites | Passed (see evidence below) |
| Native visual | Portrait mixed/same-color rods, hop/land/cancel screenshots | Passed (see evidence below) |
| Scope/performance | Baseline hashes, resource lifecycle, cached arrays review | Passed (see evidence below) |

## Definition of Done
- [x] QFramework ownership and no rules mutation verified.
- [x] Acceptance criteria covered with actual evidence.
- [x] Loading, cancellation, lifecycle and performance checked.
- [x] Validation/compilation/tests passed; limitations reported.
- [x] Status Verified only after evidence.

## Decision log
- 2026-10-20: User approved bounded design; add screen-facing shaded capsule mesh rather than physics joints to keep the illustrated reference style and existing synchronized hop timing.

## Verification evidence (2026-10-20)
- Tests written first: job d2c3793fb723425d8671557c547d41ec failed 3/3 on the missing shaded rod, proving the new rendering contract was absent. Initial green run found only a 2.98e-8 MaterialPropertyBlock float readback difference; color comparisons now use 1e-5 tolerance, substantially below one color channel step.
- Full EditMode: 31f167e4e4f04d53b98e613a154a37db, 256/256 passed.
- Full PlayMode: d2dc4c2c8f214be4bc2c8be234a85a60, 173/173 passed in 519.45s. Includes all 95 scenes, queue layouts, old linked atomic activation/cancellation, transport, audio, modals and UI flows.
- Final lifecycle refinement: independent regression 753cd363175d44bf8ca59a76c67eb6b4 reproduced an active old rod until frame end. Dispose now immediately hides before deferred Destroy. Endpoint palette parsing moved to edge construction; no recurring color parse.
- After that final refinement, focused PlayMode a3c6ec4ad8fb42149ba80e78381eadee passed 7/7. The full 173-test run above preceded this small cleanup/cache change; no full-suite claim is made for the new total of 174 tests.
- Native portrait captures mixed-idle.png, mixed-hop.png, mixed-land.png and same-idle.png all 1080x1920, visually inspected: sharp half-color split, rounded caps, high/low shading, subtle bend and clear quota labels. Fixtures use real source colors/quotas and only temporarily rewire the first pair in isolated memory; production level JSON unchanged.
- Native shader: BeeHarvest/HiveRoundedChain supported, zero shader errors. Shared Resources material and 34 vertices per rod. Warmed 100 connection refreshes measured zero managed allocated bytes on the Editor main thread. Coincident endpoints hide correctly.
- Scope hashes: all 95 level JSON, all existing scenes/prefabs/materials/asset files identical to baseline. Three UI assets regenerated by pre-existing Editor tests and two material float normalizations were restored through native FileUtil and checked byte-for-byte. Only three intended existing C# files changed; added view, shader, material, tests and this spec. No old art modified.
- Final Console: 0 errors. Temporary settings/economy isolation restored; Play Mode stopped; MainMenu scene restored. No Inspector changes needed.
- Project convention validator passed with 0 warnings.
- Independent read-only requesting-code-review review and final delta re-review found no remaining critical/important issues.
- Evidence directory: /Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/colored-hive-chains. Test JSON, scope, native performance and screenshots retained there.

## Verification limits
- Visual and allocation checks are from Unity 2022.3.62f3c1 Editor. Mobile GPU shader compilation/performance remains a device check; this change adds no texture, physics joint, collider or per-frame resource lookup. No device claim is made.
