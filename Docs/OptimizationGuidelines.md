# XRCV VirtualRoom — 優化方針

> Unity 6000.3.12f1 | 更新：2026-07-11  
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

## 二、PlatformSwitcher 平台定義不完整 🟡 P1

### 問題

`PlatformSwitcher.Platform` enum 只有 `PC` 和 `OpenXR`，沒有 `Quest`：

```csharp
public enum Platform { PC, OpenXR }  // Quest 缺失
```

`OnSwitchToOculus()` 目前直接把 platform 設為 `OpenXR`，表示 Quest 和 VIVE 共用同一套 UI 和 InputHandler。

### 建議

增加 `Quest` 平台選項，並在 Inspector 設置對應的 `PlatformSetting`（UI、InputHandler）。
如果 Quest 和 VIVE 的 UI/InputHandler 設計確實相同，才保持合併。

---

## 三、InputHandler_OVR 未完成 🟡 P1

### 問題

`InputHandler_OVR.cs` 大部分方法拋出 `NotImplementedException`，但類別仍存在且被引用架構。

### 建議

兩選一：
1. **移除**：如果 Quest 未來改走 OpenXR 路線（見 P0），`InputHandler_OVR` 就完全不需要了
2. **實作**：如果要保留 OculusLoader 路徑，補完 OVRInput 的實作

---

## 四、InputHandler_PC 缺失 🟡 P1

### 問題

`PlatformSwitcher` 有 `Platform.PC` 設定，但整個 Assets 目錄找不到 `InputHandler_PC.cs`。PC 模式的輸入（滑鼠、鍵盤）目前可能沒有對應的 Handler。

### 建議

確認 PC 模式的操作需求後，實作 `InputHandler_PC`，或確認 PC 模式是否有其他方式處理輸入（如 EventSystem 的滑鼠 Raycast）。

---

## 五、星圖 CSV 每次重新解析 🟡 P1

### 問題

`StarMap.CreateHipList()` 和 `CreateHipLineList()` 每次啟動都從 TextAsset 重新解析 CSV 字串，沒有快取。星星資料量可能達數千筆。

### 建議

```csharp
// 在 Awake 時解析並快取
private List<HipData> _hipCache;
private List<HipLine> _hipLineCache;

void Awake()
{
    _hipCache = CreateHipList();
    _hipLineCache = CreateHipLineList();
}
```

或考慮改用 ScriptableObject 預先序列化資料，完全省去執行時解析。

---

## 六、Photon CustomProperties 的 Key 命名 🟡 P1

### 問題

StarMap 同步用 13 個獨立 Key，格式為：
```
{topicName}_starMapControlData_{property}_{syncSenderName}
```
每次同步都要 Set 13 個屬性，產生 13 次 Photon 封包。

### 建議

把 13 個屬性打包成一個 JSON/byte array，用單一 Key 同步：
```csharp
// 單次 SetCustomProperties 傳遞所有資料
var data = JsonUtility.ToJson(starMapControlData);
PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable {
    { $"{topicName}_data_{syncSenderName}", data }
});
```
減少網路封包數量，也簡化 UpdateTopicControllerProperties 的讀取邏輯。

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

- `com.unity.xr.oculus 4.5.2`：Unity 6 可用但 deprecated，追蹤 Meta 後續更新
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
P0（盡快）:
  └─ 將 SwitchToOculus 從 OculusLoader 改為 OpenXR Feature 切換
     → 解除對 com.unity.xr.oculus 的依賴

P1（下個版本）:
  ├─ 補 InputHandler_PC
  ├─ PlatformSwitcher 加入 Quest 平台 enum
  ├─ InputHandler_OVR 決定保留還是移除
  ├─ StarMap 資料快取
  └─ Photon 同步打包

P2（有空再做）:
  ├─ 降低 ZodiacsController 更新頻率
  ├─ 減少 Singleton 靜態依賴
  └─ 恢復 Build 按鈕
```
