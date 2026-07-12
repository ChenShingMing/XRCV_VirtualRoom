# CLAUDE.md — creativeXRworld 專案指令

## 語言
所有回覆必須使用**繁體中文**。

---

## GitHub Release 上傳 SOP

每次請你上傳新版本時，請按照以下流程執行。

### 工具與設定

```powershell
$gh       = "C:\Program Files\GitHub CLI\gh.exe"
$repo     = "ChenShingMing/XRCV_VirtualRoom"
$apiKey   = "AIzaSyCioQs9R6V1MT28lttoHF2HXDD80JoiNL4"
$firestoreUrl = "https://firestore.googleapis.com/v1/projects/creativexrworld/databases/(default)/documents/AppConfig/version?key=$apiKey"

$ver = "v1.0"   # 改成當次版本號（語意化版號，例如 v1.0、v1.1、v1.10）
```

### Step 1：計算 SHA-256

檔名格式固定為 `creativeXRworld_{版本號}_{日期}`（版本號在前、日期在後），例如 `creativeXRworld_v1.0_20260713.apk`。
Quest APK 由 PlatformBuilder 建置時已自動用這個格式命名；PC ZIP 是手動打包，打包時請自己套用同樣格式。

```powershell
# Android APK（PlatformBuilder 產出檔名已含版本號）
$apkHash = (Get-FileHash "路徑\creativeXRworld_${ver}_yyyyMMdd.apk" -Algorithm SHA256).Hash.ToLower()
$apkSize = (Get-Item "路徑\creativeXRworld_${ver}_yyyyMMdd.apk").Length

# PC ZIP（先打包，排除 BurstDebugInformation）
$items = Get-ChildItem "PC build 目錄" | Where-Object { $_.Name -notlike "*BurstDebug*" }
Compress-Archive -Path ($items.FullName) -DestinationPath "路徑\creativeXRworld_${ver}_yyyyMMdd_PC.zip" -Force
$zipHash = (Get-FileHash "路徑\creativeXRworld_${ver}_yyyyMMdd_PC.zip" -Algorithm SHA256).Hash.ToLower()
$zipSize = (Get-Item "路徑\creativeXRworld_${ver}_yyyyMMdd_PC.zip").Length
```

### Step 2：建立 GitHub Release 並上傳

```powershell
# 建立 Release（若已存在則略過）
& $gh release create $ver --repo $repo --title $ver --notes "更新說明" --latest

# 上傳 APK
& $gh release upload $ver "路徑\creativeXRworld_${ver}_yyyyMMdd.apk" --repo $repo --clobber

# 上傳 PC ZIP
& $gh release upload $ver "路徑\creativeXRworld_${ver}_yyyyMMdd_PC.zip" --repo $repo --clobber
```

### Step 3：更新 Firestore Manifest

```powershell
$versionNumber = $ver.TrimStart("v")   # "v1.0" -> "1.0"，Firestore 裡存純數字版號

$body = @{
  fields = @{
    latestVersion      = @{ stringValue  = $versionNumber }
    minRequiredVersion = @{ stringValue  = $versionNumber }
    forceUpdate        = @{ booleanValue = $false }
    releaseNote        = @{ stringValue  = $ver }   # 畫面上只顯示版本號，不放中文說明
    android = @{ mapValue = @{ fields = @{
      downloadUrl = @{ stringValue  = "https://github.com/$repo/releases/download/$ver/creativeXRworld_${ver}_yyyyMMdd.apk" }
      checksum    = @{ stringValue  = "sha256:$apkHash" }
      sizeBytes   = @{ integerValue = "$apkSize" }
    }}}
    pc = @{ mapValue = @{ fields = @{
      downloadUrl = @{ stringValue  = "https://github.com/$repo/releases/download/$ver/creativeXRworld_${ver}_yyyyMMdd_PC.zip" }
      checksum    = @{ stringValue  = "sha256:$zipHash" }
      sizeBytes   = @{ integerValue = "$zipSize" }
    }}}
  }
} | ConvertTo-Json -Depth 10

# 務必以 UTF-8（無 BOM）位元組送出，Windows PowerShell 5.1 的 Invoke-RestMethod
# 預設用系統當地編碼組字串，中文字元會在送出前就被吃掉變成 "?"。
$utf8Bytes = [System.Text.Encoding]::UTF8.GetBytes($body)
Invoke-RestMethod -Uri $firestoreUrl -Method Patch -Body $utf8Bytes -ContentType "application/json; charset=utf-8"
```

### GitHub Release 備註格式

每次上傳，release notes 使用以下 Markdown 格式：

```markdown
## creativeXRworld v{版本號}

### 更新內容
- （使用者提供的更新說明）

### 檔案
| 平台 | 檔名 | SHA-256 |
|------|------|---------|
| Android / Quest | creativeXRworld_{版本號}_{日期}.apk | `{apkHash}` |
| PC (Windows) | creativeXRworld_{版本號}_{日期}_PC.zip | `{zipHash}` |
```

---

## 版本號規則

- 格式：語意化版號 `主版號.次版號`，例如 `1.0`、`1.1`、`1.10`（**不再使用 `yyyyMMdd` 日期格式**）
- `Application.version` = `bundleVersion` in ProjectSettings
- 版本比較邏輯在 `VersionManifest.cs`，是逐段數字比較（`1.9 < 1.10`），可以放心用兩位數以上的次版號
- `minRequiredVersion`：設為與 `latestVersion` 相同（強制更新），測試時設 `"0.1"`（一定小於任何正式版號）
- `releaseNote`：畫面上只顯示版本號本身（例如 `v1.0`），不要放中文說明（避免 PowerShell 編碼問題，見下方 Step 3 備註）

---

## 常用路徑

| 項目 | 路徑 |
|------|------|
| PC build 目錄 | `Build\202607\PC\` |
| Quest build 目錄 | `Build\202607\Quest\` |
| GitHub CLI | `C:\Program Files\GitHub CLI\gh.exe` |
| AutoUpdate 元件 | `Assets/2. XRCV_VirtualRoom/Core/AutoUpdate/` |
| LoadingScene | `Assets/2. XRCV_VirtualRoom/Core/LoadingScene/LoadingScene.unity` |

---

## 注意事項

- PC ZIP 必須排除 `BurstDebugInformation_DoNotShip` 資料夾
- `minRequiredVersion` 測試期間設 `"20260101"`，正式發版前改回當版版號
- Build 前確認 Build Settings 第一個場景是 LoadingScene
- 上傳前先確認 GitHub repo 是 **Public**（Private 的 Release asset 無法直接下載）
