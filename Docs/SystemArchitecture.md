# XRCV VirtualRoom — 系統架構文件

> Unity 6000.3.12f1 | 更新：2026-07-11

---

## 一、專案概述

| 項目 | 說明 |
|------|------|
| 專案名稱 | creativeXRworld |
| 定位 | VR 多人協作教育平台 |
| 網路框架 | Photon PUN 2 |
| XR 框架 | OpenXR 1.16.1 + XRI 3.3.1 |
| 支援平台 | Windows Standalone（PC / Quest Link）、Android（VIVE OpenXR、Oculus APK） |
| 授權系統 | Firebase Firestore |

---

## 二、目錄結構

```
Assets/
├── 2. XRCV_VirtualRoom/          ← 本專案自定義程式碼
│   ├── Core/                     ← 平台無關的核心系統
│   │   ├── Classroom/            ← 教室管理、平台切換
│   │   ├── OpenXR/               ← OpenXR 輸入、鍵盤、Raycaster
│   │   ├── Oculus/               ← OVR 輸入（未完成）、UI Cursor
│   │   ├── Room/                 ← 房間材質管理
│   │   ├── LoadingScene/         ← 場景加載
│   │   └── LicenseInformation/   ← 授權資料、Firebase 整合
│   └── Topics/
│       └── StartMap/             ← 星圖課程（唯一已完整實作的 Topic）
├── VirtualRoomPlugIn/            ← 框架基礎層（ClassroomManager、Topic 抽象）
│   └── 1. VirtualRoom/Core/
│       ├── ClassroomManager/
│       ├── TopicManager/
│       └── InputManager/
├── Photon/                       ← Photon PUN 2 SDK
├── PlayMaker/                    ← PlayMaker FSM
├── Plugins/Sirenix/              ← Odin Inspector
└── Samples/XR Interaction Toolkit/2.4.3/  ← XRI Starter Assets
```

---

## 三、核心系統

### 3.1 ClassroomManager（中樞）

**路徑**: `VirtualRoomPlugIn/1. VirtualRoom/Core/ClassroomManager/ClassroomManager.cs`  
**繼承**: `MonoBehaviourPunCallbacks`  
**模式**: Singleton

教室的總指揮，負責：
- 授權驗證（Firebase）→ `OnPassLicenseEvent / OnFailLicenseEvent`
- 玩家管理（Photon 房間、座位數）
- 教學模式切換：導學（Guidance）/ 自學（SelfStudy）
- 委派課程控制給 `TopicManager`
- 委派輸入控制給 `InputActionManager`

```
重要 UnityEvent:
  OnPassLicenseEvent       授權通過
  OnFailLicenseEvent       授權失敗
  OnSetTopic               課程開始
  OnExitTopic              課程結束
  OnSwitchTeachingToGuidance / ToSelfStudy   教學模式
```

---

### 3.2 平台切換系統

#### PlatformBuilder（編輯器工具）
**路徑**: `Core/Classroom/Editor/PlatformBuilder.cs`

| 按鈕 | 動作 |
|------|------|
| Switch to PC | Build Target → Windows64 |
| Switch to Oculus | Android + 換成 OculusLoader（舊路徑） |
| Switch to OpenXR | Android + 換成 OpenXRLoader（VIVE） |

執行後會呼叫：
1. `PlatformSwitcher.OnSwitchToXxx()` — 切換 Scene 層的 UI/InputHandler
2. `ClassroomManager.UpdateVersion()` — 更新版本號

#### PlatformSwitcher（Scene 元件）
**路徑**: `Core/Classroom/PlatformSwitcher.cs`  
**繼承**: `SerializedMonoBehaviour`（Odin）

用 `Dictionary<Platform, PlatformSetting>` 管理每個平台的：
- `mainUIGameObject` — 對應的主 UI Canvas
- `inputHandler` — 對應的輸入處理器
- `activeGameObjects` — 需要啟/關的物件列表

> 注意：`PlatformSwitcher.Platform` 只有 `PC` 和 `OpenXR` 兩種，`OnSwitchToOculus()` 目前也映射到 `OpenXR`。

---

### 3.3 輸入系統

```
InputHandler (abstract, MonoBehaviour)
├── InputHandler_OpenXR   — 目前主用，XRI Action-based
│   ├── rayInteractor     — XRRayInteractor（UI 射線）
│   ├── mainMenuAction    — 主選單按鈕
│   ├── menuAction        — 課程選單按鈕
│   ├── triggerAction     — 觸發（畫線/點）
│   └── penModeAction     — 切換筆模式
└── InputHandler_OVR      — 未完整實作，暫不使用
```

`InputActionManager` 接收 InputHandler 的輸入事件，再分發到：
- 主選單開關
- 課程選單開關（有權限檢查）
- 提示點 / 線條繪製（RPC 同步）
- 筆模式切換

---

### 3.4 XR UI 輔助

| 類別 | 職責 |
|------|------|
| `XRRayInteractorUICursor` | 追蹤射線 Hit，更新 Cursor 位置和方向 |
| `OpenXRCanvasRaycasterAdder` | 場景啟動時自動為所有 Canvas 加上 TrackedDeviceGraphicRaycaster |
| `OpenXRInputFieldKeyboardListener` | InputField 選中時彈出 MRTK 虛擬鍵盤 |
| `AlignPosToController` | 把選單/提示對齊到左手控制器位置 |

---

### 3.5 課程系統（Topic Framework）

```
TopicManager
  └─ Dictionary<string, Topic>
        └─ Topic                          ← 課程生命週期容器
              ├─ OnInit()                 初始化控制器
              ├─ OnEnter() / OnExit()     進入/離開
              ├─ OnUpdate()               每幀（同步目標更新）
              └─ OnSync()                 每 0.5s 同步一次
                    ↓
              TopicController (abstract)
                    ├─ SendTopicControllerProperties()   老師/監控者發送
                    └─ UpdateTopicControllerProperties() 學生接收更新
```

**同步策略**：
- **導學模式**：所有學生跟隨 Master Client（老師）的 `syncSenderName`
- **自學模式**：每人獨立操作，不同步
- **媒介**：Photon `CustomProperties`（房間自定義屬性）
- **頻率**：0.5 秒

---

## 四、已實作課程：StarMap 星圖

### 4.1 元件結構

```
StarMapController (Singleton)
  ├── StarMap                  ← 星星粒子 + 星座線 LineRenderer
  │     └── CreateHipHierarchy ← 生成星座 GameObject 層級
  ├── StarMapRotate            ← 天球旋轉（依緯度、日期、時間）
  ├── StarMapControlData       ← 所有可控參數的資料容器
  │     ├── LocationData       ← ScriptableObject 位置清單
  │     └── PanoramicController
  ├── DayNightEnvironmentControl (Singleton) ← 天空盒日夜切換
  ├── SunRotate                ← 太陽方位角計算
  └── UI 群組
        ├── ControlCanvasManager
        ├── StarMapHUDController (Singleton) ← 星座名稱標籤
        └── LocalicationSelectController
```

### 4.2 同步屬性（13 個）

Key 格式：`{topicName}_starMapControlData_{property}_{syncSenderName}`

| 類型 | 屬性 |
|------|------|
| bool | graticule, linkLine, nameAndModel, usePanoramic |
| int | Year, Month, Day, Hour, currentlocalicationIndex |
| float | rotateSpeed, latitude, longitude |
| string | currentLocalicationName |

### 4.3 星圖資料

- 星星資料：CSV（hipId, pos, color, magnitude, parallax）
- 星座線：CSV（星座縮寫, 起點hipId, 終點hipId）
- 星座 3D 模型：`Resources/Prefabs/{縮寫}`
- 全景背景：`Assets/StartMap/Art/LocationData/{地點名}/day.png, night.png`

---

## 五、XR 設定狀態

### Loaders（XRGeneralSettings.asset）
| 平台 | Loader |
|------|--------|
| Android | OpenXR ✅ |
| Standalone | OpenXR ✅ |

### OpenXR Features（OpenXR Package Settings.asset）
| Feature | Android | Standalone |
|---------|:-------:|:----------:|
| OculusXR Feature | ✅ | ✅ |
| OculusTouchControllerProfile | ❌ | ✅ |
| VIVEFocus3Profile | ✅ | ❌ |
| MetaQuestTouchPlusControllerProfile | ❌ | ❌ |

> Standalone 的 OculusTouchControllerProfile 已於 2026-07-11 啟用，供 Quest Link 測試。

---

## 六、場景清單

| 場景 | 路徑 | 用途 |
|------|------|------|
| LoadScene | Assets/3. Scene/LoadScene.unity | 初始加載 |
| Main | Assets/3. Scene/Main.unity | 主教室（含所有課程） |
| LoadingScene | Core/LoadingScene/LoadingScene.unity | 場景切換過渡 |
| XRDemoScene | Assets/3. Scene/XRDemoScene.unity | XR 功能演示 |
| SampleScene | Topics/StartMap/Scenes/SampleScene.unity | 星圖獨立測試 |

---

## 七、已知未完成項目

| 項目 | 狀態 | 說明 |
|------|------|------|
| InputHandler_OVR | ❌ 未實作 | 大部分方法拋出 NotImplementedException |
| InputHandler_PC | ❓ 未找到 | PlatformSwitcher 有 PC 模式但找不到 PC 輸入處理器 |
| PlatformBuilder Build 按鈕 | 🚫 被註解 | `/* ... */` 包起來，Switch 按鈕仍可用 |
| OculusLoader 路徑 | ⚠️ Deprecated | `com.unity.xr.oculus 4.5.2` 在 Unity 6 已標為廢棄 |
