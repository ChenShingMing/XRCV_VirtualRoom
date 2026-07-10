# 03 — 輸入系統

> 最後驗證：2026-07-11（場景：Main.unity，Runtime 讀取確認）

---

## 架構概述

```
InputManager (GameObject)  ←  InputActionManager component
  ├── Input_PC      [inactive]  — InputHandler_PC
  ├── Input_OVR     [inactive]  — InputHandler_OVR（未完整實作）
  └── Input_OpenXR  [active]   — InputHandler_OpenXR
```

**場景路徑**：`===== Classroom =====/ClassroomManager/InputManager`

`InputActionManager` 決定哪個 Handler 是當前作用中，並接收 Handler 輸入後分發到 ClassroomManager 的各個功能。

---

## 抽象基類：InputHandler

**路徑**：`VirtualRoomPlugIn/1. VirtualRoom/Core/InputManager/InputHandler.cs`

```csharp
public abstract class InputHandler : MonoBehaviour
{
    public abstract void InputHandle();
    public abstract Vector3 GetInputPointerOnGazeSphere();
}
```

每幀由 `InputActionManager` 呼叫作用中 Handler 的 `InputHandle()`。

---

## InputActionManager

**路徑**：`VirtualRoomPlugIn/1. VirtualRoom/Core/InputManager/InputActionManager.cs`

接收 Handler 輸入後，呼叫 ClassroomManager 的對應功能：

| 方法 | 觸發動作 |
|------|---------|
| `TriggerMainMenu()` | 開關主選單 |
| `TriggerMenu()` | 開關課程選單（有權限檢查） |
| `TriggerFirstPersonView(bool)` | PC 第一人稱視角旋轉 |
| `OnSubmitDownTrigger(Quaternion)` | 開始放提示點 / 線條 |
| `OnSubmitTrigger(Quaternion)` | 持續畫線 |
| `OnSubmitUpTrigger(Quaternion)` | 結束畫線 |
| `TriggerMonitorPointerClick()` | 監控模式指標點擊 |
| `TriggerPenPode() / SetPenPode()` | 切換筆模式 |
| `SetCurrentInputHandler(InputHandler)` | 切換作用中的 InputHandler |

---

## InputHandler_PC（PC 平台）

**路徑**：`VirtualRoomPlugIn/1. VirtualRoom/Core/InputManager/InputHandler_PC.cs`  
**場景**：`Input_PC`（`[inactive]` — PC 模式時啟用）  
**狀態**：✅ 完整實作

| 按鍵 / 動作 | 觸發功能 |
|------------|---------|
| `ESC` (KeyUp) | TriggerMainMenu |
| `O` (KeyDown) | TriggerMenu |
| 左鍵滑鼠按住 | TriggerFirstPersonView(true) |
| 左鍵滑鼠放開 | TriggerFirstPersonView(false) + TriggerMonitorPointerClick |
| 中鍵滑鼠 Down | OnSubmitDownTrigger（以主相機旋轉為方向）|
| 中鍵滑鼠持續 | OnSubmitTrigger |
| 中鍵滑鼠 Up | OnSubmitUpTrigger |
| `I` (KeyDown) | TriggerPenPode |

視角指向：`GazeSphere.RayHitOnSphere(Camera.main.ScreenPointToRay(Input.mousePosition))`

---

## InputHandler_OpenXR（XR 平台）

**路徑**：`Assets/2. XRCV_VirtualRoom/Core/OpenXR/InputHandler_OpenXR.cs`  
**場景**：`Input_OpenXR`（`[active]` — 當前作用中）  
**狀態**：✅ 完整實作

### 綁定的 Input Actions

| 屬性 | 對應功能 |
|------|---------|
| `mainMenuAction` | 主選單（通常綁 Menu 按鈕）|
| `menuAction` | 課程選單（通常綁 Primary 按鈕）|
| `triggerAction` | 提示點 / 線條（Trigger 按鍵）|
| `penModeAction` | 切換筆模式（通常綁 Secondary 按鈕）|

### 重要屬性

| 屬性 | 說明 |
|------|------|
| `rayInteractor` | 右手 `XRRayInteractor`（UI 射線偵測）|
| `uiLayerMask` | UI 層遮罩，用於判斷是否指向 UI |

### 主要方法

- `InputHandle()` — 每幀偵測 Action 觸發，轉呼叫 InputActionManager
- `IsPointingAtUI()` — 判斷右手射線是否命中 UI 層物件
- `GetControllerRotation()` — 取得右手控制器旋轉（用於 OnSubmitTrigger 方向）

---

## InputHandler_OVR（Oculus 原生 SDK）

**路徑**：`Assets/2. XRCV_VirtualRoom/Core/Oculus/InputHandler_OVR.cs`  
**場景**：`Input_OVR`（`[inactive]`，從未啟用）  
**狀態**：❌ 大部分方法拋出 `NotImplementedException`

> 背景：此 Handler 對應 `Switch to Oculus`（OculusLoader 路徑）。  
> P0 完成後（改為 OpenXR Feature 路徑），Quest Android 將使用 `InputHandler_OpenXR`，  
> `InputHandler_OVR` 可以安全移除。

---

## XR 控制器 UI 輔助

### XRRayInteractorUICursor

**路徑**：`Assets/2. XRCV_VirtualRoom/Core/Oculus/XRRayInteractorUICursor.cs`  
**場景**：`EventSystem_OpenXR/UIHelpers`（`[inactive]`）

追蹤 `XRRayInteractor` 的 Hit 結果，更新場景中 Cursor 的位置與方向。  
（UIHelpers 本身為 inactive，需確認是否已棄用或在特定條件下啟動。）

### OpenXRCanvasRaycasterAdder

**路徑**：`Assets/2. XRCV_VirtualRoom/Core/Oculus/OpenXRCanvasRaycasterAdder.cs`  
**場景**：`XR Origin (XR Rig)` 上的 component  

場景 Awake 時自動為所有 Canvas 和 Dropdown 範本加上 `TrackedDeviceGraphicRaycaster`，  
確保 XR 射線能與 UI 互動，不需要手動逐一設定。

### OpenXRInputFieldKeyboardListener

**路徑**：`Assets/2. XRCV_VirtualRoom/Core/OpenXR/OpenXRInputFieldKeyboardListener.cs`  
**場景**：`EventSystem_OpenXR/OpenXRInputFieldKeyboardListener`（`[active]`）

監聽場景中所有 `TMP_InputField` 的 OnSelect 事件，  
當使用者點擊文字輸入框時自動彈出 MRTK `NonNativeKeyboard` 並定位到合適位置。
