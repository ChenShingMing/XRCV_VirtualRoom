# 07 — 課程系統與 StarMap

> 最後驗證：2026-07-11（場景：Main.unity，Runtime 讀取確認）

---

## 一、課程框架（Topic Framework）

### 抽象層

```
TopicManager                            ← 管理所有課程
  └── Dictionary<string, Topic>
        └── Topic                       ← 課程生命週期容器
              └── TopicController (abstract)  ← 具體課程邏輯
```

### 設計模式

- **Template Method**：`Topic` 定義生命週期（Init / Enter / Update / Sync / Exit），子類覆寫
- **Strategy**：`TopicController` 是策略介面，StarMapTopicController 是具體實作
- **Observer**：`ClassroomManager` 的 UnityEvent 在 `SetTopic` / `ExitTopic` 時廣播

---

### TopicManager

**路徑**：`VirtualRoomPlugIn/1. VirtualRoom/Core/TopicManager/TopicManager.cs`  
**場景**：`===== Classroom =====/ClassroomManager/TopicManager`

| 屬性 | 值（Runtime 確認）|
|------|-----------------|
| syncFrequency | 0.5 秒 |
| currentTopic | 無（未進入課程）|
| topicDic 數量 | 1 |

**目前設定的課程：**

| Dictionary Key | topicName | Controller | Controller GO |
|---------------|-----------|------------|---------------|
| `虛擬星象館` | 虛擬星象館 | StarMapTopicController | StarMap |

---

### Topic 生命週期

```
進入課程：ClassroomManager.SetTopic(topicName)
  → TopicManager.SetTopic()
      → 退出當前課程（OnExit）
      → Topic.OnEnter()
          ├── 啟動 controller GameObject
          └── 設定 syncSenderName
      → OnSetTopic.Invoke()

每幀：
  → TopicManager.TopicHandle()
      → topic.OnUpdate()    ← 更新同步目標

每 0.5s：
  → TopicManager.TopicSyncHandle()
      ├── 如果是 Master Client（老師 / 導學模式）
      │   → topic.OnSync() → SendTopicControllerProperties()
      └── 如果是 Client（學生）
          → topic.OnSync() → UpdateTopicControllerProperties()

退出課程：ClassroomManager.ExitTopic()
  → TopicManager.ExitTopic()
      → Topic.OnExit()
          └── 關閉 controller GameObject
      → OnExitTopic.Invoke()
```

---

### 同步策略

| 教學模式 | 行為 |
|---------|------|
| Guidance（導學）| 所有學生跟隨 `syncSenderName`（Master Client / 老師）的課程狀態 |
| SelfStudy（自學）| 每人獨立，`syncSenderName` = 自己的 PlayerID |

**同步媒介**：Photon Room Custom Properties  
**Key 格式**：`{topicName}_starMapControlData_{property}_{syncSenderName}`

---

## 二、StarMap 課程（唯一實作）

### 場景結構

**Topic 設定**：TopicManager.topicDic["虛擬星象館"]  
**Controller GO**：場景中的 `StarMap` GameObject（位於 StarMap 課程相關物件群）

```
StarMapTopicController      (TopicController 具體實作)
  └── 掛在 StarMap GameObject 上
        │
        ↓ 取得參考
StarMapController (Singleton)
  ├── StarMap                     ← 星星粒子 + 星座線 LineRenderer
  │     └── CreateHipHierarchy   ← 生成星座 GameObject 層級和 3D 模型
  ├── StarMapRotate               ← 天球旋轉（依緯度、datetime 計算）
  ├── StarMapControlData          ← 所有可控參數的資料容器（序列化）
  │     ├── LocationData          ← ScriptableObject，位置清單
  │     └── PanoramicController   ← 360° 背景切換
  ├── DayNightEnvironmentControl (Singleton)  ← 天空盒日夜切換
  ├── SunRotate                   ← 太陽方位角 / 高度角計算
  └── UI 群組
        ├── ControlCanvasManager  ← 控制面板 Toggle / Text 更新
        ├── StarMapHUDController (Singleton)  ← 星座名稱標籤
        └── LocalicationSelectController  ← 地點選擇 ScrollView
```

---

### Photon 同步屬性（13 個 CustomProperties）

Key 格式：`{topicName}_starMapControlData_{property}_{syncSenderName}`

| 類型 | 屬性名稱 | 說明 |
|------|---------|------|
| bool | graticule | 顯示坐標網格 |
| bool | linkLine | 顯示星座連線 |
| bool | nameAndModel | 顯示星座名稱和 3D 模型 |
| bool | usePanoramic | 使用全景背景 |
| int | Year | 年份 |
| int | Month | 月份 |
| int | Day | 日期 |
| int | Hour | 小時 |
| int | currentlocalicationIndex | 位置清單索引（-1 = 自訂）|
| float | rotateSpeed | 天球自轉速度 |
| float | latitude | 緯度 |
| float | longitude | 經度 |
| string | currentLocalicationName | 位置名稱 |

---

### StarMapControlData 參數分類

**星座相關**
- `graticule` — 坐標網格
- `linkLine` — 星座連線
- `nameAndModel` — 星座名稱 + 3D 模型

**日期時間**
- `dateTime`（DateTime 物件）
- `Year, Month, Day, Hour`（整數，與 dateTime 同步）
- `rotateSpeed`（RotateSpeed enum：None / Slow / Normal / Fast → 0 / 0.5 / 3 / 8 deg/s）

**位置**
- `latitude, longitude` — 觀測點經緯度
- `currentlocalicationIndex` — 位置索引（-1 = 自訂輸入）
- `currentLocalicationName` — 位置名稱
- `usePanoramic` — 是否顯示全景背景
- `currentDay360, currentNight360` — 當前位置的日 / 夜全景紋理

---

### 資料來源

| 類型 | 路徑 |
|------|------|
| 星星資料（CSV）| 由 `StarMap.starFile` TextAsset 指定 |
| 星座線資料（CSV）| 由 `StarMap.lineFile` TextAsset 指定 |
| 星座 3D 模型 | `Resources/Prefabs/{星座縮寫}`（如 `Resources/Prefabs/UMi`）|
| 位置全景圖 | `Assets/StartMap/Art/LocationData/{地點名}/day.png, night.png` |
| 位置清單 | LocationData ScriptableObject |

---

### BVColorTest（天文顏色計算）

**路徑**：`Topics/StartMap/Scripts/Core/Main/BVColorTest.cs`

靜態方法 `Bv2rgb(float bv)` 將天文學 B-V 色指數轉換為 RGB 顏色，  
決定每顆星的渲染顏色（藍白星 → 偏藍，紅巨星 → 偏紅）。

---

## 三、新增課程的步驟

1. 繼承 `TopicController`，實作：
   - `SendTopicControllerProperties()` — 老師端發送 Photon 屬性
   - `UpdateTopicControllerProperties()` — 學生端接收並套用
   - `OnSwitchTeachingToGuidance()` / `OnSwitchTeachingToSelfStudy()`

2. 建立 `Topic` 子類（或直接使用），設定 `controller` 引用。

3. 在 TopicManager 的 `topicDic` Inspector 中新增 Key-Value。

4. 在主選單 UI 加入課程按鈕，呼叫 `ClassroomManager.SetTopic("{key}")`。

> 參考實作：`StarMapTopic.cs` + `StarMapTopicController.cs`
