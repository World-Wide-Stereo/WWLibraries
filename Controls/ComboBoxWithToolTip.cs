using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

public class ComboBoxWithToolTip : ComboBox
{
    private bool _droppedDown;
    private List<string> _toolTips = new List<string>();
    private int _prevDrawIndex = -1;
    private ToolTip _tip = new ToolTip();

    public ComboBoxWithToolTip()
    {
        this.DisplayMember = "Key";
        this.ValueMember = "Value";

        this.DropDownStyle = ComboBoxStyle.DropDownList;
        this.DropDown += new EventHandler(DropDownListWithToolTip_DropDown);
        this.DropDownClosed += new EventHandler(DropDownListWithToolTip_DropDownClosed);

        base.DrawMode = DrawMode.OwnerDrawFixed;

        _tip.AutomaticDelay = 0;
        _tip.AutoPopDelay = 10000;
        _tip.InitialDelay = 10;
        _tip.IsBalloon = true;
        _tip.ReshowDelay = 10;
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        if (e.Index >= 0)
        {
            object item = e.Index < Items.Count ? Items[e.Index] : this.Text;

            Rectangle bounds = e.Bounds;

            Rectangle textBounds = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
            if (RightToLeft == RightToLeft.Yes)
            {
                // For a RightToLeft checked list box, we want the text to be drawn at the left. So we override the X position.
                textBounds.X = bounds.X;
            }

            // Setup text font, color, and text.
            string text = "";
            Color backColor;
            Color foreColor;
            if (Enabled)
            {
                backColor = BackColor;
                foreColor = ForeColor;
            }
            else
            {
                backColor = SystemColors.Control;
                foreColor = SystemColors.GrayText;
            }
            Font font = this.Font;

            object value = FilterItemOnProperty(item);

            if (value != null)
            {
                text = value.ToString();
            }

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                backColor = SystemColors.Highlight;
                foreColor = SystemColors.HighlightText;
            }

            // Draw the text.
            using (Brush b = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(b, textBounds);
            }

            Rectangle stringBounds = new Rectangle(textBounds.X + 1, textBounds.Y, textBounds.Width - 1, textBounds.Height);

            using (StringFormat format = new StringFormat())
            {
                // Adjust string format for Rtl controls.
                if (RightToLeft == RightToLeft.Yes)
                {
                    format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                }

                format.FormatFlags |= StringFormatFlags.NoWrap;

                // Do actual drawing.
                using (SolidBrush brush = new SolidBrush(foreColor))
                {
                    e.Graphics.DrawString(text, font, brush, stringBounds, format);
                }
            }

            // Draw the focus rect if required.
            if ((e.State & DrawItemState.Focus) == DrawItemState.Focus && (e.State & DrawItemState.NoFocusRect) != DrawItemState.NoFocusRect)
            {
                ControlPaint.DrawFocusRectangle(e.Graphics, textBounds, foreColor, backColor);
            }
        }

        if (!this.DesignMode && e.Index != -1 && (e.State & DrawItemState.Selected) == DrawItemState.Selected)
        {
            if (_droppedDown && _prevDrawIndex != e.Index)
            {
                _tip.Show(_toolTips[e.Index], this);
                _prevDrawIndex = e.Index;
            }
        }
    }

    private void DropDownListWithToolTip_DropDownClosed(object sender, EventArgs e)
    {
        _droppedDown = false;
        _tip.Hide(this);
    }

    private void DropDownListWithToolTip_DropDown(object sender, EventArgs e)
    {
        _droppedDown = true;
    }

    public void Add(string text, object value, string toolTip)
    {
        base.Items.Add(new KeyValuePair<string, object>(text, value));
        _toolTips.Add(toolTip);
    }

    public new KeyValuePair<string, object> SelectedItem
    {
        get
        {
            KeyValuePair<string, object> result = new KeyValuePair<string, object>();

            if (this.SelectedIndex != -1)
            {
                result = (KeyValuePair<string, object>)base.SelectedItem;
            }

            return result;
        }
    }

    public new string SelectedText
    {
        get
        {
            string result = "";

            if (this.SelectedIndex != -1)
            {
                result = SelectedItem.Key;
            }

            return result;
        }
    }

    public new object SelectedValue
    {
        get
        {
            object result = null;

            if (this.SelectedIndex != -1)
            {
                result = SelectedItem.Value;
            }

            return result;
        }
    }

    public string ToolTip(int index)
    {
        string result = "";
        
        if (index >= 0 && index < _toolTips.Count)
        {
            result = _toolTips[index];
        }

        return result;
    }

    public bool ToolTipIsBallon
    {
        get { return _tip.IsBalloon; }
        set { _tip.IsBalloon = value; }
    }
}
