
using System;
using System.Windows.Forms;

namespace FlashEditor;

public partial class SettingsDialog : Form
{
    public Font CurrentFont { get; private set; }
    public ThemeSetting CurrentTheme { get; private set; }
    public ToolButtonSizeSetting CurrentToolButtonSize { get; private set; }

    public SettingsDialog(Font currentFont, ThemeSetting currentTheme, ToolButtonSizeSetting currentToolButtonSize)
    {
        InitializeComponent();
        CurrentFont = currentFont;
        CurrentTheme = currentTheme;
        CurrentToolButtonSize = currentToolButtonSize;

        // 初期値反映
        UpdateFontPreview();

        // テーマ選択肢の設定
        cmbTheme.Items.Add("システム設定に従う");
        cmbTheme.Items.Add("ライト");
        cmbTheme.Items.Add("ダーク");
        cmbTheme.SelectedIndex = (int)CurrentTheme;
        // ComboBox のオーナードロー描画イベントを登録
        cmbTheme.DrawItem += CmbOwnerDraw_DrawItem;

        // ツールボタンサイズ選択肢の設定
        cmbToolButtonSize.Items.Add("小 (32×32)");
        cmbToolButtonSize.Items.Add("標準 (40×40)");
        cmbToolButtonSize.Items.Add("大 (48×48)");
        cmbToolButtonSize.SelectedIndex = (int)CurrentToolButtonSize;
        // ComboBox のオーナードロー描画イベントを登録（テーマと共通）
        cmbToolButtonSize.DrawItem += CmbOwnerDraw_DrawItem;

        ThemeManager.ApplyTheme(this, ThemeManager.Resolve(CurrentTheme));
    }

    // ComboBox のオーナードロー描画（テーマに合わせた選択色を使用・共通処理）
    private void CmbOwnerDraw_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0) return;
        var cmb = sender as ComboBox;
        if (cmb == null) return;

        // 現在のテーマに応じた色を決定
        bool isDark = ThemeManager.CurrentTheme == ThemeManager.ThemeMode.Dark;
        // 選択中の項目の背景色
        Color selectedBg = isDark ? Color.FromArgb(70, 70, 80) : Color.FromArgb(200, 220, 240);
        // 通常状態の背景色
        Color normalBg = isDark ? Color.FromArgb(50, 50, 50) : SystemColors.Window;
        // テキスト色
        Color textColor = isDark ? Color.WhiteSmoke : SystemColors.WindowText;

        // 選択状態かどうかで背景色を切り替え
        bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        using var bgBrush = new SolidBrush(isSelected ? selectedBg : normalBg);
        e.Graphics.FillRectangle(bgBrush, e.Bounds);

        // テキスト描画 (垂直中央揃え)
        string text = cmb.Items[e.Index]?.ToString() ?? "";
        using var textBrush = new SolidBrush(textColor);
        float yPos = e.Bounds.Y + (e.Bounds.Height - e.Font!.Height) / 2f;
        e.Graphics.DrawString(text, e.Font, textBrush, e.Bounds.X + 6, yPos);

        // フォーカス枠
        e.DrawFocusRectangle();
    }

    private void UpdateFontPreview()
    {
        lblFontPreview.Font = new Font(CurrentFont.FontFamily, 16f, CurrentFont.Style, GraphicsUnit.Point);
        lblFontPreview.Text = $"{CurrentFont.Name}, {CurrentFont.Size}pt";
    }

    private void btnChangeFont_Click(object sender, EventArgs e)
    {
        using var fd = new FontDialog();
        fd.Font = CurrentFont;
        if (fd.ShowDialog() == DialogResult.OK)
        {
            CurrentFont = fd.Font;
            UpdateFontPreview();
        }
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        CurrentTheme = (ThemeSetting)cmbTheme.SelectedIndex;
        CurrentToolButtonSize = (ToolButtonSizeSetting)cmbToolButtonSize.SelectedIndex;
        this.DialogResult = DialogResult.OK;
        this.Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        this.DialogResult = DialogResult.Cancel;
        this.Close();
    }
}
