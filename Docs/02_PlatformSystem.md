# 02 — 平台切換系統

> 最後驗證：2026-07-11（場景：Main.unity，Runtime 讀取確認）

---

## 系統概述

平台切換由兩個元件協同完成：

| 元件 | 類型 | 職責 |
|------|------|------|
| `PlatformBuilder` | EditorWindow | Build 前切換 XR Loader、Build Target，呼叫場景切換 |
| `PlatformSwitcher` | Scene MonoBehaviour | 場景內物件（相機、UI、EventSystem、InputHandler）的啟關 |

---

## PlatformBuilder（Editor 工具）

**路徑**：`Assets/2. XRCV_VirtualRoom/Core/Classroom/Editor/PlatformBuilder.cs`  
**開啟方式**：Unity 選單 → `PlatformBuilder > Builder`

### 目前可用按鈕

| 按鈕 | 執行動作 |
|------|---------|
| Switch to PC | `BuildTarget → StandaloneWindows64`，呼叫 `OnSwitchToPC()` |
| Switch to OpenXR | `BuildTarget → Android`，移除 OculusLoader，加入 OpenXRLoader，呼叫 `OnSwitchToOpenXR()` |
| Switch to Oculus | `BuildTarget → Android`，移除 OpenXRLoader，加入 OculusLoader，呼叫 `OnSwitchToOculus()` |

> ⚠️ Build 按鈕群目前被 `/* */` 全部註解，只有 Switch 按鈕可用。

### 切換後呼叫流程

```
PlatformBuilder.SwitchToXxx()
  ├─ EditorUserBuildSettings.SwitchActiveBuildTarget(...)
  ├─ AddLoaderForAndroid<T>() / RemoveLoaderForAndroid<T>()  ← XR Loader 操作
  ├─ CallPlatformSwitcherMethod("OnSwitchToXxx")             ← 呼叫場景內 PlatformSwitcher
  ├─ CallClassroomManagerMethod()                            ← ClassroomManager.UpdateVersion()
  └─ SaveCurrentScene()                                      ← 若 Scene 有變動則儲存
```

### ⚠️ 已知技術債（P0）

`Switch to Oculus` 目前走 **OculusLoader 原生 SDK 路徑**（`com.unity.xr.oculus 4.5.2`），  
此套件在 Unity 6 已標為 Deprecated。  
建議改為 **切換 OpenXR Feature** 的方式，詳見 [OptimizationGuidelines.md](OptimizationGuidelines.md)。

---

## PlatformSwitcher（Scene 元件）

**路徑**：`Assets/2. XRCV_VirtualRoom/Core/Classroom/PlatformSwitcher.cs`  
**場景位置**：`----------Main-----------/PlatformSwitcher`  
**繼承**：`SerializedMonoBehaviour`（Odin Inspector）

### 場景子物件

```
PlatformSwitcher
├── EventSystem_PC      [inactive] — StandaloneInputModule（PC 滑鼠鍵盤事件）
└── EventSystem_OpenXR  [active]  — XRUIInputModule（XR 控制器 UI 事件）
      ├── XR Interaction Manager  — XRInteractionManager
      ├── Input Action Manager    — Unity InputActionManager（Action Asset 掛載點）
      ├── OpenXRInputFieldKeyboardListener  — 監聽 InputField，彈出虛擬鍵盤
      └── UIHelpers  [inactive]   — XRRayInteractorUICursor
            └── Canvas → Image    — 射線 UI Cursor 圖示
```

### Platform 定義

```csharp
public enum Platform { PC, OpenXR }
```

> 注意：目前沒有獨立的 `Quest` 平台。VIVE 和 Quest Android 在 Scene 層共用 `OpenXR` 設定，  
> 差異只在 XR Loader / Feature 層（由 PlatformBuilder 控制）。

### platformSwitchSettings 完整設置（Runtime 驗證）

#### PC 平台
| 欄位 | 值 |
|------|----|
| mainUIGameObject | `MainUICanvas_Main`（位於 MainUI_XR/UI/） |
| inputHandler | `Input_PC` → `InputHandler_PC` |
| activeGameObjects[0] | `MainCamera`（PC 第一人稱相機，FirstPersonCam）|
| activeGameObjects[1] | `EventSystem_PC`（StandaloneInputModule）|
| activeGameObjects[2] | `MainUI_PC`（PC 版 UI 根物件）|

#### OpenXR 平台（VIVE / Quest 共用）
| 欄位 | 值 |
|------|----|
| mainUIGameObject | `MainUICanvas_Main`（位於 MainUI_XR/UI/） |
| inputHandler | `Input_OpenXR` → `InputHandler_OpenXR` |
| activeGameObjects[0] | `XR Origin (XR Rig)`（VR 相機與控制器）|
| activeGameObjects[1] | `MainUI_XR`（XR 版 UI 根物件）|
| activeGameObjects[2] | `EventSystem_OpenXR`（XRUIInputModule）|

### 切換邏輯（SwitchPlatform）

```
SwitchPlatform() 執行時：
  1. classroomManager.mainUICanvas_Current = 目標平台的 mainUIGameObject
  2. classroomManager.inputActionManager.SetCurrentInputHandler(目標 inputHandler)
  3. 遍歷所有 PlatformSetting：
       目標平台 → SetEnable(true)
       其他平台 → SetEnable(false)

SetEnable(bool) 對每個設定執行：
  - mainUIGameObject.SetActive(value)
  - inputHandler.gameObject.SetActive(value)
  - activeGameObjects 清單內每個物件.SetActive(value)
```

### 各平台切換結果對照

| 物件 | PC 模式 | OpenXR 模式 |
|------|:-------:|:-----------:|
| MainCamera | ON | OFF |
| XR Origin (XR Rig) | OFF | ON |
| MainUI_PC | ON | OFF |
| MainUI_XR | ON（透過 inputHandler 啟關）| ON |
| EventSystem_PC | ON | OFF |
| EventSystem_OpenXR | OFF | ON |
| Input_PC | ON | OFF |
| Input_OpenXR | OFF | ON |

> `MainUICanvas_Main` 兩個平台共用同一個物件參考，不做額外切換。
