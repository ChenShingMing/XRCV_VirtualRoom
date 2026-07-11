# 09 — 自動更新系統

> 建立日期：2026-07-11 | 狀態：Phase 0–1 完成

---

## 一、需求背景

目前應用程式直接提供 PC 執行檔（.exe）與 Android 安裝包（.apk），未上架任何平台。每次版本更新，所有使用者必須手動重新安裝，維護成本高。

**目標**：應用程式啟動時，若有網路連線，自動比對版本並下載更新：
- Android / Quest：使用者只需點一下系統確認對話框
- PC：完全自動，程式重啟後即為新版本

**首次部署**：第一版含更新系統的 build 由廠商安裝，之後不再需要人工介入。

---

## 二、整體架構

### 雙層更新策略

```
Layer 1：Binary 更新（現在實作）
  用途：程式碼修改、功能新增、系統修正
  觸發：app version 版號變動
  內容：Android → 完整 APK；PC → 完整資料夾 ZIP
  頻率：較低（主要版本、功能版本）

Layer 2：AssetBundle 更新（後期加入）
  用途：新 360 場景、教材圖片 / 影片
  觸發：asset bundle version 獨立版號變動
  內容：只下載新增 / 變動的 bundle
  頻率：較高（每次新增教學內容）
```

### 基礎設施

| 角色 | 服務 | 說明 |
|------|------|------|
| 版本 Manifest | Firebase Firestore `AppConfig/version` | 存放最新版本號、下載 URL、Checksum |
| 檔案存放 | GitHub Releases | APK / ZIP 實體檔案，穩定直連 URL |
| 下載協定 | HTTPS（UnityWebRequest） | 不依賴任何額外 SDK |

> Firebase Storage 需付費，改用 **GitHub Releases**（免費、無流量上限、單檔 ≤ 2GB）。

---

## 三、Firestore Manifest 結構

### Collection / Document 路徑

```
AppConfig/version          ← Layer 1 App 版本
AppConfig/assetBundles     ← Layer 2 AssetBundle 版本（後期）
```

### `AppConfig/version` 欄位定義

```json
{
  "latestVersion":      "20260711",
  "minRequiredVersion": "20260711",
  "forceUpdate":        false,
  "releaseNote":        "首版含自動更新系統",

  "android": {
    "downloadUrl": "https://github.com/ChenShingMing/XRCV_VirtualRoom/releases/download/v20260711/creativeXRworld_20260711.apk",
    "checksum":    "sha256:<hash>",
    "sizeBytes":   436207616
  },

  "pc": {
    "downloadUrl": "https://github.com/ChenShingMing/XRCV_VirtualRoom/releases/download/v20260711/creativeXRworld_20260711_PC.zip",
    "checksum":    "sha256:<hash>",
    "sizeBytes":   0
  }
}
```

| 欄位 | 型別 | 說明 |
|------|------|------|
| `latestVersion` | string | 最新版號，格式 `yyyyMMdd`（或 `yyyyMMdd-N` 同日多版） |
| `minRequiredVersion` | string | 低於此版本強制更新，不可略過 |
| `forceUpdate` | bool | `true` → UI 不顯示「稍後」按鈕 |
| `releaseNote` | string | 顯示給使用者的更新說明 |
| `android.downloadUrl` | string | GitHub Release APK 直連 URL |
| `android.checksum` | string | `sha256:<hex>`，下載後驗證 |
| `android.sizeBytes` | int | 用於進度條百分比計算 |
| `pc.*` | — | 同上，對應 PC ZIP |

### 版本比對邏輯

```csharp
// 本地版本從 Application.version 讀取
// 雲端版本從 Firestore latestVersion 讀取
// 比較方式：字串直接比較（yyyyMMdd 格式可直接大小比較）
bool needsUpdate = string.Compare(localVersion, cloudVersion) < 0;
bool isForced    = needsUpdate && (forceUpdate || localVersion < minRequiredVersion);
```

---

## 四、啟動更新流程

```
App 啟動（Loading Scene）
  ↓
NetworkReachability 檢查
  ├─ 無網路 → Toast「離線模式，跳過更新」→ 進 License 驗證
  └─ 有網路
        ↓
  VersionCheckService.CheckAsync()
  讀取 Firestore "AppConfig/version"（複用現有 Firebase REST / native 路徑）
        ↓
  本地版本 ≥ latestVersion？
  ├─ 是 → 進 License 驗證（原有流程）
  └─ 否
        ↓
  isForced？
  ├─ true  → 強制更新 UI（無「稍後」按鈕）
  └─ false → 提示 UI（有「稍後更新」按鈕）
        ↓ 使用者確認 / 強制
  DownloadManager.DownloadAsync()
  UnityWebRequest 下載，每秒更新進度條
        ↓
  ChecksumVerifier.Verify()（SHA256）
  失敗 → 錯誤提示「下載失敗，請重試」（可重試，不可卡住啟動）
        ↓ 驗證成功
  UpdateApplier（平台判斷）
  ├─ Android / Quest：
  │    寫入 Application.temporaryCachePath/update.apk
  │    呼叫 Android PackageInstaller API
  │    系統對話框「是否安裝？」→ 使用者點一下
  │    安裝完成自動重啟
  └─ PC（Windows）：
       ZIP 存入 %TEMP%\xrcv_update.zip
       寫入 %TEMP%\xrcv_update.bat
       Application.Quit()
       bat：等程式關閉 → PowerShell 解壓 ZIP 覆蓋安裝目錄 → 重啟 EXE
```

---

## 五、元件設計

### 新增元件清單

| 元件 | 掛載位置 | 職責 |
|------|---------|------|
| `UpdateManager.cs` | Loading Scene | 協調整個更新流程，呼叫下方各元件 |
| `VersionCheckService.cs` | — | 向 Firestore 查詢 `AppConfig/version`，回傳版本資訊 |
| `DownloadManager.cs` | — | UnityWebRequest 下載、進度事件、逾時處理 |
| `ChecksumVerifier.cs` | — | SHA256 計算與比對（靜態工具類） |
| `UpdateApplier_Android.cs` | — | 呼叫 Android PackageInstaller Java API |
| `UpdateApplier_PC.cs` | — | 寫 bat script，執行 Application.Quit |
| `UpdateUI.cs` | Loading Scene UI | 進度條、通知對話框、錯誤提示 |

### VersionCheckService

```csharp
// 複用 FirebaseLicenseInfoManager 的 GPS 偵測邏輯
// Quest → Firebase REST API
// 一般 Android / PC → Firebase native SDK

public class VersionManifest
{
    public string latestVersion;
    public string minRequiredVersion;
    public bool   forceUpdate;
    public string releaseNote;
    public PlatformAsset android;
    public PlatformAsset pc;
}

public class PlatformAsset
{
    public string downloadUrl;
    public string checksum;
    public long   sizeBytes;
}
```

### DownloadManager

```csharp
// Coroutine 下載，每幀更新進度
// 事件：OnProgress(float 0~1)、OnComplete(string filePath)、OnError(string msg)
// 逾時：連線 60 秒，下載無上限（大檔案）
// 失敗：顯示錯誤，允許重試，絕不卡住啟動流程
```

### UpdateApplier_Android（Quest）

```csharp
// 需要 AndroidManifest.xml 權限：
//   <uses-permission android:name="android.permission.REQUEST_INSTALL_PACKAGES" />

// Java 呼叫路徑：
// FileProvider → content:// URI → PackageInstaller → 系統安裝對話框
```

### UpdateApplier_PC（Windows）

```bat
@echo off
REM %TEMP%\xrcv_update.bat 內容範例
timeout /t 2 /nobreak > NUL
powershell -Command "Expand-Archive -Path '%TEMP%\xrcv_update.zip' -DestinationPath '%INSTALL_DIR%' -Force"
start "" "%INSTALL_DIR%\creativeXRworld.exe"
del "%~f0"
```

---

## 六、平台差異對照

| 項目 | Quest / Android | PC（Windows） |
|------|:--------------:|:-------------:|
| 下載格式 | APK | ZIP（含完整資料夾） |
| 安裝方式 | PackageInstaller | PowerShell 解壓覆蓋 |
| 使用者操作 | 點一下「安裝」 | 無需操作 |
| 重啟方式 | 系統自動重啟 | bat 重啟 EXE |
| 打包指令 | Unity Build And Run | Unity Build → 手動 ZIP（排除 BurstDebugInformation） |

---

## 七、GitHub Releases 操作流程

### 發布新版本（每次更新）

```powershell
$gh = "C:\Program Files\GitHub CLI\gh.exe"
$ver = "v20260711"   # 改成當次版本號

# 1. 建立 Release
& $gh release create $ver --repo ChenShingMing/XRCV_VirtualRoom --title $ver --notes "更新說明" --latest

# 2. 上傳 Android APK
& $gh release upload $ver "Build\202607\Oculus\creativeXRworld_<date>.apk" --repo ChenShingMing/XRCV_VirtualRoom --clobber

# 3. 上傳 PC ZIP（排除 BurstDebugInformation 後打包）
$items = Get-ChildItem "Build\202607\PC" | Where-Object { $_.Name -notlike "*BurstDebug*" }
Compress-Archive -Path ($items.FullName) -DestinationPath "Build\202607\creativeXRworld_<date>_PC.zip" -Force
& $gh release upload $ver "Build\202607\creativeXRworld_<date>_PC.zip" --repo ChenShingMing/XRCV_VirtualRoom --clobber
```

### 更新 Firestore Manifest

```powershell
$apiKey = "AIzaSyCioQs9R6V1MT28lttoHF2HXDD80JoiNL4"
$url    = "https://firestore.googleapis.com/v1/projects/creativexrworld/databases/(default)/documents/AppConfig/version?key=$apiKey"

$body = @{
  fields = @{
    latestVersion      = @{ stringValue  = "20260711" }
    minRequiredVersion = @{ stringValue  = "20260711" }
    forceUpdate        = @{ booleanValue = $false }
    releaseNote        = @{ stringValue  = "更新說明" }
    android = @{ mapValue = @{ fields = @{
      downloadUrl = @{ stringValue  = "https://github.com/ChenShingMing/XRCV_VirtualRoom/releases/download/v20260711/creativeXRworld_20260711.apk" }
      checksum    = @{ stringValue  = "sha256:<hash>" }
      sizeBytes   = @{ integerValue = "436207616" }
    }}}
    pc = @{ mapValue = @{ fields = @{
      downloadUrl = @{ stringValue  = "https://github.com/ChenShingMing/XRCV_VirtualRoom/releases/download/v20260711/creativeXRworld_20260711_PC.zip" }
      checksum    = @{ stringValue  = "sha256:<hash>" }
      sizeBytes   = @{ integerValue = "0" }
    }}}
  }
} | ConvertTo-Json -Depth 10

Invoke-RestMethod -Uri $url -Method Patch -Body $body -ContentType "application/json"
```

---

## 八、安全考量

| 項目 | 設計 |
|------|------|
| 傳輸安全 | HTTPS only（GitHub / Firebase 預設） |
| 完整性驗證 | SHA256 checksum，驗證失敗不安裝 |
| 下載失敗處理 | 顯示錯誤 + 允許重試，**絕不卡住啟動** |
| PC bat script | 寫入 `%TEMP%`，路徑固定；替換前保留舊 EXE 備份 |
| 版本回滾 | 可在 Firestore manifest 加 `rollbackVersion` 欄位備用 |
| 逾時設定 | 連線逾時 60 秒；下載無上限（大檔案需要時間） |

---

## 九、對現有架構的影響

| 現有元件 | 影響 | 說明 |
|----------|------|------|
| `LoadingManager.cs` | 極低 | 在原有啟動事件前插入 `UpdateManager`，不改既有邏輯 |
| `FirebaseLicenseInfoManager.cs` | 無 | `VersionCheckService` 獨立實作，不修改授權系統 |
| `ClassroomManager.cs` | 無 | 只讀取 `Application.version` 做本地版本比對 |
| `AndroidManifest.xml` | 低 | 新增 `REQUEST_INSTALL_PACKAGES` 一行 |
| Photon / XR / Topic 系統 | 無 | 完全不影響 |

---

## 十、實作階段規劃

| 階段 | 內容 | 狀態 |
|------|------|------|
| **Phase 0** | GitHub Releases 建立、APK/ZIP 上傳、Firestore manifest 設定 | ✅ 完成 |
| **Phase 1** | `VersionCheckService` + `UpdateManager` 骨架 + 版本比對 | ✅ 完成 |
| **Phase 2** | `DownloadManager` + `UpdateUI` 進度條 | 待實作 |
| **Phase 3** | `ChecksumVerifier` + 錯誤處理 | 待實作 |
| **Phase 4** | `UpdateApplier_Android`（Quest） | 待實作 |
| **Phase 5** | `UpdateApplier_PC` | 待實作 |
| **Phase 6** | Loading Scene 整合 + 端對端測試 | 待實作 |
| **Layer 2** | AssetBundle 增量更新 | 另行規劃 |
