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
  Resources/GameTuning.asset    # 默认调参（也可放 Settings/）
  Scripts/Framework/            # 轻量 App 服务层
    GameServices.cs             # 组合根（非 DI）
    GameTuning.cs               # ScriptableObject 调参
    GameSettings.cs             # 静音 / 震动开关（PlayerPrefs）
    AudioService.cs             # SFX（尊重静音；空 clip 空操作）
    AppLifecycle.cs             # 前后台暂停门闩
  Scripts/Flow/                 # 自研可等待 Promise 框架
    Flow.cs / Flow.T.cs         # Flow / Flow<T>（可继承）
    StateMachine/               # FlowStateMachine + Host
    Flow.WhenAll.cs             # WhenAll 2–16 路组合
    FlowAwaiter.cs / FlowRunner.cs
    Examples/SplashCoverFlow.cs # 继承 Flow 的示例
  Scripts/Gameplay/
    GameFlowController.cs       # 启动 FSM：Booting → … → Playing
    GameManager.cs              # 对局 FSM：Idle ⇄ Flipping
    MatchState.cs / GameFlowState.cs / SplashView.cs / GameBootstrap.cs
    CoinController.cs / CoinSparkBurst.cs
  Scripts/UI/GameUI.cs
  Editor/CoinFlipEditorMenu.cs
```

## Flow 框架（带返回值的可等待）

行业常见写法（对齐 UniTask 一类库）：

- 继承 `Flow` / `Flow<T>`，或实现 `IFlowAwaitable` / `IFlowAwaitable<T>`
- `await flow`（在 async Task 中）或 `yield return flow.ToYieldInstruction()`（协程）
- `Flow.WhenAll(...)` 支持 **2–16** 个带返回值的 Flow，结果为 ValueTuple；可选 `CancellationToken`
- 工厂：`Delay` / `NextFrame` / `FromCoroutine` / `Create` / `FromResult`（均支持取消令牌）
- 主线程：`TrySet*` 自动切回主线程完成；`await Flow.SwitchToMainThread()`；`FlowRunner.Ensure` 禁止非主线程创建
- **取消**：`flow.AttachCancellation(ct)`，或工厂/`WhenAll` 传入 `CancellationToken`
- **Forget**：`flow.Forget()` 观察结果；故障会 `Debug.LogException`，取消不报错
- **未观察异常**：完成后若无人 `await`/`Forget`/`OnCompleted`，下一帧上报
- **对象池**：工厂创建的 `Flow`/`Flow<T>` 在 `GetResult`/`Forget` 后回收（子类不入池）
- **Awaiter**：实现 `ICriticalNotifyCompletion`（`UnsafeOnCompleted`）
- **PlayerLoop 时机**：`await Flow.Yield(PlayerLoopTiming.EndOfFrame)`；`NextFrame(timing)`
- **组合子**：`WhenAnyIndex` / typed `WhenAny`、`Timeout`、`Then` / `ContinueWith`
- **async Flow**：可写 `async Flow` / `async Flow<T>`（`AsyncFlowMethodBuilder`）
- **进度**：`Flow.CreateProgress<T>(...)`；`Create((flow, progress) => ...)`
- **续体调度**：`FlowRunner.ContinuationScheduling` = `Post`（默认）/ `Run`（主线程内联，少拖一帧）
- **StartRoutineAsFlow**：非主线程启动协程也可拿到可取消的 `Flow`
- **单续体优化**：第二等待者走字段，第三起才分配 List
- **编辑器双重异常栈**：`TrySetException` 附带设置点堆栈
- **ValueFlow / ValueFlow&lt;T&gt;**：已完成结果的零分配 struct awaitable
- **WhenAll 生成**：菜单 `CoinFlip/Flow/Regenerate WhenAll (2–16)`
- **流程状态机**：`FlowStateMachine<TState,TTrigger>`，支持 `AutoAdvanceTo` 自动推进、`Permit` 触发边、Enter/Exit → `Flow`、错误策略与 Busy 门闩
- **FSM 查询与等待**：`CanFire` / `TryFireAsync` / `IsIn` / `History` / `WaitUntilAsync` / `WaitUntilIdleAsync`
- **FSM Host**：可选 `FlowStateMachineHost<TState,TTrigger>`（生命周期 CTS + Start 时自动 `StartAsync`）

```csharp
var fsm = FlowStateMachine.Create<GameFlowState, GameFlowTrigger>()
    .Initial(GameFlowState.Booting)
    .OnError(FlowStateErrorPolicy<GameFlowState>.GoTo(GameFlowState.Failed))
    .State(GameFlowState.Booting, s => s.OnEnter(Boot).AutoAdvanceTo(GameFlowState.Splash))
    .State(GameFlowState.Splash, s => s.OnEnter(Splash).AutoAdvanceTo(GameFlowState.Entering))
    .State(GameFlowState.Entering, s => s.OnEnter(Enter).AutoAdvanceTo(GameFlowState.Playing))
    .State(GameFlowState.Playing, s => s.OnEnter(_ => Flow.Completed()))
    .State(GameFlowState.Failed, s => s.OnEnter(_ => Flow.Completed()).AutoAdvanceTo(GameFlowState.Playing))
    .Build();

await fsm.StartAsync(); // Booting → Splash → Entering → Playing
if (fsm.CanFire(GameFlowTrigger.ForcePlay))
    await fsm.TryFireAsync(GameFlowTrigger.ForcePlay);
```

## 启动流程

由 `GameFlowController` + `FlowStateMachine` 驱动（自动推进）：

`None` → `Booting` → `Splash`（`SplashCoverFlow`）→ `Entering` → `Playing`

状态 Enter 故障时进入 `Failed`，再自动 fail-open 到 `Playing`。未进入 `Playing` 前，抛币与清零输入锁定。

## 对局流程（Match FSM）

由 `GameManager` 内嵌 `FlowStateMachine<MatchState, MatchTrigger>`：

`Idle` —Flip→ `Flipping`（OnEnter：`coin.TryFlip` + 等待落地）—AutoAdvance→ `Idle`

Busy 时 Ignore；Enter 故障 GoTo `Idle`。`CanAcceptGameplayInput` 同时要求启动态 Playing、对局 Idle，且 App 未暂停。

## App 服务层

Booting 时 `GameServices.Ensure` 挂到根物体，经 `GameBuildContext` 分发给 Manager / Coin / UI：

| 服务 | 职责 |
|------|------|
| `GameTuning` | Splash/淡出/抛币手感/奖励文案/SFX 槽；缺省走 `Resources/GameTuning` 或运行时默认 |
| `GameSettings` | `Muted`、`HapticsEnabled`（预留）独立 PlayerPrefs |
| `AudioService` | `PlayToss` / `PlayLand` / `PlayUiClick`；静音或空 clip 时 no-op |
| `AppLifecycle` | `OnApplicationPause` / `Focus` → `IsPaused`；**不**改 `timeScale`，不取消飞行中抛币 |

HUD「声音」按钮切换静音。暂停时只锁新输入（`TryFlip` 返回 false）。

## 说明

不含 `Library/`。请使用与 `ProjectSettings/ProjectVersion.txt` 一致的编辑器版本。  
本环境无 Unity Editor，未做 Play Mode 实机验证；请在编辑器中点验：Booting 后 Services 非空、静音后无 SFX、切后台再回前台时抛币按钮锁定/恢复。
