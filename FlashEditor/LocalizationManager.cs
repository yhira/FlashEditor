using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace FlashEditor;

public static class LocalizationManager
{
    private static Dictionary<string, string> _strings = new Dictionary<string, string>();
    public static string CurrentLanguage { get; private set; } = "ja";

    // 読み込み済みの言語一覧。キー: ファイル名(拡張子なし), 値: LangName(日本語等)
    // 設定画面のコンボボックスで選択できるようにするため保持します。
    public static List<LanguageInfo> AvailableLanguages { get; private set; } = new List<LanguageInfo>();

    public class LanguageInfo
    {
        public string Code { get; set; } = "";
        public string DisplayName { get; set; } = "";

        public override string ToString() => DisplayName;
    }

    /// <summary>
    /// 利用可能な言語ファイルのリストを読み込みます
    /// </summary>
    public static void Initialize()
    {
        AvailableLanguages.Clear();
        string langDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lang");
        
        if (Directory.Exists(langDir))
        {
            var files = Directory.GetFiles(langDir, "*.json");
            foreach (var file in files)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    
                    if (dict != null && dict.TryGetValue("LangName", out var langName))
                    {
                        var code = Path.GetFileNameWithoutExtension(file);
                        AvailableLanguages.Add(new LanguageInfo { Code = code, DisplayName = langName });
                    }
                }
                catch (Exception ex)
                {
                    AppData.ReportError("言語ファイルの読み込みに失敗しました", ex);
                }
            }
            
            // 優先度等で並び替える場合はここでソート
            AvailableLanguages = AvailableLanguages.OrderBy(l => l.Code == "ja" ? 0 : l.Code == "en" ? 1 : 2).ThenBy(l => l.DisplayName).ToList();
        }
    }

    /// <summary>
    /// 対象の言語を読み込みます。
    /// </summary>
    public static void LoadLanguage(string langCode)
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lang", $"{langCode}.json");
        
        // 指定された言語が存在しない場合は日本語(ja)か英語(en)にフォールバック
        if (!File.Exists(filePath))
        {
            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lang", "ja.json");
            langCode = "ja";
            if (!File.Exists(filePath))
            {
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lang", "en.json");
                langCode = "en";
            }
        }

        try
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                _strings = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                CurrentLanguage = langCode;
            }
            else
            {
                _strings.Clear();
            }
        }
        catch (Exception ex)
        {
            _strings.Clear();
            AppData.ReportError("言語データの読み込みに失敗しました", ex);
        }
    }

    /// <summary>
    /// キーからローカライズされた文字列を取得します。
    /// 見つからない場合はキー自身を返します（デバッグのため）。
    /// </summary>
    public static string GetString(string key)
    {
        if (_strings.TryGetValue(key, out var value))
        {
            return value;
        }
        return $"[{key}]";
    }
}
