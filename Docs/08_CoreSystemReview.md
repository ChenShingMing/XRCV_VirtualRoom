# 08 — 核心系統程式碼審查

> 審查日期：2026-07-11 | 範圍：VirtualRoomPlugIn 框架層 + 應用層 Core + StarMap 接層
> 狀態：📋 規劃中（待選擇執行項目）

---

## 審查範圍

```
VirtualRoomPlugIn/1. VirtualRoom/Core/
  ClassroomManager、InputManager、Player、PunNetworkManager、
  TopicManager、NewMonitorManager、UI 系列、GazeController、
  LineController、CurvedLineRenderer、PointerController

Assets/2. XRCV_VirtualRoom/Core/
  LoadingManager、RoomMaterialManager、FirebaseLicenseInfoManager、
  LicenseInformation、OpenXRInputFieldKeyboardListener、MenuUITip

Assets/2. XRCV_VirtualRoom/Topics/StartMap/Scripts/VirtualRoomPlugIn/
  StarMapTopic、StarMapTopicController、StarMapTopicData
```

---

## 🔴 Bug（功能錯誤）

### B1 — PunNetworkManager：OnLeftLobby() 呼叫錯誤方法

- **檔案**：`VirtualRoomPlugIn/1. VirtualRoom/Core/PunNetworkManager/PunNetworkManager.cs`
- **問題**：`OnLeftLobby()` 回調內呼叫了 `OnJoinedLobby()`，離開大廳反而觸發加入大廳邏輯
- **修法**：`OnLeftLobby()` 應清空房間列表或切換到離線 UI 狀態，而非重新呼叫 `OnJoinedLobby()`
- **風險**：低（僅修正回調邏輯）

### B2 — MenuUITip：Update 邏輯永遠不執行

- **檔案**：`Assets/2. XRCV_VirtualRoom/Core/Classroom/MenuUITip.cs`
- **問題**：`controllersActive` 判斷依賴 OVRInput（已移除），永遠為 false，Update 整個 if block 永遠不進入
- **修法**：確認 MenuUITip 功能是否仍需要，若需要改用 OpenXR InputAction；若不需要則整個 Update 可移除
- **風險**：低

---

## 🔴 高效能問題

### P1 — InputActionManager：Camera.main 每次輸入觸發時查找

- **檔案**：`VirtualRoomPlugIn/1. VirtualRoom/Core/InputManager/InputActionManager.cs`
- **問題**：`OnSubmitDownTrigger()`、`OnSubmitTrigger()`、`OnSubmitUpTrigger()` 中多次呼叫 `Camera.main`（行 78、85、110、141）
- **修法**：加入 `private Camera _mainCamera;`，`Awake()` 快取，方法內改用 `_mainCamera`
- **風險**：低

### P2 — InputHandler_PC：Camera.main 每幀查找

- **檔案**：`VirtualRoomPlugIn/1. VirtualRoom/Core/InputManager/InputHandler_PC.cs`
- **問題**：`InputHandle()` 在每幀 Update 中呼叫，內部多次 `Camera.main`（行 10、41、46、51）
- **修法**：`Awake()` 快取 `Camera.main`
- **風險**：低

### P3 — Player：FindObjectsOfTypeAll 全場景掃描

- **檔案**：`VirtualRoomPlugIn/1. VirtualRoom/Core/Player/Player.cs`
- **問題**：`GetIdentityByName()` 使用 `Resources.FindObjectsOfTypeAll<Player>()`，每次呼叫掃遍所有場景物件（含未啟用物件）
- **修法**：改用靜態 `List<Player>` 在 `OnEnable/OnDisable` 自行維護，或改用 `FindObjectsByType<Player>(FindObjectsSortMode.None)`
- **風險**：低（需確認呼叫頻率與多場景情境）

### P4 — GazeController：FixedUpdate 每幀 Camera.main + 複雜計算

- **檔案**：`VirtualRoomPlugIn/1. VirtualRoom/Other/GazeController/Scripts/GazeController.cs`
- **問題**：`FixedUpdate()` 中多處 `Camera.main`（行 141、201、230）；`FollowTipPointView()` 每幀進行大量向量計算
- **修法**：`Awake()` 快取 `Camera.main`；評估是否需要在每個物理幀都執行（改為 `Update()` + 節流）
- **風險**：中（GazeController 涉及多人注視同步，注意 IsMine 判斷）

### P5 — NewMonitorManager：SetPlayerGazeMode 每次全場景掃描

- **檔案**：`VirtualRoomPlugIn/1. VirtualRoom/Core/MonitorManager/NewMonitorManager.cs`
- **問題**：`SetPlayerGazeMode()` 每次呼叫 `FindObjectsOfTypeAll<GazeController>()`；`SendHandle()`/`ReceiveHandle()` 每幀在 `Update()` 執行
- **修法**：快取 GazeController 列表（`OnEnable` 時初始化）；SendHandle 改為每 0.05s 節流或用事件驅動
- **風險**：中（Monitor 模式涉及多人同步）

### P6 — LineController：Draw() 每幀 Instantiate

- **檔案**：`VirtualRoomPlugIn/1. VirtualRoom/Other/LineController/LineController.cs`
- **問題**：畫線時每幀 `Instantiate(linepoint)` 建立新 GameObject，產生持續 GC 壓力；`BezierPathDraw()` 同樣邏輯
- **修法**：改用 `LineRenderer` 直接管理頂點（`SetPositions()`），省去大量 GameObject 開銷
- **風險**：高（涉及多人同步畫線的 RPC 傳送邏輯，改架構需同步修改 Photon 傳送部分）

### P7 — MainUI：FixedUpdate 中 O(n²) Monitor 列表刷新

- **檔案**：`VirtualRoomPlugIn/1. VirtualRoom/Core/UI/MainUI.cs`
- **問題**：`FixedUpdate()` 每幀呼叫 `ReflashMonitorList()`，其內為嵌套迴圈；同時呼叫 `GetStudentNumber()` 遍歷 PlayerList
- **修法**：改用 Photon 回調（`OnPlayerEnteredRoom`、`OnPlayerLeftRoom`、`OnRoomPropertiesUpdate`）事件驅動刷新，不在 FixedUpdate 跑
- **風險**：中（需確認所有觸發時機）

### P8 — Topic：OnUpdate() 每幀查詢 Photon CurrentRoom

- **檔案**：`VirtualRoomPlugIn/1. VirtualRoom/Core/TopicManager/Topic.cs`
- **問題**：`OnUpdate()` 每幀呼叫 `PhotonNetwork.CurrentRoom.GetPlayer()`，Photon API 每幀呼叫開銷不低
- **修法**：快取 `syncSenderName` 對應的 Player 物件，僅在 Room Properties 變動時更新（`OnRoomPropertiesUpdate` 回調）
- **風險**：中（影響所有 Topic 子類行為）

---

## 🟡 中效能問題

### P9 — Room_Teacher / Room_Student / Room_Monitor：FixedUpdate 每幀讀 CustomProperties

- **檔案**：
  - `VirtualRoomPlugIn/1. VirtualRoom/Core/UI/Room_Teacher.cs`
  - `VirtualRoomPlugIn/1. VirtualRoom/Core/UI/Room_Student.cs`
  - `VirtualRoomPlugIn/1. VirtualRoom/Core/UI/Room_Monitor.cs`
- **問題**：三個 UI 面板各自在 `FixedUpdate()` 每幀讀取 `CustomProperties["CurrentTopic"]`
- **修法**：改用 `OnRoomPropertiesUpdate(Hashtable changedProps)` 回調，只在有變化時更新
- **風險**：低

### P10 — RoomInformation：FixedUpdate 每幀字串轉換

- **檔案**：`VirtualRoomPlugIn/1. VirtualRoom/Core/UI/RoomInformation.cs`
- **問題**：`FixedUpdate()` 每幀做字串轉換並更新 UI Text，但房間資訊幾乎不變
- **修法**：改用 `OnEnable()` 或由 `LobbyPanel` 在列表更新時呼叫
- **風險**：低

### P11 — InfoPanel：FixedUpdate 每幀讀 licenseInformation

- **檔案**：`VirtualRoomPlugIn/1. VirtualRoom/Core/UI/InfoPanel.cs`
- **問題**：`FixedUpdate()` 每幀讀取 `ClassroomManager.ins.licenseInformation.schoolName` 並設定 Text
- **修法**：改在 `OnEnable()` 設定一次，或在 License 載入完成的 Callback 中更新
- **風險**：低

### P12 — MainUIManager：FixedUpdate 呼叫空方法

- **檔案**：`VirtualRoomPlugIn/1. VirtualRoom/Core/UI/MainUIManager.cs`
- **問題**：`FixedUpdate()` 每幀呼叫 `ReflashMonitorList()`，但該方法實作**完全為空**
- **修法**：移除空方法呼叫，若方法未來需要實作，不應放在 FixedUpdate
- **風險**：極低

### P13 — PenPanel：FixedUpdate 每幀 Raycast

- **檔案**：`VirtualRoomPlugIn/1. VirtualRoom/Core/UI/PenPanel.cs`
- **問題**：`FixedUpdate()` 中每幀呼叫 `GetInputPointerOnGazeSphere()`（含球體 Raycast）
- **修法**：加節流（0.05s）或改為事件驅動（僅在觸發輸入時計算）
- **風險**：低

### P14 — VersionGetter：Update 每幀設定版本號

- **檔案**：`VirtualRoomPlugIn/1. VirtualRoom/Core/ClassroomManager/VersionGetter.cs`
- **問題**：`Update()` 每幀做 null check 並設定版本號文字，版本號不會在執行期變動
- **修法**：改在 `Start()` 設定一次，或在 `OnEnable()` 時更新
- **風險**：極低

### P15 — InRoomPanel：FixedUpdate 每幀呼叫 GetIdentityInfo

- **檔案**：`VirtualRoomPlugIn/1. VirtualRoom/Core/UI/InRoomPanel.cs`
- **問題**：`FixedUpdate()` 每幀呼叫 `ClassroomManager.ins.GetIdentityInfo()`
- **修法**：改在身份變動事件（加入房間、CustomProperties 更新）時刷新
- **風險**：低

### P16 — OpenXRInputFieldKeyboardListener：Camera.main 每次開鍵盤查找

- **檔案**：`Assets/2. XRCV_VirtualRoom/Core/OpenXR/OpenXRInputFieldKeyboardListener.cs`
- **問題**：`OpenKeyboard()` 每次呼叫 `Camera.main`；`Start()` 呼叫 `FindObjectsByType`
- **修法**：`Awake()` 快取 Camera；`positionSource` 已有欄位，在 Awake 時就指定
- **風險**：低

---

## 🟢 死碼 / 清理

### C1 — ClassroomManager：RPCSendMonitorData 空方法

- **檔案**：`VirtualRoomPlugIn/1. VirtualRoom/Core/ClassroomManager/ClassroomManager.cs`
- **問題**：`[PunRPC] RPCSendMonitorData()` 方法體為空，從未實作
- **修法**：若無計畫實作，直接移除；否則加上 `// TODO` 標記

### C2 — PointController：整個檔案只有空方法

- **檔案**：`VirtualRoomPlugIn/1. VirtualRoom/Other/PointerController/Scripts/PointController.cs`
- **問題**：只有空的 `Start()` 和 `Update()`，沒有任何邏輯
- **修法**：確認場景中是否有掛載，若無則直接刪除檔案

### C3 — InfoPanel、Room_Student：空的 Start()

- **檔案**：`InfoPanel.cs`、`Room_Student.cs`
- **問題**：Unity 仍會每幀呼叫空的 lifecycle 方法有微小開銷
- **修法**：移除空的 `Start()`

### C4 — LineController：BezierPathDraw 死碼

- **檔案**：`VirtualRoomPlugIn/1. VirtualRoom/Other/LineController/LineController.cs`
- **問題**：`BezierPathDraw()` 呼叫被 comment out，方法本身殘留
- **修法**：移除整個方法

### C5 — StarMapTopicData：SetData/ApplyTo 邏輯重複

- **檔案**：`Assets/2. XRCV_VirtualRoom/Topics/StartMap/Scripts/VirtualRoomPlugIn/StarMapTopicData.cs`
- **問題**：`SetData()` 與 `ApplyTo()` 各自逐欄位賦值，邏輯完全對稱但各自維護
- **修法**：低優先，可接受；若未來欄位增加，考慮用共用私有方法避免漏改

---

## 架構建議

### A1 — GazeController：靜態欄位多實例衝突風險

- **檔案**：`VirtualRoomPlugIn/1. VirtualRoom/Other/GazeController/Scripts/GazeController.cs`
- **問題**：`static GazePointView Tip_PointView`、`static GameObject TipArrow`、`static Canvas GazeCanvas` 在多人場景中若有多個 GazeController 實例，後者會覆蓋前者設定
- **修法**：確認每個玩家的 GazeController 是否都操作自己的私有物件（IsMine 判斷），若是則靜態欄位應改為 instance 欄位

### A2 — LobbyPanel：動態建立 / 刪除 RoomInformation GameObject

- **檔案**：`VirtualRoomPlugIn/1. VirtualRoom/Core/UI/LobbyPanel.cs`
- **問題**：`UpdateRoomListCount()` 用 `while` 迴圈動態 `Instantiate` 或 `Destroy` 房間項目，每次房間列表更新都有 GC
- **修法**：改用 Object Pool（固定預先建立最大數量的項目，SetActive 控制顯示）

### A3 — PunNetworkManager：RemoveAllListeners 範圍過大

- **檔案**：`VirtualRoomPlugIn/1. VirtualRoom/Core/PunNetworkManager/PunNetworkManager.cs`
- **問題**：`OnJoinedRoom()` 中呼叫某 UnityEvent 的 `RemoveAllListeners()`，可能移除其他系統透過 Inspector 添加的監聽器
- **修法**：改用具名方法配對 `AddListener`/`RemoveListener`，或確認該 UnityEvent 只由此處管理

### A4 — LineController：Instantiate 畫線架構

- **檔案**：`VirtualRoomPlugIn/1. VirtualRoom/Other/LineController/LineController.cs`
- **問題**：每個畫線點都 `Instantiate` 一個 GameObject，繪製一條線產生數十至數百個物件
- **修法**：用單一 `LineRenderer` 管理所有頂點（`AddPosition()` / `SetPositions()`），大幅降低物件數與 GC（高改動風險，需同步 Photon RPC 傳送邏輯）

---

## 執行狀態表

| 編號 | 分類 | 檔案 | 嚴重度 | 狀態 |
|------|------|------|--------|------|
| B1 | Bug | PunNetworkManager.cs | 🔴 高 | ✅ 完成 |
| B2 | Bug | MenuUITip.cs | 🔴 高 | ✅ 完成 |
| P1 | 效能 | InputActionManager.cs | 🔴 高 | ✅ 完成 |
| P2 | 效能 | InputHandler_PC.cs | 🔴 高 | ✅ 完成 |
| P3 | 效能 | Player.cs | 🔴 高 | ✅ 完成 |
| P4 | 效能 | GazeController.cs | 🔴 高 | ✅ 完成 |
| P5 | 效能 | NewMonitorManager.cs | 🔴 高 | ✅ 完成 |
| P6 | 效能 | LineController.cs | 🔴 高 | ⏸ 跳過（架構高風險） |
| P7 | 效能 | MainUI.cs | 🔴 高 | ✅ 完成 |
| P8 | 效能 | Topic.cs | 🔴 高 | ✅ 完成 |
| P9 | 效能 | Room_Teacher/Student/Monitor.cs | 🟡 中 | ✅ 完成 |
| P10 | 效能 | RoomInformation.cs | 🟡 中 | ✅ 完成 |
| P11 | 效能 | InfoPanel.cs | 🟡 中 | ✅ 完成 |
| P12 | 效能 | MainUIManager.cs | 🟡 中 | ✅ 完成 |
| P13 | 效能 | PenPanel.cs | 🟡 中 | ✅ 完成 |
| P14 | 效能 | VersionGetter.cs | 🟡 中 | ✅ 完成 |
| P15 | 效能 | InRoomPanel.cs | 🟡 中 | ✅ 完成 |
| P16 | 效能 | OpenXRInputFieldKeyboardListener.cs | 🟡 中 | ✅ 已是最佳（無需修改） |
| C1 | 死碼 | ClassroomManager.cs | 🟢 低 | ⏳ 待決定 |
| C2 | 死碼 | PointController.cs | 🟢 低 | ⏳ 待決定 |
| C3 | 死碼 | InfoPanel.cs / Room_Student.cs | 🟢 低 | ⏳ 待決定 |
| C4 | 死碼 | LineController.cs | 🟢 低 | ⏳ 待決定 |
| C5 | 架構 | StarMapTopicData.cs | 🟢 低 | ⏳ 待決定 |
| A1 | 架構 | GazeController.cs | 🟡 中 | ⏳ 待決定 |
| A2 | 架構 | LobbyPanel.cs | 🟡 中 | ⏳ 待決定 |
| A3 | 架構 | PunNetworkManager.cs | 🟡 中 | ⏳ 待決定 |
| A4 | 架構 | LineController.cs | 🔴 高 | ⏳ 待決定 |
