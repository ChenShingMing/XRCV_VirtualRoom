# 04 — 教室系統（ClassroomManager）

> 最後驗證：2026-07-11（場景：Main.unity，Runtime 讀取確認）

---

## 場景結構

```
===== Classroom =====
  └── ClassroomManager  [active]
        ├── [Transform]
        ├── [ClassroomManager]          ← 教室中樞腳本
        └── [PhotonView]                ← Photon 網路同步
        │
        ├── LicenseCreater  [inactive]  ← 授權碼生成工具（Editor 用）
        │     ├── LicenseInformation_PU
        │     ├── LicenseInformation_Wang
        │     ├── LicenseInformation_MinDao
        │     └── LicenseInformation_ChungWen
        │
        ├── LicenseInformation_Current  [active]  ← 當前載入的授權資料
        │
        ├── FirebaseLicenseInformationManager  [active]  ← Firebase 讀寫
        │
        ├── InputManager  [active]      ← InputActionManager
        │     ├── Input_PC      [inactive]  — InputHandler_PC
        │     ├── Input_OVR     [inactive]  — InputHandler_OVR
        │     └── Input_OpenXR  [active]   — InputHandler_OpenXR
        │
        ├── TopicManager  [active]      ← 課程管理
        │
        ├── PunNetworkManager  [active] ← Photon 房間管理
        │
        ├── LineController  [active]    ← 畫線系統
        │
        └── MonitorManager  [active]    ← 監控模式管理
              └── Pointer               ← 監控畫面指標
```

---

## ClassroomManager 核心屬性（Runtime 驗證值）

| 屬性 | 當前值 | 說明 |
|------|--------|------|
| `version` | v20241013 | 版本號，PlatformBuilder 切換時更新 |
| `joinType` | Member | 加入類型（Member / Monitor） |
| `teachingType` | Guidance | 教學模式（Guidance / SelfStudy） |
| `isPenMode` | false | 筆模式狀態 |
| `inputActionManager` | InputManager | 指向 InputManager GameObject |
| `topicManager` | TopicManager | 指向 TopicManager GameObject |
| `mainUICanvas_Current` | MainUICanvas_Main | 當前主 UI Canvas |

---

## 主要 UnityEvents

| 事件 | 觸發時機 |
|------|---------|
| `OnPassLicenseEvent` | 授權驗證通過後 |
| `OnFailLicenseEvent` | 授權驗證失敗後 |
| `OnSetTopic` | 課程開始時 |
| `OnExitTopic` | 課程退出時 |
| `OnSwitchTeachingTypeToGuidance` | 切換為導學模式 |
| `OnSwitchTeachingTypeToSelfStudy` | 切換為自學模式 |

---

## 授權系統

### 資料流

```
遊戲啟動
  ↓
ClassroomManager 讀取 PlayerPrefs 中的授權碼
  ↓
FirebaseLicenseInfoManager.ReadData(licenseKey)
  ↓ (從 Firestore 拉取)
LicenseInformation_Current 填入資料
  ├── schoolName, logo
  ├── photonAppID
  ├── seatInfo_Stu（學生座位數）
  └── seatInfo_Teacher（教師座位數）
  ↓
OnPassLicenseEvent.Invoke()  或  OnFailLicenseEvent.Invoke()
```

### 預設授權資料（LicenseCreater 子物件）

| 物件名稱 | 學校 |
|---------|------|
| LicenseInformation_PU | — |
| LicenseInformation_Wang | — |
| LicenseInformation_MinDao | 明道中學 |
| LicenseInformation_ChungWen | — |

> 這些預設資料在 `LicenseCreater`（inactive）下，僅供開發時快速切換測試用，不影響 Runtime 授權流程。

---

## Photon 網路

### 加入類型（JoinType）

| 類型 | 說明 |
|------|------|
| Member | 一般使用者（學生或老師），可操作課程 |
| Monitor | 監控者（螢幕前管理員），只能觀察和操作指標 |

### 教學模式（TeachingType）

| 模式 | 同步行為 |
|------|---------|
| Guidance（導學） | 所有學生強制跟隨 Master Client（老師）的課程狀態 |
| SelfStudy（自學） | 每個學生獨立操作，不同步 |

### RPC 方法（透過 ClassroomManager 的 PhotonView）

| 方法 | 功能 |
|------|------|
| `RPCSetTipPointViwe(Vector3)` | 同步提示點位置 |
| `RPCSetLinePoint(Vector3, Quaternion)` | 同步線條繪製 |
| `RPCSetPenColor(Color)` | 同步筆顏色 |
| `RPCClearCanvas()` | 清除所有畫布 |

---

## 課程管理（TopicManager）

**場景位置**：`===== Classroom =====/ClassroomManager/TopicManager`

| 屬性 | 值 |
|------|-----|
| syncFrequency | 0.5 秒 |
| currentTopic | 無（未開始課程） |
| topicDic 數量 | 1 個 |

### 目前設定的 Topics

| Key | topicName | Controller 類型 | Controller GO |
|-----|-----------|----------------|---------------|
| `虛擬星象館` | 虛擬星象館 | StarMapTopicController | StarMap |

---

## LineController（畫線系統）

**場景位置**：`===== Classroom =====/ClassroomManager/LineController`

管理所有多人同步的線條和提示點，接受 ClassroomManager 的 RPC 指令後在本地渲染。

---

## MonitorManager（監控系統）

**場景位置**：`===== Classroom =====/ClassroomManager/MonitorManager`  
**Component**：`NewMonitorManager`

管理監控模式下的螢幕指標（`Pointer`），可在監控畫面上顯示教師的指向位置。

子物件：
- `Pointer` — Canvas + Pointer component，顯示指標圖示
