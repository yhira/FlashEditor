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
        
        bool isDark = ThemeManager.IsDark;
        Color linkColor = isDark ? Color.LightSkyBlue : Color.Blue;
        lnkUrl.LinkColor = linkColor;
        lnkUrl.ActiveLinkColor = linkColor;
        lnkUrl.VisitedLinkColor = linkColor;
    }

    private void AboutDialog_Load(object sender, EventArgs e)
    {
        // 多言語対応のテキストを適用
        this.Text = LocalizationManager.GetString("About_Title") ?? "About";
        lblTitle.Text = "Flash Editor";
        // アセンブリからバージョン情報を自動取得
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        lblVersion.Text = $"Version {version?.Major}.{version?.Minor}.{version?.Build}";
        
        // 製品バージョンからビルド日時文字列を取得
        var attrib = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        string buildTime = attrib?.InformationalVersion ?? "Unknown";
        lblBuild.Text = $"Build {buildTime}";
        
        lblAuthor.Text = LocalizationManager.GetString("About_Author") ?? "Author: yhira";
        btnOk.Text = LocalizationManager.GetString("Button_OK") ?? "OK";
    }

    private void LnkUrl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        // 共通ヘルパーでURLを開く
        MainForm.OpenUrl("https://nelab.jp/");
    }
}
