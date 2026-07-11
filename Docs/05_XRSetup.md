# 05 — XR 設定與 XR Origin 結構

> 最後驗證：2026-07-11（場景：Main.unity，XR Package Settings 直接讀取）

---

## XR Origin 場景結構

**場景路徑**：`----------Main-----------/Camera/XR Origin (XR Rig)`

```
Camera  [active]
  ├── MainCamera  [inactive]          ← PC 模式第一人稱相機
  │     Components: Camera, AudioListener, FirstPersonCam
  │
  └── XR Origin (XR Rig)  [active]   ← XR 模式 VR 相機與控制器
        Components: XROrigin, XRInputModalityManager, OpenXRCanvasRaycasterAdder
        Layer: Ignore Raycast
        │
        ├── Camera Offset
        │     ├── Main Camera  [active]         ← VR 主相機
        │     │     Components: Camera, AudioListener, TrackedPoseDriver
        │     │     Tag: MainCamera
        │     │
        │     ├── Left Controller  [active]
        │     │     Components: ActionBasedControllerManager, ActionBasedController, XRInteractionGroup
        │     │
        │     ├── Left Controller Stabilized  [active]
        │     │     Components: XRTransformStabilizer
        │     │     └── Left Controller Stabilized Attach
        │     │
        │     ├── Right Controller  [active]
        │     │     Components: ActionBasedControllerManager, ActionBasedController, XRInteractionGroup
        │     │     └── Ray Interactor  [active]    ← 右手 UI 射線
        │     │           Components: XRRayInteractor, LineRenderer,
        │     │                       XRInteractorLineVisual, SortingGroup
        │     │           Position offset: (0, -0.02, -0.035)
        │     │
        │     └── Right Controller Stabilized  [active]
        │           Components: XRTransformStabilizer
        │           └── Right Controller Stabilized Attach
        │
        └── Locomotion System  [inactive]        ← 移動系統（預設關閉）
              Components: LocomotionSystem
              ├── Turn  [active]
              │     Components: ActionBasedSnapTurnProvider, ActionBasedContinuousTurnProvider
              ├── Move  [active]
              │     Components: DynamicMoveProvider  ← 繼承 ActionBasedContinuousMoveProvider（含 #pragma disable CS0618）
              ├── Grab Move  [inactive]
              │     Components: GrabMoveProvider × 2, TwoHandedGrabMoveProvider
              ├── Teleportation  [active]
              │     Components: TeleportationProvider
              └── Climb  [active]
                    Components: ClimbProvider
```

### 注意事項

- **Locomotion System 整體為 inactive**：目前專案不開放玩家自由移動，課程中位置固定。
- **右手才有 Ray Interactor**：UI 互動只有右手支援，左手不做 UI 射線。
- **OpenXRCanvasRaycasterAdder** 掛在 XR Origin 上，Awake 時自動為所有 Canvas 加入 `TrackedDeviceGraphicRaycaster`。

---

## XR Loader 設定（XRGeneralSettings.asset）

| 平台 | 啟用的 Loader |
|------|-------------|
| Android | OpenXR Loader |
| Standalone（PC） | OpenXR Loader |

---

## OpenXR Features 設定（OpenXR Package Settings.asset）

### Android 平台

| Feature | 啟用 | 說明 |
|---------|:----:|------|
| OculusXR Feature | ✅ | Quest Android 必要 |
| OculusTouchControllerProfile | ✅ | Quest 控制器輸入 |
| MetaQuestTouchPlusControllerProfile | ✅ | Quest Touch Plus 控制器 |
| VIVEFocus3Profile | ✅ | VIVE Focus 3 控制器 Profile |
| ViveEnterpriseCommand Android | ❌ | ⚠️ Quest 上 SIGSEGV crash：Meta runtime 不支援 HTC OpenXR 擴展（XR_HTC_*） |
| ViveCompositionLayer Android | ❌ | ⚠️ 同上，必須關閉 |
| ViveCompositionLayerPassthrough Android | ❌ | ⚠️ 同上，必須關閉 |
| VIVEFocus3Feature Android | ❌ | ⚠️ 同上，必須關閉 |

> **重要**：上方四個 VIVE Android features 在 Quest 上啟用會導致 app 啟動即 crash（Thread-4 SIGSEGV，`libunity.so`）。  
> 原因：這些 feature 試圖初始化 HTC 專屬 OpenXR 擴展，Meta runtime 完全不支援，native 層直接崩潰。  
> VIVE Focus 3 實體機出包時才需要重新啟用。

### Standalone 平台（PC / Quest Link）

| Feature | 啟用 | 說明 |
|---------|:----:|------|
| OculusXR Feature | ✅ | Quest Link 必要 |
| OculusTouchControllerProfile | ✅ | Quest Link 控制器輸入（2026-07-11 啟用）|
| 其餘 Features | ❌ | — |

---

## Quest Link 測試設定

Quest Link 讓 Quest 2 作為顯示 / 輸入裝置，遊戲在 PC 上執行。

### 前置條件

1. PC 安裝 **Meta Quest Link** app
2. Quest 2 透過 USB 或 Air Link 連接 PC
3. Meta Quest Link app → `Settings > General > Set Meta Quest Link as active OpenXR runtime` ✅
4. Unity Build Target 切換為 **Windows Standalone**（可用 PlatformBuilder → Switch to PC）

### 測試方式

```
切換到 PC Build Target（PlatformBuilder → Switch to PC）
  ↓
確認 Quest Link 已連線
  ↓
Unity Play Mode → XR 系統初始化 → Quest Link OpenXR Runtime
  ↓
畫面輸出到 Quest 2 頭戴裝置
Game Window 顯示鏡像畫面
```

---

## 平台切換後的 XR Feature 狀態對照

| 目標 | 需要的 Feature 組合 |
|------|-------------------|
| VIVE Focus 3（Android APK） | OpenXRLoader + OculusXR(on) + VIVEFocus3Profile(on) + VIVEFocus3Feature(on) + ViveCompositionLayer(on) |
| Quest Link（PC 測試） | OpenXRLoader + OculusXR(on) + OculusTouch Standalone(on) |
| Quest（Android APK）| OpenXRLoader + OculusXR(on) + OculusTouch(on) + MetaQuestTouchPlus(on) + **全部 VIVE Android features(off)** |

---

## Quest APK 出包注意事項

### AndroidManifest.xml（必要）

`Assets/Plugins/Android/AndroidManifest.xml` 必須包含 VR 類別，否則 app 以 2D 面板模式啟動（XR Session 卡在 VISIBLE，無法進入 FOCUSED）：

```xml
<category android:name="com.oculus.intent.category.VR" />
```

### VIVE OpenXR Features（必要關閉）

Quest 出包前，確認以下四個 VIVE Android features 為 **關閉** 狀態  
（`Assets/XR/Settings/OpenXR Package Settings.asset`）：

- ViveEnterpriseCommand Android
- ViveCompositionLayer Android  
- ViveCompositionLayerPassthrough Android
- VIVEFocus3Feature Android

> 忘記關閉 → app 啟動即 crash，無任何 C# 錯誤訊息（native crash）。

### Gradle 設定

| 檔案 | 用途 |
|------|------|
| `Assets/Plugins/Android/settingsTemplate.gradle` | 將 Firebase 本地 m2repository 加入 settings 層 repo，修正 `:launcher` 找不到 Firebase 依賴的問題 |
| `Assets/Plugins/Android/gradleTemplate.properties` | 移除已廢棄的 `android.enableR8`（AGP 8.x 不相容）|

---

## DynamicMoveProvider 說明

**路徑**：`Assets/Samples/XR Interaction Toolkit/2.4.3/Starter Assets/Scripts/DynamicMoveProvider.cs`

繼承 `ActionBasedContinuousMoveProvider`（Unity 6 中已標為 deprecated），  
目前以 `#pragma warning disable CS0618` 暫時壓制警告。  
等 XRI 提供正式替代方案後更新。
