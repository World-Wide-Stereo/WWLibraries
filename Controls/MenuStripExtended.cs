using System;
using System.Drawing;
using System.Windows.Forms;

public class MenuStripExtended : MenuStrip
{
    public MenuStripExtended()
    {
        this.Renderer = new CustomRenderer();
    }

    private class CustomRenderer : ToolStripProfessionalRenderer
    {
        /// <summary>
        /// Respects the BackColor and ForeColor of ToolStripSeparator objects.
        /// </summary>
        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            ToolStripSeparator toolStripSeparator = (ToolStripSeparator)e.Item;

            // Get the separator's height and width.
            int height = toolStripSeparator.Height;
            int width = toolStripSeparator.Width;

            // Fill the background.
            var solidBrush = new SolidBrush(toolStripSeparator.BackColor);
            e.Graphics.FillRectangle(solidBrush, 0, 0, width, height);
            solidBrush.Dispose();

            // Draw the line.
            if (e.Vertical)
            {
                var bounds = new Rectangle(Point.Empty, toolStripSeparator.Size);
                bounds.Y += 3;
                bounds.Height = Math.Max(0, bounds.Height - 6);
                if (bounds.Height >= 4) bounds.Inflate(0, -2);
                var pen = new Pen(toolStripSeparator.ForeColor);
                int x = bounds.Width / 2;
                e.Graphics.DrawLine(pen, x, bounds.Top, x, bounds.Bottom - 1);
                pen.Dispose();
            }
            else
            {
                var pen = new Pen(toolStripSeparator.ForeColor);
                e.Graphics.DrawLine(pen, 4, height / 2, width - 4, height / 2);
                pen.Dispose();
            }
        }
    }
}
