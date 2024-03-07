using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ww.Utilities.Extensions;

public abstract class DataGridViewPlus<T> : DataGridView
{
    public BindingList<T> Data { get; private set; }
    private readonly Dictionary<int, Action<IEnumerable<T>>> columnActions = new Dictionary<int, Action<IEnumerable<T>>>(); 
    private bool _refreshing;

    protected DataGridViewPlus()
    {
        this.CellValueChanged += dgv_CellValueChanged;
        this.CurrentCellDirtyStateChanged += dgv_CurrentCellDirtyStateChanged;
        this.AutoGenerateColumns = false;
    }

    #region Public
    public void Bind(IList<T> data)
    {
        Data = new BindingList<T>(data);
        this.DataSource = Data;
    }

    public void SetColumnAction(DataGridViewColumn column, Action<IEnumerable<T>> action)
    {
        columnActions.Update(column.Index, action);
    }
    public void SetColumnAction(int column, Action<IEnumerable<T>> action)
    {
        columnActions.Update(column, action);
    }

    public void RefreshAllRows()
    {
        foreach (DataGridViewRow row in this.Rows)
        {
            var data = (T)row.DataBoundItem;
            RefreshRow(row, data);
        }
    }

    public abstract void RefreshRow(DataGridViewRow row, T data);

    #endregion




    #region Events

    private void dgv_CellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
        if (columnActions.ContainsKey(e.ColumnIndex)) columnActions[e.ColumnIndex].Invoke(Data);
            
            //if (e.ColumnIndex == colNewLocation.Index)
        //{
        //    colNewAction.Visible = _details.Any(x => x.ShowNewAction);
        //}
    }

    private void dgv_CurrentCellDirtyStateChanged(object sender, EventArgs e)
    {
        if (ReadOnly) return;
        if (!(CurrentCell is DataGridViewTextBoxCell))
        {
            if (!_refreshing)
            {
                _refreshing = true;
                CommitEdit(DataGridViewDataErrorContexts.Commit);
                RefreshAllRows();
                _refreshing = false;
            }
            else
            {
                CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }
    }
    #endregion


        #region Styling
        protected enum CellStyle
        {
            Normal,
            Disabled,
            Invisible,
        }

        protected void SetCellStyle(DataGridViewCell cell, CellStyle style)
        { 
            //if (cell is DataGridViewButtonCell) return;
            var cellstyle = cell.OwningColumn.InheritedStyle ?? cell.OwningColumn.DefaultCellStyle;
            if (this.ReadOnly && style == CellStyle.Normal) style = CellStyle.Disabled;
            switch (style)
            {
                case CellStyle.Normal:
                    cell.ReadOnly = false;
                    if (cell is DataGridViewButtonCell)
                    {
                        var button = (DataGridViewDisableButtonCell)cell;
                        button.Enabled = true;
                        button.Visible = true;
                        button.Style.NullValue = "(Select)";
                        if (button.Value != null && button.Value.ToString() == "") button.Value = null;
                    }
                    else
                    {
                        cell.Style = new DataGridViewCellStyle(cellstyle);
                        if (cell is DataGridViewComboBoxCell)
                        {
                            var combo = cell as DataGridViewComboBoxCell;
                            combo.DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox;
                        }
                        else if (cell is DataGridViewCheckBoxCell)
                        {
                            var box = (DataGridViewCheckBoxCell)cell;
                            box.FlatStyle = FlatStyle.Standard;
                        }
                    }
                    break;
                case CellStyle.Disabled:
                    cell.ReadOnly = true;
                    if (cell is DataGridViewDisableButtonCell)
                    {
                        var button = (DataGridViewDisableButtonCell)cell;
                        //button.FlatStyle = FlatStyle.Flat;
                        button.Enabled = false;
                        button.Visible = true;
                        button.Style.NullValue = " ";
                    }
                    else
                    {
                        cell.Style = new DataGridViewCellStyle(cellstyle) {ForeColor = Color.Gray};
                        if (cell is DataGridViewComboBoxCell)
                        {
                            var combo = cell as DataGridViewComboBoxCell;
                            combo.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
                        }
                        else if (cell is DataGridViewCheckBoxCell)
                        {
                            var box = (DataGridViewCheckBoxCell)cell;
                            box.FlatStyle = FlatStyle.Flat;
                        }
                    }
                    break;
                case CellStyle.Invisible:
                    cell.ReadOnly = true;
                    if (cell is DataGridViewDisableButtonCell)
                    {
                        var button = (DataGridViewDisableButtonCell)cell;
                        button.Enabled = false;
                        button.Visible = false;
                        button.Style.NullValue = " ";
                    }
                    else if (cell is DataGridViewCheckBoxCell)
                    {
                        var box = (DataGridViewCheckBoxCell)cell;
                        box.Style.ForeColor = SystemColors.AppWorkspace;
                        box.Style.BackColor = SystemColors.AppWorkspace;
                        box.FlatStyle = FlatStyle.Flat;
                    }
                    else
                    {
                        cell.Style = new DataGridViewCellStyle(cellstyle)
                                         {
                                             ForeColor = SystemColors.AppWorkspace,
                                             BackColor = SystemColors.AppWorkspace,
                                             SelectionForeColor = cellstyle.SelectionBackColor,
                                         };
                        if (cell is DataGridViewComboBoxCell)
                        {
                            var combo = cell as DataGridViewComboBoxCell;
                            combo.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("style");
            }
        }
        #endregion
}
