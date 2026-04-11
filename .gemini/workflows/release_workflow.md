# FlashEditor リリース（配布）ワークフロー

このドキュメントは、FlashEditorアプリケーションをユーザーに配布する際の、正式なパッケージ（ZIP）作成手順を定義します。

## 1. 配布に含めるファイル

リリース用のZIPパッケージには必ず以下のファイルを含めます。

* **`FlashEditor.exe`**
  （場所：`bin\Release\net9.0-windows\win-x64\publish\` 等にある単一実行ファイル）
* **`lang` フォルダ**
  多言語対応のJSONファイル群。
* **`README.md`**
  プロジェクト直下にある利用方法。
* **`LICENSE`**
  プロジェクトのライセンス。

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
  ├── README.md
  └── LICENSE
```

## 3. ZIPファイル自動作成スクリプト

リリースビルドが完了したら、手動ミスの防止として以下のスクリプト（PowerShell）を利用して配布ファイルを作成します。

**実行場所:** プロジェクトルートディレクトリ

```powershell
# 1. 出力先・作業用フォルダを定義
$distDir = "dist\FlashEditor"

# 2. 作業用フォルダ作成 (langフォルダ含む)
New-Item -ItemType Directory -Path "$distDir\lang" -Force | Out-Null

# 3. リリースファイルを収集
Copy-Item -Path "FlashEditor\bin\Release\net9.0-windows\win-x64\publish\FlashEditor.exe" -Destination $distDir
Copy-Item -Path "FlashEditor\bin\Release\net9.0-windows\win-x64\publish\lang\*" -Destination "$distDir\lang" -Recurse
Copy-Item -Path "README.md" -Destination $distDir
Copy-Item -Path "LICENSE" -Destination $distDir

# 4. ZIPに圧縮
Compress-Archive -Path $distDir -DestinationPath "dist\FlashEditor_Release.zip" -Force

# 5. 作業用フォルダの後片付け
Remove-Item -Path $distDir -Recurse -Force

Write-Host "リリースパッケージが dist\ フォルダに作成されました！"
Get-ChildItem -Path dist
```

## 4. 注意事項

* 生成された `.zip` ファイルおよび `dist/` ディレクトリは `.gitignore` の対象としてあるため、誤ってGitリポジトリにコミットされることはありません。
