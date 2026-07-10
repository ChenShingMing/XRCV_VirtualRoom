# 06 — UI 系統

> 最後驗證：2026-07-11（場景：Main.unity，Runtime 讀取確認）

---

## 概述

主 UI 分為兩套，隨平台切換啟關：

| 根物件 | 平台 | 場景路徑 | 當前狀態 |
|--------|------|---------|---------|
| `MainUI_XR` | OpenXR（VIVE / Quest） | `===== Classroom =====/MainUI_XR` | active |
| `MainUI_PC` | PC | `===== Classroom =====/MainUI_PC` | inactive |

兩套 UI 的 Canvas 命名相同但各自獨立，PlatformSwitcher 的 `activeGameObjects` 負責切換。

---

## MainUI_XR（XR 版 UI）

**Components**：`MainUIManager`、`InfoPanel`、`PenPanel`、`AlignPosToController`

```
MainUI_XR  [active]
  ├── UI/
  │     ├── MainUICanvas_License  [inactive]  — 授權輸入畫面
  │     │     Canvas + CanvasGroup + ContentSizeFitter + GraphicRaycaster
  │     │     └── MainPanel
  │     │
  │     ├── MainUICanvas_Main  [active]       — 主選單（課程選擇、教學模式等）
  │     │     Canvas + CanvasGroup + ContentSizeFitter + GraphicRaycaster
  │     │     └── MainPanel → 5 個子物件
  │     │
  │     ├── MainUICanvas_Info  [active]       — 資訊面板（版本、學校、連線狀態）
  │     │     Canvas + CanvasGroup + ContentSizeFitter + GraphicRaycaster
  │     │     └── InfoPanel → 2 個子物件
  │     │
  │     └── MainUICanvas_Pen  [active]        — 筆模式面板（顏色、清除）
  │           Canvas + CanvasGroup + ContentSizeFitter + GraphicRaycaster
  │           └── Panel
  │
  ├── MainUITip  [active]                     — 對齊左手控制器的提示
  │     Components: AlignPosToController, MenuUITip
  │     ├── MenuTip  [inactive]               — 按鍵提示（如：「按 O 開選單」）
  │     │     Canvas + GraphicRaycaster
  │     └── ConnectTip  [inactive]            — 連線提示
  │           Canvas + GraphicRaycaster
  │
  └── PenTip  [inactive]                      — 3D 筆尖 Mesh（筆模式時顯示）
        Components: MeshFilter, MeshRenderer
```

### Canvas 共用設定
所有 Canvas 均使用 `CanvasGroup` 控制顯示，`ContentSizeFitter` 自動調整尺寸，  
`GraphicRaycaster` 讓 XR 射線可以點擊 UI 元素（加上 `TrackedDeviceGraphicRaycaster` 由 `OpenXRCanvasRaycasterAdder` 自動補充）。

---

## MainUI_PC（PC 版 UI）

```
MainUI_PC  [inactive]
  └── UI/
        ├── MainUICanvas_License_PC  [inactive]  — PC 版授權輸入畫面
        ├── MainUICanvas_Main        [inactive]  — PC 版主選單
        └── MainUICanvas_Pen         [inactive]  — PC 版筆模式面板
```

> PC 版 UI 沒有 `MainUICanvas_Info`，資訊顯示方式不同。

---

## mainUICanvas_Current 共用機制

`ClassroomManager.mainUICanvas_Current` 在兩個平台都指向 **同一個** `MainUICanvas_Main` 物件（位於 `MainUI_XR/UI/`）。

這個設計讓 ClassroomManager 不需要分辨平台，直接對 `mainUICanvas_Current` 操作即可。

**PC 模式下**，`MainUI_PC` 也有自己的 `MainUICanvas_Main`，但 `mainUICanvas_Current` 的指向在 PlatformSwitcher 初始化時依平台設定，所以：
- PC 模式 → `mainUICanvas_Current` = `MainUI_PC/UI/MainUICanvas_Main`（inactive 根物件下的 Canvas，隨 MainUI_PC 一起啟動）
- OpenXR 模式 → `mainUICanvas_Current` = `MainUI_XR/UI/MainUICanvas_Main`（已 active）

---

## EventSystem 架構

### EventSystem_PC（PC 模式）
```
EventSystem_PC  [inactive]
  Components: EventSystem, StandaloneInputModule
```
處理滑鼠和鍵盤的 UI 事件。

### EventSystem_OpenXR（XR 模式）
```
EventSystem_OpenXR  [active]
  Components: EventSystem, XRUIInputModule
  │
  ├── XR Interaction Manager          — XRInteractionManager（全域 XR 互動管理）
  ├── Input Action Manager            — Unity InputActionManager（綁定 InputActionAsset）
  ├── OpenXRInputFieldKeyboardListener — InputField 選中時彈出虛擬鍵盤
  └── UIHelpers  [inactive]           — XRRayInteractorUICursor（射線 Cursor 位置更新）
        └── Canvas → Image            — Cursor 圖示
```

`XRUIInputModule` 讓控制器的射線可以觸發 Unity UI 事件（Button.onClick、Toggle 等）。

---

## 虛擬鍵盤

**場景路徑**：`----------Main-----------/NonNativeKeyboard`  
**Component**：`NonNativeKeyboard`（MRTK）、`TrackedDeviceGraphicRaycaster`、`Canvas`

```
NonNativeKeyboard  [active]
  ├── keyboard_Background   [active]
  ├── keyboard_Alpha        [active]  — 字母鍵盤
  ├── keyboard_Symbols      [inactive]
  ├── keyboard_Space_Alpha  [active]
  ├── keyboard_Space_Url    [inactive]
  └── keyboard_Space_Email  [inactive]
```

由 `OpenXRInputFieldKeyboardListener` 在 InputField 選中時喚醒，並自動定位到控制器前方。
