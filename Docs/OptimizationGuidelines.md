# XRCV VirtualRoom — 優化方針

> Unity 6000.3.12f1 | 更新：2026-07-11（P0 ✅ P1 ✅）  
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

## 三、InputHandler_OVR 未完成 ✅ 已完成（P1）

- P0 確認 Quest 改走 OpenXR，`InputHandler_OVR` 已無用途
- 保留空殼（加上 `[Obsolete]`）以避免場景中 `Input_OVR`（inactive）出現 Missing Script 警告
- 待確認 `Input_OVR` GameObject 可安全移除後，整個檔案可一併刪除（P2 可選）

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

## 七、多 Singleton 的場景耦合 🟢 P2

### 問題

`StarMapController`、`DayNightEnvironmentControl`、`StarMapHUDController` 都是 Singleton，它們透過靜態 `.ins` 存取，會在場景卸載時遺留 null reference 風險。

### 建議

考慮改用 **Service Locator** 或讓 `ClassroomManager` 管理這些參考，降低靜態狀態依賴。

---

## 八、ZodiacsController 每幀做 Viewport 計算 🟢 P2

### 問題

`ZodiacsController.CheckAllZodiacsStatus()` 可能每幀對所有星座做 `WorldToViewportPoint` 計算並更新顯示狀態。

### 建議

降低更新頻率（如每 0.1 秒），或只在視角改變超過一定角度時才重新計算。

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
  ├─ ZodiacsController 節流（FixedUpdate → Update 0.1s）
  ├─ Singleton OnDestroy 清除（StarMapController / DayNight / HUD）
  ├─ Input_OVR / InputHandler_OVR 完整移除
  └─ 恢復 Build 按鈕：⏭️ 跳過（手動 Build 工作流即可）

效能掃描修正 ✅（2026-07-11）:
  ├─ Camera.main 快取（AlignPosToController / XRRayInteractorUICursor /
  │   StarMapHUDController / InfoCanvasUI）
  ├─ InfoCanvasUI UI 更新節流（每幀 → 0.1s）
  └─ ControlCanvasManager UI 更新節流 + Toggle 值比對 guard

待觀察:
  └─ DynamicMoveProvider CS0618 警告，等 XRI 正式替代方案
```
