using System.Text.Json;
using System.Text;

namespace FlashEditor;

public class AppData
{
    private const string ConfigFile = "config.json";
    private const string MemoFile = "memo.txt";
    private const string HistoryFile = "history.dat";

    // 設定プロパティ
    public AppConfig Config { get; private set; } = new AppConfig();
    
    // 履歴管理
    public HistoryManager History { get; private set; } = new HistoryManager();

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

        // 履歴読み込み
        History.Load(HistoryFile);
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

        // 履歴保存
        History.Save(HistoryFile);
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

public class HistoryManager
{
    private const int MaxHistory = 200;
    // 最低変化文字数 (この差分未満の変更は記録しない)
    private const int MinChangeThreshold = 10;
    private readonly Stack<string> _undoStack = new();
    private readonly Stack<string> _redoStack = new();

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public void Push(string text)
    {
        // 直前の状態と同じなら追加しない
        if (_undoStack.Count > 0 && _undoStack.Peek() == text) return;

        // 変化量が少なすぎる場合はスキップ (大きな変更のみ記録)
        if (_undoStack.Count > 0)
        {
            var lastText = _undoStack.Peek();
            int diff = Math.Abs(text.Length - lastText.Length);
            if (diff < MinChangeThreshold && diff > 0)
            {
                // 改行の追加や貼り付けなど「構造的な変化」は許可
                int lastLines = lastText.Split('\n').Length;
                int currentLines = text.Split('\n').Length;
                if (Math.Abs(currentLines - lastLines) < 2) return;
            }
        }

        _undoStack.Push(text);
        _redoStack.Clear();

        // 制限を超えたら古いものを捨てる
        if (_undoStack.Count > MaxHistory)
        {
            var list = _undoStack.ToList();
            list.RemoveAt(list.Count - 1);
            
            _undoStack.Clear();
            for (int i = list.Count - 1; i >= 0; i--)
            {
                _undoStack.Push(list[i]);
            }
        }
    }

    public string Undo(string currentText)
    {
        if (_undoStack.Count == 0) return currentText;

        // スタックトップが現在のテキストと同じ場合はスキップ（「自分自身に戻る」バグの防止）
        if (_undoStack.Peek() == currentText)
        {
            _undoStack.Pop(); // 同一エントリを捨てる
            if (_undoStack.Count == 0) return currentText; // もう戻れない
        }

        // 現在の状態をRedoスタックへ
        _redoStack.Push(currentText);

        return _undoStack.Pop();
    }

    public string Redo(string currentText)
    {
        if (_redoStack.Count == 0) return currentText;

        // スタックトップが現在のテキストと同じ場合はスキップ（一貫性のため）
        if (_redoStack.Peek() == currentText)
        {
            _redoStack.Pop(); // 同一エントリを捨てる
            if (_redoStack.Count == 0) return currentText; // もうやり直せない
        }

        // 現在の状態をUndoスタックへ
        _undoStack.Push(currentText);

        return _redoStack.Pop();
    }

    // JSON化用の中間DTO
    private class HistoryData
    {
        public List<string> UndoList { get; set; } = new();
        public List<string> RedoList { get; set; } = new();
    }

    public void Save(string filePath)
    {
        try
        {
            var data = new HistoryData
            {
                // Stack を List 化すると Top から順に（新しい順に）入る
                UndoList = _undoStack.ToList(),
                RedoList = _redoStack.ToList()
            };
            var json = JsonSerializer.Serialize(data);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex) { AppData.ReportError("履歴データの保存に失敗しました", ex); }
    }

    public void Load(string filePath)
    {
        if (!File.Exists(filePath)) return;

        try
        {
            var json = File.ReadAllText(filePath);
            var data = JsonSerializer.Deserialize<HistoryData>(json);

            _undoStack.Clear();
            _redoStack.Clear();

            if (data != null)
            {
                // リストは新しい順（Stack の Pop 順）で保存されているため、
                // Stack の下から上に積み上げるには逆順に Push する必要がある。
                data.UndoList.Reverse();
                foreach (var item in data.UndoList)
                {
                    _undoStack.Push(item);
                }

                data.RedoList.Reverse();
                foreach (var item in data.RedoList)
                {
                    _redoStack.Push(item);
                }
            }
        }
        catch (JsonException)
        {
            // 古いバイナリ形式の履歴ファイルや破損したJSONの場合はエラーを出さずに破棄
            _undoStack.Clear();
            _redoStack.Clear();
        }
        catch (Exception ex)
        {
            AppData.ReportError("履歴データの読み込みに失敗しました", ex);
        }
    }
}
