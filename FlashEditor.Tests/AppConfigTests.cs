using FluentAssertions;
using Xunit;

namespace FlashEditor.Tests;

public class AppConfigTests
{
    // ===== デフォルト値テスト =====

    [Fact]
    public void デフォルトのフォント名はMeiryo()
    {
        var config = new AppConfig();

        config.FontName.Should().Be("Meiryo");
    }

    [Fact]
    public void デフォルトのフォントサイズは18pt()
    {
        var config = new AppConfig();

        config.FontSize.Should().Be(18.0f);
    }

    [Fact]
    public void デフォルトではTopMostは無効()
    {
        var config = new AppConfig();

        config.IsTopMost.Should().BeFalse();
    }

    [Fact]
    public void デフォルトのウィンドウ位置は100x100()
    {
        var config = new AppConfig();

        config.WindowX.Should().Be(100);
        config.WindowY.Should().Be(100);
    }

    [Fact]
    public void デフォルトのウィンドウサイズは1600x1200()
    {
        var config = new AppConfig();

        config.WindowWidth.Should().Be(1600);
        config.WindowHeight.Should().Be(1200);
    }

    // ===== SetFont / GetFont テスト =====

    [Fact]
    public void SetFontでフォント名とサイズが正しく保存される()
    {
        var config = new AppConfig();
        // テスト用のフォントを作成
        using var font = new Font("Arial", 24.0f);

        config.SetFont(font);

        config.FontName.Should().Be("Arial");
        config.FontSize.Should().Be(24.0f);
    }

    [Fact]
    public void GetFontで設定済みのフォントが取得できる()
    {
        var config = new AppConfig();
        config.FontName = "Arial";
        config.FontSize = 12.0f;

        // GetFontで取得したフォントの名前とサイズを確認
        using var font = config.GetFont();

        font.Name.Should().Be("Arial");
        font.Size.Should().Be(12.0f);
    }

    [Fact]
    public void SetFontしたフォントをGetFontで往復取得できる()
    {
        var config = new AppConfig();
        using var originalFont = new Font("Consolas", 14.0f);

        config.SetFont(originalFont);
        using var retrievedFont = config.GetFont();

        retrievedFont.Name.Should().Be(originalFont.Name);
        retrievedFont.Size.Should().Be(originalFont.Size);
    }

    [Fact]
    public void 不正なフォント名ではデフォルトフォントにフォールバックする()
    {
        var config = new AppConfig();
        // 存在しないフォント名を設定
        config.FontName = "存在しないフォント名!!!@@@###";

        // 例外にならずにフォントが返される（デフォルトフォントかフォールバック）
        using var font = config.GetFont();
        font.Should().NotBeNull();
    }

    // ===== プロパティ設定テスト =====

    [Fact]
    public void 各プロパティを変更して正しく保持される()
    {
        var config = new AppConfig();

        config.IsTopMost = true;
        config.WindowX = 200;
        config.WindowY = 300;
        config.WindowWidth = 800;
        config.WindowHeight = 600;

        config.IsTopMost.Should().BeTrue();
        config.WindowX.Should().Be(200);
        config.WindowY.Should().Be(300);
        config.WindowWidth.Should().Be(800);
        config.WindowHeight.Should().Be(600);
    }
}
