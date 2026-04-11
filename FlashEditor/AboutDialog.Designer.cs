namespace FlashEditor;

partial class AboutDialog
{
    private System.ComponentModel.IContainer components = null;
    private Label lblTitle;
    private Label lblVersion;
    private Label lblBuild;
    private Label lblAuthor;
    private LinkLabel lnkUrl;
    private Button btnOk;

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
        this.lblTitle = new System.Windows.Forms.Label();
        this.lblVersion = new System.Windows.Forms.Label();
        this.lblBuild = new System.Windows.Forms.Label();
        this.lblAuthor = new System.Windows.Forms.Label();
        this.lnkUrl = new System.Windows.Forms.LinkLabel();
        this.btnOk = new System.Windows.Forms.Button();
        this.SuspendLayout();
        // 
        // lblTitle
        // 
        this.lblTitle.AutoSize = true;
        this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.lblTitle.Location = new System.Drawing.Point(20, 20);
        this.lblTitle.Name = "lblTitle";
        this.lblTitle.Size = new System.Drawing.Size(100, 21);
        this.lblTitle.TabIndex = 0;
        this.lblTitle.Text = "Flash Editor";
        // 
        // lblVersion
        // 
        this.lblVersion.AutoSize = true;
        this.lblVersion.Location = new System.Drawing.Point(20, 50);
        this.lblVersion.Name = "lblVersion";
        this.lblVersion.Size = new System.Drawing.Size(65, 15);
        this.lblVersion.TabIndex = 1;
        this.lblVersion.Text = "Version 1.0";
        // 
        // lblBuild
        // 
        this.lblBuild.AutoSize = true;
        this.lblBuild.Location = new System.Drawing.Point(20, 68);
        this.lblBuild.Name = "lblBuild";
        this.lblBuild.Size = new System.Drawing.Size(34, 15);
        this.lblBuild.TabIndex = 5;
        this.lblBuild.Text = "Build";
        // 
        // lblAuthor
        // 
        this.lblAuthor.AutoSize = true;
        this.lblAuthor.Location = new System.Drawing.Point(20, 98);
        this.lblAuthor.Name = "lblAuthor";
        this.lblAuthor.Size = new System.Drawing.Size(68, 15);
        this.lblAuthor.TabIndex = 2;
        this.lblAuthor.Text = "作者：yhira";
        // 
        // lnkUrl
        // 
        this.lnkUrl.AutoSize = true;
        this.lnkUrl.Location = new System.Drawing.Point(20, 128);
        this.lnkUrl.Name = "lnkUrl";
        this.lnkUrl.Size = new System.Drawing.Size(95, 15);
        this.lnkUrl.TabIndex = 3;
        this.lnkUrl.TabStop = true;
        this.lnkUrl.Text = "https://nelab.jp/";
        this.lnkUrl.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkUrl_LinkClicked);
        // 
        // btnOk
        // 
        this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.btnOk.Location = new System.Drawing.Point(197, 164);
        this.btnOk.Name = "btnOk";
        this.btnOk.Size = new System.Drawing.Size(75, 25);
        this.btnOk.TabIndex = 4;
        this.btnOk.Text = "OK";
        this.btnOk.UseVisualStyleBackColor = true;
        // 
        // AboutDialog
        // 
        this.AcceptButton = this.btnOk;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(284, 204);
        this.Controls.Add(this.btnOk);
        this.Controls.Add(this.lnkUrl);
        this.Controls.Add(this.lblAuthor);
        this.Controls.Add(this.lblBuild);
        this.Controls.Add(this.lblVersion);
        this.Controls.Add(this.lblTitle);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "AboutDialog";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "バージョン情報";
        this.Load += new System.EventHandler(this.AboutDialog_Load);
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
