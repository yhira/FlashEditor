using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace FlashEditor;

public partial class AboutDialog : Form
{
    public AboutDialog()
    {
        InitializeComponent();
        
        // テーマの適用 (シンプルにシステムテーマによる)
        var mode = ThemeManager.CurrentTheme;
        ThemeManager.ApplyTheme(this, mode);
        
        bool isDark = (mode == ThemeManager.ThemeMode.Dark);
        Color linkColor = isDark ? Color.LightSkyBlue : Color.Blue;
        lnkUrl.LinkColor = linkColor;
        lnkUrl.ActiveLinkColor = linkColor;
        lnkUrl.VisitedLinkColor = linkColor;
    }

    private void AboutDialog_Load(object sender, EventArgs e)
    {
        // 多言語対応のテキストを適用
        this.Text = LocalizationManager.GetString("About_Title") ?? "バージョン情報";
        lblTitle.Text = "Flash Editor";
        // アセンブリからバージョン情報を自動取得
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        lblVersion.Text = $"Version {version?.Major}.{version?.Minor}.{version?.Build}";
        lblAuthor.Text = LocalizationManager.GetString("About_Author") ?? "作者：yhira";
        btnOk.Text = LocalizationManager.GetString("Button_OK") ?? "OK";
    }

    private void LnkUrl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo("https://nelab.jp/") { UseShellExecute = true });
        }
        catch (Exception ex) { AppData.ReportError(LocalizationManager.GetString("Error_LinkOpen") ?? "Could not open link", ex); }
    }
}
