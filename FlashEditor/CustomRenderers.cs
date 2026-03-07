using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace FlashEditor;

public class CustomToolStripRenderer : ToolStripProfessionalRenderer
{
    // アイコン画像の描画をカスタマイズ（無効時は半透明に）
    protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
    {
        if (e.Item.Enabled)
        {
            base.OnRenderItemImage(e);
        }
        else
        {
            if (e.Image == null) return;

            // 無効時は透過度60%で描画
            var matrix = new ColorMatrix();
            matrix.Matrix00 = 1.0f;
            matrix.Matrix11 = 1.0f;
            matrix.Matrix22 = 1.0f;
            matrix.Matrix33 = 0.6f; // Alpha 0.6

            using var attributes = new ImageAttributes();
            attributes.SetColorMatrix(matrix);

            e.Graphics.DrawImage(e.Image, e.ImageRectangle, 0, 0, e.Image.Width, e.Image.Height, GraphicsUnit.Pixel, attributes);
        }
    }

    // チェック済みボタン（常に最前面ON）のハイライト背景を描画
    protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
    {
        var button = e.Item as ToolStripButton;
        
        if (button != null && button.Checked)
        {
            var rect = new Rectangle(0, 0, button.Width, button.Height);
            
            // テーマに基づいたハイライト色の選択
            bool isDark = (ThemeManager.CurrentTheme == ThemeManager.ThemeMode.Dark);
            Color hoverColor = isDark ? Color.FromArgb(80, 80, 80) : Color.FromArgb(200, 220, 240);
            
            using var brush = new SolidBrush(hoverColor);
            e.Graphics.FillRectangle(brush, rect);
            
            // 枠線も薄く描画
            using var pen = new Pen(isDark ? Color.FromArgb(100, 100, 100) : Color.FromArgb(150, 180, 210));
            e.Graphics.DrawRectangle(pen, 0, 0, rect.Width - 1, rect.Height - 1);
        }
        else
        {
            base.OnRenderButtonBackground(e);
        }
    }

    // ToolStrip背景をフラットに描画（ダークテーマ対応）
    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        bool isDark = (ThemeManager.CurrentTheme == ThemeManager.ThemeMode.Dark);
        
        if (isDark)
        {
            // ダークテーマ: フラットな背景
            using var brush = new SolidBrush(Color.FromArgb(45, 45, 48));
            e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }
        else
        {
            // ライトテーマ: フラットな背景
            using var brush = new SolidBrush(SystemColors.Control);
            e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }
    }

    // ToolStripの下部にエディターとの区切り線を描画
    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        bool isDark = (ThemeManager.CurrentTheme == ThemeManager.ThemeMode.Dark);
        
        // 下辺のみに区切り線を描画
        Color borderColor = isDark ? Color.FromArgb(63, 63, 70) : Color.FromArgb(204, 206, 219);
        using var pen = new Pen(borderColor);
        e.Graphics.DrawLine(pen, 0, e.AffectedBounds.Bottom - 1, e.AffectedBounds.Right, e.AffectedBounds.Bottom - 1);
    }

    // セパレーターをテーマに合った色で描画
    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        bool isDark = (ThemeManager.CurrentTheme == ThemeManager.ThemeMode.Dark);
        
        if (e.Vertical)
        {
            // 縦セパレーター
            Color sepColor = isDark ? Color.FromArgb(63, 63, 70) : Color.FromArgb(190, 195, 200);
            int x = e.Item.Width / 2;
            using var pen = new Pen(sepColor);
            e.Graphics.DrawLine(pen, x, 4, x, e.Item.Height - 4);
        }
        else
        {
            // 横セパレーター（コンテキストメニュー内）
            Color sepColor = isDark ? Color.FromArgb(63, 63, 70) : Color.FromArgb(215, 215, 215);
            int y = e.Item.Height / 2;
            // アイコン領域の右端からセパレーターを描画
            int leftMargin = e.Item.Owner is ContextMenuStrip ? 28 : 0;
            using var pen = new Pen(sepColor);
            e.Graphics.DrawLine(pen, leftMargin, y, e.Item.Width - 2, y);
        }
    }

    // コンテキストメニュー項目のホバー背景を描画
    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        bool isDark = (ThemeManager.CurrentTheme == ThemeManager.ThemeMode.Dark);

        if (e.Item.Selected && e.Item.Enabled)
        {
            // ホバー時のハイライト色
            Color hoverBg = isDark ? Color.FromArgb(62, 62, 66) : Color.FromArgb(200, 220, 240);
            var rect = new Rectangle(2, 0, e.Item.Width - 4, e.Item.Height);
            using var brush = new SolidBrush(hoverBg);
            e.Graphics.FillRectangle(brush, rect);
            // ホバー枠線
            using var pen = new Pen(isDark ? Color.FromArgb(80, 80, 84) : Color.FromArgb(150, 180, 210));
            e.Graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
        }
        else
        {
            // 通常状態：背景をテーマ色で塗りつぶし
            Color normalBg = isDark ? Color.FromArgb(43, 43, 43) : SystemColors.Control;
            using var brush = new SolidBrush(normalBg);
            e.Graphics.FillRectangle(brush, new Rectangle(Point.Empty, e.Item.Size));
        }
    }

    // コンテキストメニュー項目のテキスト色をテーマに応じて変更
    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        bool isDark = (ThemeManager.CurrentTheme == ThemeManager.ThemeMode.Dark);

        if (e.Item.Owner is ContextMenuStrip || e.Item.OwnerItem != null)
        {
            // コンテキストメニュー内のテキスト色をテーマに応じて設定
            e.TextColor = e.Item.Enabled
                ? (isDark ? Color.WhiteSmoke : SystemColors.MenuText)
                : (isDark ? Color.FromArgb(110, 110, 110) : SystemColors.GrayText);
        }
        base.OnRenderItemText(e);
    }

    // コンテキストメニュー左側のアイコン領域の背景色を変更
    protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
    {
        bool isDark = (ThemeManager.CurrentTheme == ThemeManager.ThemeMode.Dark);

        if (isDark)
        {
            // ダークテーマ：アイコン領域を少し暗めに
            using var brush = new SolidBrush(Color.FromArgb(38, 38, 38));
            e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }
        else
        {
            base.OnRenderImageMargin(e);
        }
    }
}
