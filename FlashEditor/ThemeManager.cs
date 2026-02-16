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

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    public enum ThemeMode { Light, Dark }

    public static ThemeMode CurrentTheme { get; private set; } = ThemeMode.Light;

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

        // Title Bar Dark Mode
        bool useDarkMode = (mode == ThemeMode.Dark);
        int useDarkModeInt = useDarkMode ? 1 : 0;
        
        // Try both attributes for compatibility
        DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkModeInt, sizeof(int));
        DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref useDarkModeInt, sizeof(int));
    }

    private static void ApplyDarkTheme(Form form)
    {
        form.BackColor = Color.FromArgb(30, 30, 30);
        form.ForeColor = Color.WhiteSmoke;

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
        if (c is RichTextBox rtb)
        {
            if (mode == ThemeMode.Dark)
            {
                rtb.BackColor = Color.FromArgb(30, 30, 30); // 少し明るめ
                rtb.ForeColor = Color.WhiteSmoke;
            }
            else
            {
                rtb.BackColor = SystemColors.Window;
                rtb.ForeColor = SystemColors.WindowText;
            }
        }
        else if (c is ToolStrip ts)
        {
             if (mode == ThemeMode.Dark)
            {
                ts.BackColor = Color.FromArgb(45, 45, 48);
                ts.ForeColor = Color.WhiteSmoke;
                // Renderer could be customized for better look
            }
            else
            {
                ts.BackColor = SystemColors.Control;
                ts.ForeColor = SystemColors.ControlText;
            }
        }
        else if (c is Button btn)
        {
             if (mode == ThemeMode.Dark)
            {
                btn.BackColor = Color.FromArgb(50, 50, 50);
                btn.ForeColor = Color.WhiteSmoke;
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
        catch { }
        return ThemeMode.Light;
    }
}
