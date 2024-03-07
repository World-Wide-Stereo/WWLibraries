using System.Drawing;
using System.Windows.Forms;

namespace Controls
{
    public class DateTimePickerExtended : DateTimePicker
    {
        const int WM_ERASEBKGND = 0x14;
        protected override void WndProc(ref Message m)
        {
            // Enables use of the BackColor property.
            if (m.Msg == WM_ERASEBKGND)
            {
                using (var g = Graphics.FromHdc(m.WParam))
                {
                    using (var b = new SolidBrush(this.BackColor))
                    {
                        g.FillRectangle(b, ClientRectangle);
                    }
                }
                return;
            }

            base.WndProc(ref m);
        }
    }
}
