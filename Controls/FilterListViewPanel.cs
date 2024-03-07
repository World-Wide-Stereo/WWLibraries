using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using ww.Utilities;

/// <summary>
/// FilterListViewPanel uses TextBoxes above each ListView column to filter the data shown in the ListView.
/// Use this class together with QueryThread.
/// </summary>
public class FilterListViewPanel : Panel
{
    internal readonly object resultsLock = new object();
    internal Queue<ListViewItem> results;
    public bool hasResults { get; internal set; }
    public DropDownListView lv;
    private PanelWithScrollEvents boxesPanel;
    public TextBox[] boxes;
    private Panel padding;
    private FormattableString selectOrig;
    private FormattableString selectUpdated;
    public FormattableString select
    {
        get { return selectUpdated; }
        set
        {
            selectOrig = value;
            selectUpdated = value;
        }
    }
    public FormattableString conditions { get; set; } = $"";
    public FormattableString specialClause { get; set; } = $"";
    public bool searchWithAllTextBoxesBlanked { get; set; } = false;
    public event RefreshEventHandler Refresh;
    public bool UseCaps { get; set; } = true;
    public int ReturnRows { get; set; } = 50;

    public delegate void UpdateSelectForTextBoxHandler(object sender, UpdateSelectForTextBoxEventArgs e);
    public event UpdateSelectForTextBoxHandler UpdateSelectForTextBox;
    public class UpdateSelectForTextBoxEventArgs : EventArgs
    {
        public TextBox[] boxes { get; set; }
        public FormattableStringBuilder whereClauseBuilder { get; set; }
        public int i { get; set; }
        public int colNameIndex { get; set; }
        public FormattableString selectClause { get; set; }
        public bool selectFound { get; set; }
        public bool whereClauseExists { get; set; }
    }

    public FilterListViewPanel()
    {
        this.Dock = DockStyle.Fill;

        lv = new DropDownListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
            Location = new Point(0, 20),
            Height = Height - 20,
            Width = Width,
            FullRowSelect = true,
            Scrollable = true,
            MultiSelect = true,
            HideSelection = false,
            TabIndex = 2,
        };
        lv.FilterSelectedIndexChanged += new DropDownListView.FilterSelectedIndexChangedDelegate(lv_FilterSelectedIndexChanged);
        lv.ColumnWidthChanged += new ColumnWidthChangedEventHandler(lv_ColumnWidthChanged);
        lv.ScrollPositionChanged += new ScrollEventHandler(lv_ScrollPositionChanged);
        Controls.Add(lv);

        results = new Queue<ListViewItem>();
    }

    private readonly object updateListViewLock = new object();
    internal void UpdateListView(bool clearList)
    {
        lock (updateListViewLock)
        {
            if (clearList) lv.Items.Clear();
            var resultsList = new List<ListViewItem>(ReturnRows);
            lock (resultsLock)
            {
                while (results.Count > 0 && resultsList.Count < ReturnRows)
                {
                    resultsList.Add(results.Dequeue());
                }
            }
            if (resultsList.Count > 0)
            {
                lv.Items.AddRange(resultsList.ToArray());
            }
            else if (lv.Items.Count == 0)
            {
                lv.Items.Add(new ListViewItem(new string[lv.Columns.Count]));
            }

            Cursor.Current = Cursors.Default;
            OnUpdateFinished();
            lv.TryFireScrollEvent();
        }
    }

    #region filtering text boxes
    public void CreateBoxes()
    {
        if (boxesPanel == null)
        {
            boxesPanel = new PanelWithScrollEvents
            {
                AutoScroll = true,
                Dock = DockStyle.Fill,
                PreventFocusChangeScrolling = true,
                PreventMouseWheelScrolling = true,
                TabIndex = 0,
                TabStop = false,
            };
            Controls.Add(boxesPanel);

            boxes = new TextBox[lv.Columns.Count];
            for (int i = 0; i < lv.Columns.Count; i++)
            {
                boxes[i] = new TextBoxExtended
                {
                    AutoSelect = true,
                    Width = lv.Columns[i].Width,
                    Height = 20,
                    Location = new Point(GetColumnHorizontalPosition(i), 0),
                    TabIndex = lv.Columns[i].Width == 0 ? 1 : 0,
                };
                boxes[i].TextChanged += new EventHandler(box_TextChanged);
                boxes[i].KeyDown += new KeyEventHandler(box_KeyDown);
                if (UseCaps) boxes[i].CharacterCasing = CharacterCasing.Upper;
                boxesPanel.Controls.Add(boxes[i]);
            }

            // Pad the boxesPanel with extra space to prevent scrolling on the panel from looking weird when scrolled to the end.
            padding = new Panel { Width = SystemInformation.VerticalScrollBarWidth + 4, Height = 20, Location = new Point(boxes[lv.Columns.Count - 1].Location.X + boxes[lv.Columns.Count - 1].Width, 0) };
            boxesPanel.Controls.Add(padding);
        }
    }

    /// <summary>
    /// Sets the MaxLength property on the TextBoxes.
    /// </summary>
    /// <param name="lengths">The key is the column index within the <see cref="FilterListViewPanel"/> while the value is the max length of the TextBox.</param>
    public void SetTextBoxMaxLengths(Dictionary<int, int> lengths)
    {
        foreach (KeyValuePair<int, int> length in lengths)
        {
            boxes[length.Key].MaxLength = length.Value;
        }
    }

    public void ForceRefresh()
    {
        bool allBoxesEmpty = true;
        for (int i = 0; i < lv.Columns.Count; i++)
        {
            if (boxes[i].Text.Length > 0)
            {
                allBoxesEmpty = false;
                break;
            }
        }

        if (!allBoxesEmpty)
        {
            selectUpdated = UpdateSelect();
            Refresh(this, new RefreshEventArgs());
        }
    }
    private void box_TextChanged(object sender, EventArgs e)
    {
        selectUpdated = UpdateSelect();
        Refresh(sender, new RefreshEventArgs());
    }

    private void box_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Down)
        {
            lv.Focus();
            if (lv.Items.Count > 0 && lv.SelectedItems.Count == 0)
            {
                lv.Items[0].Selected = true;
            }
        }
    }

    private FormattableString UpdateSelect()
    {
        FormattableString selectClause, whereClause, finalClause;
        lv.SqlGetSplitUpClauses(selectOrig, out selectClause, out whereClause, out finalClause);

        // Replace the original whereClause with conditions.
        FormattableStringBuilder whereClauseBuilder;
        bool whereClauseExists;
        if (this.conditions.Format.Length > 0)
        {
            whereClauseBuilder = new FormattableStringBuilder($"where ").Append(this.conditions);
            whereClauseExists = true;
        }
        else
        {
            whereClauseBuilder = new FormattableStringBuilder();
            whereClauseExists = false;
        }

        bool selectFound = false;
        int prevColNameIndex = 0;
        bool boxesHaveValues = false;
        for (int i = 0; i < boxes.Length; i++)
        {
            string selectClauseString = selectClause.ToString();
            int colNameIndex = lv.SqlGetColNameIndex(selectClauseString, i, ref prevColNameIndex);

            if (boxes[i].TextLength > 0)
            {
                boxesHaveValues = true;

                if (UpdateSelectForTextBox == null)
                {
                    whereClauseBuilder.Append(lv.SqlGetWhereForColumn(selectClauseString, i, colNameIndex, boxes[i].Text, ref selectFound, ref whereClauseExists));
                }
                else
                {
                    var e = new UpdateSelectForTextBoxEventArgs
                    {
                        boxes = boxes,
                        whereClauseBuilder = whereClauseBuilder,
                        i = i,
                        colNameIndex = colNameIndex,
                        selectClause = selectClause,
                        selectFound = selectFound,
                        whereClauseExists = whereClauseExists,
                    };
                    UpdateSelectForTextBox(this, e);
                    selectFound = e.selectFound;
                    whereClauseExists = e.whereClauseExists;
                }
            }
        }
        whereClause = whereClauseBuilder.ToFormattableString();

        if ((!boxesHaveValues || !whereClauseExists) && !searchWithAllTextBoxesBlanked)
        {
            // Replaces only the first occurence of "select" with "select top 0".
            selectClause = FormattableStringFactory.Create("select top 0" + selectClause.Format.Substring(selectClause.Format.IndexOf("select", StringComparison.OrdinalIgnoreCase) + 6), selectClause.GetArguments());
        }

        return new FormattableStringBuilder(selectClause).Append($" ").Append(whereClause).Append($" ").Append(finalClause).Append($" ").Append(specialClause).ToFormattableString();
    }

    public void UpdateSelectForTextBoxDefault(UpdateSelectForTextBoxEventArgs e, bool castStringToVarChar = false)
    {
        bool selectFound = e.selectFound;
        bool whereClauseExists = e.whereClauseExists;

        e.whereClauseBuilder.Append(lv.SqlGetWhereForColumn(e.selectClause.ToString(), e.i, e.colNameIndex, e.boxes[e.i].Text, ref selectFound, ref whereClauseExists, castStringToVarChar));

        e.selectFound = selectFound;
        e.whereClauseExists = whereClauseExists;
    }

    private int GetColumnHorizontalPosition(int col)
    {
        int val = 0;
        if (IsHandleCreated)
        {
            if (lv.Items.Count == 0)
            {
                val = 0;
            }
            else
            {
                val = lv.GetItemRect(0).Left; // hz scrolling can make this negative!
            }
        }
        for (int i = 0; i < col; i++)
        {
            val += lv.Columns[i].Width;
        }
        return val;
    }

    private void lv_FilterSelectedIndexChanged(ComboBox c)
    {
        int col = lv.Filters.IndexOf(c);
        if (c.SelectedItem == null || c.Text == "(All)")
        {
            boxes[col].Text = "";
            lv.AppliedFilters.Remove(col);
        }
        else
        {
            boxes[col].Text = c.Text.Trim();
            lv.AppliedFilters.Add(col);
        }
    }

    private void lv_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
    {
        if (boxes != null)
        {
            boxes[e.ColumnIndex].Width = lv.Columns[e.ColumnIndex].Width;
            for (int i = e.ColumnIndex + 1; i < lv.Columns.Count; i++)
            {
                boxes[i].Location = new Point(GetColumnHorizontalPosition(i), 0);
            }
            padding.Location = new Point(boxes[lv.Columns.Count - 1].Location.X + boxes[lv.Columns.Count - 1].Width, 0);
        }
    }

    private void lv_ScrollPositionChanged(object sender, ScrollEventArgs e)
    {
        if (e.ScrollOrientation == ScrollOrientation.HorizontalScroll)
        {
            boxesPanel.HorizontalScrollPosition = lv.HorizontalScrollPosition;
        }
    }
    #endregion

    #region UpdateStarted and UpdateFinished Events
    // events that denote the start and end of a query
    public event EventHandler UpdateStarted;
    public event EventHandler UpdateFinished;

    protected void OnUpdateStarted()
    {
        if (UpdateStarted != null)
        {
            UpdateStarted(this, new EventArgs());
        }
    }
    protected void OnUpdateFinished()
    {
        if (UpdateFinished != null)
        {
            UpdateFinished(this, new EventArgs());
        }
    }
    #endregion
}

public delegate void RefreshEventHandler(object sender, RefreshEventArgs e);
public class RefreshEventArgs : EventArgs { }
