# 09 — 自動更新系統規劃

> 建立日期：2026-07-11 | 狀態：✅ 規劃定案，待實作
> 四個前提問題已確認，詳見第六節。

---

## 需求背景

目前應用程式直接提供 PC 執行檔（.exe）與 Android 安裝包（.apk），未上架任何平台。每次版本更新，所有使用者必須手動重新安裝，維護成本高。

**目標**：應用程式啟動時，若有網路連線，自動比對版本並下載更新，使用者只需在第一次安裝「含更新系統的版本」後，後續所有更新均自動完成（Android 端只需一次點選系統確認對話框）。

**首次部署**：第一版含更新系統的 build，由廠商或自行前往學校安裝，之後不再需要人工介入。

---

## 確認的前提條件

| # | 問題 | 回答 | 決策影響 |
|---|------|------|----------|
| 1 | 檔案存放位置 | Firebase Storage 尚未開通，評估是否要開 / 用 Google Drive | **→ 使用 Firebase Storage**（見下方說明） |
| 2 | 更新內容類型 | 新教學功能（程式碼）+ 360 場景 + 教材（大型素材） | **→ 採用雙層更新架構** |
| 3 | Quest 使用方式 | 學校端集中使用，需要最簡單的自動更新體驗 | **→ PackageInstaller 一鍵確認，流程自動** |
| 4 | 首次部署方式 | 廠商或自行前往學校安裝 | **→ 首版手動部署，之後完全自動** |

---

## 關於檔案存放位置

### 為什麼不建議 Google Drive

- Google Drive 的「直接下載連結」對大型檔案不穩定（Google 會在大檔案上加入確認頁）
- 無法用程式可靠地取得直接 byte stream
- 適合人工手動下載，不適合程式自動下載

### 建議：開通 Firebase Storage

| 理由 | 說明 |
|------|------|
| 同一 Firebase 專案 | 已有 Firestore 和 Firebase SDK，不需新增依賴 |
| 免費額度充足 | 5 GB 儲存、每天 1 GB 下載，學校規模完全夠用 |
| 直接下載 URL | Firebase Storage 提供穩定的 HTTPS 直連 URL |
| 認證可選 | Public 下載不需要 token，設定簡單 |
| 與 version manifest 配合 | Firestore 放 manifest（含 URL），Storage 放實際檔案 |

備選方案（若不想開通 Firebase Storage）：**GitHub Releases**（免費、穩定、有直連 URL、天然版本管理）。

---

## 雙層更新架構（定案）

根據更新內容「程式碼修改 + 大型素材（360 場景、教材）」的混合特性，採用雙層策略：

```
Layer 1：Binary 更新（完整替換）
  用途：程式碼修改、功能新增、系統修正
  觸發：app version 變動
  大小：整個 exe / APK（~80–150 MB）
  頻率：較低（主要版本、功能版本）

Layer 2：AssetBundle 更新（增量下載）
  用途：新 360 場景、新教材圖片 / 影片、新課程素材
  觸發：asset bundle version 變動（獨立於 app version）
  大小：僅下載新增 / 變動的 bundle（按需）
  頻率：較高（每次新增教學內容）
```

**Layer 2 的好處**：若只是新增一個 360 場景（可能 50MB），不需要讓全校重新下載整個 APK（150MB），只下載那個 bundle 即可。

**實作順序**：Layer 1（Binary 更新）先做，穩定後再加 Layer 2（AssetBundle）。

---

## Firebase Manifest 結構

### Layer 1：App Version Manifest

```
Firestore Collection: "AppConfig"
Document: "version"
```

```json
{
  "latestVersion": "1.2.0",
  "minRequiredVersion": "1.0.0",
  "forceUpdate": true,
  "releaseNote": "新增星圖教學模組、修正教師端連線問題",
  "pc": {
    "downloadUrl": "https://firebasestorage.googleapis.com/v0/b/.../XRCV_v1.2.0.exe",
    "checksum": "sha256:abc123...",
    "sizeBytes": 85000000
  },
  "android": {
    "downloadUrl": "https://firebasestorage.googleapis.com/v0/b/.../XRCV_v1.2.0.apk",
    "checksum": "sha256:def456...",
    "sizeBytes": 120000000
  }
}
```

### Layer 2：AssetBundle Version Manifest（後期加入）

```
Firestore Collection: "AppConfig"
Document: "assetBundles"
```

```json
{
  "bundles": [
    {
      "name": "panoramic_solar_system",
      "version": "3",
      "url": "https://firebasestorage.googleapis.com/v0/b/.../panoramic_solar_system_v3",
      "checksum": "sha256:xyz...",
      "sizeBytes": 52000000,
      "platforms": ["pc", "android"]
    }
  ]
}
```

---

## 啟動流程（Layer 1 實作階段）

```
App 啟動（Loading Scene）
  ↓
NetworkReachability 檢查
  ├─ 無網路 → 顯示「離線模式，跳過更新」→ 繼續原有流程
  └─ 有網路
        ↓
  VersionCheckService.CheckAsync()
  讀取 Firestore "AppConfig/version"
        ↓
  比對版本
  ├─ 本地版本 ≥ latestVersion → 繼續原有流程
  └─ 本地版本 < latestVersion
        ├─ forceUpdate = true  → 強制更新 UI（不可關閉，無「稍後」按鈕）
        └─ forceUpdate = false → 提示 UI（可選擇「稍後更新」）
              ↓ 確認更新
        DownloadManager.DownloadAsync()
        UnityWebRequest 下載，每秒更新進度條 %
              ↓
        ChecksumVerifier.Verify()（SHA256）
        失敗 → 顯示錯誤「下載失敗，請重試」
              ↓ 驗證成功
        UpdateApplier（平台判斷）
        ├─ PC：
        │    寫入 %TEMP%\xrcv_update.bat
        │    （等待主程式關閉 → 替換 exe → 重啟）
        │    Application.Quit()
        └─ Android / Quest：
             Application.temporaryCachePath 儲存 APK
             呼叫 Android PackageInstaller API
             系統顯示「是否安裝？」對話框
             使用者點一下「安裝」→ 安裝完成自動重啟
  ↓
原有流程：License 驗證（FirebaseLicenseInfoManager）→ 主場景
```

---

## 新增元件清單

| 類別 | 職責 | 備註 |
|------|------|------|
| `UpdateManager.cs` | 協調整個更新流程的入口，掛在 Loading Scene | 呼叫下方各元件 |
| `VersionCheckService.cs` | 向 Firestore 查詢版本 manifest | 複用現有 Firebase Firestore 呼叫模式 |
| `DownloadManager.cs` | UnityWebRequest 分段下載、進度事件回報、斷點續傳（選配） | Coroutine |
| `ChecksumVerifier.cs` | SHA256 計算與比對 | 靜態工具類，System.Security.Cryptography |
| `UpdateApplier_PC.cs` | 寫 bat script 到 %TEMP%，執行 Application.Quit | `#if UNITY_STANDALONE_WIN` |
| `UpdateApplier_Android.cs` | 呼叫 Android PackageInstaller Java API | `#if UNITY_ANDROID`，AndroidJavaObject |
| `UpdateUI.cs` | 進度條、對話框（強制 / 選擇性）、錯誤提示 | 新增 UI Prefab |

---

## 對現有架構的影響

| 現有檔案 | 影響程度 | 說明 |
|----------|---------|------|
| `LoadingManager.cs` | 極低 | 在 Loading Scene 的啟動事件前插入 UpdateManager，不改既有邏輯 |
| `FirebaseLicenseInfoManager.cs` | 無 | VersionCheckService 獨立撰寫，不動原有檔案 |
| `ClassroomManager.cs` | 無 | 讀取現有 `version` 欄位做本地版本比對 |
| `AndroidManifest.xml` | 低 | 新增 `REQUEST_INSTALL_PACKAGES` 一行 |
| Photon / XR / Topic 系統 | 無 | 完全不影響 |

---

## Android 端使用者體驗（Quest）

1. 老師/學生打開 App
2. 自動進入「正在檢查更新…」畫面（2-3 秒）
3. 若有更新：顯示「發現新版本 1.2.0，正在下載…」+ 進度條
4. 下載完成後，系統彈出安裝確認對話框（Android 系統行為，無法省略）
5. 使用者點一下「安裝」
6. App 自動重啟，進入正常登入流程

**注意**：Android 系統的安裝確認對話框是平台強制行為（安全機制），無法在不越獄的情況下省略。但整個流程只需要使用者**點一下**，對學校端使用者而言可以接受。

---

## 安全考量

- **Checksum 驗證**：SHA256，防止傳輸損壞或中途竄改
- **HTTPS only**：Firebase Storage 預設 HTTPS
- **下載超時處理**：設定 timeout（建議 60s 連線 / 無上限下載），失敗顯示錯誤允許重試，不可卡住啟動
- **PC bat script**：寫入 `%TEMP%` 目錄，路徑固定，防止注入；替換前備份舊版本
- **版本回滾**：可在 Firestore manifest 加 `rollbackVersion` 欄位備用

---

## 實作優先順序

| 階段 | 內容 | 估計工期 |
|------|------|----------|
| Phase 1 | Firebase Storage 開通 + 上傳測試檔 + Firestore manifest 設定 | 0.5 天（手動） |
| Phase 2 | `VersionCheckService` + `UpdateManager` 骨架 + 版本比對邏輯 | 1 天 |
| Phase 3 | `DownloadManager` + 進度條 UI | 1 天 |
| Phase 4 | `ChecksumVerifier` + 錯誤處理 | 0.5 天 |
| Phase 5 | `UpdateApplier_PC`（PC 端優先，測試方便） | 1 天 |
| Phase 6 | `UpdateApplier_Android`（Quest） + AndroidManifest 權限 | 1.5 天 |
| Phase 7 | Loading Scene 整合 + 端對端測試（舊版 → 自動更新 → 新版） | 1 天 |
| **Layer 2** | AssetBundle 增量更新（360 場景、教材）| 另行規劃 |

**總計**：約 6.5 開發天（Layer 1 Binary 更新完整實作）
