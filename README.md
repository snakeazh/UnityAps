# CoinFlip — Unity 抛硬币（Android）

基于 **Unity 2022.3.62f3** 的竖屏抛硬币小游戏，目标平台 **Android**。  
抛硬币表现对齐休闲向《我的暴富日记》式 H5：暖色日记桌面、卡通金币、蓄力上抛 → 弹跳落地 → 花/字揭晓。

## 功能

- 点击屏幕或「抛一次」：蓄力下压 → 高抛快转 → 落地弹跳 → 轻微晃定
- 结果用「花 / 字」（一元硬币俗称），大字 punch 弹出
- 落地金色火花 + 浮动奖励文案（+¥1.88 / +¥0.88）
- 统计本地持久化；「清零」重置
- 竖屏与安全区适配

## 快速开始

1. 安装 [Unity Hub](https://unity.com/download) 与 **Unity 2022.3.62f3**（勾选 **Android Build Support**）
2. 用 Hub **打开本仓库根目录**
3. 打开 `Assets/Scenes/Main.unity` → Play

## Android 构建

| 菜单 | 作用 |
|------|------|
| `CoinFlip / Configure Android Player Settings` | 包名、竖屏、IL2CPP、ARM、MinSDK 23 / TargetSDK 34 |
| `CoinFlip / Build Android APK (Development)` | 输出 `Builds/Android/CoinFlip.apk` |
| `CoinFlip / Setup Main Scene` | 重新生成 Main 场景 |

- 产品名：`CoinFlip`
- 包名：`com.unityaps.coinflip`
- 方向：Portrait · IL2CPP · ARMv7+ARM64

## 工程结构

```
Assets/
  Scenes/Main.unity
  Scripts/Flow/                 # 自研可等待 Promise 框架
    Flow.cs / Flow.T.cs         # Flow / Flow<T>（可继承）
    IFlowAwaitable.cs           # 外部可实现的可等待接口
    Flow.WhenAll.cs             # WhenAll 2–16 路组合
    FlowAwaiter.cs / FlowRunner.cs
    Examples/SplashCoverFlow.cs # 继承 Flow 的示例
  Scripts/Gameplay/
    GameFlowController.cs       # 启动：Booting → Splash → Entering → Playing
    GameFlowState.cs / SplashView.cs / GameBootstrap.cs
    CoinController.cs / GameManager.cs / CoinSparkBurst.cs
  Scripts/UI/GameUI.cs
  Editor/CoinFlipEditorMenu.cs
```

## Flow 框架（带返回值的可等待）

行业常见写法（对齐 UniTask 一类库）：

- 继承 `Flow` / `Flow<T>`，或实现 `IFlowAwaitable` / `IFlowAwaitable<T>`
- `await flow`（在 async Task 中）或 `yield return flow.ToYieldInstruction()`（协程）
- `Flow.WhenAll(...)` 支持 **2–16** 个带返回值的 Flow，结果为 ValueTuple
- 工厂：`Delay` / `NextFrame` / `FromCoroutine` / `Create` / `FromResult`
- 主线程：`TrySet*` 自动切回主线程完成；`await Flow.SwitchToMainThread()`；`FlowRunner.Ensure` 禁止非主线程创建

```csharp
// 继承后可直接等待
public sealed class LoadConfigFlow : Flow<string>
{
    // 完成后调用 SetResult(json) / SetException(ex)
}

var (a, b) = await Flow.WhenAll(flowA, flowB);
yield return Flow.Delay(0.3f).ToYieldInstruction();

// 后台线程回到主线程后再碰 Unity API
await Flow.SwitchToMainThread();
transform.position = Vector3.zero;
```

## 启动流程

由 `GameFlowController` 驱动（内部已用 Flow）：

`Booting` → `Splash`（`SplashCoverFlow`）→ `Entering` → `Playing`

未进入 `Playing` 前，抛币与清零输入锁定。

## 说明

不含 `Library/`。请使用与 `ProjectSettings/ProjectVersion.txt` 一致的编辑器版本。
