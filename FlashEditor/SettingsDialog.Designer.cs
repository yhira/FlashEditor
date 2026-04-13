
namespace FlashEditor;

partial class SettingsDialog
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        // プレビュー用GDIフォントを明示解放（リーク防止）
        if (disposing)
        {
            _previewFont?.Dispose();
            // キャンセル時用: FontDialogで生成されたフォントが残っていれば解放
            // （元の親フォームのフォントは解放しない）
            if (CurrentFont != _originalFont)
                CurrentFont?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.lblFont = new System.Windows.Forms.Label();
        this.lblFontPreview = new System.Windows.Forms.Label();
        this.btnChangeFont = new System.Windows.Forms.Button();
        this.lblTheme = new System.Windows.Forms.Label();
        this.cmbTheme = new System.Windows.Forms.ComboBox();
        this.lblToolButtonSize = new System.Windows.Forms.Label();
        this.cmbToolButtonSize = new System.Windows.Forms.ComboBox();
        this.lblLanguage = new System.Windows.Forms.Label();
        this.cmbLanguage = new System.Windows.Forms.ComboBox();
        this.btnOK = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.SuspendLayout();
        //
        // ── 行1: フォントプレビュー ──
        //
        this.lblFont.AutoSize = true;
        this.lblFont.Location = new System.Drawing.Point(32, 28);
        this.lblFont.Name = "lblFont";
        this.lblFont.Text = "フォント：";
        //
        this.lblFontPreview.Location = new System.Drawing.Point(220, 20);
        this.lblFontPreview.Name = "lblFontPreview";
        this.lblFontPreview.Size = new System.Drawing.Size(350, 40);
        this.lblFontPreview.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.lblFontPreview.AutoEllipsis = true;
        this.lblFontPreview.Text = "サンプルテキスト";
        //
        // ── 行2: フォント変更ボタン ──
        //
        this.btnChangeFont.Location = new System.Drawing.Point(220, 68);
        this.btnChangeFont.Name = "btnChangeFont";
        this.btnChangeFont.Size = new System.Drawing.Size(220, 40);
        this.btnChangeFont.TabIndex = 0;
        this.btnChangeFont.Text = "フォント変更...";
        this.btnChangeFont.UseVisualStyleBackColor = true;
        this.btnChangeFont.Click += new System.EventHandler(this.btnChangeFont_Click);
        //
        //
        // ── 行3: テーマ ──
        //
        this.lblTheme.AutoSize = true;
        this.lblTheme.Location = new System.Drawing.Point(32, 142);
        this.lblTheme.Name = "lblTheme";
        this.lblTheme.Text = "テーマ：";
        //
        // cmbTheme
        //
        this.cmbTheme.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
        this.cmbTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbTheme.FormattingEnabled = true;
        this.cmbTheme.ItemHeight = 36;
        this.cmbTheme.Location = new System.Drawing.Point(220, 134);
        this.cmbTheme.Name = "cmbTheme";
        this.cmbTheme.Size = new System.Drawing.Size(350, 42);
        this.cmbTheme.TabIndex = 1;
        //
        // ── 行4: ツールボタンサイズ ──
        //
        this.lblToolButtonSize.AutoSize = true;
        this.lblToolButtonSize.Location = new System.Drawing.Point(32, 208);
        this.lblToolButtonSize.Name = "lblToolButtonSize";
        this.lblToolButtonSize.Text = "ボタンサイズ：";
        //
        // cmbToolButtonSize
        //
        this.cmbToolButtonSize.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
        this.cmbToolButtonSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbToolButtonSize.FormattingEnabled = true;
        this.cmbToolButtonSize.ItemHeight = 36;
        this.cmbToolButtonSize.Location = new System.Drawing.Point(220, 200);
        this.cmbToolButtonSize.Name = "cmbToolButtonSize";
        this.cmbToolButtonSize.Size = new System.Drawing.Size(350, 42);
        this.cmbToolButtonSize.TabIndex = 2;
        //
        // ── 行5: 言語設定 ──
        //
        this.lblLanguage.AutoSize = true;
        this.lblLanguage.Location = new System.Drawing.Point(32, 274);
        this.lblLanguage.Name = "lblLanguage";
        this.lblLanguage.Text = "言語：";
        //
        // cmbLanguage
        //
        this.cmbLanguage.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
        this.cmbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbLanguage.FormattingEnabled = true;
        this.cmbLanguage.ItemHeight = 36;
        this.cmbLanguage.Location = new System.Drawing.Point(220, 266);
        this.cmbLanguage.Name = "cmbLanguage";
        this.cmbLanguage.Size = new System.Drawing.Size(350, 42);
        this.cmbLanguage.TabIndex = 3;
        //
        // ── 行6: ボタン行 (右寄せ) ──
        //
        this.btnOK.Location = new System.Drawing.Point(308, 340);
        this.btnOK.Name = "btnOK";
        this.btnOK.Size = new System.Drawing.Size(120, 40);
        this.btnOK.TabIndex = 4;
        this.btnOK.Text = "OK";
        this.btnOK.UseVisualStyleBackColor = true;
        this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
        //
        this.btnCancel.Location = new System.Drawing.Point(440, 340);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(130, 40);
        this.btnCancel.TabIndex = 5;
        this.btnCancel.Text = "キャンセル";
        this.btnCancel.UseVisualStyleBackColor = true;
        this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
        //
        // ── SettingsDialog ──
        //
        this.AcceptButton = this.btnOK;
        this.CancelButton = this.btnCancel;
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.ClientSize = new System.Drawing.Size(604, 420);
        this.ControlBox = false;
        this.Controls.Add(this.lblFont);
        this.Controls.Add(this.lblFontPreview);
        this.Controls.Add(this.btnChangeFont);
        this.Controls.Add(this.lblTheme);
        this.Controls.Add(this.cmbTheme);
        this.Controls.Add(this.lblToolButtonSize);
        this.Controls.Add(this.cmbToolButtonSize);
        this.Controls.Add(this.lblLanguage);
        this.Controls.Add(this.cmbLanguage);
        this.Controls.Add(this.btnOK);
        this.Controls.Add(this.btnCancel);
        this.Font = new System.Drawing.Font("Yu Gothic UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "SettingsDialog";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.ShowInTaskbar = false; // タスクバーに表示しない
        this.Text = "Flash Editor 設定";
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    private System.Windows.Forms.Label lblFont;
    private System.Windows.Forms.Label lblFontPreview;
    private System.Windows.Forms.Button btnChangeFont;
    private System.Windows.Forms.Label lblTheme;
    private System.Windows.Forms.ComboBox cmbTheme;
    private System.Windows.Forms.Label lblToolButtonSize;
    private System.Windows.Forms.ComboBox cmbToolButtonSize;
    private System.Windows.Forms.Label lblLanguage;
    private System.Windows.Forms.ComboBox cmbLanguage;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
}
