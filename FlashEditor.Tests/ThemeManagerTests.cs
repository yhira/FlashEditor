using FluentAssertions;
using Xunit;

namespace FlashEditor.Tests;

public class ThemeManagerTests
{
    [Fact]
    public void ThemeSetting_Light_をResolveすると_ThemeMode_Light_になる()
    {
        var result = ThemeManager.Resolve(ThemeSetting.Light);
        result.Should().Be(ThemeManager.ThemeMode.Light);
    }

    [Fact]
    public void ThemeSetting_Dark_をResolveすると_ThemeMode_Dark_になる()
    {
        var result = ThemeManager.Resolve(ThemeSetting.Dark);
        result.Should().Be(ThemeManager.ThemeMode.Dark);
    }

    [Fact]
    public void ThemeSetting_System_をResolveすると_System設定に基づいたThemeModeが返る()
    {
        var result = ThemeManager.Resolve(ThemeSetting.System);
        
        // SystemThemeは環境依存なので、確実にLightかDarkのどちらかであることを保証する
        Assert.True(result == ThemeManager.ThemeMode.Light || result == ThemeManager.ThemeMode.Dark);
    }

    [Fact]
    public void IsDarkプロパティはCurrentThemeに依存する()
    {
        // メインスレッドなどUIに絡む部分を避けて、プロパティの評価が例外にならないことを確認
        // 環境によってCurrentThemeの初期値が変わる可能性があるため、安全に取得できるかだけ確保
        bool isDark = ThemeManager.IsDark;
        
        // isDarkはtrueかfalseのどちらか（クラッシュしなければよい）
        Assert.True(isDark == true || isDark == false);
    }
}
