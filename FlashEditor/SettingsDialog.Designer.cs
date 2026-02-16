
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
        this.btnChangeFont = new System.Windows.Forms.Button();
        this.lblFontPreview = new System.Windows.Forms.Label();
        this.chkTopMost = new System.Windows.Forms.CheckBox();
        this.btnOK = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.SuspendLayout();
        // 
        // btnChangeFont
        // 
        this.btnChangeFont.Location = new System.Drawing.Point(24, 24);
        this.btnChangeFont.Name = "btnChangeFont";
        this.btnChangeFont.Size = new System.Drawing.Size(200, 46);
        this.btnChangeFont.TabIndex = 0;
        this.btnChangeFont.Text = "フォント変更...";
        this.btnChangeFont.UseVisualStyleBackColor = true;
        this.btnChangeFont.Click += new System.EventHandler(this.btnChangeFont_Click);
        // 
        // lblFontPreview
        // 
        this.lblFontPreview.AutoSize = true;
        this.lblFontPreview.Location = new System.Drawing.Point(236, 32);
        this.lblFontPreview.Name = "lblFontPreview";
        this.lblFontPreview.Size = new System.Drawing.Size(156, 30);
        this.lblFontPreview.TabIndex = 1;
        this.lblFontPreview.Text = "サンプルテキスト";
        // 
        // chkTopMost
        // 
        this.chkTopMost.AutoSize = true;
        this.chkTopMost.Location = new System.Drawing.Point(24, 100);
        this.chkTopMost.Name = "chkTopMost";
        this.chkTopMost.Size = new System.Drawing.Size(252, 34);
        this.chkTopMost.TabIndex = 2;
        this.chkTopMost.Text = "起動時に最前面表示";
        this.chkTopMost.UseVisualStyleBackColor = true;
        // 
        // btnOK
        // 
        this.btnOK.Location = new System.Drawing.Point(232, 180);
        this.btnOK.Name = "btnOK";
        this.btnOK.Size = new System.Drawing.Size(150, 46);
        this.btnOK.TabIndex = 3;
        this.btnOK.Text = "OK";
        this.btnOK.UseVisualStyleBackColor = true;
        this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
        // 
        // btnCancel
        // 
        this.btnCancel.Location = new System.Drawing.Point(394, 180);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(150, 46);
        this.btnCancel.TabIndex = 4;
        this.btnCancel.Text = "キャンセル";
        this.btnCancel.UseVisualStyleBackColor = true;
        this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
        // 
        // SettingsDialog
        // 
        this.AcceptButton = this.btnOK;
        this.CancelButton = this.btnCancel;
        this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 29F); // Font scaled (7*2, 14.5(approx)*2)
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(568, 300); // Increased height from 250 to 300
        this.ControlBox = false; // Close button disabled for simplicity with large dialog or keep standard
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.btnOK);
        this.Controls.Add(this.chkTopMost);
        this.Controls.Add(this.lblFontPreview);
        this.Controls.Add(this.btnChangeFont);
        this.Font = new System.Drawing.Font("Yu Gothic UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "SettingsDialog";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Flash Editor 設定";
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    private System.Windows.Forms.Button btnChangeFont;
    private System.Windows.Forms.Label lblFontPreview;
    private System.Windows.Forms.CheckBox chkTopMost;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
}
