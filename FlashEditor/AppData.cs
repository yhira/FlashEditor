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
            catch { /* 無視してデフォルト使用 */ }
        }

        // Memo読み込み
        if (File.Exists(MemoFile))
        {
            try
            {
                MemoContent = File.ReadAllText(MemoFile, Encoding.UTF8);
            }
            catch { /* 無視 */ }
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
        catch { }

        // Memo保存
        try
        {
            File.WriteAllText(MemoFile, MemoContent, Encoding.UTF8);
        }
        catch { }

        // 履歴保存
        History.Save(HistoryFile);
    }
}

public class AppConfig
{
    public string FontName { get; set; } = "Meiryo";
    public float FontSize { get; set; } = 18.0f;
    public bool IsTopMost { get; set; } = false;
    
    public int WindowX { get; set; } = 100;
    public int WindowY { get; set; } = 100;
    public int WindowWidth { get; set; } = 1600;
    public int WindowHeight { get; set; } = 1200;

    public void SetFont(Font font)
    {
        FontName = font.Name;
        FontSize = font.Size;
    }

    public Font GetFont()
    {
        try
        {
            return new Font(FontName, FontSize);
        }
        catch
        {
            return SystemFonts.DefaultFont;
        }
    }
}

public class HistoryManager
{
    private const int MaxHistory = 1000;
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

        // 現在の状態をRedoスタックへ
        _redoStack.Push(currentText);

        return _undoStack.Pop();
    }

    public string Redo(string currentText)
    {
        if (_redoStack.Count == 0) return currentText;

        // 現在の状態をUndoスタックへ
        _undoStack.Push(currentText);

        return _redoStack.Pop();
    }

    public void Save(string filePath)
    {
        try
        {
            using var fs = new FileStream(filePath, FileMode.Create);
            using var writer = new BinaryWriter(fs);
            
            // Undo Stack
            writer.Write(_undoStack.Count);
            // StackはPop順（新しい順）に出るので、読み込み時に逆順にする必要があるが
            // ここでは単純にリスト化して保存（新しい順）
            foreach (var item in _undoStack)
            {
                writer.Write(item);
            }

            // Redo Stack
            writer.Write(_redoStack.Count);
            foreach (var item in _redoStack)
            {
                writer.Write(item);
            }
        }
        catch { }
    }

    public void Load(string filePath)
    {
        if (!File.Exists(filePath)) return;

        try
        {
            using var fs = new FileStream(filePath, FileMode.Open);
            using var reader = new BinaryReader(fs);

            _undoStack.Clear();
            _redoStack.Clear();

            int undoCount = reader.ReadInt32();
            var undoList = new List<string>();
            for (int i = 0; i < undoCount; i++)
            {
                undoList.Add(reader.ReadString());
            }
            // 保存時は新しい順なので、Stackに入れるときは古い順（逆順）に入れる必要がある
            // List[0]が最新。StackにPushするときは、最後に取り出したい順（最新）に入れるのが普通だが
            // Stackの性質上、最後に入れたものが最初に出る(LIFO)。
            // 保存: A, B, C (Cが最新) -> Write: C, B, A
            // 読み込み: C, B, A
            // そのままPushすると: Stack Bottom [C, B, A] Top となり、Popすると A が出る（最古）。
            // これは間違い。
            // 正しくは、Cを取り出したいなら、Stack Top に C が来るべき。
            // Bottom [A, B, C] Top にするには、A, B, C の順でPushする必要がある。
            // つまり、読み込んだリスト (C, B, A) を逆順にして (A, B, C) にしてからPushする。
            for (int i = undoList.Count - 1; i >= 0; i--)
            {
                _undoStack.Push(undoList[i]);
            }

            int redoCount = reader.ReadInt32();
            var redoList = new List<string>();
            for (int i = 0; i < redoCount; i++)
            {
                redoList.Add(reader.ReadString());
            }
            for (int i = redoList.Count - 1; i >= 0; i--)
            {
                _redoStack.Push(redoList[i]);
            }
        }
        catch { }
    }
}
