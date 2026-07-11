# XRCV VirtualRoom — 優化方針

> Unity 6000.3.12f1 | 更新：2026-07-11（P0 ✅ P1 ✅）｜效能最佳化規劃：2026-07-11  
> 本文件列出技術債、架構改進點，以及優先級建議。

---

## 優先級定義

- 🔴 **P0 — 高風險**：影響出版或未來可維護性，越快處理越好
- 🟡 **P1 — 建議改進**：有明確改善空間，下個版本處理
- 🟢 **P2 — 可選優化**：效能或品質提升，有閒再做

---

## 一、平台支援架構 🔴 P0

### 問題

`com.unity.xr.oculus 4.5.2` 在 Unity 6 已標為 **Deprecated**，Meta 官方已停止主動維護，未來 Unity 版本可能移除。

目前 `SwitchToOculusForAndroid()` 走的是：
```
Android + 移除 OpenXRLoader + 加入 OculusLoader（原生 SDK）
```
這條路徑繞過了 OpenXR，導致：
- XRI 的 Action Binding 無法使用（因為 Action 是綁在 OpenXR 的 Interaction Profile 上）
- OculusTouchControllerProfile 設定無效
- 與 VIVE 路徑的 InputHandler 設計不一致

### 建議方案：兩個平台都走 OpenXR，只切換 Feature

```
Android VIVE 模式        Android Quest 模式
OpenXR Loader            OpenXR Loader
  + VIVEFocus3Profile     + OculusXRFeature
  + VIVE XR Support       + OculusTouchControllerProfile
  - OculusXR              - VIVE Features
```

#### PlatformBuilder 改寫方向

```csharp
// 新的 SwitchToQuestForAndroid()
public void SwitchToQuestForAndroid()
{
    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
    // 不再換 Loader，改為切換 OpenXR Features
    SetOpenXRFeature<OculusXRFeature>(BuildTargetGroup.Android, true);
    SetOpenXRFeature<OculusTouchControllerProfile>(BuildTargetGroup.Android, true);
    SetOpenXRFeature<VIVEFocus3Profile>(BuildTargetGroup.Android, false);
    // ... 關閉其他 VIVE features
}

// 使用 OpenXRFeatureSetManager 或直接寫 OpenXR Package Settings
```

完成後可從 manifest.json 移除 `com.unity.xr.oculus`。

---

## 二、PlatformSwitcher 平台定義不完整 ✅ 已完成（P1）

- `Platform` enum 加入 `Quest`
- `OnSwitchToOculus()` 改為設定 `Platform.Quest`
- `SwitchPlatform()` 自動 fallback 到 `Platform.OpenXR` 設置（Quest 和 VIVE 場景配置相同：同一套 XR Origin / MainUI_XR / InputHandler_OpenXR）
- 如未來 Quest 需要獨立設置，在 Inspector 新增 `Quest` key 的 `PlatformSetting` 即可生效

---

## 三、InputHandler_OVR 未完成 ✅ 完整移除（2026-07-11）

- `InputHandler_OVR.cs` 已刪除
- 場景中無 `Input_OVR` GameObject（搜尋確認）
- 所有 .cs 檔無任何 `InputHandler_OVR` 或 `Input_OVR` 引用

---

## 四、InputHandler_PC ✅ 已確認存在（P1）

- 路徑：`VirtualRoomPlugIn/1. VirtualRoom/Core/InputManager/InputHandler_PC.cs`
- 完整實作：ESC = 主選單、O = 課程選單、LMB = 第一人稱視角、MMB = 畫線、I = 筆模式
- 場景中 `Input_PC`（inactive）掛載此 Handler，切換到 PC 模式時啟用

---

## 五、星圖 CSV 解析 ✅ 已確認無問題（P1）

- `StarMap.Awake()` 呼叫 `CreateHipList()` 和 `CreateHipLineList()` 各一次，結果存入 `hipList`（private）和 `hipLineList`（public）欄位
- Topic 生命週期只做 `SetActive(true/false)`，不銷毀重建 GameObject，因此 `Awake` 只執行一次
- CSV 已有隱式快取，無需額外修改

---

## 六、Photon 同步打包 ✅ 已完成（P1）

原本 13 個獨立 CustomProperty key → 壓縮為單一 JSON 字串 key：
```
{topicName}_starMapData_{syncSenderName}
```
- `StarMapTopicData` 加上 `[System.Serializable]` + `ApplyTo()` 方法
- `SendTopicControllerProperties()` 使用 `JsonUtility.ToJson()` 一次傳送
- `UpdateTopicControllerProperties()` 加上 `ContainsKey()` 保護，避免 Room 初始化前的 NullReferenceException
- 每次 Photon 同步從 13 次 SetCustomProperties 降為 1 次

---

## 七、多 Singleton 的場景耦合 ✅ 已完成（2026-07-11）

三個 Singleton 的 null 安全性已全部到位：

| 類別 | OnDestroy 清除 | 呼叫端 null 檢查 |
|------|:-:|:-:|
| `StarMapController` | ✅ | ✅（所有呼叫端均有 `if (ins)` 判斷） |
| `DayNightEnvironmentControl` | ✅ | ✅（`StarMapControlData` 兩處加上 `ins != null` guard） |
| `StarMapHUDController` | ✅ | ✅ |

> Service Locator 完整重構保留為未來架構演進選項，不在此版本執行（風險高、收益有限）。

---

## 八、ZodiacsController 每幀做 Viewport 計算 ✅ 已完成（2026-07-11）

- `checkInterval = 0.1f` 節流，已從每幀降為每 0.1 秒執行一次
- `Camera.main` 改為 `_mainCamera`（`Awake()` 快取，lazy fallback 補值）
- `CheckAllZodiacsStatus()` 開頭加入 `if (_mainCamera == null) _mainCamera = Camera.main` 保護平台切換情境

---

## 九、Unity 6 API 補強 🟡 P1

### 已完成（上次升級修復）

| 項目 | 狀態 |
|------|------|
| FindObjectOfType → FindFirstObjectByType | ✅ 已修復 |
| FindObjectsOfType → FindObjectsByType | ✅ 已修復 |
| Rigidbody2D.isKinematic → bodyType | ✅ 已修復 |
| PrefabInstanceStatus.Disconnected 移除 | ✅ 已修復 |
| PlayMaker DefinesHelper SetScriptingDefineSymbolsForGroup | ✅ 已修復 |
| Google.IOSResolver / Firebase.Editor validateReferences | ✅ 已修復 |

### 待觀察

- `com.unity.xr.oculus 4.5.2`：✅ 已從 manifest.json 移除（2026-07-11）
- `DynamicMoveProvider` 使用 `#pragma warning disable CS0618`（繼承 deprecated 基類），等 XRI 提供正式替代方案後更新

---

## 十、Build 按鈕恢復 🟢 P2

`PlatformBuilder` 的 Build 按鈕群目前被 `/* ... */` 整個註解掉。  
完成平台切換架構更新（見 P0）後，可重新啟用並加上：
- Build 前自動儲存 Scene
- Build 路徑選擇器（目前硬編碼 `Build/{date}/`）
- Build 結果通知

---

## 執行優先順序

```
P0 ✅（2026-07-11 完成）:
  ├─ PlatformBuilder 改為 OpenXR Feature 切換，移除 OculusLoader 依賴
  └─ com.unity.xr.oculus 套件從 manifest.json 移除

P1 ✅（2026-07-11 完成）:
  ├─ InputHandler_PC：確認已存在（VirtualRoomPlugIn/）
  ├─ PlatformSwitcher：加入 Quest enum + fallback 邏輯
  ├─ InputHandler_OVR：移除 GameObject + 刪除 .cs
  ├─ StarMap CSV：確認已有 Awake 快取，無需修改
  └─ Photon 同步：13 key → 單一 JSON key

P2 ✅（2026-07-11 完成）:
  ├─ ZodiacsController 節流（0.1s）+ Camera.main 快取
  ├─ Singleton OnDestroy 清除（StarMapController / DayNight / HUD）
  ├─ DayNightEnvironmentControl.ins null guard（StarMapControlData 兩處）
  ├─ Input_OVR / InputHandler_OVR 完整移除（確認場景無殘留）
  └─ 恢復 Build 按鈕：⏭️ 跳過（手動 Build 工作流即可）

效能掃描修正 ✅（2026-07-11）:
  ├─ Camera.main 快取（AlignPosToController / XRRayInteractorUICursor /
  │   StarMapHUDController / InfoCanvasUI）
  ├─ InfoCanvasUI UI 更新節流（每幀 → 0.1s）
  └─ ControlCanvasManager UI 更新節流 + Toggle 值比對 guard

Bug 修正 ✅（2026-07-11）:
  ├─ Plasma shader 星座連線黑色：o.Albedo → o.Emission（夜間自發光）
  └─ CSV 移除 28 個自連 row（StartID==EndID 產生 artifact）

待觀察:
  └─ DynamicMoveProvider CS0618 警告，等 XRI 正式替代方案
```

---

## 效能最佳化規劃（待執行，2026-07-11 規劃）

> 調查範圍：網路同步、星空系統、3D 模型、貼圖、Update 數量、UI

### 調查結論摘要

- 場景 MonoBehaviour 數：1529，Update() 42 個，FixedUpdate() 23 個
- 貼圖資產：100 個，其中 95 個 >=1024px，多張全景圖達 8192×4096
- 3D 模型：13 個 FBX（12 星座 + 經緯球），全部 meshCompression=Off、isReadable=True
- StarMap 系統有 6 個 FixedUpdate（StarMap、StarMapController、DayNight、BVColorTest、StarMapRotate、SunRotate）
- Photon 同步：已完成 P1 優化（13 key → 1 JSON），目前只有 StarMapTopicController 使用 CustomProperties
- LocationDataManager（全景圖本地載入器）：**場景中無實例，StreamingAssets 無資料，確認未使用 → 排除優化**
- 全景圖大檔（8192×4096 PNG）：確認是否有 Material 引用，若無則不影響執行時效能

---

### 🔴 P0 — 立即處理

#### BVColorTest.FixedUpdate() 每幀執行複雜顏色計算
- **檔案**：`Assets/2. XRCV_VirtualRoom/Topics/StartMap/Scripts/Core/Main/BVColorTest.cs`
- **問題**：`FixedUpdate()` 每幀呼叫 `SetColor()`，內有 `float.Parse(BVText.text)` + BV→RGB 完整色彩空間轉換
- 這是調試工具用的 UI 元件（Debug 用 `Image` + `Text`），不應在正式版每幀執行
- **修法**：將 `FixedUpdate()` 清空（保留靜態方法供 StarMap.cs 呼叫）；同時 `Update()` 本體也是空的可一併移除

---

### 🟡 P1 — 高優先

#### ① 13 個 FBX 模型 Import Settings 未最佳化
- **路徑**：`Assets/2. XRCV_VirtualRoom/Topics/StartMap/Art/Models/*.FBX`
- **問題**：
  - `isReadable = True`：Mesh 資料在 CPU RAM 與 GPU VRAM 各存一份，Quest 記憶體珍貴
  - `meshCompression = Off`：可開啟 Medium 節省約 30% 磁碟/記憶體
- **修法**：批次修改 13 個 FBX Import Settings
  - `isReadable = False`
  - `meshCompression = Medium`

#### ② 全景圖 Android 平台格式未指定（需先確認是否有被引用）
- **問題**：多張 2048–8192px 的全景 PNG，Android 平台使用 `default(Compressed)` 而非明確 ASTC
- Quest 設備需 ASTC 格式；8192px 超出部分 Quest 硬體上限
- **前置確認**：先檢查這批大圖是否有 Material 或 Script 引用（若無引用不打包）
- **修法**：有引用的全景圖 → Android 平台設定 `ASTC 4x4`，maxSize 鎖 `4096`

---

### 🟢 P2 — 建議優化

#### ③ DayNightEnvironmentControl.FixedUpdate() 節流
- **檔案**：`Assets/2. XRCV_VirtualRoom/Topics/StartMap/Scripts/Core/DayNightEnvironmentControl.cs`
- **問題**：`FixedUpdate()` 每幀檢查 `sunTrans.position.y`，但日夜切換極低頻（StarMap 使用期間可能完全不切換）
- **修法**：加 1 秒節流（checkInterval = 1f），與 ZodiacsController 同模式

#### ④ Resources.Load 12 個星座 Prefab → Inspector 直接引用
- **問題**：`CreateHipHierarchy.CreateHipGameObject()` 用字串 `Resources.Load("Prefabs/" + name)` 動態載入
- 字串查找 + Resources 系統有額外開銷；若名稱不符直接靜默失敗
- **修法**：`CreateHipHierarchy` 加 `[SerializeField] GameObject[] zodiacPrefabs`，Inspector 直接拖入，移除 Resources 依賴

#### ⑤ 非使用中 Camera 關閉 enabled
- **問題**：`Main Camera`（PC 模式）和 `MainCamera`（XR 模式）目前同時 active，閒置的 Camera 仍執行 culling
- **修法**：在 `PlatformSwitcher.SwitchPlatform()` 切換時同步設定非使用 Camera 的 `enabled = false`

---

### 執行順序

```
效能最佳化 ✅（2026-07-11 完成）:
  P0:
    └─ BVColorTest.FixedUpdate() 清空 ✅

  P1:
    ├─ 13 個 FBX：isReadable=False、meshCompression=Medium ✅
    └─ 全景圖大檔 68 張：Android ASTC 4x4 + maxSize 4096 ✅
         (10 張無引用略過：Custom_Day/Night.png, noise2.png, CloudyCrown_Midnight_*.png)

  P2 ✅（2026-07-11 完成）:
    ├─ DayNightEnvironmentControl FixedUpdate 節流（1s）
    ├─ Resources.Load 12 prefab → zodiacPrefabs[] Inspector 直接引用（需在 Inspector 拖入）
    └─ PlatformSetting.platformCamera 欄位：切換時設定 camera.enabled（可選，Inspector 拖入）

  排除:
    └─ LocationDataManager 快取（系統未啟用，StreamingAssets 無資料）
```
