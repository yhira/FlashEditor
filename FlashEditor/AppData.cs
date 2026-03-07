using System.Text.Json;
using System.Text;

namespace FlashEditor;

public class AppData
{
    private const string ConfigFile = "config.json";
    private const string MemoFile = "memo.txt";

    // 設定プロパティ
    public AppConfig Config { get; private set; } = new AppConfig();

    public string MemoContent { get; set; } = "";

    // エラーをログファイルに記録し、ダイアログで表示する
    internal static void ReportError(string context, Exception ex)
    {
        // ログファイルに記録
        try
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
            string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {context}: {ex.Message}\n";
            File.AppendAllText(logPath, entry);
        }
        catch { /* ログ書き込み自体の失敗は無視 */ }

        // ユーザーにダイアログで通知
        MessageBox.Show(
            $"{context}\n\n{ex.Message}",
            "FlashEditor - エラー",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    public void Load()
    {
        // Config読み込み
        if (File.Exists(ConfigFile))
        {
            try
            {
                var json = File.ReadAllText(ConfigFile);
                Config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
            catch (Exception ex) { ReportError("設定ファイルの読み込みに失敗しました", ex); }
        }

        // Memo読み込み
        if (File.Exists(MemoFile))
        {
            try
            {
                MemoContent = File.ReadAllText(MemoFile, Encoding.UTF8);
            }
            catch (Exception ex) { ReportError("メモファイルの読み込みに失敗しました", ex); }
        }
    }

    public void Save()
    {
        // Config保存
        try
        {
            var json = JsonSerializer.Serialize(Config);
            File.WriteAllText(ConfigFile, json);
        }
        catch (Exception ex) { ReportError("設定ファイルの保存に失敗しました", ex); }

        // Memo保存
        try
        {
            File.WriteAllText(MemoFile, MemoContent, Encoding.UTF8);
        }
        catch (Exception ex) { ReportError("メモファイルの保存に失敗しました", ex); }
    }

    // 非同期でメモのみを保存する（クラッシュ耐性向上のためのバックアップ用）
    public async void SaveMemoAsync(string text)
    {
        MemoContent = text;
        try
        {
            // 他の保存処理との競合を避けるため FileShare.ReadWrite などを検討するか、単純に再試行
            using var sourceStream = new FileStream(MemoFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
            var bytes = Encoding.UTF8.GetBytes(text);
            await sourceStream.WriteAsync(bytes, 0, bytes.Length);
        }
        catch (Exception ex)
        {
            // バックグラウンドでのバックアップ失敗は目障りになるためダイアログではなくログにのみ記録
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
                string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] メモの自動バックアップに失敗しました: {ex.Message}\n";
                File.AppendAllText(logPath, entry);
            }
            catch { }
        }
    }
}

public enum ThemeSetting { System, Light, Dark }

// ツールボタンサイズの設定値 (Small=32, Medium=40, Large=48)
public enum ToolButtonSizeSetting { Small, Medium, Large }

public class AppConfig
{
    // テーマ設定 (System = OS設定に従う, Light = ライト, Dark = ダーク)
    public ThemeSetting Theme { get; set; } = ThemeSetting.System;
    // ツールボタンサイズ設定
    public ToolButtonSizeSetting ToolButtonSize { get; set; } = ToolButtonSizeSetting.Medium;
    
    // 使用する言語 (デフォルトはシステムの言語から判定)
    public string Language { get; set; } = GetSystemLanguageCode();
    
    // 言語設定に応じたデフォルトフォントを自動選択
    public string FontName { get; set; } = GetDefaultFontName();

    // 言語コードに応じた最適なデフォルトフォント名を返す
    private static string GetDefaultFontName()
    {
        var lang = GetSystemLanguageCode();
        return lang switch
        {
            "ja"    => "Yu Gothic UI",
            "zh-CN" => "Microsoft YaHei UI",
            "zh-TW" => "Microsoft JhengHei UI",
            "ko"    => "Malgun Gothic",
            _       => "Segoe UI",
        };
    }

    // システムの言語(CurrentUICulture)から適切な言語コードを返す
    private static string GetSystemLanguageCode()
    {
        var culture = System.Globalization.CultureInfo.CurrentUICulture.Name;
        if (culture.StartsWith("ja")) return "ja";
        if (culture.StartsWith("zh-CN") || culture.StartsWith("zh-Hans")) return "zh-CN";
        if (culture.StartsWith("zh-TW") || culture.StartsWith("zh-Hant")) return "zh-TW";
        if (culture.StartsWith("ko")) return "ko";
        if (culture.StartsWith("es")) return "es";
        if (culture.StartsWith("pt")) return "pt";
        if (culture.StartsWith("it")) return "it";
        if (culture.StartsWith("de")) return "de";
        if (culture.StartsWith("fr")) return "fr";
        return "en"; // 未知の言語は英語にフォールバック
    }
    public float FontSize { get; set; } = 18.0f;
    // フォントスタイル (Bold/Italic 等を保存)
    public int FontStyleValue { get; set; } = (int)FontStyle.Regular;
    public bool IsTopMost { get; set; } = false;
    
    public int WindowX { get; set; } = 100;
    public int WindowY { get; set; } = 100;
    public int WindowWidth { get; set; } = 1600;
    public int WindowHeight { get; set; } = 1200;

    // ToolButtonSizeSetting からピクセルサイズを返すヘルパー
    public int GetToolButtonPixelSize()
    {
        return ToolButtonSize switch
        {
            ToolButtonSizeSetting.Small  => 32,
            ToolButtonSizeSetting.Large  => 48,
            _                            => 40, // Medium (デフォルト)
        };
    }

    public void SetFont(Font font)
    {
        FontName = font.Name;
        FontSize = font.Size;
        FontStyleValue = (int)font.Style;
    }

    public Font GetFont()
    {
        try
        {
            // 保存されたフォント名・サイズ・スタイルからフォントオブジェクトを作成
            return new Font(FontName, FontSize, (FontStyle)FontStyleValue);
        }
        catch
        {
            // 失敗した場合はシステムのデフォルトフォントを返す
            return SystemFonts.DefaultFont;
        }
    }
}
