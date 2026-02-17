using FluentAssertions;
using Xunit;

namespace FlashEditor.Tests;

public class AppDataTests : IDisposable
{
    // テスト用の一時ディレクトリ
    private readonly string _tempDir;
    // テスト前のカレントディレクトリを保存
    private readonly string _originalDir;

    public AppDataTests()
    {
        // 一時ディレクトリを作成してカレントディレクトリを変更
        _tempDir = Path.Combine(Path.GetTempPath(), $"FlashEditorTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _originalDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(_tempDir);
    }

    // テスト後にカレントディレクトリを元に戻し、一時ディレクトリを削除
    public void Dispose()
    {
        Directory.SetCurrentDirectory(_originalDir);
        try
        {
            Directory.Delete(_tempDir, true);
        }
        catch { /* テスト環境のクリーンアップ失敗は無視 */ }
    }

    // ===== 基本動作テスト =====

    [Fact]
    public void 新規AppDataのデフォルト値が正しい()
    {
        var appData = new AppData();

        appData.MemoContent.Should().BeEmpty();
        appData.Config.Should().NotBeNull();
        appData.History.Should().NotBeNull();
    }

    [Fact]
    public void ファイルが存在しない状態でLoadしてもエラーにならない()
    {
        var appData = new AppData();

        // 例外が発生しないことを確認
        var act = () => appData.Load();
        act.Should().NotThrow();

        // デフォルト値のまま
        appData.MemoContent.Should().BeEmpty();
        appData.Config.FontName.Should().Be("Meiryo");
    }

    // ===== Save/Load テスト =====

    [Fact]
    public void SaveしたメモをLoadで復元できる()
    {
        var appData = new AppData();
        appData.MemoContent = "テストメモの内容です。\n改行も含みます。";

        appData.Save();

        // 別のインスタンスで読み込み
        var loaded = new AppData();
        loaded.Load();

        loaded.MemoContent.Should().Be("テストメモの内容です。\n改行も含みます。");
    }

    [Fact]
    public void Saveした設定をLoadで復元できる()
    {
        var appData = new AppData();
        appData.Config.FontName = "Consolas";
        appData.Config.FontSize = 20.0f;
        appData.Config.IsTopMost = true;
        appData.Config.WindowX = 500;
        appData.Config.WindowY = 300;

        appData.Save();

        // 別のインスタンスで読み込み
        var loaded = new AppData();
        loaded.Load();

        loaded.Config.FontName.Should().Be("Consolas");
        loaded.Config.FontSize.Should().Be(20.0f);
        loaded.Config.IsTopMost.Should().BeTrue();
        loaded.Config.WindowX.Should().Be(500);
        loaded.Config.WindowY.Should().Be(300);
    }

    [Fact]
    public void 空のメモを保存して復元できる()
    {
        var appData = new AppData();
        appData.MemoContent = "";

        appData.Save();

        var loaded = new AppData();
        loaded.Load();

        loaded.MemoContent.Should().BeEmpty();
    }

    [Fact]
    public void 日本語を含むメモが文字化けせずに保存復元できる()
    {
        var appData = new AppData();
        // 様々な日本語文字種を含むテキスト
        appData.MemoContent = "漢字・ひらがな・カタカナ・ＡＢＣ・①②③・🎉🎊";

        appData.Save();

        var loaded = new AppData();
        loaded.Load();

        loaded.MemoContent.Should().Be("漢字・ひらがな・カタカナ・ＡＢＣ・①②③・🎉🎊");
    }

    // ===== ファイル生成確認テスト =====

    [Fact]
    public void Saveすると設定ファイルとメモファイルが生成される()
    {
        var appData = new AppData();
        appData.MemoContent = "ファイル確認テスト";

        appData.Save();

        // ファイルが存在することを確認
        File.Exists("config.json").Should().BeTrue();
        File.Exists("memo.txt").Should().BeTrue();
        File.Exists("history.dat").Should().BeTrue();
    }
}
