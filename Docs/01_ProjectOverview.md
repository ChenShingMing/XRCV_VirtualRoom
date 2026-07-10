# 01 — 專案總覽

> 最後驗證：2026-07-11（場景：Main.unity，Unity 6000.3.12f1）

---

## 基本資訊

| 項目 | 內容 |
|------|------|
| 專案名稱 | creativeXRworld |
| 定位 | VR 多人協作教育平台 |
| Unity 版本 | 6000.3.12f1（從 2021.3 升級） |
| 目前版本號 | v20241013 |
| 當前授權學校 | 大德國中（學生 10 座、教師 1 座） |

---

## 支援平台

| 模式 | Build Target | XR 路徑 | 用途 |
|------|-------------|---------|------|
| PC | Windows Standalone | 無 XR | 滑鼠鍵盤操作 |
| Quest Link | Windows Standalone | OpenXR + OculusXR Feature | PC 串流至 Quest 2 測試 |
| VIVE (Android) | Android | OpenXR + VIVE Features | VIVE Focus 3 APK |
| Quest (Android) | Android | OpenXR + OculusXR Feature | Quest 2 APK（目前走舊 OculusLoader，待 P0 改善） |

---

## 主要技術棧

| 分類 | 套件 / 框架 | 版本 |
|------|------------|------|
| XR 管理 | com.unity.xr.management | 4.5.4 |
| OpenXR | com.unity.xr.openxr | 1.16.1 |
| XR 互動 | com.unity.xr.interaction.toolkit | 3.3.1 |
| VIVE OpenXR | com.htc.upm.vive.openxr | git（最新） |
| Oculus SDK（待移除） | com.unity.xr.oculus | 4.5.2 |
| 網路多人 | Photon PUN 2 | — |
| 授權後端 | Firebase Firestore | — |
| 虛擬鍵盤 | MRTK NonNativeKeyboard | — |
| UI 框架 | TextMeshPro | — |
| 編輯器擴展 | Odin Inspector | 3.3.1+ |
| 視覺腳本 | PlayMaker | — |
| 建模工具 | ProBuilder 6 | — |

---

## 場景清單

| 場景名稱 | 路徑 | 用途 |
|---------|------|------|
| LoadScene | `Assets/3. Scene/LoadScene.unity` | 啟動進入點，初始加載 |
| **Main** | `Assets/3. Scene/Main.unity` | **主教室場景，所有系統都在此** |
| LoadingScene | `Core/LoadingScene/LoadingScene.unity` | 場景切換過渡 UI |
| XRDemoScene | `Assets/3. Scene/XRDemoScene.unity` | XR 功能演示 |
| SampleScene | `Topics/StartMap/Scenes/SampleScene.unity` | 星圖獨立測試 |

---

## 目錄結構

```
Assets/
├── 2. XRCV_VirtualRoom/          ← 本專案自定義程式碼（應用層）
│   ├── Core/
│   │   ├── Classroom/            ← PlatformSwitcher、DontDestroyOnLoad
│   │   ├── Classroom/Editor/     ← PlatformBuilder（Editor 工具）
│   │   ├── OpenXR/               ← InputHandler_OpenXR、鍵盤、Raycaster
│   │   ├── Oculus/               ← InputHandler_OVR（未完整）、UI Cursor
│   │   ├── Room/                 ← 房間材質管理
│   │   ├── LoadingScene/         ← 場景加載管理
│   │   └── LicenseInformation/   ← 授權資料、Firebase 整合
│   └── Topics/
│       └── StartMap/             ← 星圖課程（目前唯一完整 Topic）
│
├── VirtualRoomPlugIn/            ← 框架基礎層（抽象定義）
│   └── 1. VirtualRoom/Core/
│       ├── ClassroomManager/     ← ClassroomManager.cs
│       ├── TopicManager/         ← Topic.cs、TopicController.cs、TopicManager.cs
│       └── InputManager/         ← InputHandler.cs（abstract）、InputHandler_PC.cs
│
├── Photon/                       ← Photon PUN 2 SDK
├── PlayMaker/                    ← PlayMaker FSM
├── Plugins/Sirenix/              ← Odin Inspector
└── Samples/XR Interaction Toolkit/2.4.3/  ← XRI Starter Assets
```

---

## 相關文件

- [02 平台切換系統](02_PlatformSystem.md)
- [03 輸入系統](03_InputSystem.md)
- [04 教室系統](04_ClassroomSystem.md)
- [05 XR 設定](05_XRSetup.md)
- [06 UI 系統](06_UISystem.md)
- [07 課程系統與 StarMap](07_TopicSystem.md)
- [OptimizationGuidelines 優化方針](OptimizationGuidelines.md)
