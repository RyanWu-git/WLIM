# ChatAppWpf

## 依赖与环境
- Windows 10/11
- Visual Studio 2022（勾选“桌面开发（.NET）”工作负载）或安装 .NET 8 SDK

## 配置 Supabase
1. 复制 `appsettings.template.json` 为 `appsettings.json`
2. 填入：
   - `SupabaseUrl`
   - `SupabaseAnonKey`

`appsettings.json` 已被加入忽略规则，不建议提交到仓库。

## 运行
- 用 Visual Studio 打开 `ChatAppWpf.csproj`，直接运行（F5）

## 当前实现范围
- 登录窗口：调用 Supabase Auth 的 password grant 登录
- 主窗口：左侧聊天/好友双 Tab，右侧聊天窗（目前为演示数据）

## 下一步（接入真实数据）
- 使用 `SupabaseRestClient` 拉取会话/消息与发送消息
- 需要在 Supabase 端开启 RLS 并完成表结构与策略
