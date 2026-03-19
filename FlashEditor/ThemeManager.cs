using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;

namespace FlashEditor;

public static class ThemeManager
{
    // Windows 10/11 Dark Mode Title Bar support
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string? pszSubIdList);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    public enum ThemeMode { Light, Dark }

    public static ThemeMode CurrentTheme { get; private set; } = ThemeMode.Light;

    // テーマ判定のショートカットプロパティ
    public static bool IsDark => CurrentTheme == ThemeMode.Dark;

    // === ダークテーマの共通色定数 ===
    public static readonly Color DarkBackground = Color.FromArgb(30, 30, 30);
    public static readonly Color DarkToolStripBackground = Color.FromArgb(45, 45, 48);
    public static readonly Color DarkControlBackground = Color.FromArgb(50, 50, 50);
    public static readonly Color DarkText = Color.WhiteSmoke;
    public static readonly Color DarkBorder = Color.FromArgb(63, 63, 70);

    // DWM Dark Mode 属性を設定する共通ヘルパー
    private static void SetDwmDarkMode(IntPtr handle, bool darkMode)
    {
        int value = darkMode ? 1 : 0;
        DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, sizeof(int));
        DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref value, sizeof(int));
    }

    public static void ApplyTheme(Form form, ThemeMode mode)
    {
        CurrentTheme = mode;

        if (mode == ThemeMode.Dark)
        {
            ApplyDarkTheme(form);
        }
        else
        {
            ApplyLightTheme(form);
        }

        // Title Bar Dark Mode（共通ヘルパーを使用）
        SetDwmDarkMode(form.Handle, mode == ThemeMode.Dark);
    }

    private static void ApplyDarkTheme(Form form)
    {
        form.BackColor = DarkBackground;
        form.ForeColor = DarkText;

        foreach (Control c in form.Controls)
        {
            UpdateControlTheme(c, ThemeMode.Dark);
        }
    }

    private static void ApplyLightTheme(Form form)
    {
        form.BackColor = SystemColors.Control;
        form.ForeColor = SystemColors.ControlText;

        foreach (Control c in form.Controls)
        {
            UpdateControlTheme(c, ThemeMode.Light);
        }
    }

    private static void UpdateControlTheme(Control c, ThemeMode mode)
    {
        bool isDark = (mode == ThemeMode.Dark);

        if (c is RichTextBox rtb)
        {
            if (isDark)
            {
                rtb.BackColor = DarkBackground;
                rtb.ForeColor = DarkText;
                // スクロールバーをダークモードに対応させる
                if (!rtb.IsHandleCreated) { _ = rtb.Handle; }
                SetWindowTheme(rtb.Handle, "DarkMode_Explorer", null);
            }
            else
            {
                rtb.BackColor = SystemColors.Window;
                rtb.ForeColor = SystemColors.WindowText;
                // スクロールバーをライトモードに戻す
                if (rtb.IsHandleCreated)
                {
                    SetWindowTheme(rtb.Handle, "", null);
                }
            }
        }
        else if (c is ToolStrip ts)
        {
            if (isDark)
            {
                ts.BackColor = DarkToolStripBackground;
                ts.ForeColor = DarkText;
            }
            else
            {
                ts.BackColor = SystemColors.Control;
                ts.ForeColor = SystemColors.ControlText;
            }

            foreach (ToolStripItem item in ts.Items)
            {
                if (item is ToolStripTextBox tstb)
                {
                    if (isDark)
                    {
                        tstb.BackColor = DarkControlBackground;
                        tstb.ForeColor = DarkText;
                    }
                    else
                    {
                        tstb.BackColor = SystemColors.Window;
                        tstb.ForeColor = SystemColors.WindowText;
                    }
                }
            }
        }
        else if (c is Button btn)
        {
            if (isDark)
            {
                btn.BackColor = DarkControlBackground;
                btn.ForeColor = DarkText;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderColor = Color.Gray;
            }
            else
            {
                btn.BackColor = SystemColors.Control;
                btn.ForeColor = SystemColors.ControlText;
                btn.FlatStyle = FlatStyle.Standard;
            }
        }
        // コンボボックスのテーマ適用
        else if (c is ComboBox cmb)
        {
            if (isDark)
            {
                cmb.BackColor = DarkControlBackground;
                cmb.ForeColor = DarkText;
                // FlatStyleがFlatだとWinFormsが独自描画してしまいSetWindowThemeが効かないためStandardにする
                cmb.FlatStyle = FlatStyle.Standard;
                
                // コントロールのハンドルが作成されていることを確認してDWM属性とテーマを適用
                if (!cmb.IsHandleCreated)
                {
                    _ = cmb.Handle; // 強制的にハンドルを作成
                }
                // 共通ヘルパーを使用
                SetDwmDarkMode(cmb.Handle, true);
                SetWindowTheme(cmb.Handle, "DarkMode_CFD", null);
            }
            else
            {
                cmb.BackColor = SystemColors.Window;
                cmb.ForeColor = SystemColors.WindowText;
                cmb.FlatStyle = FlatStyle.Standard;

                if (cmb.IsHandleCreated)
                {
                    // 共通ヘルパーを使用
                    SetDwmDarkMode(cmb.Handle, false);
                    SetWindowTheme(cmb.Handle, "", null); // テーマ属性をリセット
                }
            }
        }
        // CheckBox, Label etc automatically inherit Form's ForeColor usually, unless explicitly set
        
        // Recursive
        foreach (Control child in c.Controls)
        {
            UpdateControlTheme(child, mode);
        }
    }

    public static ThemeMode GetSystemTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key != null)
            {
                var val = key.GetValue("AppsUseLightTheme");
                if (val is int i && i == 0) return ThemeMode.Dark;
            }
        }
        catch (Exception ex) { AppData.ReportError(LocalizationManager.GetString("Error_ThemeDetect") ?? "Failed to detect system theme", ex); }
        return ThemeMode.Light;
    }

    // ThemeSetting (ユーザー設定値) から ThemeMode (実際の表示モード) へ変換する
    public static ThemeMode Resolve(ThemeSetting setting)
    {
        return setting switch
        {
            ThemeSetting.Light => ThemeMode.Light,
            ThemeSetting.Dark => ThemeMode.Dark,
            _ => GetSystemTheme() // System (Auto)
        };
    }
}
