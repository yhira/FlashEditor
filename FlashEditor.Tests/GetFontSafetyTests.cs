using FluentAssertions;
using Xunit;

namespace FlashEditor.Tests;

/// <summary>
/// AppConfig.GetFont() の安全性テスト
/// SystemFonts.DefaultFont（共有リソース）を直接返さないことを検証する回帰テスト
/// </summary>
public class GetFontSafetyTests
{
    // GetFontが毎回新しい独立インスタンスを返すことを検証
    [Fact]
    public void GetFontは毎回異なるインスタンスを返す()
    {
        var config = new AppConfig();
        config.FontName = "Arial";
        config.FontSize = 12.0f;

        using var font1 = config.GetFont();
        using var font2 = config.GetFont();

        // 同じ設定値でも別インスタンスであることを確認
        font1.Should().NotBeSameAs(font2);
    }

    // 不正なフォント名でもSystemFonts.DefaultFontと同一参照を返さないことを確認
    [Fact]
    public void 不正フォント名でもSystemFontsの共有参照を返さない()
    {
        var config = new AppConfig();
        config.FontName = "存在しないフォント名!!!@@@###";

        using var font = config.GetFont();

        // SystemFonts.DefaultFont と同一参照でないことを確認
        // （同一参照を返すと、呼び出し側がDisposeした時にシステム全体がクラッシュする）
        font.Should().NotBeSameAs(SystemFonts.DefaultFont);
    }

    // GetFontが返したフォントをDisposeしても次回のGetFontに影響しないことを確認
    [Fact]
    public void GetFont結果をDisposeしても次のGetFontに影響しない()
    {
        var config = new AppConfig();
        config.FontName = "Arial";
        config.FontSize = 14.0f;

        // 1回目のフォントを取得して即座にDispose
        var font1 = config.GetFont();
        font1.Dispose();

        // 2回目の取得が例外なく成功することを確認
        var act = () =>
        {
            using var font2 = config.GetFont();
            font2.Name.Should().Be("Arial");
        };
        act.Should().NotThrow();
    }

    // フォールバック時（不正フォント名）でもDisposeしても安全であることを確認
    [Fact]
    public void フォールバックフォントをDisposeしてもシステムフォントが破壊されない()
    {
        var config = new AppConfig();
        config.FontName = "完全に無効なフォント名";

        // フォールバックフォントを取得してDispose
        var fallbackFont = config.GetFont();
        fallbackFont.Dispose();

        // SystemFonts.DefaultFont が依然として正常にアクセスできることを確認
        var act = () =>
        {
            var defaultFont = SystemFonts.DefaultFont;
            defaultFont.Name.Should().NotBeNullOrEmpty();
            defaultFont.Size.Should().BeGreaterThan(0);
        };
        act.Should().NotThrow();
    }

    // SetFont → GetFont のラウンドトリップでスタイル情報も保持されることを確認
    [Fact]
    public void SetFontでBoldItalicスタイルが正しく保存復元される()
    {
        var config = new AppConfig();
        using var boldItalic = new Font("Arial", 16.0f, FontStyle.Bold | FontStyle.Italic);

        config.SetFont(boldItalic);
        using var restored = config.GetFont();

        restored.Style.Should().HaveFlag(FontStyle.Bold);
        restored.Style.Should().HaveFlag(FontStyle.Italic);
    }
}
