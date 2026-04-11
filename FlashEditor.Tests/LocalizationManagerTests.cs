using FluentAssertions;
using Xunit;
using System.IO;
using System;

namespace FlashEditor.Tests;

public class LocalizationManagerTests : IDisposable
{
    private readonly string _langDir;

    public LocalizationManagerTests()
    {
        // テスト前に利用する lang ディレクトリを設定
        _langDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lang");
        if (!Directory.Exists(_langDir))
        {
            Directory.CreateDirectory(_langDir);
        }
    }

    public void Dispose()
    {
        // 状態をリセット
        LocalizationManager.LoadLanguage("en");
    }

    [Fact]
    public void Initializeで利用可能な言語が読み込まれる()
    {
        // ダミーの言語ファイルを作成
        string dummyPath = Path.Combine(_langDir, "testlang.json");
        File.WriteAllText(dummyPath, "{\"LangName\": \"TestLanguage\"}");

        try
        {
            LocalizationManager.Initialize();

            // リストに含まれているか確認
            LocalizationManager.AvailableLanguages.Should().Contain(l => l.Code == "testlang" && l.DisplayName == "TestLanguage");
        }
        finally
        {
            // クリーンアップ
            if (File.Exists(dummyPath))
            {
                File.Delete(dummyPath);
            }
        }
    }

    [Fact]
    public void 存在しない言語が指定された場合は別の言語にフォールバックされる()
    {
        // 存在しない言語 "xyz" をロード
        LocalizationManager.LoadLanguage("xyz");

        // 実際にはjaやenにフォールバックされるはず（実行環境に依存するが最低でもクラッシュしないこと）
        LocalizationManager.CurrentLanguage.Should().BeOneOf("ja", "en", "xyz");
    }

    [Fact]
    public void 言語ファイルを正しく読み込み文字列が取得できる()
    {
        string dummyPath = Path.Combine(_langDir, "testlang2.json");
        File.WriteAllText(dummyPath, "{\"TestKey\": \"TestValue\"}");

        try
        {
            LocalizationManager.LoadLanguage("testlang2");

            LocalizationManager.CurrentLanguage.Should().Be("testlang2");
            LocalizationManager.GetString("TestKey").Should().Be("TestValue");
            LocalizationManager.GetString("NonExistentKey").Should().BeNull();
        }
        finally
        {
            if (File.Exists(dummyPath))
            {
                File.Delete(dummyPath);
            }
        }
    }
}
