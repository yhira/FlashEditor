
using System;
using System.Windows.Forms;

namespace FlashEditor;

public partial class SettingsDialog : Form
{
    public Font CurrentFont { get; private set; }

    public SettingsDialog(Font currentFont)
    {
        InitializeComponent();
        CurrentFont = currentFont;

        // 初期値反映
        lblFontPreview.Font = CurrentFont;
        lblFontPreview.Text = $"{CurrentFont.Name}, {CurrentFont.Size}pt";
        
        ThemeManager.ApplyTheme(this, ThemeManager.CurrentTheme);
    }

    private void btnChangeFont_Click(object sender, EventArgs e)
    {
        using var fd = new FontDialog();
        fd.Font = CurrentFont;
        if (fd.ShowDialog() == DialogResult.OK)
        {
            CurrentFont = fd.Font;
            lblFontPreview.Font = CurrentFont;
            lblFontPreview.Text = $"{CurrentFont.Name}, {CurrentFont.Size}pt";
        }
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        this.DialogResult = DialogResult.OK;
        this.Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        this.DialogResult = DialogResult.Cancel;
        this.Close();
    }
}
