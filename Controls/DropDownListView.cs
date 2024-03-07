using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Controls;
using ww.Utilities;
using ww.Utilities.Extensions;

public enum FilterStyles { Hierarchical, Flat };

public class DropDownListView : ListView
{
    #region Internal Classes & Variables Declarations

    public List<ComboBox> Filters; // holds the ComboBoxes that form the dropdowns
    private List<Button> FilterButtons;
    private List<Button> ColumnHeaderButtons;
    private ListViewColumnSorter listsort;
    private SortOrder[] SortDirections;
    private int HorizontalScrollPosition_OldVar;
    public HashSet<int> AppliedFilters;
    private bool hzscrolled;
    string[] boxes;

    private FilterStyles UseFilterStyle;

    private ColumnHeader CurrentSortColumn;
    private SortOrder CurrentSortOrder;

    #region Structs and constants for handling NM_CUSTOMDRAW messages
    [StructLayout(LayoutKind.Sequential)]
    public struct NMHDR
    {
        public IntPtr hwndFrom;
        public int idFrom;
        public int code;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct NMCUSTOMDRAW
    {
        public NMHDR hdr;
        public uint dwDrawStage;
        public IntPtr hdc;
        public RECT rc;
        public IntPtr dwItemSpec;
        public uint uItemState;
        public IntPtr lItemParam;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct NMLVCUSTOMDRAW
    {
        public NMCUSTOMDRAW nmcd;
        public uint drText;
        public uint drTextBk;
        public int iSubtItem;
        public uint dwItemType;
        public uint drFace;
        public int iIconEffect;
        public int iIconPhase;
        public int iPartID;
        public int iStateID;
        public RECT rcText;
        public uint uAlign;
    }
    #endregion

    #region Structs and constants for the GetScrollInfo API call
    private const int SB_HORZ = 0;
    private const int SB_VERT = 1;
    private const int SB_CTL = 2;
    private const int SIF_RANGE = 0x0001;
    private const int SIF_PAGE = 0x0002;
    private const int SIF_POS = 0x0004;
    private const int SIF_TRACKPOS = 0x0010;
    private const int SIF_ALL = (SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS);

    [StructLayout(LayoutKind.Sequential)]
    private struct ScrollInfoStruct
    {
        public int cbSize;
        public int fMask;
        public int nMin;
        public int nMax;
        public int nPage;
        public int nPos;
        public int nTrackPos;
    }
    #endregion

    #region New ColumnHeaderCollection class
    public class ListViewColumnHeaderCollectionNewEvent : ListView.ColumnHeaderCollection
    {
        private ListView parent;
        public ListViewColumnHeaderCollectionNewEvent(ListView owner) : base(owner)
        {
            parent = owner;
        }
        public new void Add(string str, int width, HorizontalAlignment horiz)
        {
            ColumnHeader ch = new ColumnHeader();
            ch.Text = str;
            ch.Width = width;
            ch.TextAlign = horiz;
            Add(ch);
        }
        public new void Add(ColumnHeader ch)
        {
            base.Add(ch);
            ((DropDownListView)parent).AddedColumn(ch);
        }
        public new void AddRange(ColumnHeader[] ch)
        {
            base.AddRange(ch);
            ((DropDownListView)parent).AddedColumnRange(ch);
        }
    }
    #endregion

    #region New ListViewItemCollection class
    public class ListViewItemCollectionNewEvent : ListViewItemCollection
    {
        private readonly DropDownListView parent; // Becomes null after it is disposed?
        public ListViewItemCollectionNewEvent(ListView owner) : base(owner)
        {
            parent = (DropDownListView)owner;
        }
        public new void Add(ListViewItem Lvi)
        {
            if (parent != null && !parent.IsDisposed)
            {
                base.Add(Lvi);
                parent.AddedItem(Lvi);
            }
        }
        public new void Add(string str)
        {
            if (parent != null && !parent.IsDisposed)
            {
                base.Add(str);
                parent.AddedItem(str);
            }
        }
        public new void AddRange(ListViewItem[] Lvi)
        {
            if (parent != null && !parent.IsDisposed)
            {
                base.AddRange(Lvi);
                parent.AddedItemRange(Lvi);
            }
        }
        public new void Insert(int index, ListViewItem Lvi)
        {
            if (parent != null && !parent.IsDisposed)
            {
                base.Insert(index, Lvi);
                parent.AddedItem(Lvi);
            }
        }
        public new void Remove(ListViewItem Lvi)
        {
            if (parent != null && !parent.IsDisposed)
            {
                base.Remove(Lvi);
                parent.RemovedItem(Lvi);
            }
        }
        public new void RemoveAt(int index)
        {
            if (parent != null && !parent.IsDisposed)
            {
                ListViewItem lvi = this[index];
                base.RemoveAt(index);
                parent.RemovedItem(index, lvi);
            }
        }
        public new void Clear()
        {
            if (parent != null && !parent.IsDisposed)
            {
                base.Clear();
                parent.ClearedItems();
            }
        }
    }
    #endregion

    public delegate void ListViewItemDelegate(ListViewItem item);
    public delegate void ListViewColumnHeaderDelegate(ColumnHeader ch);
    public delegate void ListViewItemRangeDelegate(ListViewItem[] item);
    public delegate void ListViewColumnHeaderRangeDelegate(ColumnHeader[] ch);
    public delegate void ListViewRemoveDelegate(ListViewItem item);
    public delegate void ListViewRemoveAtDelegate(int index, ListViewItem item);
    public delegate void FilterSelectedIndexChangedDelegate(ComboBox c);

    public delegate void ListViewItemClearDelegate();

    //Next come the event declarations:

    public event ListViewItemDelegate ItemAdded;
    public event ListViewColumnHeaderDelegate ColumnHeaderAdded;
    public event ListViewItemRangeDelegate ItemRangeAdded;
    public event ListViewColumnHeaderRangeDelegate ColumnHeaderRangeAdded;
    public event ListViewRemoveDelegate ItemRemoved;
    public event ListViewRemoveAtDelegate ItemRemovedAt = null;
    public event FilterSelectedIndexChangedDelegate FilterSelectedIndexChanged = null;

    public event ListViewItemClearDelegate ItemsCleared = null;

    public event EventHandler ColumnSort;

    //Now explicitly hide the derived Items propery by declaring it as new:
    public new ListViewItemCollectionNewEvent Items;
    public new ListViewColumnHeaderCollectionNewEvent Columns;

    //Next we provide the methods that the extended
    //"ListViewItemCollection" inner 
    //class will call and inside their implementation we 
    //raise our events to notify the Observers. 
    private void AddedItem(ListViewItem lvi)
    {
        this.ItemAdded(lvi);
    }
    private void AddedItem(string str)
    {
        this.ItemAdded(new ListViewItem(str));
    }
    private void AddedColumn(ColumnHeader ch)
    {
        this.ColumnHeaderAdded(ch);
    }
    private void AddedColumnRange(ColumnHeader[] ch)
    {
        this.ColumnHeaderRangeAdded(ch);
    }
    private void AddedItemRange(ListViewItem[] lvi)
    {
        if (ItemRangeAdded != null) ItemRangeAdded(lvi);
    }
    private void RemovedItem(ListViewItem lvi)
    {
        if (ItemRemoved != null) ItemRemoved(lvi);
    }
    private void RemovedItem(int index, ListViewItem item)
    {
        if (ItemRemovedAt != null) ItemRemovedAt(index, item);
    }
    private void ClearedItems()
    {
        if (ItemsCleared != null) ItemsCleared();
    }
    #endregion

    #region Initialization
    public DropDownListView()
    {
        this.Dock = DockStyle.Fill;
        this.View = View.Details;
        this.FullRowSelect = true;
        this.Scrollable = true;
        this.HideSelection = false;
        boxes = new string[0];
        HeaderStyle = ColumnHeaderStyle.Clickable;
        ListViewItemSorter = listsort = new MultiTypeListViewColumnSorter();
        Items = new ListViewItemCollectionNewEvent(this);
        Columns = new ListViewColumnHeaderCollectionNewEvent(this);

        Sorting = SortOrder.None;

        Filters = new List<ComboBox>();
        FilterButtons = new List<Button>();
        ColumnHeaderButtons = new List<Button>();
        AppliedFilters = new HashSet<int>();
        ColumnHeaderAdded += new ListViewColumnHeaderDelegate(ListViewF_ColumnHeaderAdded);
        ColumnHeaderRangeAdded += new ListViewColumnHeaderRangeDelegate(ListViewD_ColumnHeaderRangeAdded);
        ItemAdded += new ListViewItemDelegate(DropDownListView_ItemAdded);
        ItemRangeAdded += new ListViewItemRangeDelegate(DropDownListView_ItemRangeAdded);
        ItemsCleared += new ListViewItemClearDelegate(DropDownListView_ItemsCleared);

        FilterStyle = FilterStyles.Hierarchical;

        HorizontalScrollPosition_OldVar = 0;
        hzscrolled = false;
        OnFontChanged(null);

        CurrentSortOrder = SortOrder.None;
    }
    #endregion

    #region Functions
    //this returns the right edge of the specified column
    private int GetColumnPosition(int ColumnNumber)
    {
        int val = 0;
        for (int i = 0; i <= ColumnNumber; i++) val += Columns[i].Width;
        return val;
    }
    private int GetColumnLeft(int ColumnNumber)
    {
        int val = 0;
        for (int i = 0; i < ColumnNumber; i++) val += Columns[i].Width;
        return val;
    }

    private int GetHorizontalScrollPosition()
    {
        ScrollInfoStruct si = new ScrollInfoStruct();
        si.fMask = SIF_ALL;
        si.cbSize = Marshal.SizeOf(si);
        Win32.GetScrollInfo(Handle, SB_HORZ, ref si);
        return si.nPos;
    }
    #endregion

    #region Scrollbars
    private int _lastVerticalScrollPosition;
    private int _lastHorizontalScrollPosition;
    private int _fontWidth;

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
    [Description("Fired when either scrollbar's position changes.")]
    public event ScrollEventHandler ScrollPositionChanged;

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

    // Key presses includes navigation keys, but also typing and pasting can
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
    public void TryFireScrollEvent()
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
    #endregion

    #region Display
    private void SetButtonPositions()
    {
        SuspendLayout();
        HorizontalScrollPosition_OldVar = GetHorizontalScrollPosition();
        // 6px gutters added between buttons to make column resizing easier
        for (int i = 0; i < FilterButtons.Count; i++)
        {
            Filters[i].Hide();
            Button b = FilterButtons[i]; // this is the dropdown arrow 
            Button cb = ColumnHeaderButtons[i]; // this is the column header
            cb.Left = GetColumnLeft(i) - HorizontalScrollPosition_OldVar;
            if (cb.Width != Columns[i].Width - b.Width - 3) cb.Width = Columns[i].Width - b.Width - 3;
            b.Left = GetColumnLeft(i) + Columns[i].Width - b.Width - HorizontalScrollPosition_OldVar - 3;
        }
        ResumeLayout();
    }

    private void b_Paint(object sender, PaintEventArgs e)
    {
        e.Graphics.FillPolygon(new SolidBrush(AppliedFilters.Contains(FilterButtons.IndexOf((Button)sender)) ? Color.Red : Color.Black), CreatePointArray(new[] { 7, 7, 14, 7, 11, 11 }));
    }
    #endregion

    #region Events
    // This is required to interoperate with the ManualFilters object
    public event EventHandler WindowRepaint = null;
    protected void OnWindowRepaint()
    {
        WindowRepaint?.Invoke(this, EventArgs.Empty);
    }

    protected override void WndProc(ref Message m)  
    {
        bool suppress = false;

        switch (m.Msg)
        {
            case Win32.WM_PAINT:
                OnWindowRepaint(); //fire the window repaint event
                break;
            case Win32.WM_ERASEBKGND:
                //right now I'm suppressing one background erase msg per hz scroll msg
                //I'm not sure if it's reducing the flicker very much though
                suppress = hzscrolled;
                hzscrolled = false;
                break;
            case Win32.WM_NOTIFY:
                NMHDR h = (NMHDR)Marshal.PtrToStructure(m.LParam, typeof(NMHDR));
                if (h.code == Win32.NM_CUSTOMDRAW)
                {
                    NMLVCUSTOMDRAW lvcd = (NMLVCUSTOMDRAW)Marshal.PtrToStructure(m.LParam, typeof(NMLVCUSTOMDRAW));

                    if (lvcd.nmcd.dwDrawStage == Win32.CDDS_PREPAINT)
                    {
                        //request a postpaint notify message for the whole listview and
                        //request a prepaint notify message for each column header
                        //System.Console.WriteLine("Prepaint: "+h.hwndFrom.ToInt32()+" "+lvcd.nmcd.rc.left+","+lvcd.nmcd.rc.top+" "+lvcd.nmcd.rc.right+","+lvcd.nmcd.rc.bottom);
                        m.Result = new IntPtr(Win32.CDRF_NOTIFYPOSTPAINT | Win32.CDRF_NOTIFYITEMDRAW);
                        suppress = true;
                    }
                    else if (lvcd.nmcd.dwDrawStage == Win32.CDDS_POSTPAINT)
                    {
                        //the listview is finished drawing itself
                        //now draw our column headers on top
                        //System.Console.WriteLine("Postpaint: "+h.hwndFrom.ToInt32());
                        SetButtonPositions();
                        InvalidateColumnHeaders();
                    }
                    else if (lvcd.nmcd.dwDrawStage == Win32.CDDS_ITEMPREPAINT)
                    {
                        // tell Windows we drew the item manually
                        m.Result = new IntPtr(Win32.CDRF_SKIPDEFAULT | Win32.CDRF_NOTIFYPOSTPAINT);
                        suppress = true;
                    }
                }
                break;
        }

        if (m.Msg == Win32.WM_VSCROLL || m.Msg == Win32.WM_HSCROLL || m.Msg == Win32.WM_MOUSEWHEEL)
        {
            TryFireScrollEvent();
        }

        if (!suppress) base.WndProc(ref m);
    }

    private void InvalidateColumnHeaders()
    {
        foreach (Button b in FilterButtons) b.Invalidate();
        foreach (Button b in ColumnHeaderButtons) b.Invalidate();
    }

    private void ListViewF_ColumnHeaderAdded(ColumnHeader ch)
    {
        ComboBox c = new DropDownComboBox();
        c.SelectedIndexChanged += new EventHandler(c_SelectedIndexChanged);
        Filters.Add(c);
        Button b = new Button();  // drop down arrow button
        Button cb = new Button(); // column header button
        b.Size = new Size(20, 16);
        cb.BackColor = b.BackColor = SystemColors.ControlLight;
        cb.TabStop = b.TabStop = false;
        b.Paint += new PaintEventHandler(b_Paint);
        b.Click += new EventHandler(b_Click);
        cb.Click += new EventHandler(cb_Click);
        FilterButtons.Add(b);
        Controls.Add(FilterButtons[FilterButtons.Count - 1]);

        cb.Size = new Size(ch.Width - b.Width, b.Height);
        cb.Location = new Point(GetColumnPosition(ch.Index), 0);
        cb.Text = ch.Text;
        cb.Font = new Font(new FontFamily("Arial"), (float)8);
        cb.TextAlign = HorizontalAlignmentToContentAlignment(ch.TextAlign);
        ColumnHeaderButtons.Add(cb);

        Controls.Add(ColumnHeaderButtons[ColumnHeaderButtons.Count - 1]);

        Controls.Add(Filters[Filters.Count - 1]);
    }
    private void ListViewD_ColumnHeaderRangeAdded(ColumnHeader[] ch)
    {
        for (int i = 0; i < ch.Length; i++) ListViewF_ColumnHeaderAdded(ch[i]);
    }
    private void DropDownListView_ItemAdded(ListViewItem item)
    {
        for (int i = 0; i < item.SubItems.Count; i++)
        {
            ComboBox c = Filters[i];
            if (c.Items.IndexOf(item.SubItems[i].Text) < 0) c.Items.Add(item.SubItems[i].Text);
        }
    }
    private void DropDownListView_ItemRangeAdded(ListViewItem[] items)
    {
        foreach (ListViewItem i in items) DropDownListView_ItemAdded(i);
    }

    private void b_Click(object sender, EventArgs e)
    {
        Button b = (Button)sender;
        b.Parent.Select();
        int index = FilterButtons.IndexOf((Button)sender);
        ComboBox c = Filters[index];
        c.Size = new Size(Columns[index].Width, b.Height);
        c.Location = new Point(b.Location.X + b.Width - Columns[index].Width, b.Location.Y - (c.Height - b.Height));
        c.DroppedDown = true;
        c.BringToFront();
        b.BringToFront();
        ColumnHeaderButtons[index].BringToFront();
    }
    private void c_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (FilterSelectedIndexChanged != null)
        {
            FilterSelectedIndexChanged((ComboBox)sender);
        }
        Refresh();
    }
    private void cb_Click(object sender, EventArgs e)
    {
        Button b = (Button)sender;
        b.Parent.Select();

        ColumnHeader ch = Columns[ColumnHeaderButtons.IndexOf(b)];
        if (CurrentSortColumn != ch)
        {
            CurrentSortOrder = SortOrder.Ascending;
        }
        else if (CurrentSortOrder == SortOrder.Ascending)
        {
            CurrentSortOrder = SortOrder.Descending;
        }
        else CurrentSortOrder = SortOrder.Ascending;

        CurrentSortColumn = ch;
        OnColumnSort(new ColumnSortEventArgs(CurrentSortColumn, CurrentSortOrder));

        PerformSort(ColumnHeaderButtons.IndexOf((Button)sender));
        for (int i = 0; i < ColumnHeaderButtons.Count; i++) ColumnHeaderButtons[i].Invalidate();
    }
    #endregion //Events

    #region Sorting

    #region private void PerformSort(int ColumnNumber)
    private void PerformSort(int ColumnNumber)
    {
        if (SortDirections == null)
        {
            SortDirections = new SortOrder[Columns.Count];
            for (int i = 0; i < Columns.Count; i++) SortDirections[i] = SortOrder.None;
            listsort.Order = SortDirections[0] = SortOrder.Ascending;
        }
        else
        {
            for (int i = 0; i < Columns.Count; i++)
            {
                if (i != ColumnNumber)
                {
                    SortDirections[i] = SortOrder.None;
                }
            }
            switch (SortDirections[ColumnNumber])
            {
                case SortOrder.Ascending:
                    {
                        listsort.Order = SortDirections[ColumnNumber] = SortOrder.Descending;
                        break;
                    }
                default:
                    {
                        listsort.Order = SortDirections[ColumnNumber] = SortOrder.Ascending;
                        break;
                    }
            }
        }
        listsort.SortColumn = ColumnNumber;
        Sort();
    }
    #endregion

    #region public ListViewColumnSorter ColumnSorter
    public ListViewColumnSorter ColumnSorter
    {
        get
        {
            return listsort;
        }
        set
        {
            listsort = value;
            ListViewItemSorter = listsort;
        }
    }
    #endregion
    #endregion //Sorting

    #region DropDownComboBox class
    /// <summary>
    /// ComboBox that implements the column header drop-down menu.
    /// </summary>
    private class DropDownComboBox : ComboBox
    {
        protected static string All;
        protected static ComboBoxSorter sorter;

        static DropDownComboBox()
        {
            sorter = new ComboBoxSorter();
            All = "(All)";
        }
        public DropDownComboBox()
        {
            DropDownStyle = ComboBoxStyle.DropDownList;
            Sorted = false;
            Items.Add(All);
            SelectedIndex = 0;
            Visible = false;
        }
        protected override void OnLostFocus(EventArgs e)
        {
            Hide();
        }
        protected override void OnDropDown(EventArgs e)
        {
            // Copies the dropdown elements into an arraylist so that they 
            // can be sorted with our custom sorter to keep (All) at the top.
            // Then copies the items back.
            base.OnDropDown(e);
            if (!Items.Contains(All)) Items.Add(All);
            ArrayList list = new ArrayList(Items);
            list.Sort(sorter);
            Items.Clear();
            Items.AddRange(list.ToArray());
        }
        protected class ComboBoxSorter : IComparer
        {
            public int Compare(object x, object y)
            {
                if (x.ToString() == y.ToString()) return 0;
                if (x.ToString() == All) return -1;
                if (y.ToString() == All) return 1;
                return (CaseInsensitiveComparer.Default.Compare(x, y));
            }
        }
    }
    #endregion

    private void DropDownListView_ItemsCleared()
    {
        if (FilterStyle == FilterStyles.Hierarchical)
        {
            ClearUnappliedDropDowns();
        }
        else
        {
            ClearAllDropDowns();
        }
    }
    private void ClearAllDropDowns()
    {
        foreach (ComboBox c in Filters)
        {
            c.Items.Clear();
        }
    }
    private void ClearUnappliedDropDowns()
    {
        foreach (ComboBox c in Filters)
        {
            if (c.SelectedIndex < 1) c.Items.Clear();
        }
    }
    public FilterStyles FilterStyle
    {
        get
        {
            return UseFilterStyle;
        }
        set
        {
            UseFilterStyle = value;
        }
    }

    protected void OnColumnSort(EventArgs e)
    {
        if (ColumnSort != null) ColumnSort(this, e);
    }

    public class ColumnSortEventArgs : EventArgs
    {
        public ColumnSortEventArgs(ColumnHeader c, SortOrder o)
            : base()
        {
            Column = c;
            Order = o;
        }
        public ColumnHeader Column;
        public SortOrder Order;
    }

    public ContentAlignment HorizontalAlignmentToContentAlignment(HorizontalAlignment h)
    {
        switch (h)
        {
            case HorizontalAlignment.Left:
                return ContentAlignment.MiddleLeft;
            case HorizontalAlignment.Right:
                return ContentAlignment.MiddleRight;
            default:
                return ContentAlignment.MiddleCenter;
        }
    }
    public PointF[] CreatePointArray(int[] coordinates)
    {
        PointF[] p = new PointF[coordinates.Length / 2];
        for (int i = 0; i < coordinates.Length; i += 2) p[i / 2] = new PointF(coordinates[i], coordinates[i + 1]);
        return p;
    }
    public void AddColumn(string colname, int colwidth, string type)
    {
        ColumnHeader ch = new ColumnHeader();
        ch.Name = colname;
        ch.Text = colname;
        ch.Width = colwidth;
        if (type == "n") ch.TextAlign = HorizontalAlignment.Right;
        ch.Tag = type;
        Columns.Add(ch);
    }
    public void ClearAll()
    {
        Filters.Clear();
        FilterButtons.Clear();
        ColumnHeaderButtons.Clear();
        Columns.Clear();
    }

    #region Build Query
    public void SqlGetSplitUpClauses(FormattableString sql, out FormattableString selectClause, out FormattableString whereClause, out FormattableString finalClause)
    {
        int whereIndex = sql.Format.LastIndexOf("where", StringComparison.OrdinalIgnoreCase);
        int groupByIndex = sql.Format.LastIndexOf("group by", StringComparison.OrdinalIgnoreCase);
        if (groupByIndex > -1 && groupByIndex < whereIndex) groupByIndex = -1; // Could happen if there is a subquery.
        int orderByIndex = sql.Format.LastIndexOf("order by", StringComparison.OrdinalIgnoreCase);
        if (orderByIndex > -1 && orderByIndex < whereIndex) orderByIndex = -1; // Could happen if there is a subquery.

        if (groupByIndex > -1)
        {
            finalClause = sql.Substring(groupByIndex, sql.Format.Length - groupByIndex);
            selectClause = sql.Substring(0, groupByIndex);
        }
        else if (orderByIndex > -1)
        {
            finalClause = sql.Substring(orderByIndex, sql.Format.Length - orderByIndex);
            selectClause = sql.Substring(0, orderByIndex);
        }
        else
        {
            finalClause = $"";
            selectClause = sql;
        }

        if (whereIndex > -1)
        {
            whereClause = selectClause.Substring(whereIndex, selectClause.Format.Length - whereIndex);
            selectClause = selectClause.Substring(0, whereIndex);
        }
        else
        {
            whereClause = $"";
        }
    }

    public int SqlGetColNameIndex(string selectClause, int i, ref int prevColNameIndex)
    {
        int colNameIndex = selectClause.IndexOf(this.Columns[i].Text + ",", prevColNameIndex, StringComparison.Ordinal);
        if (colNameIndex == -1)
        {
            colNameIndex = selectClause.IndexOf(this.Columns[i].Text + " ", prevColNameIndex, StringComparison.Ordinal);
            if (colNameIndex == -1)
            {
                colNameIndex = selectClause.IndexOf(this.Columns[i].Text + "\r", prevColNameIndex, StringComparison.Ordinal);
                if (colNameIndex == -1)
                {
                    colNameIndex = selectClause.IndexOf(this.Columns[i].Text + "\n", prevColNameIndex, StringComparison.Ordinal);
                    if (colNameIndex == -1)
                    {
                        colNameIndex = selectClause.IndexOf(this.Columns[i].Text, prevColNameIndex, StringComparison.Ordinal);
                    }
                }
            }
        }

        prevColNameIndex = colNameIndex;
        return colNameIndex;
    }

    public FormattableString SqlGetWhereForColumn(string selectClause, int i, int colNameIndex, string boxVal, ref bool selectFound, ref bool whereClauseExists, bool castStringToVarChar = false)
    {
        var whereForColumn = new FormattableStringBuilder();
        string colName = this.Columns[i].Text;

        if (boxVal.Length > 0)
        {
            if (selectClause.Substring(colNameIndex - (colNameIndex > 0 && selectClause.Substring(colNameIndex - 1, 1).EqualsAnyOf(new[] { "[", "\"" }, StringComparison.OrdinalIgnoreCase) ? 4 : 3), 2).Equals("as", StringComparison.OrdinalIgnoreCase))
            {
                int j = colNameIndex - 5;
                do
                {
                    if (!selectFound && selectClause.Substring(j, 6).Equals("select", StringComparison.OrdinalIgnoreCase))
                    {
                        colName = selectClause.Substring(6, selectClause.IndexOf("as ", j + 4, StringComparison.OrdinalIgnoreCase) - 1 - 6);
                        selectFound = true;
                        break;
                    }
                    else if (selectClause[j] == ',')
                    {
                        // Make sure there is an equal number of opening and closing parentheses in case this is a calculated column.
                        colName = selectClause.Substring(j + 1, selectClause.IndexOf("as ", j, StringComparison.OrdinalIgnoreCase) - j - 2);
                        if (colName.Count(x => x == ')') != colName.Count(x => x == '('))
                        {
                            j--;
                            continue;
                        }
                        break;
                    }
                    j--;
                } while (true);
            }
            colName = colName.Trim();
            FormattableString colNameFormattable = FormattableStringFactory.Create(colName);

            if (whereClauseExists)
            {
                whereForColumn.Append($" and ");
            }
            else
            {
                whereForColumn.Append($"where ");
                whereClauseExists = true;
            }

            switch (this.Columns[i].Tag.ToString())
            {
                case "n":
                {
                    whereForColumn.Append(colNameFormattable).Append($" = {boxVal.TrimStart('0')}");
                    break;
                }
                case "d":
                {
                    bool parsedSuccessfully;
                    DateTime date = boxVal.ToDateTime(out parsedSuccessfully);
                    if (parsedSuccessfully)
                    {
                        whereForColumn.Append(colNameFormattable).Append($" = {date.Date}");
                    }
                    else
                    {
                        whereForColumn.Append($"1 = 2");
                    }
                    break;
                }
                case "dt":
                {
                    bool parsedSuccessfully;
                    DateTime date = boxVal.ToDateTime(out parsedSuccessfully);
                    if (parsedSuccessfully)
                    {
                        whereForColumn.Append(colNameFormattable).Append($" >= {date.Date} and ").Append(colNameFormattable).Append($" <= {date.Date.AddDays(1).AddMilliseconds(-1)}");
                    }
                    else
                    {
                        whereForColumn.Append($"1 = 2");
                    }
                    break;
                }
                case "b":
                {
                    if (boxVal.Equals("true", StringComparison.OrdinalIgnoreCase) || boxVal.Equals("yes", StringComparison.OrdinalIgnoreCase))
                    {
                        whereForColumn.Append(colNameFormattable).Append($" = true");
                    }
                    else if (boxVal.Equals("false", StringComparison.OrdinalIgnoreCase) || boxVal.Equals("no", StringComparison.OrdinalIgnoreCase))
                    {
                        whereForColumn.Append(colNameFormattable).Append($" = false");
                    }
                    else
                    {
                        whereForColumn.Append($"1 = 2");
                    }
                    break;
                }
                default:
                {
                    if (castStringToVarChar)
                    {
                        whereForColumn.Append(colNameFormattable).Append($" like cast ({boxVal + "%"} as varchar)");
                    }
                    else
                    {
                        whereForColumn.Append(colNameFormattable).Append($" like {boxVal + "%"}");
                    }
                    break;
                }
            }
        }

        return whereForColumn.ToFormattableString();
    }

    public FormattableString SetFilters(ComboBox c, FormattableString sql)
    {
        if (boxes.Length == 0)
        {
            boxes = new string[this.Columns.Count];
            for (int i = 0; i < boxes.Length; i++) boxes[i] = "";
        }

        int colIndex = Filters.IndexOf(c);
        if (c.SelectedItem == null || c.Text == "(All)")
        {
            boxes[colIndex] = "";
            AppliedFilters.Remove(colIndex);
        }
        else
        {
            boxes[colIndex] = c.Text;
            AppliedFilters.Add(colIndex);
        }


        FormattableString selectClause, whereClauseTemp, finalClause;
        SqlGetSplitUpClauses(sql, out selectClause, out whereClauseTemp, out finalClause);

        var whereClauseBuilder = new FormattableStringBuilder(whereClauseTemp);
        bool whereClauseExists = whereClauseBuilder.ContainsAnyText;
        bool selectFound = false;
        int prevColNameIndex = 0;
        for (int i = 0; i < boxes.Length; i++)
        {
            string selectClauseString = selectClause.ToString();
            int colNameIndex = SqlGetColNameIndex(selectClauseString, i, ref prevColNameIndex);
            whereClauseBuilder.Append(SqlGetWhereForColumn(selectClauseString, i, colNameIndex, boxes[i], ref selectFound, ref whereClauseExists));
        }

        if (!whereClauseExists) whereClauseBuilder = new FormattableStringBuilder($"where 1 = 2"); // Default to empty ListView.
        return new FormattableStringBuilder(selectClause).Append($" ").Append(whereClauseBuilder.ToFormattableString()).Append($" ").Append(finalClause).ToFormattableString();
    }
    #endregion

    #region Win32
    private static class Win32
    {
        public const uint WM_HSCROLL = 0x114;
        public const uint WM_VSCROLL = 0x115;
        public const uint WM_MOUSEWHEEL = 0x20A;
        public const int SB_VERT = 0x1;
        public const int SB_HORZ = 0x0;

        public const int WM_PAINT = 0x000F;
        public const int WM_ERASEBKGND = 0x0014;

        public const int WM_NOTIFY = 0x4E;
        public const int NM_CUSTOMDRAW = -12;
        public const int CDDS_PREPAINT = 0x00000001;
        public const int CDDS_POSTPAINT = 0x00000002;
        public const int CDDS_PREERASE = 0x00000003;
        public const int CDDS_POSTERASE = 0x00000004;
        public const int CDRF_DODEFAULT = 0x00000000;
        public const int CDRF_SKIPDEFAULT = 0x00000004;
        public const int CDRF_NOTIFYPOSTPAINT = 0x00000010;
        public const int CDRF_NOTIFYITEMDRAW = 0x00000020;
        public const int CDRF_NOTIFYITEMERASE = 128;
        public const int CDDS_ITEM = 0x00010000;
        public const int CDDS_ITEMPREPAINT = (CDDS_ITEM | CDDS_PREPAINT);
        public const int CDDS_ITEMPOSTPAINT = (CDDS_ITEM | CDDS_POSTPAINT);
        public const int CDDS_ITEMPREERASE = (CDDS_ITEM | CDDS_PREERASE);
        public const int CDDS_ITEMPOSTERASE = (CDDS_ITEM | CDDS_POSTERASE);
        public const int CDDS_SUBITEM = 0x00020000;

        [DllImport("user32.dll")]
        public static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

        [DllImport("user32.dll")]
        public static extern int GetScrollPos(IntPtr hWnd, int nBar);

        [DllImport("user32.dll", EntryPoint = "PostMessageA")]
        public static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetScrollInfo(IntPtr hWnd, int n, ref ScrollInfoStruct lpScrollInfo);
    }
    #endregion
}
