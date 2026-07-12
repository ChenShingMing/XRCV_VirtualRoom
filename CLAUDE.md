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
```

### Step 1：計算 SHA-256

```powershell
# Android APK
$apkHash = (Get-FileHash "路徑\creativeXRworld_yyyyMMdd.apk" -Algorithm SHA256).Hash.ToLower()
$apkSize = (Get-Item "路徑\creativeXRworld_yyyyMMdd.apk").Length

# PC ZIP（先打包，排除 BurstDebugInformation）
$items = Get-ChildItem "PC build 目錄" | Where-Object { $_.Name -notlike "*BurstDebug*" }
Compress-Archive -Path ($items.FullName) -DestinationPath "路徑\creativeXRworld_yyyyMMdd_PC.zip" -Force
$zipHash = (Get-FileHash "路徑\creativeXRworld_yyyyMMdd_PC.zip" -Algorithm SHA256).Hash.ToLower()
$zipSize = (Get-Item "路徑\creativeXRworld_yyyyMMdd_PC.zip").Length
```

### Step 2：建立 GitHub Release 並上傳

```powershell
$ver = "v20260711"   # 改成當次版本號

# 建立 Release（若已存在則略過）
& $gh release create $ver --repo $repo --title $ver --notes "更新說明" --latest

# 上傳 APK
& $gh release upload $ver "路徑\creativeXRworld_yyyyMMdd.apk" --repo $repo --clobber

# 上傳 PC ZIP
& $gh release upload $ver "路徑\creativeXRworld_yyyyMMdd_PC.zip" --repo $repo --clobber
```

### Step 3：更新 Firestore Manifest

```powershell
$body = @{
  fields = @{
    latestVersion      = @{ stringValue  = "20260711" }
    minRequiredVersion = @{ stringValue  = "20260711" }
    forceUpdate        = @{ booleanValue = $false }
    releaseNote        = @{ stringValue  = "更新說明" }
    android = @{ mapValue = @{ fields = @{
      downloadUrl = @{ stringValue  = "https://github.com/$repo/releases/download/v20260711/creativeXRworld_20260711.apk" }
      checksum    = @{ stringValue  = "sha256:$apkHash" }
      sizeBytes   = @{ integerValue = "$apkSize" }
    }}}
    pc = @{ mapValue = @{ fields = @{
      downloadUrl = @{ stringValue  = "https://github.com/$repo/releases/download/v20260711/creativeXRworld_20260711_PC.zip" }
      checksum    = @{ stringValue  = "sha256:$zipHash" }
      sizeBytes   = @{ integerValue = "$zipSize" }
    }}}
  }
} | ConvertTo-Json -Depth 10

Invoke-RestMethod -Uri $firestoreUrl -Method Patch -Body $body -ContentType "application/json"
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
| Android / Quest | creativeXRworld_{日期}.apk | `{apkHash}` |
| PC (Windows) | creativeXRworld_{日期}_PC.zip | `{zipHash}` |
```

---

## 版本號規則

- 格式：`yyyyMMdd`（同日多版加 `-N`，例如 `20260711-2`）
- `Application.version` = `bundleVersion` in ProjectSettings
- `minRequiredVersion`：設為與 `latestVersion` 相同（強制更新），測試時設 `"20260101"`

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
