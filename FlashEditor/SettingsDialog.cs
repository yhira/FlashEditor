
using System;
using System.Windows.Forms;

namespace FlashEditor;

public partial class SettingsDialog : Form
{
    public Font CurrentFont { get; private set; }
    public ThemeSetting CurrentTheme { get; private set; }
    public ToolButtonSizeSetting CurrentToolButtonSize { get; private set; }
    public string CurrentLanguage { get; private set; }

    public SettingsDialog(Font currentFont, ThemeSetting currentTheme, ToolButtonSizeSetting currentToolButtonSize, string currentLanguage)
    {
        InitializeComponent();
        CurrentFont = currentFont;
        CurrentTheme = currentTheme;
        CurrentToolButtonSize = currentToolButtonSize;
        CurrentLanguage = currentLanguage;

        // UIテキストのローカライズ適用
        this.Text = LocalizationManager.GetString("Settings_Title") ?? "Flash Editor Settings";
        lblFont.Text = LocalizationManager.GetString("Settings_Font") ?? "Font:";
        btnChangeFont.Text = LocalizationManager.GetString("Settings_ChangeFont") ?? "Change Font...";
        lblTheme.Text = LocalizationManager.GetString("Settings_Theme") ?? "Theme:";
        lblToolButtonSize.Text = LocalizationManager.GetString("Settings_ToolButtonSize") ?? "Button Size:";
        lblLanguage.Text = LocalizationManager.GetString("Settings_Language") ?? "Language:";
        btnOK.Text = LocalizationManager.GetString("Button_OK") ?? "OK";
        btnCancel.Text = LocalizationManager.GetString("Button_Cancel") ?? "Cancel";

        // 初期値反映
        UpdateFontPreview();

        // テーマ選択肢の設定
        cmbTheme.Items.Add(LocalizationManager.GetString("Settings_Theme_System") ?? "System Default");
        cmbTheme.Items.Add(LocalizationManager.GetString("Settings_Theme_Light") ?? "Light");
        cmbTheme.Items.Add(LocalizationManager.GetString("Settings_Theme_Dark") ?? "Dark");
        cmbTheme.SelectedIndex = (int)CurrentTheme;
        // ComboBox のオーナードロー描画イベントを登録
        cmbTheme.DrawItem += CmbOwnerDraw_DrawItem;

        // ツールボタンサイズ選択肢の設定
        cmbToolButtonSize.Items.Add(LocalizationManager.GetString("Settings_ToolButtonSize_Small") ?? "Small");
        cmbToolButtonSize.Items.Add(LocalizationManager.GetString("Settings_ToolButtonSize_Medium") ?? "Medium");
        cmbToolButtonSize.Items.Add(LocalizationManager.GetString("Settings_ToolButtonSize_Large") ?? "Large");
        cmbToolButtonSize.SelectedIndex = (int)CurrentToolButtonSize;
        // ComboBox のオーナードロー描画イベントを登録（テーマと共通）
        cmbToolButtonSize.DrawItem += CmbOwnerDraw_DrawItem;

        // 言語設定
        int selectedLangIndex = 0;
        for (int i = 0; i < LocalizationManager.AvailableLanguages.Count; i++)
        {
            var langInfo = LocalizationManager.AvailableLanguages[i];
            cmbLanguage.Items.Add(langInfo);
            if (langInfo.Code == CurrentLanguage)
            {
                selectedLangIndex = i;
            }
        }
        if (cmbLanguage.Items.Count > 0)
        {
            cmbLanguage.SelectedIndex = selectedLangIndex;
        }
        cmbLanguage.DrawItem += CmbOwnerDraw_DrawItem;

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

    // 自前で生成したプレビュー用フォントを追跡（親フォームの共有フォントは Dispose しない）
    private Font? _previewFont;

    private void UpdateFontPreview()
    {
        // 以前に自前で生成したフォントがあれば解放
        var oldFont = _previewFont;
        _previewFont = new Font(CurrentFont.FontFamily, 16f, CurrentFont.Style, GraphicsUnit.Point);
        lblFontPreview.Font = _previewFont;
        lblFontPreview.Text = $"{CurrentFont.Name}, {CurrentFont.Size}pt";
        oldFont?.Dispose();
    }

    private void btnChangeFont_Click(object sender, EventArgs e)
    {
        using var fd = new FontDialog();
        fd.Font = CurrentFont;
        // フォントサイズを10ptから48ptまでに制限してUI崩れを防止
        fd.MinSize = 10;
        fd.MaxSize = 48;
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
        if (cmbLanguage.SelectedItem is LocalizationManager.LanguageInfo langInfo)
        {
            CurrentLanguage = langInfo.Code;
        }
        this.DialogResult = DialogResult.OK;
        this.Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        this.DialogResult = DialogResult.Cancel;
        this.Close();
    }
}
