using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

/// <summary>
/// Panel with support for getting and setting the scrollbar position, as well as listening to scroll events.
/// </summary>
public class PanelWithScrollEvents : Panel
{
    private int _lastVerticalScrollPosition;
    private int _lastHorizontalScrollPosition;
    private int _fontWidth;

    public PanelWithScrollEvents()
    {
        OnFontChanged(null);
    }

    [DefaultValue(0)]
    [Category("Appearance")]
    [Description("Gets or sets the vertical scrollbar's position.")]
    public int VerticalScrollPosition
    {
        set { SetScroll(value, Win32.WM_VSCROLL, Win32.SB_VERT); }
        get { return GetScroll(Win32.SB_VERT); }
    }

    [DefaultValue(0)]
    [Category("Appearance")]
    [Description("Gets or sets the horizontal scrollbar's position.")]
    public int HorizontalScrollPosition
    {
        set { SetScroll(value, Win32.WM_HSCROLL, Win32.SB_HORZ); }
        get { return GetScroll(Win32.SB_HORZ); }
    }

    [Category("Property Changed")]
    [Description("Fired when the scrollbar's vertical position changes.")]
    public event ScrollEventHandler ScrollPositionChanged;

    [DefaultValue(false)]
    [Category("Behavior")]
    public bool PreventMouseWheelScrolling { get; set; }

    [DefaultValue(false)]
    [Category("Behavior")]
    public bool PreventFocusChangeScrolling { get; set; }

    // Fire scroll event if the scroll-bars are moved.
    protected override void WndProc(ref Message message)
    {
        if (message.Msg == Win32.WM_VSCROLL || message.Msg == Win32.WM_HSCROLL || message.Msg == Win32.WM_MOUSEWHEEL)
        {
            TryFireScrollEvent();
        }
        base.WndProc(ref message);
    }

    // Changing the size of the font can cause a scroll event. Also, when
    // scrolling horizontally, the event will notify whether the scroll
    // was a large or small change. For vertical, small increments are 1
    // line, but for horizontal, it is several pixels. To guess what a
    // small increment is, get the width of the W character and anything
    // smaller than that will be represented as a small increment.
    protected override void OnFontChanged(EventArgs e)
    {
        using (Graphics graphics = this.CreateGraphics())
        {
            _fontWidth = (int)graphics.MeasureString("W", this.Font).Width;
        }
        TryFireScrollEvent();
        base.OnFontChanged(e);
    }

    // Key presses include navigation keys, but also typing and pasting can
    // cause a scroll event.
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        TryFireScrollEvent();
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void SetScroll(int value, uint windowsMessage, int scrollBarMessage)
    {
        Win32.SetScrollPos(this.Handle, scrollBarMessage, value, true);
        Win32.PostMessage(this.Handle, windowsMessage, 4 + 0x10000 * value, 0);
    }

    private int GetScroll(int scrollBarMessage)
    {
        return Win32.GetScrollPos(this.Handle, scrollBarMessage);
    }

    // Fire both horizontal and vertical scroll events seperately, one
    // after the other. These first test if a scroll actually occurred
    // and won't fire if there was no actual movement.
    protected void TryFireScrollEvent()
    {
        // Don't do anything if there is no event handler.
        if (ScrollPositionChanged == null)
            return;
        TryFireHorizontalScrollEvent();
        TryFireVerticalScrollEvent();
    }

    private void TryFireHorizontalScrollEvent()
    {
        int lastScrollPosition = _lastHorizontalScrollPosition;
        int scrollPosition = HorizontalScrollPosition;

        // Don't do anything if there was no change in position.
        if (scrollPosition == lastScrollPosition)
            return;

        _lastHorizontalScrollPosition = scrollPosition;

        ScrollPositionChanged(
            this,
            new ScrollEventArgs
            (scrollPosition < lastScrollPosition - _fontWidth
                    ? ScrollEventType.LargeDecrement
                    : scrollPosition > lastScrollPosition + _fontWidth
                        ? ScrollEventType.LargeIncrement
                        : scrollPosition < lastScrollPosition
                            ? ScrollEventType.SmallDecrement
                            : ScrollEventType.SmallIncrement,
                lastScrollPosition,
                scrollPosition,
                ScrollOrientation.HorizontalScroll)
        );
    }

    private void TryFireVerticalScrollEvent()
    {
        int lastScrollPosition = _lastVerticalScrollPosition;
        int scrollPosition = VerticalScrollPosition;

        // Don't do anything if there was no change in position.
        if (scrollPosition == lastScrollPosition)
            return;

        _lastVerticalScrollPosition = scrollPosition;

        ScrollPositionChanged(
            this,
            new ScrollEventArgs
            (scrollPosition < lastScrollPosition - 1
                    ? ScrollEventType.LargeDecrement
                    : scrollPosition > lastScrollPosition + 1
                        ? ScrollEventType.LargeIncrement
                        : scrollPosition < lastScrollPosition
                            ? ScrollEventType.SmallDecrement
                            : ScrollEventType.SmallIncrement,
                lastScrollPosition,
                scrollPosition,
                ScrollOrientation.VerticalScroll)
        );
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        if (!PreventMouseWheelScrolling)
        {
            base.OnMouseWheel(e);
        }
    }

    protected override Point ScrollToControl(Control activeControl)
    {
        return PreventFocusChangeScrolling ? DisplayRectangle.Location : base.ScrollToControl(activeControl);
    }

    private static class Win32
    {
        public const uint WM_HSCROLL = 0x114;
        public const uint WM_VSCROLL = 0x115;
        public const uint WM_MOUSEWHEEL = 0x20A;
        public const int SB_VERT = 0x1;
        public const int SB_HORZ = 0x0;

        [DllImport("user32.dll")]
        public static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

        [DllImport("user32.dll")]
        public static extern int GetScrollPos(IntPtr hWnd, int nBar);

        [DllImport("user32.dll", EntryPoint = "PostMessageA")]
        public static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);
    }
}
