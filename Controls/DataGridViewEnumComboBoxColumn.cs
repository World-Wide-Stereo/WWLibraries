using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ww.Utilities.Extensions;

public partial class DataGridViewEnumComboBoxColumn : DataGridViewComboBoxColumn // where T : struct, IConvertible
{
    public Type EnumType { get; private set; }

    public void SetEnum<T>() where T : struct, IConvertible
    {
        if (typeof (T).IsEnum)
        {
            EnumType = typeof (T);
            ValueMember = "Value";
            DisplayMember = "Display";
            var data = Enum.GetValues(EnumType).Cast<Enum>().Select(x => new
                                                               {
                                                                   Display = x.GetLabel(),
                                                                   Value = x,
                                                               }).ToList();
            DataSource = data;
            this.CellTemplate.Value = data.First().Value;
        }
        else
        {
            EnumType = null;
        }
    }
    public void SetEnum<T>(IEnumerable<T> options) where T : struct, IConvertible
    {
        if (typeof(T).IsEnum)
        {
            EnumType = typeof(T);
            ValueMember = "Value";
            DisplayMember = "Display";
            var data = options.Cast<Enum>().Select(x => new
                                                            {
                                                                Display = x.GetLabel(),
                                                                Value = x,
                                                            }).ToList();
            DataSource = data;
            this.CellTemplate.Value = data.First().Value;
        }
        else
        {
            EnumType = null;
        }
    }

    public void SetEnumsForCell<T>(DataGridViewComboBoxCell cell, IEnumerable<T> options) where T : struct, IConvertible
    {
        if (typeof(T) == EnumType)
        {
            cell.ValueMember = "Value";
            cell.DisplayMember = "Display";
            var data = options.Cast<Enum>().Select(x => new
                                                            {
                                                                Display = x.GetLabel(),
                                                                Value = x,
                                                            }).ToList();
            cell.DataSource = data;
            cell.Value = data.First().Value;
        }
        else
        {
            throw new InvalidCastException("Can't convert cell value from " + EnumType.Name + " to " + typeof(T).Name);
        }
    }

    public T GetEnumForCell<T>(DataGridViewComboBoxCell cell) where T : struct, IConvertible
    {
        if (typeof(T) != EnumType) throw new InvalidCastException("Can't convert cell value from " + EnumType.Name + " to " + typeof(T).Name);

        return (T)cell.Value;
    }

    public DataGridViewEnumComboBoxColumn()
    {
        InitializeComponent();
    }

    public DataGridViewEnumComboBoxColumn(IContainer container)
    {
        container.Add(this);
        InitializeComponent();
    }

    protected override void OnDataGridViewChanged()
    {
        if (this.DataGridView != null) this.DataGridView.CellFormatting += DataGridView_CellFormatting;
    }

    public void DataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
        var grid = sender as DataGridView;
        if (grid != null && grid.Columns[e.ColumnIndex] == this)
        {
            var cell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewComboBoxCell;
            if (cell.ReadOnly || !grid.Enabled) cell.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
            else cell.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
        }
    }
}
