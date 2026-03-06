
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
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.lblFont = new System.Windows.Forms.Label();
        this.lblFontPreview = new System.Windows.Forms.Label();
        this.btnChangeFont = new System.Windows.Forms.Button();
        this.lblTheme = new System.Windows.Forms.Label();
        this.cmbTheme = new System.Windows.Forms.ComboBox();
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
        this.lblFontPreview.Location = new System.Drawing.Point(160, 20);
        this.lblFontPreview.Name = "lblFontPreview";
        this.lblFontPreview.Size = new System.Drawing.Size(370, 40);
        this.lblFontPreview.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.lblFontPreview.AutoEllipsis = true;
        this.lblFontPreview.Text = "サンプルテキスト";
        //
        // ── 行2: フォント変更ボタン ──
        //
        this.btnChangeFont.Location = new System.Drawing.Point(160, 68);
        this.btnChangeFont.Name = "btnChangeFont";
        this.btnChangeFont.Size = new System.Drawing.Size(220, 40);
        this.btnChangeFont.TabIndex = 0;
        this.btnChangeFont.Text = "フォント変更...";
        this.btnChangeFont.UseVisualStyleBackColor = true;
        this.btnChangeFont.Click += new System.EventHandler(this.btnChangeFont_Click);
        //
        // ── 行3: テーマ (余白を多めに確保) ──
        //
        this.lblTheme.AutoSize = true;
        this.lblTheme.Location = new System.Drawing.Point(32, 142); // 132 -> 142 に下げて余白確保
        this.lblTheme.Name = "lblTheme";
        this.lblTheme.Text = "テーマ：";
        //
        // ComboBox (ItemHeight をさらに拡大)
        //
        this.cmbTheme.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
        this.cmbTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbTheme.FormattingEnabled = true;
        this.cmbTheme.ItemHeight = 36; // 32 -> 36 に拡大
        this.cmbTheme.Location = new System.Drawing.Point(160, 134); // 124 -> 134 に調整
        this.cmbTheme.Name = "cmbTheme";
        this.cmbTheme.Size = new System.Drawing.Size(370, 42); // ItemHeight + 枠分
        this.cmbTheme.TabIndex = 1;
        //
        // ── 行4: ボタン行 (右寄せ) ──
        //
        this.btnOK.Location = new System.Drawing.Point(268, 205); // 190 -> 205
        this.btnOK.Name = "btnOK";
        this.btnOK.Size = new System.Drawing.Size(120, 40);
        this.btnOK.TabIndex = 2;
        this.btnOK.Text = "OK";
        this.btnOK.UseVisualStyleBackColor = true;
        this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
        //
        this.btnCancel.Location = new System.Drawing.Point(400, 205);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(130, 40);
        this.btnCancel.TabIndex = 3;
        this.btnCancel.Text = "キャンセル";
        this.btnCancel.UseVisualStyleBackColor = true;
        this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
        //
        // ── SettingsDialog ──
        //
        this.AcceptButton = this.btnOK;
        this.CancelButton = this.btnCancel;
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.ClientSize = new System.Drawing.Size(564, 270); // 254 -> 270 に拡大
        this.ControlBox = false;
        this.Controls.Add(this.lblFont);
        this.Controls.Add(this.lblFontPreview);
        this.Controls.Add(this.btnChangeFont);
        this.Controls.Add(this.lblTheme);
        this.Controls.Add(this.cmbTheme);
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
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
}
