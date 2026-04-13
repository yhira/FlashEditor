namespace FlashEditor;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        // Timer・ToolTip・Icon の GDI リソースを明示的に解放（リーク防止）
        if (disposing)
        {
            _autoSaveTimer.Dispose();
            _tooltipTimer.Dispose();
            _customToolTip.Dispose();
            // フォームアイコンのGDIリソースを明示解放
            this.Icon?.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        this.toolStrip1 = new System.Windows.Forms.ToolStrip();
        this.tsbNewMemo = new System.Windows.Forms.ToolStripButton();
        this.toolStripSeparator0 = new System.Windows.Forms.ToolStripSeparator();
        this.tsbTopMost = new System.Windows.Forms.ToolStripButton();
        this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
        this.tsbUndo = new System.Windows.Forms.ToolStripButton();
        this.tsbRedo = new System.Windows.Forms.ToolStripButton();
        this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
        this.tsbCut = new System.Windows.Forms.ToolStripButton();
        this.tsbCopy = new System.Windows.Forms.ToolStripButton();
        this.tsbPaste = new System.Windows.Forms.ToolStripButton();
        this.tsbDelete = new System.Windows.Forms.ToolStripButton();
        this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
        this.tsbFind = new System.Windows.Forms.ToolStripButton();
        this.tsbGoogleSearch = new System.Windows.Forms.ToolStripButton();
        this.tsbSettings = new System.Windows.Forms.ToolStripButton();
        this.tsbAbout = new System.Windows.Forms.ToolStripButton();
        this.txtMain = new System.Windows.Forms.RichTextBox();
        this.toolStrip1.SuspendLayout();
        this.SuspendLayout();
        // 
        // toolStrip1
        // 
        // ボタンの論理的グルーピング: ドキュメント操作 → 履歴 → 編集 → 検索 → 設定
        this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.tsbNewMemo,
        this.tsbTopMost,
        this.toolStripSeparator0,
        this.tsbUndo,
        this.tsbRedo,
        this.toolStripSeparator1,
        this.tsbCut,
        this.tsbCopy,
        this.tsbPaste,
        this.tsbDelete,
        this.toolStripSeparator2,
        this.tsbFind,
        this.tsbGoogleSearch,
        this.toolStripSeparator3,
        this.tsbSettings,
        this.tsbAbout});
        this.toolStrip1.Location = new System.Drawing.Point(0, 0);
        this.toolStrip1.Name = "toolStrip1";
        this.toolStrip1.Size = new System.Drawing.Size(800, 25);
        this.toolStrip1.TabIndex = 0;
        this.toolStrip1.Text = "toolStrip1";
        // 
        // tsbNewMemo
        // 
        this.tsbNewMemo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.tsbNewMemo.Name = "tsbNewMemo";
        this.tsbNewMemo.Size = new System.Drawing.Size(23, 22);
        this.tsbNewMemo.Text = "新しいメモ";
        this.tsbNewMemo.ToolTipText = "新しいメモ";
        // 
        // toolStripSeparator0
        // 
        this.toolStripSeparator0.Name = "toolStripSeparator0";
        this.toolStripSeparator0.Size = new System.Drawing.Size(6, 25);
        // 
        // tsbTopMost
        // 
        this.tsbTopMost.CheckOnClick = true;
        this.tsbTopMost.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.tsbTopMost.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.tsbTopMost.Name = "tsbTopMost";
        this.tsbTopMost.Size = new System.Drawing.Size(23, 22);
        this.tsbTopMost.Text = "常に最前面";
        this.tsbTopMost.ToolTipText = "常に最前面";
        // 
        // toolStripSeparator1
        // 
        this.toolStripSeparator1.Name = "toolStripSeparator1";
        this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
        // 
        // tsbUndo
        // 
        this.tsbUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.tsbUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.tsbUndo.Name = "tsbUndo";
        this.tsbUndo.Size = new System.Drawing.Size(23, 22);
        this.tsbUndo.Text = "元に戻す";
        this.tsbUndo.ToolTipText = "元に戻す (Ctrl+Z)";
        // 
        // tsbRedo
        // 
        this.tsbRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.tsbRedo.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.tsbRedo.Name = "tsbRedo";
        this.tsbRedo.Size = new System.Drawing.Size(23, 22);
        this.tsbRedo.Text = "やり直し";
        this.tsbRedo.ToolTipText = "やり直し (Ctrl+Y)";
        // 
        // toolStripSeparator2
        // 
        this.toolStripSeparator2.Name = "toolStripSeparator2";
        this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
        // 
        // tsbCut
        // 
        this.tsbCut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.tsbCut.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.tsbCut.Name = "tsbCut";
        this.tsbCut.Size = new System.Drawing.Size(23, 22);
        this.tsbCut.Text = "切り取り";
        this.tsbCut.ToolTipText = "切り取り (Ctrl+X)";
        // 
        // tsbCopy
        // 
        this.tsbCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.tsbCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.tsbCopy.Name = "tsbCopy";
        this.tsbCopy.Size = new System.Drawing.Size(23, 22);
        this.tsbCopy.Text = "コピー";
        this.tsbCopy.ToolTipText = "コピー (Ctrl+C)";
        // 
        // tsbPaste
        // 
        this.tsbPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.tsbPaste.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.tsbPaste.Name = "tsbPaste";
        this.tsbPaste.Size = new System.Drawing.Size(23, 22);
        this.tsbPaste.Text = "貼り付け";
        this.tsbPaste.ToolTipText = "貼り付け (Ctrl+V)";
        // 
        // tsbDelete
        // 
        this.tsbDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.tsbDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.tsbDelete.Name = "tsbDelete";
        this.tsbDelete.Size = new System.Drawing.Size(23, 22);
        this.tsbDelete.Text = "削除";
        this.tsbDelete.ToolTipText = "削除 (Delete)";
        // 
        // toolStripSeparator3
        // 
        this.toolStripSeparator3.Name = "toolStripSeparator3";
        this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
        // 
        // tsbFind
        // 
        this.tsbFind.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.tsbFind.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.tsbFind.Name = "tsbFind";
        this.tsbFind.Size = new System.Drawing.Size(23, 22);
        this.tsbFind.Text = "検索";
        this.tsbFind.ToolTipText = "検索 (Ctrl+F)";
        // 
        // tsbGoogleSearch
        // 
        this.tsbGoogleSearch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.tsbGoogleSearch.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.tsbGoogleSearch.Name = "tsbGoogleSearch";
        this.tsbGoogleSearch.Size = new System.Drawing.Size(23, 22);
        this.tsbGoogleSearch.Text = "Googleで検索";
        this.tsbGoogleSearch.ToolTipText = "Googleで検索";
        // 
        // tsbSettings
        // 
        this.tsbSettings.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
        this.tsbSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.tsbSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.tsbSettings.Name = "tsbSettings";
        this.tsbSettings.Size = new System.Drawing.Size(23, 22);
        this.tsbSettings.Text = "設定";
        this.tsbSettings.ToolTipText = "設定";
        // 
        // tsbAbout
        // 
        this.tsbAbout.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
        this.tsbAbout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.tsbAbout.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.tsbAbout.Name = "tsbAbout";
        this.tsbAbout.Size = new System.Drawing.Size(23, 22);
        this.tsbAbout.Text = "バージョン情報";
        this.tsbAbout.ToolTipText = "バージョン情報";
        // 
        // txtMain
        //
        this.txtMain.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtMain.Location = new System.Drawing.Point(0, 25);
        this.txtMain.Name = "txtMain";
        this.txtMain.Size = new System.Drawing.Size(800, 425);
        this.txtMain.TabIndex = 1;
        this.txtMain.Text = "";
        this.txtMain.DetectUrls = true;
        this.txtMain.HideSelection = false;
        // 
        // MainForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 600);
        this.Controls.Add(this.txtMain);
        this.Controls.Add(this.toolStrip1);
        this.Name = "MainForm";
        this.Text = "Flash Editor";
        this.toolStrip1.ResumeLayout(false);
        this.toolStrip1.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ToolStrip toolStrip1;
    private System.Windows.Forms.ToolStripButton tsbNewMemo;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator0;
    private System.Windows.Forms.ToolStripButton tsbTopMost;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    private System.Windows.Forms.ToolStripButton tsbUndo;
    private System.Windows.Forms.ToolStripButton tsbRedo;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    private System.Windows.Forms.ToolStripButton tsbCut;
    private System.Windows.Forms.ToolStripButton tsbCopy;
    private System.Windows.Forms.ToolStripButton tsbPaste;
    private System.Windows.Forms.ToolStripButton tsbDelete;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
    private System.Windows.Forms.ToolStripButton tsbFind;
    private System.Windows.Forms.ToolStripButton tsbGoogleSearch;
    private System.Windows.Forms.ToolStripButton tsbSettings;
    private System.Windows.Forms.ToolStripButton tsbAbout;
    private System.Windows.Forms.RichTextBox txtMain;
}
