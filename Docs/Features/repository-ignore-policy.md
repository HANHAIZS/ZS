---
id: repository-ignore-policy
title: 仓库忽略本机Skill与生成文件
status: Verified
owner: Project
created: 2026-10-21
updated: 2026-10-21
---

## Goal
用户要求忽略不必要目录后重新上传，并明确个人.agents/skills不上传。停止跟踪本机skill及生成报告，保留本机文件和可独立校验的工程源码。

## User-visible behavior
GitHub当前分支不包含.agents/skills、onboarding-report与MCP本机安装来源报告；缓存/工具会话/构建物不纳入。原本机文件继续可用，所有游戏代码/资源/关卡/Packages/ProjectSettings保持。项目治理校验在干净克隆中不依赖个人skills，必需配置/文档/架构/CI检查保持。

## Out of scope
游戏行为、Unity资产变更、卸载本机skill或MCP、删资源/依赖、重写历史/强制推送。旧历史中的文件仍可回看，不声明已从所有历史清除。

## Ownership and architecture
项目.gitignore负责排除本机文件；Scripts/validate_game_foundation_project.py的governance required集合去掉3项个人skill路径；AGENTS.md明确本机skill存在时使用，缺失时依照项目文档与原规则。Framework与游戏QFramework角色不变。

## State and persistence
只改变Git跟踪范围与校验工具，不改玩家状态/存档/运行时。

## UI, prefab, scene, and Inspector impact
无。所有Assets、Packages和ProjectSettings游戏文件保留，不新增Resources资源或Inspector绑定。

## Integrations and failure behavior
GitHub Actions沿用原workflow，干净checkout可通过规范校验；缺少关键共享治理文档仍明确失败。本机skill保留，git rm --cached只移出索引。以普通提交更新远端，推送冲突时不强推。

## Performance considerations
无运行时影响；后续提交排除生成文件，本次无需重传原游戏资源。

## Acceptance criteria
- [x] 当前索引不含.agents/skills及两份本机报告，本机字节仍与原版本相同。
- [x] 缓存/工具临时目录忽略，游戏文件/依赖及.meta保持跟踪。
- [x] 无个人skill的干净索引导出通过validator；关键文档缺失仍被拒绝；原项目/base校验通过。
- [x] 独立清理提交推送并核对GitHub最新提交一致。

## Verification matrix
| Boundary | Evidence | Result |
| --- | --- | --- |
| Git | 索引、忽略例子、本机保留、范围检查 | Passed |
| Governance | 干净导出正向/缺失关键文档负向、base校验 | Passed |
| Runtime | 无游戏/Unity资产变化，不启动Unity或重跑玩法测试 | Not applicable |

## Definition of Done
- [x] 范围与本机保留/源码完整有证据。
- [x] 校验与远端提交核对通过。


## Verification results
- 原本机13份skill和2份报告均存在且逐字节匹配旧HEAD；索引已无这些文件，git ls-files -ci为空。
- Library/Temp/Captures/work/.codex/Artifacts和Blender备份忽略例子正确；游戏Prefab、依赖package.json、project.json与原.meta继续跟踪。
- git diff的Assets/Packages/ProjectSettings范围仅Packages/unity-mcp-install-source.txt停止跟踪；没有游戏代码/数据/资产或依赖版本修改。
- 使用git checkout-index完整导出当前暂存快照到临时目录，确认不包含.agents、onboarding-report、MCP本机来源报告；在该干净副本运行validator通过，0警告。
- 临时副本删除Docs/Foundation/Architecture.md后validator按预期拒绝，仍保留共享治理检查。临时副本已清理；真实项目含base HEAD的规范校验同样通过，0警告。
- 本次不启动Unity或跑玩法测试，因为无Unity/游戏文件修改；验证覆盖Git跟踪和校验工具实际边界。
- 清理提交375d103已普通推送到origin/main；远端refs/heads/main精确读回375d10344c316362bbc7129f0c232714b26ceb8f，与本地清理提交一致，未强推或重写历史。
