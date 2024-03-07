using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Controls
{
    public partial class ScrollEventListView : FunctionKeyListView
    {
        // ScrollBar types
        private const int SB_HORZ = 0;
        private const int SB_VERT = 1;

        // ListView messages
        private const uint LVM_SCROLL = 0x1014;

        // Windows messages
        private const int WM_HSCROLL = 0x114;
        private const int WM_VSCROLL = 0x115;
        public event ScrollEventHandler HScroll;
        public event ScrollEventHandler VScroll;

        public int HScrollPosition
        {
            get
            {
                return GetScrollPos(this.Handle, SB_HORZ);
            }
            set
            {
                int prevPos;
                int scrollVal = 0;

                if (ShowGroups == true)
                {
                    prevPos = GetScrollPos(this.Handle, SB_HORZ);
                    scrollVal = -(prevPos - value);
                }
                else
                {
                    // TODO: Add setScrollPosition if ShowGroups == false
                }
                SendMessage(this.Handle, LVM_SCROLL, (IntPtr)scrollVal, (IntPtr)0);
            }
        }
        public int vScrollPosition
        {
            get
            {
                return GetScrollPos(this.Handle, SB_VERT);
            }
            set
            {
                int prevPos;
                int scrollVal = 0;

                if (ShowGroups == true)
                {
                    prevPos = GetScrollPos(this.Handle, SB_VERT);
                    scrollVal = -(prevPos - value);
                }
                else
                {
                    // TODO: Add setScrollPosition if ShowGroups == false
                }
                SendMessage(this.Handle, LVM_SCROLL, (IntPtr)0, (IntPtr)scrollVal);
            }
        }

        protected virtual void OnScroll(ScrollEventArgs e, bool vertical)
        {
            if (this.VScroll != null && vertical) this.VScroll(this, e);
            else if (this.HScroll != null) this.HScroll(this, e);
        }
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            base.WndProc(ref m);

            switch (m.Msg)
            {
                case WM_HSCROLL:
                    this.OnScroll(new ScrollEventArgs((ScrollEventType)(m.WParam.ToInt32() & 0xffff), GetScrollPos(this.Handle, SB_HORZ)), false);
                    break;
                case WM_VSCROLL:
                    this.OnScroll(new ScrollEventArgs((ScrollEventType)(m.WParam.ToInt32() & 0xffff), GetScrollPos(this.Handle, SB_VERT)), true);
                    break;
            }
        }

        [DllImport("user32.dll")]
        public static extern int SendMessage(
              int hWnd,      // handle to destination window
              uint Msg,       // message
              long wParam,  // first message parameter
              long lParam   // second message parameter
              );

        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int wMsg,
                                       int wParam, int lParam);

        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, uint wMsg,
                                       IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int GetScrollPos(IntPtr hWnd, int nBar);
    }
}
