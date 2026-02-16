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
}
