using FluentAssertions;
using Xunit;

namespace FlashEditor.Tests;

/// <summary>
/// AppData.SaveMemoAsync の非同期保存テスト
/// FileShare.ReadWrite 変更後のファイル競合耐性を検証する
/// </summary>
// カレントディレクトリを変更するテスト同士の並列実行を防止
[Collection("FileSystemTests")]
public class SaveMemoAsyncTests : IDisposable
{
    // テスト用の一時ディレクトリ
    private readonly string _tempDir;
    // テスト前のカレントディレクトリを保存
    private readonly string _originalDir;

    public SaveMemoAsyncTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"FlashEditorTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _originalDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(_tempDir);
    }

    public void Dispose()
    {
        Directory.SetCurrentDirectory(_originalDir);
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    // 非同期保存が正常に動作することを確認
    [Fact]
    public async Task SaveMemoAsyncでメモが正しく保存される()
    {
        var appData = new AppData();
        string testText = "非同期保存テスト\n改行付きテキスト";

        await appData.SaveMemoAsync(testText);

        // ファイルが生成され、内容が一致することを確認
        File.Exists("memo.txt").Should().BeTrue();
        var content = File.ReadAllText("memo.txt", System.Text.Encoding.UTF8);
        content.Should().Be(testText);
    }

    // 非同期保存後にMemoContentプロパティも更新されていることを確認
    [Fact]
    public async Task SaveMemoAsyncでMemoContentプロパティも更新される()
    {
        var appData = new AppData();
        string testText = "プロパティ更新テスト";

        await appData.SaveMemoAsync(testText);

        appData.MemoContent.Should().Be(testText);
    }

    // FileShare.ReadWriteにより、非同期保存中に同期保存が競合してもクラッシュしないことを確認
    [Fact]
    public async Task 非同期保存と同期保存が競合してもクラッシュしない()
    {
        var appData = new AppData();
        appData.MemoContent = "同期保存テスト";

        // 非同期保存を開始
        var asyncTask = appData.SaveMemoAsync("非同期保存テスト");

        // 同期保存を即座に実行（競合シナリオ）
        var act = () => appData.Save();
        act.Should().NotThrow();

        // 非同期保存の完了を待機
        await asyncTask;

        // いずれかのテキストでファイルが保存されていること（順序は不定）
        var content = File.ReadAllText("memo.txt", System.Text.Encoding.UTF8);
        content.Should().NotBeNullOrEmpty();
    }

    // 空文字列の保存が正常に動作することを確認
    [Fact]
    public async Task 空文字列の非同期保存が正常に動作する()
    {
        var appData = new AppData();

        await appData.SaveMemoAsync("");

        File.Exists("memo.txt").Should().BeTrue();
        var content = File.ReadAllText("memo.txt", System.Text.Encoding.UTF8);
        content.Should().BeEmpty();
    }

    // 大きなテキストの保存が正常に動作することを確認
    [Fact]
    public async Task 大きなテキストの非同期保存が正常に動作する()
    {
        var appData = new AppData();
        // 約100KBのテキストを生成
        string largeText = new string('あ', 50000);

        await appData.SaveMemoAsync(largeText);

        var content = File.ReadAllText("memo.txt", System.Text.Encoding.UTF8);
        content.Should().Be(largeText);
    }
}
