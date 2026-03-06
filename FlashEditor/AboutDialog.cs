using System.Diagnostics;
using System.Drawing;
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
        // 多言語対応があればここで適用可能だが、シンプルなテキストにする
        lblTitle.Text = "Flash Editor";
        lblVersion.Text = "Version 1.0";
        lblAuthor.Text = "作者：yhira";
    }

    private void LnkUrl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo("https://nelab.jp/") { UseShellExecute = true });
        }
        catch { }
    }
}
