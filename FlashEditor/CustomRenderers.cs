using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace FlashEditor;

public class CustomToolStripRenderer : ToolStripProfessionalRenderer
{
    protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
    {
        if (e.Item.Enabled)
        {
            base.OnRenderItemImage(e);
        }
        else
        {
            if (e.Image == null) return;

            // 半透明かつ彩度を少し落とした描画を行う
            // 無効時はデフォルトだと完全グレーになるが、ここでは彩度50% + 透明度50%程度にする

            // 彩度は落とさず (100%)、透過度を60% (0.6) にする
            var matrix = new ColorMatrix();
            matrix.Matrix00 = 1.0f;
            matrix.Matrix11 = 1.0f;
            matrix.Matrix22 = 1.0f;
            matrix.Matrix33 = 0.6f; // Alpha 0.6

            using var attributes = new ImageAttributes();
            attributes.SetColorMatrix(matrix);

            // 描画
            e.Graphics.DrawImage(e.Image, e.ImageRectangle, 0, 0, e.Image.Width, e.Image.Height, GraphicsUnit.Pixel, attributes);
        }
    }

    protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
    {
        var button = e.Item as ToolStripButton;
        
        // ボタンがチェック済み（常に最前面が有効）の場合、ホバー時と同様の背景を描画
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
}
