using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using Controls;
using Controls.ComboBoxOfCheckBoxes;

public class QuickControl
{
    public static Font font = new Font("Microsoft Sans Serif", 8);
    public static Color fontcolor = Color.Black;
    public static bool enable = true;
    public static DateTimePickerFormat dtformat = DateTimePickerFormat.Short;
    public static ContentAlignment align = ContentAlignment.MiddleLeft;

    public static Label label(int x, int y, int size, string value, Control addto, int numberLines = 1, float fontSize = 8, FontStyle fontStyle = FontStyle.Regular)
    {
        var l = new Label
        {
            Location = new Point(x, y),
            Size = new Size(size, 20 * numberLines),
            Font = font,
            Text = value,
            ForeColor = fontcolor,
            TextAlign = align,
        };
        if (fontStyle != FontStyle.Regular || fontSize != 8)
        {
            l.Font = new Font(font.FontFamily.Name, fontSize, fontStyle);
        }
        if (addto != null) addto.Controls.Add(l);
        return l;
    }

    public static LinkLabel link(int x, int y, int size, string value, string f, Control addto, object tag = null)
    {
        var l = new LinkLabel
        {
            Location = new Point(x, y),
            Size = new Size(size, 20),
            Text = value
        };
        if (tag != null) l.Tag = tag;
        //AttachEvent(l,"Click",f);
        if (addto != null) addto.Controls.Add(l);
        return l;
    }

    public static CheckBox ckbox(int x, int y, int size, bool value, Control addto, string text = "", ContentAlignment contentAlignment = 0, object tag = null)
    {
        var c = new CheckBox
        {
            Location = new Point(x, y),
            Size = new Size(size, 18),
            Text = text,
            Enabled = enable,
            Checked = value,
        };
        if (contentAlignment != 0) c.CheckAlign = contentAlignment;
        if (tag != null) c.Tag = tag;
        if (addto != null) addto.Controls.Add(c);
        return c;
    }

    public static TextBoxExtended txbox(int x, int y, int size, string value, Control addto, object tag = null, int numberLines = 1, TextBoxExtended.InputTypeEnum inputType = TextBoxExtended.InputTypeEnum.String, bool allowNegativeNumbers = true)
    {
        var t = new TextBoxExtended
        {
            Location = new Point(x, y),
            Size = new Size(size, 20 * numberLines),
            Text = value,
            Enabled = enable,
            Font = font,
            InputType = inputType,
            AllowNegativeNumbers = allowNegativeNumbers
        };
        if (numberLines > 1) t.Multiline = true;
        if (tag != null) t.Tag = tag;
        if (addto != null) addto.Controls.Add(t);
        return t;
    }

    public static RicherTextBox.RicherTextBox rtfBox(int x, int y, int size, string value, Control addto, object tag = null, int numberLines = 1, TextBoxExtended.InputTypeEnum inputType = TextBoxExtended.InputTypeEnum.String, bool allowNegativeNumbers = true, bool menusEnabled = true)
    {
        var r = new RicherTextBox.RicherTextBox
        {
            Location = new Point(x, y),
            Size = new Size(size, 20 * numberLines),
            Rtf = value,
            Enabled = enable,
            Font = font,
            //We don't want to allow loading and saving for individual rtf boxes.
            GroupSaveAndLoadVisible = false,
            ToolStripVisible = menusEnabled,
            FindReplaceVisible = menusEnabled
        };
        if (tag != null)
            r.Tag = tag;
        if (addto != null)
            addto.Controls.Add(r);
        return r;
    }

    public static Button button(int x, int y, int size, string value, Control addto, object tag = null)
    {
        var b = new Button
        {
            Location = new Point(x, y),
            Size = new Size(size, 20),
            Text = value,
            Font = font
        };
        if (tag != null) b.Tag = tag;
        if (addto != null) addto.Controls.Add(b);
        return b;
    }

    public static DateTimePicker date(int i, int j, int s, DateTime value, Control addto)
    {
        var d = new DateTimePicker
        {
            Format = dtformat,
            Location = new Point(i, j),
            Size = new Size(20, s),
            Value = value,
            Enabled = enable
        };
        if (addto != null) addto.Controls.Add(d);
        return d;
    }

    public static DateTimePicker time(int i, int j, int s, DateTime value, Control addto)
    {
        var t = new DateTimePicker
        {
            ShowUpDown = true,
            Format = DateTimePickerFormat.Time,
            Location = new Point(i, j),
            Size = new Size(20, s),
            Value = value,
            Enabled = enable
        };
        if (addto != null) addto.Controls.Add(t);
        return t;
    }

    /*public ColorDialog color(int i, int j, int s, bool e, Color value, TabPage tab, Panel panel, Form form)
	{
		ColorDialog c = new  ColorDialog();
		c.Location = new System.Drawing.Point(i, j);
		c.Size = new System.Drawing.Size(20, s);
		c.Color = value;
		c.Enabled = e;
		if(tab!=null) tab.Controls.AddRange(new System.Windows.Forms.Control[] { c });
		else if (form!=null) form.Controls.AddRange(new System.Windows.Forms.Control[] { c });
		else panel.Controls.AddRange(new System.Windows.Forms.Control[] { c });
		return(c);
	}*/

    public static ComboBox combo(int x, int y, int size, IEnumerable<object> range, Control addto, ComboBoxStyle style = ComboBoxStyle.DropDownList, string textToSelect = null, bool sorted = false)
    {
        var c = new ComboBox
        {
            DropDownStyle = style,
            Location = new Point(x, y),
            Size = new Size(size, 20),
            Sorted = sorted,
            Enabled = enable
        };
        c.Items.AddRange(range.ToArray());
        if (textToSelect != null) c.Text = textToSelect;
        if (addto != null) addto.Controls.Add(c);
        return c;
    }
    public static ComboBox combo(int x, int y, int size, List<ListItem> dataSource, Control addto, ComboBoxStyle style = ComboBoxStyle.DropDownList, dynamic itemToSelect = null, ItemToSelectType? itemToSelectType = null)
    {
        var c = new ComboBox
        {
            DropDownStyle = style,
            Location = new Point(x, y),
            Size = new Size(size, 20),
            DataSource = dataSource.ToList(),
            ValueMember = "Value",
            DisplayMember = "Text",
            Enabled = enable,
        };
        if (addto != null) addto.Controls.Add(c);
        if (itemToSelect != null && itemToSelectType != null)
        {
            switch (itemToSelectType)
            {
                case ItemToSelectType.DisplayText:
                {
                    c.Text = itemToSelect;
                    break;
                }
                case ItemToSelectType.ListItemValue:
                {
                    var listItemToSelect = dataSource.Find(z => (dynamic)z.Value == itemToSelect);
                    if (listItemToSelect != null) c.Text = listItemToSelect.Text;
                    break;
                }
                case ItemToSelectType.ListItemText:
                {
                    var listItemToSelect = dataSource.Find(z => z.Text == itemToSelect);
                    if (listItemToSelect != null) c.Text = listItemToSelect.Text;
                    break;
                }
                default:
                {
                    throw new ArgumentException("Unsupported ItemToSelectType: " + itemToSelectType);
                }
            }
        }
        return c;
    }
    public static ComboBox comboOfCheckBoxes(int x, int y, int size, List<ListItem> dataSource, Control addto, ComboBoxStyle style = ComboBoxStyle.DropDownList, List<dynamic> itemsToSelect = null, ItemToSelectType? itemToSelectType = null)
    {
        var c = new ComboBoxOfCheckBoxes
        {
            DropDownStyle = style,
            DropDownHeight = 160,
            Location = new Point(x, y),
            Size = new Size(size, 20),
            Enabled = enable,
        };
        c.Items.AddRange(dataSource.Select(z => z.Text).ToArray());
        int indexOffset = c.AddAllItem ? 2 : 1;
        for (int i = 0; i < dataSource.Count; i++)
        {
            c.CheckBoxItems[i + indexOffset].Tag = dataSource[i].Value;
        }
        if (addto != null) addto.Controls.Add(c);
        if (itemsToSelect != null && itemToSelectType != null)
        {
            foreach (var itemToSelect in itemsToSelect)
            {
                switch (itemToSelectType)
                {
                    case ItemToSelectType.DisplayText:
                    {
                        c.Text = itemToSelect;
                        break;
                    }
                    case ItemToSelectType.ListItemValue:
                    {
                        var listItemToSelect = dataSource.Find(z => (dynamic)z.Value == itemToSelect);
                        if (listItemToSelect != null)
                        {
                            var checkBoxToSelect = c.CheckBoxItems.Find(z => z.Tag == listItemToSelect.Value);
                            if (checkBoxToSelect != null) checkBoxToSelect.Checked = true;
                        }
                        break;
                    }
                    case ItemToSelectType.ListItemText:
                    {
                        var listItemToSelect = dataSource.Find(z => z.Text == itemToSelect);
                        if (listItemToSelect != null)
                        {
                            var checkBoxToSelect = c.CheckBoxItems.Find(z => z.Tag == listItemToSelect.Value);
                            checkBoxToSelect.Checked = true;
                        }
                        break;
                    }
                    default:
                    {
                        throw new ArgumentException("Unsupported ItemToSelectType: " + itemToSelectType);
                    }
                }
            }
        }
        return c;
    }

    public enum ItemToSelectType
    {
        DisplayText,
        ListItemValue,
        ListItemText,
    }

    public static RadioButton radio(int x, int y, int size, bool value, string label, Control addto)
    {
        var r = new RadioButton
        {
            Location = new Point(x, y),
            Size = new Size(size, 18),
            Checked = value,
            Text = label
        };
        if (addto != null) addto.Controls.Add(r);
        return r;
    }

    public static Panel panel(int x, int y, int width, int length, Control addto)
    {
        var p = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(length, width)
        };
        if (addto != null) addto.Controls.Add(p);
        return p;
    }

    public static PictureBox image(int x, int y, int width, int length, string fileName, Control addto, object tag = null)
    {
        var p = new PictureBox
        {
            Location = new Point(x, y),
            Size = new Size(length, width)
        };
        double factor = 1.0;
        var bitmap = new Bitmap(new System.IO.FileInfo(fileName).FullName);
        if (bitmap.Height > bitmap.Width) factor = bitmap.Height / (double)length;
        else factor = bitmap.Width / (double)width;
        var sizedimage = new Bitmap(bitmap, (int)(bitmap.Width / factor), (int)(bitmap.Height / factor));
        bitmap.Dispose();
        p.Image = sizedimage;
        if (tag != null) p.Tag = tag;
        if (addto != null) addto.Controls.Add(p);
        return p;
    }
}
