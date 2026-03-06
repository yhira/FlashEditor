using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using Timer = System.Windows.Forms.Timer;

namespace FlashEditor;

public partial class MainForm : Form
{
    private readonly AppData _appData = new();
    private readonly Timer _snapshotTimer = new();
    private bool _isUndoRedoAction = false;

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
        InitializeComponent();
        
        // エディターの枠線を消してマージン領域をシームレスにする
        txtMain.BorderStyle = BorderStyle.None;

        // アイコン生成
        toolStrip1.ImageScalingSize = new Size(32, 32);
        toolStrip1.Renderer = new CustomToolStripRenderer(); // カスタムレンダラー適用 (無効時の表示変更)
        // テーマ適用 (初期化時にシステム設定を見る)
        ApplyTheme(ThemeManager.GetSystemTheme());

        // コンテキストメニュー生成 (アイコン付き)
        var contextMenu = new ContextMenuStrip();
        ((ToolStripMenuItem)contextMenu.Items.Add("切り取り", CreateIcon(DrawCutIcon, 16), (s, e) => txtMain.Cut())).ShortcutKeys = Keys.Control | Keys.X;
        ((ToolStripMenuItem)contextMenu.Items.Add("コピー", CreateIcon(DrawCopyIcon, 16), (s, e) => txtMain.Copy())).ShortcutKeys = Keys.Control | Keys.C;
        ((ToolStripMenuItem)contextMenu.Items.Add("貼り付け", CreateIcon(DrawPasteIcon, 16), (s, e) => PastePlainText())).ShortcutKeys = Keys.Control | Keys.V;
        ((ToolStripMenuItem)contextMenu.Items.Add("削除", CreateIcon(DrawDeleteIcon, 16), (s, e) => DeleteSelectedText())).ShortcutKeys = Keys.Delete;
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("Googleで検索", CreateIcon(DrawSearchIcon, 16), TsbGoogleSearch_Click);
        contextMenu.Items.Add(new ToolStripSeparator());
        ((ToolStripMenuItem)contextMenu.Items.Add("すべて選択", null, (s, e) => txtMain.SelectAll())).ShortcutKeys = Keys.Control | Keys.A;
        
        txtMain.ContextMenuStrip = contextMenu;

        // イベントハンドラ設定
        this.Load += MainForm_Load;
        this.FormClosing += MainForm_FormClosing;
        
        tsbNewMemo.Click += TsbNewMemo_Click;
        tsbTopMost.Click += TsbTopMost_Click;
        tsbUndo.Click += TsbUndo_Click;
        tsbRedo.Click += TsbRedo_Click;
        
        tsbCut.Click += (s, e) => txtMain.Cut();
        tsbCopy.Click += (s, e) => txtMain.Copy();
        tsbPaste.Click += (s, e) => PastePlainText();
        tsbDelete.Click += (s, e) => DeleteSelectedText();
        
        tsbGoogleSearch.Click += TsbGoogleSearch_Click;
        tsbSettings.Click += TsbSettings_Click;

        txtMain.TextChanged += TxtMain_TextChanged;
        txtMain.LinkClicked += TxtMain_LinkClicked;
        // 選択状態が変わったらボタンの有効/無効を更新
        txtMain.SelectionChanged += (s, e) => UpdateSelectionButtons();
        // リサイズ時にマージンを再適用
        txtMain.SizeChanged += (s, e) => UpdateEditorMargin();
        // Ctrl+Vの装飾付き貼り付けを抑制してプレーンテキスト貼り付けにする
        txtMain.KeyDown += TxtMain_KeyDown;

        // 履歴スナップショット用タイマー (3秒間操作がなければ履歴保存)
        _snapshotTimer.Interval = 3000;
        _snapshotTimer.Tick += SnapshotTimer_Tick;

        // システムのテーマ変更を監視するなら WndProc で WM_SETTINGCHANGE をフックする必要があるが
        // 簡易的に今回は起動時のみ、あるいは設定画面での切り替えとする

        // ドラッグ＆ドロップ有効化
        txtMain.EnableAutoDragDrop = true;

        // フォームの最小サイズを設定（横600px × 縦300px）
        this.MinimumSize = new Size(600, 300);
    }

    // テーマを適用する。引数に null が渡された場合は設定ファイルから解決する。
    private void ApplyTheme(ThemeManager.ThemeMode? forcedMode = null)
    {
        // 強制指定がなければ設定値から解決
        ThemeManager.ThemeMode mode = forcedMode ?? ThemeManager.Resolve(_appData.Config.Theme);

        ThemeManager.ApplyTheme(this, mode);
        GenerateIcons(mode);
    }

    // テーマに応じたアウトライン色を取得する
    private static Color GetOutlineColor(ThemeManager.ThemeMode mode)
    {
        bool isDark = (mode == ThemeManager.ThemeMode.Dark);
        return isDark ? Color.FromArgb(200, 200, 200) : Color.FromArgb(60, 60, 60);
    }

    private void GenerateIcons(ThemeManager.ThemeMode mode = ThemeManager.ThemeMode.Light)
    {
        // テーマに応じた共通アウトライン色
        bool isDark = (mode == ThemeManager.ThemeMode.Dark);
        Color outline = GetOutlineColor(mode);

        // === 新しいメモ (紙) ===
        tsbNewMemo.Image = CreateIcon(g => {
            using var pen = new Pen(outline, 1.5f);
            // 紙本体 (白)
            PointF[] paper = {
                new(6, 2), new(22, 2), new(26, 6), new(26, 30), new(6, 30)
            };
            using var paperBrush = new SolidBrush(isDark ? Color.FromArgb(230, 230, 230) : Color.White);
            g.FillPolygon(paperBrush, paper);
            g.DrawPolygon(pen, paper);
            // 折り線 (右上角の折り)
            PointF[] fold = { new(22, 2), new(22, 6), new(26, 6) };
            using var foldBrush = new SolidBrush(Color.FromArgb(180, 200, 230));
            g.FillPolygon(foldBrush, fold);
            g.DrawPolygon(pen, fold);
            // テキスト行
            using var linePen = new Pen(Color.FromArgb(170, 170, 170), 1.2f);
            g.DrawLine(linePen, 9, 12, 23, 12);
            g.DrawLine(linePen, 9, 16, 23, 16);
            g.DrawLine(linePen, 9, 20, 20, 20);
            // 「+」マーク (緑)
            using var plusPen = new Pen(Color.FromArgb(60, 180, 80), 2f);
            g.DrawLine(plusPen, 16, 24, 16, 30);
            g.DrawLine(plusPen, 13, 27, 19, 27);
        });

        // === ピン (Push Pin) - 状態に応じて切り替え ===
        GeneratePinIcon(outline);

        // === 元に戻す (Undo - 左向き矢印) ===
        tsbUndo.Image = CreateIcon(g => {
            // 塗りつぶした矢印
            var arrowFill = Color.FromArgb(70, 130, 220);
            using var fillBrush = new SolidBrush(arrowFill);
            // 矢印の三角 + 弧
            PointF[] arrow = {
                new PointF(6, 16), new PointF(14, 8), new PointF(14, 12),
                new PointF(20, 12), new PointF(22, 14), new PointF(22, 22),
                new PointF(20, 24), new PointF(14, 24), new PointF(14, 20),
                new PointF(14, 20), new PointF(6, 16)
            };
            g.FillPolygon(fillBrush, arrow);
            using var pen = new Pen(outline, 1.5f);
            g.DrawPolygon(pen, arrow);
        });

        // === やり直し (Redo - 右向き矢印) ===
        tsbRedo.Image = CreateIcon(g => {
            var arrowFill = Color.FromArgb(70, 130, 220);
            using var fillBrush = new SolidBrush(arrowFill);
            PointF[] arrow = {
                new PointF(26, 16), new PointF(18, 8), new PointF(18, 12),
                new PointF(12, 12), new PointF(10, 14), new PointF(10, 22),
                new PointF(12, 24), new PointF(18, 24), new PointF(18, 20),
                new PointF(18, 20), new PointF(26, 16)
            };
            g.FillPolygon(fillBrush, arrow);
            using var pen = new Pen(outline, 1.5f);
            g.DrawPolygon(pen, arrow);
        });

        // === はさみ (Cut) - 下向きハサミ ===
        // === はさみ (Cut) - 上向き、赤い持ち手 ===
        tsbCut.Image = CreateIcon(g => {
            using var pen = new Pen(outline, 1.5f);
            
            // 刃 (銀色)
            using var bladeBrush = new SolidBrush(Color.FromArgb(180, 180, 180));
            // 左上向きの刃
            PointF[] bladeL = { new(16, 16), new(6, 4), new(9, 3), new(18, 14) };
            g.FillPolygon(bladeBrush, bladeL);
            g.DrawPolygon(pen, bladeL);
            // 右上向きの刃
            PointF[] bladeR = { new(16, 16), new(26, 4), new(23, 3), new(14, 14) };
            g.FillPolygon(bladeBrush, bladeR);
            g.DrawPolygon(pen, bladeR);

            // 持ち手 (赤)
            using var handleBrush = new SolidBrush(Color.Tomato);
            
            // 左下の持ち手
            g.FillEllipse(handleBrush, 4, 18, 10, 10);
            g.DrawEllipse(pen, 4, 18, 10, 10);
            g.FillEllipse(Brushes.White, 6, 20, 6, 6); // 穴
            g.DrawEllipse(pen, 6, 20, 6, 6);

            // 右下の持ち手
            g.FillEllipse(handleBrush, 18, 18, 10, 10);
            g.DrawEllipse(pen, 18, 18, 10, 10);
            g.FillEllipse(Brushes.White, 20, 20, 6, 6); // 穴
            g.DrawEllipse(pen, 20, 20, 6, 6);

            // 支点 (ネジ)
            g.FillEllipse(Brushes.DimGray, 14, 14, 4, 4);
        });

        // === コピー (2枚の紙) ===
        tsbCopy.Image = CreateIcon(g => {
            using var pen = new Pen(outline, 1.5f);
            // 背面の紙 (青)
            using var backBrush = new SolidBrush(Color.FromArgb(140, 180, 230));
            g.FillRectangle(backBrush, 10, 3, 17, 21);
            g.DrawRectangle(pen, 10, 3, 17, 21);
            // 前面の紙 (白)
            using var frontBrush = new SolidBrush(isDark ? Color.FromArgb(220, 220, 220) : Color.White);
            g.FillRectangle(frontBrush, 5, 8, 17, 21);
            g.DrawRectangle(pen, 5, 8, 17, 21);
            // 行を示すライン
            using var linePen = new Pen(Color.FromArgb(180, 180, 180), 1f);
            g.DrawLine(linePen, 8, 14, 19, 14);
            g.DrawLine(linePen, 8, 18, 19, 18);
            g.DrawLine(linePen, 8, 22, 16, 22);
        });

        // === 貼り付け (クリップボード) ===
        tsbPaste.Image = CreateIcon(g => {
            using var pen = new Pen(outline, 1.5f);
            // ボード (茶色)
            using var boardBrush = new SolidBrush(Color.FromArgb(180, 140, 80));
            g.FillRectangle(boardBrush, 5, 6, 22, 23);
            g.DrawRectangle(pen, 5, 6, 22, 23);
            // 紙 (白)
            using var pastePaperBrush = new SolidBrush(isDark ? Color.FromArgb(230, 230, 230) : Color.White);
            g.FillRectangle(pastePaperBrush, 8, 10, 16, 17);
            g.DrawRectangle(pen, 8, 10, 16, 17);
            // クリップ (銀色)
            using var clipBrush = new SolidBrush(Color.FromArgb(160, 170, 180));
            g.FillRectangle(clipBrush, 12, 3, 8, 6);
            g.DrawRectangle(pen, 12, 3, 8, 6);
            // テキスト行
            using var linePen = new Pen(Color.FromArgb(180, 180, 180), 1f);
            g.DrawLine(linePen, 11, 15, 21, 15);
            g.DrawLine(linePen, 11, 19, 21, 19);
        });

        // === 削除 (ゴミ箱) ===
        tsbDelete.Image = CreateIcon(g => {
            using var pen = new Pen(outline, 1.5f);
            // ゴミ箱本体 (グレー)
            using var bodyBrush = new SolidBrush(Color.FromArgb(170, 175, 180));
            g.FillRectangle(bodyBrush, 8, 10, 16, 19);
            // 台形風に少し下すぼまり
            g.DrawLine(pen, 8, 10, 8, 29);
            g.DrawLine(pen, 24, 10, 24, 29);
            g.DrawLine(pen, 8, 29, 24, 29);
            // フタ
            using var lidBrush = new SolidBrush(Color.FromArgb(130, 135, 140));
            g.FillRectangle(lidBrush, 6, 7, 20, 4);
            g.DrawRectangle(pen, 6, 7, 20, 4);
            // 取っ手
            g.DrawArc(pen, 12, 3, 8, 5, 180, 180);
            // 縦のライン
            using var linePen = new Pen(Color.FromArgb(140, 145, 150), 1.5f);
            g.DrawLine(linePen, 13, 14, 13, 26);
            g.DrawLine(linePen, 16, 14, 16, 26);
            g.DrawLine(linePen, 19, 14, 19, 26);
        });

        // === Googleで検索 (虫眼鏡) ===
        tsbGoogleSearch.Image = CreateIcon(g => {
            using var pen = new Pen(outline, 1.5f);
            // レンズ (水色)
            using var lensBrush = new SolidBrush(Color.FromArgb(160, 210, 240));
            g.FillEllipse(lensBrush, 4, 4, 18, 18);
            g.DrawEllipse(pen, 4, 4, 18, 18);
            // ハイライト
            using var highlightPen = new Pen(Color.White, 2f);
            g.DrawArc(highlightPen, 8, 7, 10, 10, 200, 80);
            // 持ち手 (オレンジ)
            using var handlePen = new Pen(Color.FromArgb(220, 150, 50), 4f);
            g.DrawLine(handlePen, 20, 20, 28, 28);
            using var handleOutline = new Pen(outline, 1.5f);
            g.DrawLine(handleOutline, 19, 21, 28, 29);
            g.DrawLine(handleOutline, 21, 19, 29, 28);
        });

        // === 設定 (歯車) ===
        tsbSettings.Image = CreateIcon(g => {
            using var pen = new Pen(outline, 1.5f);
            // 歯車本体 (灰色/青灰色)
            var gearColor = Color.FromArgb(130, 150, 170);
            using var gearBrush = new SolidBrush(gearColor);
            // 外周の歯
            int cx = 16, cy = 16, outerR = 13, innerR = 10;
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
            g.FillPolygon(gearBrush, pts.ToArray());
            g.DrawPolygon(pen, pts.ToArray());
            // 中央の穴 (背景色)
            using var holeBrush = new SolidBrush(isDark ? Color.FromArgb(45, 45, 48) : Color.FromArgb(240, 240, 240));
            g.FillEllipse(holeBrush, cx - 4, cy - 4, 8, 8);
            g.DrawEllipse(pen, cx - 4, cy - 4, 8, 8);
        });
    }

    // ピンアイコン生成 (TopMost状態に応じて傾き変更)
    private void GeneratePinIcon(Color outline)
    {
        bool tilted = tsbTopMost.Checked;
        tsbTopMost.Image = CreateIcon(g => {
            // 共通の描画ロジック
            // 有効時は右に45度傾ける (時計回り)
            // 無効時はまっすぐ (0度)
            float angle = tilted ? 45f : 0f;

            var state = g.Save();
            
            // 下方へのシフト補正 (回転時に見切れ防止)
            g.TranslateTransform(16, 16);
            g.RotateTransform(angle);
            g.TranslateTransform(-16, -16);
            
            // まっすぐな状態での描画定義 (中心: 16,16 周辺)
            // 針の先端が (16, 30) あたりに来るように調整

            // 針 (銀色/白)
            // 中心軸 X=16
            using var needleBrush = new LinearGradientBrush(new Point(14, 20), new Point(18, 20), Color.WhiteSmoke, Color.LightGray);
            g.FillPolygon(needleBrush, new PointF[] { new(14, 20), new(18, 20), new(16, 30) });

            // 本体色 (赤グラデーション)
            Color redLight = Color.FromArgb(255, 80, 80);
            Color redDark = Color.FromArgb(220, 40, 40);
            using var redBrush = new LinearGradientBrush(new Point(8, 4), new Point(24, 20), redLight, redDark);

            // 頭 (Head)
            g.FillEllipse(redBrush, 10, 2, 12, 10); // 16中心 => 10..22

            // 首 (Neck)
            g.FillRectangle(redBrush, 13, 11, 6, 6); // 16中心 => 13..19

            // 襟 (Collar)
            g.FillEllipse(redBrush, 8, 14, 16, 8); // 16中心 => 8..24

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

    // 指定サイズのアイコンを生成する（デフォルト32x32、コンテキストメニュー用は16x16）
    private Image CreateIcon(Action<Graphics> drawAction, int size = 32)
    {
        var bmp = new Bitmap(size, size);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        drawAction(g);
        return bmp;
    }

    // コンテキストメニュー用アイコン描画 (16x16)
    private void DrawCutIcon(Graphics g)
    {
        using var pen = new Pen(Color.FromArgb(60, 60, 60), 1f);
        using var bladeBrush = new SolidBrush(Color.FromArgb(180, 180, 180));
        using var handleBrush = new SolidBrush(Color.Tomato);

        // 刃
        PointF[] b1 = { new(8, 8), new(3, 2), new(4, 2), new(9, 7) };
        PointF[] b2 = { new(8, 8), new(13, 2), new(12, 2), new(7, 7) };
        g.FillPolygon(bladeBrush, b1);
        g.DrawPolygon(pen, b1);
        g.FillPolygon(bladeBrush, b2);
        g.DrawPolygon(pen, b2);

        // 持ち手
        g.FillEllipse(handleBrush, 2, 9, 5, 5);
        g.DrawEllipse(pen, 2, 9, 5, 5);
        g.FillEllipse(Brushes.White, 3, 10, 3, 3);
        
        g.FillEllipse(handleBrush, 9, 9, 5, 5);
        g.DrawEllipse(pen, 9, 9, 5, 5);
        g.FillEllipse(Brushes.White, 10, 10, 3, 3);
        
        // ネジ
        g.FillEllipse(Brushes.DimGray, 7, 7, 2, 2);
    }

    private void DrawCopyIcon(Graphics g)
    {
        using var pen = new Pen(Color.FromArgb(60, 60, 60), 1f);
        using var backBrush = new SolidBrush(Color.FromArgb(140, 180, 230));
        g.FillRectangle(backBrush, 5, 1, 9, 11);
        g.DrawRectangle(pen, 5, 1, 9, 11);
        g.FillRectangle(Brushes.White, 2, 4, 9, 11);
        g.DrawRectangle(pen, 2, 4, 9, 11);
    }

    private void DrawPasteIcon(Graphics g)
    {
        using var pen = new Pen(Color.FromArgb(60, 60, 60), 1f);
        using var boardBrush = new SolidBrush(Color.FromArgb(180, 140, 80));
        g.FillRectangle(boardBrush, 2, 3, 12, 12);
        g.DrawRectangle(pen, 2, 3, 12, 12);
        g.FillRectangle(Brushes.White, 4, 5, 8, 9);
        g.DrawRectangle(pen, 4, 5, 8, 9);
        using var clipBrush = new SolidBrush(Color.FromArgb(160, 170, 180));
        g.FillRectangle(clipBrush, 6, 1, 4, 3);
        g.DrawRectangle(pen, 6, 1, 4, 3);
    }

    private void DrawDeleteIcon(Graphics g)
    {
        using var pen = new Pen(Color.FromArgb(60, 60, 60), 1f);
        using var bodyBrush = new SolidBrush(Color.FromArgb(170, 175, 180));
        g.FillRectangle(bodyBrush, 4, 5, 8, 10);
        g.DrawLine(pen, 4, 5, 4, 15);
        g.DrawLine(pen, 12, 5, 12, 15);
        g.DrawLine(pen, 4, 15, 12, 15);
        using var lidBrush = new SolidBrush(Color.FromArgb(130, 135, 140));
        g.FillRectangle(lidBrush, 3, 3, 10, 3);
        g.DrawRectangle(pen, 3, 3, 10, 3);
        g.DrawArc(pen, 6, 1, 4, 3, 180, 180);
    }

    private void DrawSearchIcon(Graphics g)
    {
        using var pen = new Pen(Color.FromArgb(60, 60, 60), 1f);
        using var lensBrush = new SolidBrush(Color.FromArgb(160, 210, 240));
        g.FillEllipse(lensBrush, 2, 2, 9, 9);
        g.DrawEllipse(pen, 2, 2, 9, 9);
        using var handlePen = new Pen(Color.FromArgb(220, 150, 50), 2.5f);
        g.DrawLine(handlePen, 10, 10, 14, 14);
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        _appData.Load();

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
        // 描画完了後にマージン設定とキャレットを確実に先頭へ移動し、フォーカスを当てて点滅させる
        this.BeginInvoke(new Action(() =>
        {
            UpdateEditorMargin();
            txtMain.Select(0, 0);
            txtMain.ScrollToCaret();
            txtMain.Focus();
        }));
        
        // 初回履歴プッシュ
        _appData.History.Push(txtMain.Text);
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
        catch { }
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        // 状態保存
        _appData.Config.WindowX = this.Location.X;
        _appData.Config.WindowY = this.Location.Y;
        _appData.Config.WindowWidth = this.Size.Width;
        _appData.Config.WindowHeight = this.Size.Height;
        _appData.Config.IsTopMost = this.TopMost;
        
        // Fontは変更時に保存しているのでここではサイズなど
        // (SettingsDialogで変更しているので同期不要だが念のため)
        
        _appData.MemoContent = txtMain.Text;

        // 最後の変更を履歴に確実に残す
        _appData.History.Push(txtMain.Text);

        _appData.Save();
    }

    private void ApplySettings()
    {
        txtMain.Font = _appData.Config.GetFont();
        this.TopMost = _appData.Config.IsTopMost;
        tsbTopMost.Checked = this.TopMost;

        // 保存された設定に基づいてテーマを適用
        ApplyTheme();
    }

    private void TsbNewMemo_Click(object? sender, EventArgs e)
    {
        // 日本語の曜日名
        string[] dayNames = { "日", "月", "火", "水", "木", "金", "土" };
        var now = DateTime.Now;
        // 曜日を日本語で取得
        string dayOfWeek = dayNames[(int)now.DayOfWeek];
        // フォーマット: 2026/02/17 火 0:04:41
        string dateHeader = now.ToString($"yyyy/MM/dd") + $" {dayOfWeek} " + now.ToString("H:mm:ss");
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

        // スナップショット保存
        _snapshotTimer.Stop();
        _appData.History.Push(txtMain.Text);
        UpdateUndoRedoButtons();
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
        // 設定ダイアログを表示 (現在のフォントとテーマ設定を渡す)
        using var dlg = new SettingsDialog(txtMain.Font, _appData.Config.Theme);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            _appData.Config.SetFont(dlg.CurrentFont);
            _appData.Config.Theme = dlg.CurrentTheme;
            // 変更を即座に反映
            ApplySettings();
        }
    }

    private void TsbUndo_Click(object? sender, EventArgs e)
    {
        if (_appData.History.CanUndo)
        {
            _isUndoRedoAction = true;
            // 現在のテキストをRedo用に保存してから戻す
            // HistoryManager.Undo内で現在のテキストをRedoStackに積む処理があるので
            // ここでは戻り値を受け取るだけ
            txtMain.Text = _appData.History.Undo(txtMain.Text);
            _isUndoRedoAction = false;
            UpdateUndoRedoButtons();
        }
    }

    private void TsbRedo_Click(object? sender, EventArgs e)
    {
        if (_appData.History.CanRedo)
        {
            _isUndoRedoAction = true;
            txtMain.Text = _appData.History.Redo(txtMain.Text);
            _isUndoRedoAction = false;
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
            catch { }
        }
    }

    private void TxtMain_TextChanged(object? sender, EventArgs e)
    {
        UpdateUndoRedoButtons(); // Undo/Redoボタンの有効無効更新(文字入力したのでRedoは消えるはずだがManager次第)

        if (_isUndoRedoAction) return;

        // タイマーリセット
        _snapshotTimer.Stop();
        _snapshotTimer.Start();
    }

    private void SnapshotTimer_Tick(object? sender, EventArgs e)
    {
        _snapshotTimer.Stop();
        _appData.History.Push(txtMain.Text);
        UpdateUndoRedoButtons();
    }

    private void UpdateUndoRedoButtons()
    {
        tsbUndo.Enabled = _appData.History.CanUndo;
        tsbRedo.Enabled = _appData.History.CanRedo;
    }

    private void TxtMain_LinkClicked(object? sender, LinkClickedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.LinkText))
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.LinkText) { UseShellExecute = true });
            }
            catch { }
        }
    }

    // Ctrl+Vの装飾付き貼り付けを抑制
    private void TxtMain_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.V)
        {
            e.Handled = true;
            PastePlainText();
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
