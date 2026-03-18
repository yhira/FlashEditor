using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using Timer = System.Windows.Forms.Timer;

namespace FlashEditor;

public partial class MainForm : Form
{
    private readonly AppData _appData = new();
    private readonly Timer _autoSaveTimer = new();
    private readonly Timer _tooltipTimer = new();
    private readonly ToolTip _customToolTip = new();
    private ToolStripItem? _hoveredItem;
    
    // Search Bar Components
    private Panel pnlSearchBar = null!;
    private ToolStrip tsSearch = null!;
    private ToolStripTextBox txtSearchInput = null!;
    private ToolStripButton btnFindPrev = null!;
    private ToolStripButton btnFindNext = null!;
    private ToolStripButton btnMatchCase = null!;
    private ToolStripButton btnWholeWord = null!;
    private ToolStripLabel lblSearchMessage = null!;
    private ToolStripButton btnCloseSearch = null!;

    // Win32 API: RichTextBox 内部のテキスト描画領域を設定するために使用
    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref RECT lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left, Top, Right, Bottom; }

    // テキスト描画領域の設定用メッセージ定数
    private const int EM_SETRECT = 0xB3;
    // エディター内側の余白（ピクセル）
    private const int EditorMargin = 8;

    /// <summary>
    /// RichTextBox 内部のテキスト描画領域にマージンを設定する
    /// </summary>
    private void UpdateEditorMargin()
    {
        // ハンドルが作成されていない場合はスキップ
        if (!txtMain.IsHandleCreated) return;

        var rect = new RECT
        {
            Left   = EditorMargin,
            Top    = EditorMargin,
            Right  = txtMain.ClientSize.Width  - EditorMargin,
            Bottom = txtMain.ClientSize.Height - EditorMargin,
        };
        SendMessage(txtMain.Handle, EM_SETRECT, IntPtr.Zero, ref rect);
    }

    public MainForm()
    {
        // 起動時の白画面チラつき防止のため、初期化処理開始時は完全に透明にしておく
        this.Opacity = 0;

        InitializeComponent();
        InitializeSearchBar();
        
        // エディターの枠線を消してマージン領域をシームレスにする
        txtMain.BorderStyle = BorderStyle.None;

        // アイコン生成（設定値に基づくサイズで表示、描画は20x20座標をスケーリング）
        int iconSize = _appData.Config.GetToolButtonPixelSize();
        toolStrip1.ImageScalingSize = new Size(iconSize, iconSize);
        toolStrip1.Renderer = new CustomToolStripRenderer(); // カスタムレンダラー適用 (無効時の表示変更)
        // ToolStripの左端グリップを非表示
        toolStrip1.GripStyle = ToolStripGripStyle.Hidden;

        // ToolStrip標準のツールチップ表示を無効化し、カスタムツールチップを初期化
        toolStrip1.ShowItemToolTips = false;
        _customToolTip.OwnerDraw = true;
        _customToolTip.Draw += CustomToolTip_Draw;
        _customToolTip.Popup += CustomToolTip_Popup;
        
        _tooltipTimer.Interval = 500;
        _tooltipTimer.Tick += TooltipTimer_Tick;

        foreach (ToolStripItem item in toolStrip1.Items)
        {
            item.MouseEnter += ToolStripItem_MouseEnter;
            item.MouseLeave += ToolStripItem_MouseLeave;
            
            // MouseDownでToolTipを消す（クリック後に出残りするのを防ぐ）
            item.MouseDown += (s, e) => {
                _tooltipTimer.Stop();
                _customToolTip.Hide(toolStrip1);
            };
        }

        // ToolStripの上下パディングを追加してアイコンを大きく見せる
        toolStrip1.Padding = new Padding(4, 2, 4, 2);
        // テーマ適用 (初期化時にシステム設定を見る)
        ApplyTheme(ThemeManager.GetSystemTheme());

        // コンテキストメニュー構築
        BuildContextMenu();

        // イベントハンドラ設定
        this.Load += MainForm_Load;
        this.Shown += MainForm_Shown;
        this.FormClosing += MainForm_FormClosing;
        
        tsbNewMemo.Click += TsbNewMemo_Click;
        tsbTopMost.Click += TsbTopMost_Click;
        tsbUndo.Click += TsbUndo_Click;
        tsbRedo.Click += TsbRedo_Click;
        
        tsbCut.Click += (s, e) => txtMain.Cut();
        tsbCopy.Click += (s, e) => txtMain.Copy();
        tsbPaste.Click += (s, e) => PastePlainText();
        tsbDelete.Click += (s, e) => DeleteSelectedText();
        
        tsbFind.Click += (s, e) => ShowSearchBar();
        tsbGoogleSearch.Click += TsbGoogleSearch_Click;
        tsbSettings.Click += TsbSettings_Click;
        tsbAbout.Click += TsbAbout_Click;

        txtMain.TextChanged += TxtMain_TextChanged;
        txtMain.LinkClicked += TxtMain_LinkClicked;
        // 選択状態が変わったらボタンの有効/無効を更新
        txtMain.SelectionChanged += (s, e) => UpdateSelectionButtons();
        // リサイズ時にマージンを再適用
        txtMain.SizeChanged += (s, e) => UpdateEditorMargin();
        // Ctrl+Vの装飾付き貼り付けを抑制してプレーンテキスト貼り付けにする
        txtMain.KeyDown += TxtMain_KeyDown;

        // メモ自動保存用タイマー (3秒間無操作でメモをファイルに保存)
        _autoSaveTimer.Interval = 3000;
        _autoSaveTimer.Tick += AutoSaveTimer_Tick;

        // システムのテーマ変更を監視するなら WndProc で WM_SETTINGCHANGE をフックする必要があるが
        // 簡易的に今回は起動時のみ、あるいは設定画面での切り替えとする

        // ドラッグ＆ドロップ有効化
        txtMain.EnableAutoDragDrop = true;

        // フォームの最小サイズを設定（横600px × 縦300px）
        this.MinimumSize = new Size(600, 300);
    }

    // コンテキストメニューを再構築する（言語変更時などにも呼ぶ）
    private void BuildContextMenu()
    {
        var contextMenu = new ContextMenuStrip();
        // ToolStrip と同じカスタムレンダラーを適用してダークテーマに対応
        contextMenu.Renderer = new CustomToolStripRenderer();
        ((ToolStripMenuItem)contextMenu.Items.Add(LocalizationManager.GetString("Menu_Cut") ?? "Cut", CreateIcon(DrawCutIcon, 16), (s, e) => txtMain.Cut())).ShortcutKeys = Keys.Control | Keys.X;
        ((ToolStripMenuItem)contextMenu.Items.Add(LocalizationManager.GetString("Menu_Copy") ?? "Copy", CreateIcon(DrawCopyIcon, 16), (s, e) => txtMain.Copy())).ShortcutKeys = Keys.Control | Keys.C;
        ((ToolStripMenuItem)contextMenu.Items.Add(LocalizationManager.GetString("Menu_Paste") ?? "Paste", CreateIcon(DrawPasteIcon, 16), (s, e) => PastePlainText())).ShortcutKeys = Keys.Control | Keys.V;
        // Delキーの標準機能（1文字削除）を妨害しないよう、ShortcutKeysの代わりにショートカットの表示名だけを設定します
        ((ToolStripMenuItem)contextMenu.Items.Add(LocalizationManager.GetString("Menu_Delete") ?? "Delete", CreateIcon(DrawDeleteIcon, 16), (s, e) => DeleteSelectedText())).ShortcutKeyDisplayString = "Del";
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(LocalizationManager.GetString("Menu_GoogleSearch") ?? "Search with Google", CreateIcon(DrawSearchIcon, 16), TsbGoogleSearch_Click);
        contextMenu.Items.Add(new ToolStripSeparator());
        // すべて選択（ローカライズ対応）
        ((ToolStripMenuItem)contextMenu.Items.Add(LocalizationManager.GetString("Menu_SelectAll") ?? "Select All", null, (s, e) => txtMain.SelectAll())).ShortcutKeys = Keys.Control | Keys.A;
        
        txtMain.ContextMenuStrip = contextMenu;
    }

    // UIのテキストを現在の言語に更新する
    private void ApplyLanguage()
    {
        tsbNewMemo.Text = LocalizationManager.GetString("Menu_NewMemo") ?? "New Memo";
        tsbNewMemo.ToolTipText = LocalizationManager.GetString("Menu_NewMemo") ?? "New Memo";

        tsbTopMost.Text = LocalizationManager.GetString("Menu_TopMost") ?? "Always on Top";
        tsbTopMost.ToolTipText = LocalizationManager.GetString("Menu_TopMost") ?? "Always on Top";

        tsbUndo.Text = LocalizationManager.GetString("Menu_Undo") ?? "Undo";
        tsbUndo.ToolTipText = (LocalizationManager.GetString("Menu_Undo") ?? "Undo") + " (Ctrl+Z)";

        tsbRedo.Text = LocalizationManager.GetString("Menu_Redo") ?? "Redo";
        tsbRedo.ToolTipText = (LocalizationManager.GetString("Menu_Redo") ?? "Redo") + " (Ctrl+Y)";

        tsbCut.Text = LocalizationManager.GetString("Menu_Cut") ?? "Cut";
        tsbCut.ToolTipText = (LocalizationManager.GetString("Menu_Cut") ?? "Cut") + " (Ctrl+X)";

        tsbCopy.Text = LocalizationManager.GetString("Menu_Copy") ?? "Copy";
        tsbCopy.ToolTipText = (LocalizationManager.GetString("Menu_Copy") ?? "Copy") + " (Ctrl+C)";

        tsbPaste.Text = LocalizationManager.GetString("Menu_Paste") ?? "Paste";
        tsbPaste.ToolTipText = (LocalizationManager.GetString("Menu_Paste") ?? "Paste") + " (Ctrl+V)";

        tsbDelete.Text = LocalizationManager.GetString("Menu_Delete") ?? "Delete";
        tsbDelete.ToolTipText = (LocalizationManager.GetString("Menu_Delete") ?? "Delete") + " (Delete)";

        tsbFind.Text = LocalizationManager.GetString("Menu_Find") ?? "Find";
        tsbFind.ToolTipText = (LocalizationManager.GetString("Menu_Find") ?? "Find") + " (Ctrl+F)";

        tsbGoogleSearch.Text = LocalizationManager.GetString("Menu_GoogleSearch") ?? "Search with Google";
        tsbGoogleSearch.ToolTipText = LocalizationManager.GetString("Menu_GoogleSearch") ?? "Search with Google";

        tsbSettings.Text = LocalizationManager.GetString("Menu_Settings") ?? "Settings";
        tsbSettings.ToolTipText = LocalizationManager.GetString("Menu_Settings") ?? "Settings";

        tsbAbout.Text = LocalizationManager.GetString("Menu_About") ?? "About";
        tsbAbout.ToolTipText = LocalizationManager.GetString("Menu_About") ?? "About";

        if (btnFindPrev != null)
        {
            btnFindPrev.ToolTipText = LocalizationManager.GetString("Search_Prev") ?? "前の候補 (Shift+Enter)";
            btnFindNext.ToolTipText = LocalizationManager.GetString("Search_Next") ?? "次の候補 (Enter)";
            btnMatchCase.ToolTipText = LocalizationManager.GetString("Search_MatchCase") ?? "大文字と小文字を区別する";
            btnWholeWord.ToolTipText = LocalizationManager.GetString("Search_WholeWord") ?? "単語単位で検索する";
            btnCloseSearch.ToolTipText = LocalizationManager.GetString("Search_Close") ?? "閉じる (Esc)";
        }

        BuildContextMenu();
    }

    // テーマを適用する。引数に null が渡された場合は設定ファイルから解決する。
    private void ApplyTheme(ThemeManager.ThemeMode? forcedMode = null)
    {
        // 強制指定がなければ設定値から解決
        ThemeManager.ThemeMode mode = forcedMode ?? ThemeManager.Resolve(_appData.Config.Theme);

        ThemeManager.ApplyTheme(this, mode);
        GenerateIcons(mode);
    }

    // テーマに応じたアウトライン色を取得する（モノラインアイコン用）
    private static Color GetOutlineColor(ThemeManager.ThemeMode mode)
    {
        bool isDark = (mode == ThemeManager.ThemeMode.Dark);
        return isDark ? Color.FromArgb(176, 176, 176) : Color.FromArgb(80, 80, 80);
    }

    private void GenerateIcons(ThemeManager.ThemeMode mode = ThemeManager.ThemeMode.Light)
    {
        // テーマに応じた共通アウトライン色（モノライン統一）
        Color outline = GetOutlineColor(mode);
        // 統一ストローク幅
        const float sw = 1.5f;

        // 古いアイコンリソースをすべて解放してGDIリークを防ぐ
        tsbNewMemo.Image?.Dispose();
        tsbUndo.Image?.Dispose();
        tsbRedo.Image?.Dispose();
        tsbCut.Image?.Dispose();
        tsbCopy.Image?.Dispose();
        tsbPaste.Image?.Dispose();
        tsbDelete.Image?.Dispose();
        tsbFind.Image?.Dispose();
        tsbGoogleSearch.Image?.Dispose();
        tsbSettings.Image?.Dispose();
        tsbAbout.Image?.Dispose();

        btnFindPrev.Image?.Dispose();
        btnFindNext.Image?.Dispose();
        btnMatchCase.Image?.Dispose();
        btnWholeWord.Image?.Dispose();
        btnCloseSearch.Image?.Dispose();

        // === 新しいメモ (紙 + 「+」マーク) ===
        tsbNewMemo.Image = CreateIcon(g => {
            using var pen = new Pen(outline, sw);
            // 紙（折り付き）
            g.DrawLine(pen, 4, 1, 13, 1);
            g.DrawLine(pen, 13, 1, 16, 4);
            g.DrawLine(pen, 16, 4, 16, 18);
            g.DrawLine(pen, 16, 18, 4, 18);
            g.DrawLine(pen, 4, 18, 4, 1);
            g.DrawLine(pen, 13, 1, 13, 4);
            g.DrawLine(pen, 13, 4, 16, 4);
            // テキスト行
            g.DrawLine(pen, 6, 7, 14, 7);
            g.DrawLine(pen, 6, 10, 14, 10);
            // 「+」マーク
            g.DrawLine(pen, 10, 13, 10, 17);
            g.DrawLine(pen, 8, 15, 12, 15);
        });

        // === ピン (Push Pin) ===
        GeneratePinIcon(outline);

        // === 元に戻す (Undo - 左回り弧矢印) ===
        tsbUndo.Image = CreateIcon(g => {
            using var pen = new Pen(outline, sw);
            // 弧 (右下から左上へ向かう)
            g.DrawArc(pen, 5, 4, 10, 10, 225, 270);
            // 矢印の先端 (左上の終点付近へ向かう)
            g.DrawLine(pen, 5, 8.5f, 2, 7);
            g.DrawLine(pen, 5, 8.5f, 4, 12);
        });

        // === やり直し (Redo - 右回り弧矢印) ===
        tsbRedo.Image = CreateIcon(g => {
            using var pen = new Pen(outline, sw);
            // 弧 (左下から右上へ向かう) - Undoと鏡写し
            g.DrawArc(pen, 5, 4, 10, 10, 45, 270);
            // 矢印の先端 (右上の終点付近へ向かう)
            g.DrawLine(pen, 15, 8.5f, 18, 7);
            g.DrawLine(pen, 15, 8.5f, 16, 12);
        });

        // === はさみ (Cut) ===
        tsbCut.Image = CreateIcon(g => {
            using var pen = new Pen(outline, sw);
            // 左の円（持ち手）
            g.DrawEllipse(pen, 2, 12, 5, 5);
            // 右の円（持ち手）
            g.DrawEllipse(pen, 13, 12, 5, 5);
            // 刃: 左持ち手→右上へ
            g.DrawLine(pen, 5, 13, 14, 3);
            // 刃: 右持ち手→左上へ
            g.DrawLine(pen, 15, 13, 6, 3);
        });

        // === コピー (2枚の四角) ===
        tsbCopy.Image = CreateIcon(g => {
            using var pen = new Pen(outline, sw);
            // 背面の四角
            g.DrawRectangle(pen, 7, 2, 10, 12);
            // 前面の四角
            g.DrawRectangle(pen, 3, 6, 10, 12);
        });

        // === 貼り付け (クリップボード) ===
        tsbPaste.Image = CreateIcon(g => {
            using var pen = new Pen(outline, sw);
            // ボード
            g.DrawRectangle(pen, 3, 4, 14, 15);
            // クリップ
            g.DrawRectangle(pen, 7, 2, 6, 3);
            // 紙の行
            g.DrawLine(pen, 6, 10, 14, 10);
            g.DrawLine(pen, 6, 13, 14, 13);
            g.DrawLine(pen, 6, 16, 11, 16);
        });

        // === 削除 (× 印) ===
        tsbDelete.Image = CreateIcon(g => {
            using var pen = new Pen(outline, sw);
            g.DrawLine(pen, 4, 4, 16, 16);
            g.DrawLine(pen, 16, 4, 4, 16);
        });

        // === 検索 (虫眼鏡) ===
        tsbFind.Image = CreateIcon(g => {
            using var pen = new Pen(outline, sw);
            g.DrawEllipse(pen, 4, 4, 8, 8);
            g.DrawLine(pen, 10, 10, 15, 15);
        });

        // === 検索パネル用アイコン（20x20ネイティブサイズで生成、スケーリング不要） ===
        btnFindPrev.Image = CreateIcon(g => {
            using var pen = new Pen(outline, sw);
            g.DrawLine(pen, 10, 4, 4, 10);
            g.DrawLine(pen, 10, 4, 16, 10);
            g.DrawLine(pen, 10, 4, 10, 16);
        });
        btnFindNext.Image = CreateIcon(g => {
            using var pen = new Pen(outline, sw);
            g.DrawLine(pen, 10, 16, 4, 10);
            g.DrawLine(pen, 10, 16, 16, 10);
            g.DrawLine(pen, 10, 16, 10, 4);
        });
        // 大文字/小文字アイコン: "A" と "a" をペン描画
        btnMatchCase.Image = CreateIcon(g => {
            using var pen = new Pen(outline, 1.8f);
            // 大文字 A（左半分）: 三角形+横棒
            g.DrawLine(pen, 1, 18, 6, 2);
            g.DrawLine(pen, 6, 2, 11, 18);
            g.DrawLine(pen, 3, 12, 9, 12);
            // 小文字 a（右半分）: 丸+縦線
            g.DrawArc(pen, 11, 9, 7, 9, 0, 360);
            g.DrawLine(pen, 18, 9, 18, 18);
        });
        // 単語単位アイコン: "" をペン描画
        btnWholeWord.Image = CreateIcon(g => {
            using var pen = new Pen(outline, 2.0f);
            // 左の " 記号
            g.DrawLine(pen, 4, 4, 3, 11);
            g.DrawLine(pen, 8, 4, 7, 11);
            // 右の " 記号
            g.DrawLine(pen, 12, 4, 11, 11);
            g.DrawLine(pen, 16, 4, 15, 11);
        });
        btnCloseSearch.Image = CreateIcon(g => {
            using var pen = new Pen(outline, sw);
            g.DrawLine(pen, 5, 5, 15, 15);
            g.DrawLine(pen, 15, 5, 5, 15);
        });

        // === Googleで検索 (Googleロゴ - 4色の「G」) ===
        tsbGoogleSearch.Image = CreateIcon(g => {
            Color blue   = Color.FromArgb(66, 133, 244);
            Color red    = Color.FromArgb(234, 67, 53);
            Color yellow = Color.FromArgb(251, 188, 5);
            Color green  = Color.FromArgb(52, 168, 83);
            using var penBlue   = new Pen(blue, 2.5f);
            using var penRed    = new Pen(red, 2.5f);
            using var penYellow = new Pen(yellow, 2.5f);
            using var penGreen  = new Pen(green, 2.5f);
            var rect = new Rectangle(2, 2, 16, 16);
            g.DrawArc(penBlue,   rect, 300, 75);
            g.DrawArc(penRed,    rect, 225, 75);
            g.DrawArc(penYellow, rect, 150, 75);
            g.DrawArc(penGreen,  rect, 75, 75);
            // 「G」の横棒（青）
            g.DrawLine(penBlue, 10, 10, 17, 10);
        });

        // === 設定 (歯車) ===
        tsbSettings.Image = CreateIcon(g => {
            using var pen = new Pen(outline, sw);
            int cx = 10, cy = 10, outerR = 8, innerR = 6;
            int teethCount = 8;
            var pts = new System.Collections.Generic.List<PointF>();
            for (int i = 0; i < teethCount; i++)
            {
                double a1 = Math.PI * 2 * i / teethCount - Math.PI / teethCount * 0.5;
                double a2 = Math.PI * 2 * i / teethCount + Math.PI / teethCount * 0.5;
                double aMid1 = (a1 + a2) / 2 - 0.15;
                double aMid2 = (a1 + a2) / 2 + 0.15;
                pts.Add(new PointF(cx + (float)(Math.Cos(a1) * innerR), cy + (float)(Math.Sin(a1) * innerR)));
                pts.Add(new PointF(cx + (float)(Math.Cos(aMid1) * outerR), cy + (float)(Math.Sin(aMid1) * outerR)));
                pts.Add(new PointF(cx + (float)(Math.Cos(aMid2) * outerR), cy + (float)(Math.Sin(aMid2) * outerR)));
                pts.Add(new PointF(cx + (float)(Math.Cos(a2) * innerR), cy + (float)(Math.Sin(a2) * innerR)));
            }
            g.DrawPolygon(pen, pts.ToArray());
            // 中央の穴
            g.DrawEllipse(pen, cx - 3, cy - 3, 6, 6);
        });

        // === バージョン情報 (i マーク) ===
        tsbAbout.Image = CreateIcon(g => {
            using var pen = new Pen(outline, sw);
            // 円
            g.DrawEllipse(pen, 2, 2, 16, 16);
            // i の上部の点
            g.DrawLine(pen, 10, 5, 10, 5.5f);
            using var fillBrush = new SolidBrush(outline);
            g.FillEllipse(fillBrush, 9f, 4.5f, 2f, 2f);
            // i の下の線
            g.DrawLine(pen, 10, 9, 10, 14);
            g.DrawLine(pen, 8.5f, 14, 11.5f, 14);
            g.DrawLine(pen, 8.5f, 9, 10, 9);
        });
    }

    // ピンアイコン生成 (TopMost状態に応じて傾き変更) - モノライン
    private void GeneratePinIcon(Color outline)
    {
        bool tilted = tsbTopMost.Checked;
        const float sw = 1.5f;

        // 古いアイコンリソースを解放してGDIリークを防ぐ
        tsbTopMost.Image?.Dispose();

        tsbTopMost.Image = CreateIcon(g => {
            float angle = tilted ? 45f : 0f;
            var state = g.Save();
            g.TranslateTransform(10, 10);
            g.RotateTransform(angle);
            g.TranslateTransform(-10, -10);

            using var pen = new Pen(outline, sw);
            // 頭（丸）
            g.DrawEllipse(pen, 6, 1, 8, 5);
            // 首
            g.DrawLine(pen, 8, 6, 8, 10);
            g.DrawLine(pen, 12, 6, 12, 10);
            // 襟（横棒）
            g.DrawLine(pen, 5, 10, 15, 10);
            // 針
            g.DrawLine(pen, 10, 10, 10, 18);

            g.Restore(state);
        });
    }

    // 選択状態に応じてボタンの有効/無効を更新
    private void UpdateSelectionButtons()
    {
        bool hasSelection = !string.IsNullOrEmpty(txtMain.SelectedText);
        tsbCut.Enabled = hasSelection;
        tsbCopy.Enabled = hasSelection;
        tsbDelete.Enabled = hasSelection;
        tsbGoogleSearch.Enabled = hasSelection;

        // クリップボードにテキストがあるかチェック
        bool hasClipboardText = Clipboard.ContainsText();
        tsbPaste.Enabled = hasClipboardText;
    }

    // 指定サイズのアイコンを生成する（デフォルトは設定値サイズ、コンテキストメニュー用は16x16）
    // ToolStripアイコンは20x20座標で描画し、設定サイズにスケーリングして鮮明に表示
    private Image CreateIcon(Action<Graphics> drawAction, int size = -1)
    {
        // サイズ未指定(-1)の場合は設定値から取得
        if (size < 0) size = _appData.Config.GetToolButtonPixelSize();
        var bmp = new Bitmap(size, size);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        // 20x20座標のアイコンをサイズに合わせてスケーリング（拡大・縮小の両方に対応）
        if (size != 20)
        {
            float scale = size / 20f;
            g.ScaleTransform(scale, scale);
        }
        drawAction(g);
        return bmp;
    }

    // === コンテキストメニュー用アイコン (16x16, モノライン統一) ===

    // コンテキストメニュー用アイコンのテーマ対応色を取得
    private static Color GetContextMenuIconColor()
    {
        bool isDark = (ThemeManager.CurrentTheme == ThemeManager.ThemeMode.Dark);
        return isDark ? Color.FromArgb(180, 180, 180) : Color.FromArgb(100, 100, 100);
    }

    // 切り取り (はさみ)
    private void DrawCutIcon(Graphics g)
    {
        using var pen = new Pen(GetContextMenuIconColor(), 1.2f);
        g.DrawEllipse(pen, 1, 9, 4, 4);
        g.DrawEllipse(pen, 10, 9, 4, 4);
        g.DrawLine(pen, 3, 10, 11, 2);
        g.DrawLine(pen, 12, 10, 4, 2);
    }

    // コピー (2枚の四角)
    private void DrawCopyIcon(Graphics g)
    {
        using var pen = new Pen(GetContextMenuIconColor(), 1.2f);
        g.DrawRectangle(pen, 5, 1, 8, 10);
        g.DrawRectangle(pen, 2, 4, 8, 10);
    }

    // 貼り付け (クリップボード)
    private void DrawPasteIcon(Graphics g)
    {
        using var pen = new Pen(GetContextMenuIconColor(), 1.2f);
        g.DrawRectangle(pen, 2, 3, 11, 12);
        g.DrawRectangle(pen, 5, 1, 5, 3);
        g.DrawLine(pen, 5, 8, 11, 8);
        g.DrawLine(pen, 5, 11, 11, 11);
    }

    // 削除 (× 印)
    private void DrawDeleteIcon(Graphics g)
    {
        using var pen = new Pen(GetContextMenuIconColor(), 1.2f);
        g.DrawLine(pen, 3, 3, 13, 13);
        g.DrawLine(pen, 13, 3, 3, 13);
    }

    // Googleで検索 (Googleロゴ)
    private void DrawSearchIcon(Graphics g)
    {
        using var penB = new Pen(Color.FromArgb(66, 133, 244), 1.8f);
        using var penR = new Pen(Color.FromArgb(234, 67, 53), 1.8f);
        using var penY = new Pen(Color.FromArgb(251, 188, 5), 1.8f);
        using var penG = new Pen(Color.FromArgb(52, 168, 83), 1.8f);
        var rect = new Rectangle(1, 1, 13, 13);
        g.DrawArc(penB, rect, 300, 75);
        g.DrawArc(penR, rect, 225, 75);
        g.DrawArc(penY, rect, 150, 75);
        g.DrawArc(penG, rect, 75, 75);
        g.DrawLine(penB, 8, 7, 14, 7);
    }

    // --- カスタムツールチップのイベント処理 ---

    private void ToolStripItem_MouseEnter(object? sender, EventArgs e)
    {
        if (sender is ToolStripItem item && !string.IsNullOrEmpty(item.ToolTipText))
        {
            _hoveredItem = item;
            _tooltipTimer.Stop();
            _tooltipTimer.Start();
        }
    }

    private void ToolStripItem_MouseLeave(object? sender, EventArgs e)
    {
        _tooltipTimer.Stop();
        if (sender is ToolStripItem item && item.Owner != null)
        {
            _customToolTip.Hide(item.Owner);
        }
        else
        {
            _customToolTip.Hide(toolStrip1);
        }
        _hoveredItem = null;
    }

    private void TooltipTimer_Tick(object? sender, EventArgs e)
    {
        _tooltipTimer.Stop();
        if (_hoveredItem != null)
        {
            var owner = _hoveredItem.Owner ?? toolStrip1;
            // ToolStrip内での位置の下に表示
            Point pt = new Point(_hoveredItem.Bounds.Left, _hoveredItem.Bounds.Bottom + 4);
            _customToolTip.Show(_hoveredItem.ToolTipText, owner, pt);
        }
    }

    private void CustomToolTip_Popup(object? sender, PopupEventArgs e)
    {
        // ツールチップのサイズに余白を持たせる
        e.ToolTipSize = new Size(e.ToolTipSize.Width + 12, e.ToolTipSize.Height + 8);
    }

    private void CustomToolTip_Draw(object? sender, DrawToolTipEventArgs e)
    {
        bool isDark = (ThemeManager.CurrentTheme == ThemeManager.ThemeMode.Dark);

        Color backColor = isDark ? Color.FromArgb(43, 43, 43) : SystemColors.Info;
        Color foreColor = isDark ? Color.WhiteSmoke : SystemColors.InfoText;
        Color borderColor = isDark ? Color.FromArgb(100, 100, 100) : SystemColors.WindowFrame;

        // 背景描画
        using var bgBrush = new SolidBrush(backColor);
        e.Graphics.FillRectangle(bgBrush, e.Bounds);

        // 枠線描画
        using var borderPen = new Pen(borderColor);
        e.Graphics.DrawRectangle(borderPen, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);

        // テキスト描画 (余白を考慮して中央揃え)
        using var textBrush = new SolidBrush(foreColor);
        using var sf = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        Font font = e.Font ?? SystemFonts.DefaultFont;
        e.Graphics.DrawString(e.ToolTipText, font, textBrush, e.Bounds, sf);
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        // 起動時の白枠チラつき（ホワイトフラッシュ）防止のため、
        // 描画と初期化がすべて完了したこのタイミングで不透明度を 100% に戻す
        this.Opacity = 1.0;
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        _appData.Load();
        
        // 言語データを読み込んでUIに適用
        LocalizationManager.LoadLanguage(_appData.Config.Language);
        ApplyLanguage();

        // ウィンドウ復元
        // 画面外に行かないように簡易チェック
        var screen = Screen.FromPoint(new Point(_appData.Config.WindowX, _appData.Config.WindowY));
        var bounds = screen.WorkingArea;
        
        int x = _appData.Config.WindowX;
        int y = _appData.Config.WindowY;
        int w = _appData.Config.WindowWidth;
        int h = _appData.Config.WindowHeight;

        // ウィンドウサイズが画面より大きい場合はクランプ
        if (w > bounds.Width) w = bounds.Width;
        if (h > bounds.Height) h = bounds.Height;

        // 右端・下端からはみ出す場合は押し戻す
        if (x + w > bounds.Right) x = bounds.Right - w;
        if (y + h > bounds.Bottom) y = bounds.Bottom - h;

        // 左端・上端からはみ出す場合は押し戻す
        if (x < bounds.Left) x = bounds.Left;
        if (y < bounds.Top) y = bounds.Top;

        this.Location = new Point(x, y);
        this.Size = new Size(w, h);

        // 設定反映
        ApplySettings();

        // テキスト復元
        txtMain.Text = _appData.MemoContent;
        // 初期ロードの「元に戻す」を無効にする（ファイル読み込みをUndoさせない）
        txtMain.ClearUndo();
        // 描画完了後にマージン設定とキャレットを確実に先頭へ移動し、フォーカスを当てて点滅させる
        this.BeginInvoke(new Action(() =>
        {
            UpdateEditorMargin();
            txtMain.Select(0, 0);
            txtMain.ScrollToCaret();
            txtMain.Focus();
        }));
        
        UpdateUndoRedoButtons();
        // 選択状態ボタンの初期化
        UpdateSelectionButtons();

        // タイトルバーアイコン設定 (実行ファイルと同じ .ico を使用)
        try
        {
            string icoPath = Path.Combine(AppContext.BaseDirectory, "FlashEditor.ico");
            if (File.Exists(icoPath))
            {
                this.Icon = new Icon(icoPath);
            }
        }
        catch (Exception ex) { AppData.ReportError(LocalizationManager.GetString("Error_IconLoad") ?? "Failed to load icon file", ex); }
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        // 自動保存タイマーを停止（Save()との書き込み競合を防止）
        _autoSaveTimer.Stop();

        // 状態保存
        _appData.Config.WindowX = this.Location.X;
        _appData.Config.WindowY = this.Location.Y;
        _appData.Config.WindowWidth = this.Size.Width;
        _appData.Config.WindowHeight = this.Size.Height;
        _appData.Config.IsTopMost = this.TopMost;
        
        // Fontは変更時に保存しているのでここではサイズなど
        // (SettingsDialogで変更しているので同期不要だが念のため)
        
        _appData.MemoContent = txtMain.Text;

        _appData.Save();
    }

    private void ApplySettings()
    {
        // 新しいフォントを生成
        var newFont = _appData.Config.GetFont();
        // 同一オブジェクトでない場合のみ差し替える
        if (txtMain.Font != newFont)
        {
            var oldFont = txtMain.Font;
            txtMain.Font = newFont;
            // フォームのデフォルトフォントと同一参照のものはDisposeしない（破壊するとフォーム全体がクラッシュする）
            if (oldFont != this.Font)
                oldFont?.Dispose();
        }

        this.TopMost = _appData.Config.IsTopMost;
        tsbTopMost.Checked = this.TopMost;

        // ツールボタンサイズを反映
        int iconSize = _appData.Config.GetToolButtonPixelSize();
        toolStrip1.ImageScalingSize = new Size(iconSize, iconSize);

        // 保存された設定に基づいてテーマを適用（アイコンも再生成される）
        ApplyTheme();
    }

    private void TsbNewMemo_Click(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        // 未知の言語コードでも安全にフォールバック
        System.Globalization.CultureInfo culture;
        try
        {
            culture = new System.Globalization.CultureInfo(LocalizationManager.CurrentLanguage);
        }
        catch
        {
            culture = System.Globalization.CultureInfo.InvariantCulture;
        }
        string dayOfWeek = culture.DateTimeFormat.GetAbbreviatedDayName(now.DayOfWeek);
        // フォーマット: 2026/02/17 Sat 0:04:41
        string dateHeader = now.ToString("yyyy/MM/dd") + $" {dayOfWeek} " + now.ToString("H:mm:ss");
        // 40文字の罫線
        string separator = new string('-', 40);
        // 冒頭に挿入するヘッダー (前に空行 + 日付 + 罫線 + 空行)
        string header = $"\n\n{dateHeader}\n{separator}\n";

        // テキストの先頭に挿入
        txtMain.SelectionStart = 0;
        txtMain.SelectionLength = 0;
        txtMain.SelectedText = header;

        // キャレットを一番先頭に移動
        txtMain.SelectionStart = 0;
        txtMain.SelectionLength = 0;
        txtMain.ScrollToCaret();
    }

    private void TsbTopMost_Click(object? sender, EventArgs e)
    {
        this.TopMost = tsbTopMost.Checked;
        _appData.Config.IsTopMost = this.TopMost;
        // ピンアイコンを状態に応じて切り替え（現在適用中のテーマを使用）
        Color outline = GetOutlineColor(ThemeManager.CurrentTheme);
        GeneratePinIcon(outline);
    }

    private void TsbSettings_Click(object? sender, EventArgs e)
    {
        // 設定ダイアログを表示 (現在のフォント・テーマ・ツールボタンサイズ・言語を渡す)
        using var dlg = new SettingsDialog(txtMain.Font, _appData.Config.Theme, _appData.Config.ToolButtonSize, _appData.Config.Language);
        // TopMostが有効な場合、ダイアログも同じZ-Orderレイヤーに置く
        dlg.TopMost = this.TopMost;
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            _appData.Config.SetFont(dlg.CurrentFont);
            _appData.Config.Theme = dlg.CurrentTheme;
            _appData.Config.ToolButtonSize = dlg.CurrentToolButtonSize;
            
            // 言語が変更された場合は再読み込みと適用を行う
            if (_appData.Config.Language != dlg.CurrentLanguage)
            {
                _appData.Config.Language = dlg.CurrentLanguage;
                LocalizationManager.LoadLanguage(_appData.Config.Language);
                ApplyLanguage(); // メインフォームのUI言語を更新
            }

            // 変更を即座に反映
            ApplySettings();
        }
    }

    private void TsbAbout_Click(object? sender, EventArgs e)
    {
        using var dlg = new AboutDialog();
        // TopMostが有効な場合、ダイアログも同じZ-Orderレイヤーに置く
        dlg.TopMost = this.TopMost;
        dlg.ShowDialog(this);
    }

    // RichTextBox内蔵のUndo機能を使用（Windows標準と同じ細かい粒度で動作）
    private void TsbUndo_Click(object? sender, EventArgs e)
    {
        if (txtMain.CanUndo)
        {
            txtMain.Undo();
            UpdateUndoRedoButtons();
        }
    }

    // RichTextBox内蔵のRedo機能を使用
    private void TsbRedo_Click(object? sender, EventArgs e)
    {
        if (txtMain.CanRedo)
        {
            txtMain.Redo();
            UpdateUndoRedoButtons();
        }
    }

    private void TsbGoogleSearch_Click(object? sender, EventArgs e)
    {
        string text = txtMain.SelectedText;
        if (string.IsNullOrEmpty(text))
        {
            text = txtMain.Text; // 未選択なら全体
        }

        if (!string.IsNullOrWhiteSpace(text))
        {
            try
            {
                string url = "https://www.google.com/search?q=" + Uri.EscapeDataString(text);
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex) { AppData.ReportError(LocalizationManager.GetString("Error_BrowserOpen") ?? "Failed to open browser", ex); }
        }
    }

    private void TxtMain_TextChanged(object? sender, EventArgs e)
    {
        // Undo/Redoボタンの有効無効をRichTextBoxの状態から更新
        UpdateUndoRedoButtons();

        // 自動保存タイマーリセット（3秒間無操作でメモをファイルに保存）
        _autoSaveTimer.Stop();
        _autoSaveTimer.Start();
    }

    // 3秒間操作がなければメモをファイルに自動保存（クラッシュ時のデータ損失を防止）
    private void AutoSaveTimer_Tick(object? sender, EventArgs e)
    {
        _autoSaveTimer.Stop();
        _appData.SaveMemoAsync(txtMain.Text);
    }

    // RichTextBox内蔵のUndo/Redo状態からボタンの有効無効を更新
    private void UpdateUndoRedoButtons()
    {
        tsbUndo.Enabled = txtMain.CanUndo;
        tsbRedo.Enabled = txtMain.CanRedo;
    }

    // --- Search Bar Methods ---
    private void InitializeSearchBar()
    {
        pnlSearchBar = new Panel { Dock = DockStyle.Bottom, Visible = false };
        tsSearch = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, RenderMode = ToolStripRenderMode.Professional, Renderer = new CustomToolStripRenderer(), ImageScalingSize = new Size(20, 20) };
        
        // Disable default tooltips in favor of our customized _tooltipTimer just like toolStrip1
        tsSearch.ShowItemToolTips = false;

        txtSearchInput = new ToolStripTextBox { AutoSize = false, Width = 200, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(8, 4, 4, 4) };
        txtSearchInput.KeyDown += TxtSearchInput_KeyDown;
        txtSearchInput.TextChanged += TxtSearchInput_TextChanged;

        btnFindPrev = new ToolStripButton { DisplayStyle = ToolStripItemDisplayStyle.Image, ToolTipText = LocalizationManager.GetString("Search_Prev") ?? "前の候補 (Shift+Enter)" };
        btnFindNext = new ToolStripButton { DisplayStyle = ToolStripItemDisplayStyle.Image, ToolTipText = LocalizationManager.GetString("Search_Next") ?? "次の候補 (Enter)" };
        btnMatchCase = new ToolStripButton { DisplayStyle = ToolStripItemDisplayStyle.Image, ToolTipText = LocalizationManager.GetString("Search_MatchCase") ?? "大文字と小文字を区別する", CheckOnClick = true };
        btnWholeWord = new ToolStripButton { DisplayStyle = ToolStripItemDisplayStyle.Image, ToolTipText = LocalizationManager.GetString("Search_WholeWord") ?? "単語単位で検索する", CheckOnClick = true };
        lblSearchMessage = new ToolStripLabel { Margin = new Padding(10, 0, 0, 0) };
        btnCloseSearch = new ToolStripButton { DisplayStyle = ToolStripItemDisplayStyle.Image, Alignment = ToolStripItemAlignment.Right, ToolTipText = LocalizationManager.GetString("Search_Close") ?? "閉じる (Esc)" };

        btnFindPrev.Click += (s, e) => FindText(false);
        btnFindNext.Click += (s, e) => FindText(true);
        btnMatchCase.CheckedChanged += (s, e) => FindText(true);
        btnWholeWord.CheckedChanged += (s, e) => FindText(true);
        btnCloseSearch.Click += (s, e) => HideSearchBar();

        tsSearch.Items.AddRange(new ToolStripItem[] {
            txtSearchInput,
            btnFindPrev,
            btnFindNext,
            new ToolStripSeparator(),
            btnMatchCase,
            btnWholeWord,
            lblSearchMessage,
            btnCloseSearch
        });
        
        pnlSearchBar.Height = tsSearch.PreferredSize.Height > 28 ? tsSearch.PreferredSize.Height : 32;

        // Attach custom tooltip logic
        foreach (ToolStripItem item in tsSearch.Items)
        {
            if (item is ToolStripTextBox) continue;
            item.MouseEnter += ToolStripItem_MouseEnter;
            item.MouseLeave += ToolStripItem_MouseLeave;
            item.MouseDown += (s, e) => {
                _tooltipTimer.Stop();
                _customToolTip.Hide(tsSearch);
            };
        }

        pnlSearchBar.Controls.Add(tsSearch);
        this.Controls.Add(pnlSearchBar);
    }

    private void TxtSearchInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            FindText(!e.Shift);
        }
    }

    private void TxtSearchInput_TextChanged(object? sender, EventArgs e)
    {
        FindText(true, true);
    }

    private void FindText(bool searchForward, bool incremental = false)
    {
        lblSearchMessage.Text = "";

        if (string.IsNullOrEmpty(txtSearchInput.Text)) return;

        string searchText = txtSearchInput.Text;
        RichTextBoxFinds options = RichTextBoxFinds.None;
        if (btnMatchCase.Checked) options |= RichTextBoxFinds.MatchCase;
        if (btnWholeWord.Checked) options |= RichTextBoxFinds.WholeWord;
        if (!searchForward) options |= RichTextBoxFinds.Reverse;

        int startIndex = 0;
        int endIndex = txtMain.TextLength;
        
        if (incremental)
        {
            // インクリメンタル検索時は、現在の選択範囲の先頭から開始して飛び跳ねを防ぐ
            startIndex = txtMain.SelectionStart;
        }
        else
        {
            if (searchForward)
                startIndex = txtMain.SelectionStart + txtMain.SelectionLength;
            else
                endIndex = txtMain.SelectionStart;
        }

        int index = -1;
        if (searchForward)
        {
            index = txtMain.Find(searchText, startIndex, endIndex, options);
            if (index == -1 && !incremental) // 見つからなかったら先頭に戻って検索
                index = txtMain.Find(searchText, 0, txtMain.TextLength, options);
        }
        else
        {
            index = txtMain.Find(searchText, 0, endIndex, options);
            if (index == -1) // 見つからなかったら末尾から検索
                index = txtMain.Find(searchText, startIndex, txtMain.TextLength, options);
        }

        if (index != -1)
        {
            txtMain.Select(index, searchText.Length);
            txtMain.ScrollToCaret();

            // 全マッチ件数と現在位置を計算して多言語対応で表示
            var (totalCount, currentIndex) = CountAllMatches(searchText, index);
            string fmt = LocalizationManager.GetString("Search_MatchCount") ?? "{0} 件中 {1} 件目";
            lblSearchMessage.Text = string.Format(fmt, totalCount, currentIndex);
            lblSearchMessage.ForeColor = ThemeManager.CurrentTheme == ThemeManager.ThemeMode.Dark
                ? Color.FromArgb(180, 180, 180) : Color.FromArgb(80, 80, 80);
        }
        else
        {
            lblSearchMessage.Text = LocalizationManager.GetString("Search_NotFound") ?? "見つかりません";
            lblSearchMessage.ForeColor = ThemeManager.CurrentTheme == ThemeManager.ThemeMode.Dark ? Color.LightCoral : Color.Red;
        }
    }

    // テキスト全体を走査して総マッチ数と、現在位置が何番目かを返す
    private (int totalCount, int currentIndex) CountAllMatches(string searchText, int currentPos)
    {
        // 検索オプションを構築（方向は常に前方で全走査）
        RichTextBoxFinds options = RichTextBoxFinds.None;
        if (btnMatchCase.Checked) options |= RichTextBoxFinds.MatchCase;
        if (btnWholeWord.Checked) options |= RichTextBoxFinds.WholeWord;

        int total = 0;
        int currentIndex = 0;
        int pos = 0;

        // 先頭から末尾まで繰り返し検索してカウント
        while (pos < txtMain.TextLength)
        {
            int found = txtMain.Find(searchText, pos, txtMain.TextLength, options);
            if (found == -1) break;
            total++;
            // 現在選択中の位置と一致したら何番目かを記録
            if (found == currentPos) currentIndex = total;
            pos = found + searchText.Length;
        }

        // 選択位置を元に戻す（Find呼び出しで選択がずれるので）
        txtMain.Select(currentPos, searchText.Length);

        return (total, currentIndex);
    }

    private void ShowSearchBar()
    {
        pnlSearchBar.Visible = true;
        pnlSearchBar.BringToFront(); // RichTextBoxの上に持ってくる
        
        if (!string.IsNullOrEmpty(txtMain.SelectedText) && !txtMain.SelectedText.Contains('\n'))
        {
            txtSearchInput.Text = txtMain.SelectedText;
        }
        txtSearchInput.Focus();
        txtSearchInput.SelectAll();
    }

    private void HideSearchBar()
    {
        if (pnlSearchBar != null && pnlSearchBar.Visible)
        {
            pnlSearchBar.Visible = false;
            txtMain.Focus();
        }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.Control | Keys.F))
        {
            ShowSearchBar();
            return true;
        }
        if (keyData == Keys.Escape && pnlSearchBar != null && pnlSearchBar.Visible)
        {
            HideSearchBar();
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void TxtMain_KeyDown(object? sender, KeyEventArgs e)
    {
        // Ctrl+Z / Ctrl+Y はRichTextBox内蔵のUndo/Redoに任せる（インターセプト不要）
        // Ctrl+Vのみ装飾付き貼り付けを抑制してプレーンテキスト貼り付けにする
        if (e.Control && e.KeyCode == Keys.V)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            PastePlainText();
        }
    }

    private void TxtMain_LinkClicked(object? sender, LinkClickedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.LinkText))
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.LinkText) { UseShellExecute = true });
            }
            catch (Exception ex) { AppData.ReportError(LocalizationManager.GetString("Error_LinkOpen") ?? "Could not open link", ex); }
        }
    }

    // 選択中のテキストを削除する
    private void DeleteSelectedText()
    {
        if (!string.IsNullOrEmpty(txtMain.SelectedText))
        {
            txtMain.SelectedText = "";
        }
    }

    // クリップボードからプレーンテキストのみ貼り付け（装飾を除去）
    private void PastePlainText()
    {
        if (Clipboard.ContainsText())
        {
            // プレーンテキストのみ取得して貼り付け
            string text = Clipboard.GetText(TextDataFormat.UnicodeText);
            txtMain.SelectedText = text;
        }
    }
}
