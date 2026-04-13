# FlashEditor リリース（配布）ワークフロー

このドキュメントは、FlashEditorアプリケーションをユーザーに配布する際の、正式なパッケージ（ZIP）作成手順を定義します。

## 1. 配布に含めるファイル

リリース用のZIPパッケージには必ず以下のファイルを含めます。

- **`FlashEditor.exe`**
  （場所：`bin\Release\net9.0-windows\win-x64\publish\` 等にある単一実行ファイル）
- **`lang` フォルダ**
  多言語対応のJSONファイル群。
- **`Readme.txt`**
  配布用パッケージの概要や注意書き（ユーザーへの一番最初の入口）。
- **`manual.md`**
  詳細な操作マニュアル。

※ `FlashEditor.pdb` は任意です。除外しても問題ありません。

## 2. 推奨される解凍後ディレクトリ構成

展開時のディレクトリ散乱を防ぐため、ZIPファイルの直下に必ず「`FlashEditor`」という親フォルダが存在する状態にします。

```text
FlashEditor/
  ├── FlashEditor.exe
  ├── lang/
  │    ├── en.json
  │    ├── ja.json
  │    └── (その他のjsonファイル...)
  ├── Readme.txt
  └── manual.md
```

## 3. 最新版のビルドと発行（必須の事前準備と教訓）

過去の教訓として、発行作業を忘れたり設定に不備があると、単に「最新の変更が反映されない」だけでなく、**「依存ファイル（dll等）が.exeから分離されてしまい、配布先でアプリが開けなくなる」** という重大な不具合が発生します（単一ファイル発行の失敗）。

これを防ぐため、**ZIPパッケージを作成する前には必ず `Release` 構成で単一ファイル（Single-File）として発行** してください。

**実行場所:** プロジェクトルートディレクトリ

```powershell
dotnet publish FlashEditor\FlashEditor.csproj -c Release -r win-x64 -p:PublishSingleFile=true
```

> [!WARNING]
> コマンド実行時に `-p:PublishSingleFile=true` を付けるか、または必ず `.csproj` 内に `<PublishSingleFile>true</PublishSingleFile>` が設定されていることを確認してください。これを怠ると、解凍後の `.exe` は起動できません。

## 4. ZIPファイル自動作成スクリプト

リリース成果物の最新ビルドが完了したら、手動ミスの防止として以下のスクリプト（PowerShell）を利用して配布ファイルを作成します。

**実行場所:** プロジェクトルートディレクトリ

```powershell
# 1. 出力先・作業用フォルダを定義
$distDir = "dist\FlashEditor"

# 2. 作業用フォルダ作成 (langフォルダ含む)
New-Item -ItemType Directory -Path "$distDir\lang" -Force | Out-Null

# 3. リリースファイルを収集
Copy-Item -Path "FlashEditor\bin\Release\net9.0-windows\win-x64\publish\FlashEditor.exe" -Destination $distDir
Copy-Item -Path "FlashEditor\bin\Release\net9.0-windows\win-x64\publish\lang\*" -Destination "$distDir\lang" -Recurse
Copy-Item -Path "Readme.txt" -Destination $distDir
Copy-Item -Path "manual.md" -Destination $distDir

# 4. ZIPに圧縮 (日時の付与)
$timestamp = Get-Date -Format "yyyyMMddHHmm"
$zipName = "FlashEditor_${timestamp}.zip"
Compress-Archive -Path $distDir -DestinationPath "dist\$zipName" -Force

# 5. 作業用フォルダの後片付け
Remove-Item -Path $distDir -Recurse -Force

Write-Host "リリースパッケージが dist\ フォルダに作成されました！"
Get-ChildItem -Path dist
```

## 5. 注意事項

- 生成された `.zip` ファイルおよび `dist/` ディレクトリは `.gitignore` の対象としてあるため、誤ってGitリポジトリにコミットされることはありません。
