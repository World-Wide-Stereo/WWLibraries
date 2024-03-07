using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualBasic.PowerPacks;
using ww.Utilities.Extensions;

public partial class EnumComboBox : ComboBox
{
    public Type EnumType { get; private set; }

    public EnumComboBox()
    {
        InitializeComponent();
    }

    public void SetEnum<T>(T defaultValue, bool addBlankDropdownItem = false, IEnumerable<T> valuesToAdd = null, bool showFullLabel = true, bool orderByName = false) where T : struct, IConvertible
    {
        if (this.EnumType != typeof(T)) SetEnum(typeof(T), addBlankDropdownItem, valuesToAdd == null ? null : valuesToAdd.Select(x => (Enum)(object)x), showFullLabel, orderByName);
        this.Text = (showFullLabel) ? (defaultValue as Enum).GetLabel() : (defaultValue as Enum).GetAbbreviation();
    }
    public void SetEnum<T>(bool addBlankDropdownItem = false, IEnumerable<T> valuesToAdd = null, bool showFullLabel = true, bool orderByName = false) where T : struct, IConvertible
    {
        SetEnum(typeof(T), addBlankDropdownItem, valuesToAdd == null ? null : valuesToAdd.Select(x => (Enum)(object)x), showFullLabel, orderByName);
    }
    private void SetEnum(Type type, bool addBlankDropdownItem = false, IEnumerable<Enum> valuesToAdd = null, bool showFullLabel = true, bool orderByName = false)
    {
        if (type.IsEnum)
        {
            EnumType = type;
            ValueMember = "Value";
            DisplayMember = "Display";

            var data = (valuesToAdd ?? Enum.GetValues(EnumType).Cast<Enum>()).Select(x => new
            {
                Display = showFullLabel ? x.GetLabel() : x.GetAbbreviation(),
                Value = x,
            });
            data = orderByName ? data.OrderBy(x => x.Display) : data.OrderBy(x => Convert.ToInt32(x.Value));

            var dataList = data.ToList();
            if (addBlankDropdownItem)
            {
                dataList.Insert(0, new { Display = "", Value = (Enum)null });
            }
            DataSource = dataList;
            this.Text = dataList[0].Display;
        }
        else
        {
            EnumType = null;
        }
    }
    public void SetEnums<T>(IEnumerable<T> options) where T : struct, IConvertible
    {
        //if (typeof(T) == EnumType)
        //{
            EnumType = typeof(T);
            ValueMember = "Value";
            DisplayMember = "Display";
            var data = options.Cast<Enum>().Select(x => new
            {
                Display = x.GetLabel(),
                Value = x,
            }).ToList();
            DataSource = data;
            this.Text = data.First().Display;
        //}
        //else
        //{
        //    throw new InvalidCastException("Can't convert type of " + typeof(T) + " to value from " + EnumType.Name + " to " + typeof(T).Name);
        //}
    }

    public void SetEnums<T>(IEnumerable<T> options, T defaultOption) where T : struct, IConvertible
    {
        SetEnum(defaultOption, valuesToAdd: options);
    }

    public T EnumValue<T>() where T : struct, IConvertible
    {
        if (typeof (T) != EnumType) throw new InvalidCastException("Can't convert value from " + EnumType.Name + " to " + typeof (T).Name);

        return (T)this.SelectedValue;
    }

    public void ConfigureForDataRepeater<T>(DataRepeater repeater, Func<T, string> getData, Action<EnumComboBox, T> setData) where T : class
    {
        repeater.ItemCloned += repeater_ItemCloned;
        repeater.DrawItem += (sender, args) => repeater_DrawItem(args, getData);
        this.SelectedIndexChanged += (sender, args) => box_SelectedIndexChanged(sender, setData);
    }

    private void box_SelectedIndexChanged<T>(object sender, Action<EnumComboBox, T> setData) where T : class
    {
        var box = (EnumComboBox)sender;
        var dataRepeaterItem = (DataRepeaterItem)box.Parent;
        var dataRepeater = (DataRepeater)box.Parent.Parent;
        var dataItem = (((BindingSource)dataRepeater.DataSource))[dataRepeaterItem.ItemIndex] as T;
        setData(box, dataItem);
    }

    private void repeater_DrawItem<T>(DataRepeaterItemEventArgs e, Func<T, string> getData) where T : class
    {
        var box = (EnumComboBox)e.DataRepeaterItem.Controls.Find(this.Name, false)[0];
        var dataRepeaterItem = (DataRepeaterItem)box.Parent;
        var dataRepeater = (DataRepeater)box.Parent.Parent;
        var dataItem = (((BindingSource)dataRepeater.DataSource))[dataRepeaterItem.ItemIndex] as T;
        box.Text = getData(dataItem);
    }

    private void repeater_ItemCloned(object sender, DataRepeaterItemEventArgs e)
    {
        var box = (EnumComboBox)e.DataRepeaterItem.Controls.Find(this.Name, false)[0];
        box.SetEnum(EnumType);
    }
}
