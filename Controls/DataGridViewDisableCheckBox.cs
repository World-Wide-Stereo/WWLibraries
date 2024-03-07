using System.Drawing;
using System.Windows.Forms;

// From http://www.codeproject.com/Articles/31829/Disabled-Checkbox-Column-in-the-DataGridView

public class DataGridViewDisableCheckBoxColumn : DataGridViewCheckBoxColumn
{
    public DataGridViewDisableCheckBoxColumn()
    {
        this.CellTemplate = new DataGridViewDisableCheckBoxCell();
    }
}

public class DataGridViewDisableCheckBoxCell : DataGridViewCheckBoxCell
{
    public DataGridViewDisableCheckBoxCell()
    {

    }
    /// <summary>
    /// Override the Paint method to show the disabled checked/unchecked datagridviewcheckboxcell.
    /// </summary>
    /// <param name="graphics"></param>
    /// <param name="clipBounds"></param>
    /// <param name="cellBounds"></param>
    /// <param name="rowIndex"></param>
    /// <param name="elementState"></param>
    /// <param name="value"></param>
    /// <param name="formattedValue"></param>
    /// <param name="errorText"></param>
    /// <param name="cellStyle"></param>
    /// <param name="advancedBorderStyle"></param>
    /// <param name="paintParts"></param>
    protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds,
        int rowIndex, DataGridViewElementStates elementState, object value,
        object formattedValue, string errorText, DataGridViewCellStyle cellStyle,
        DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
    {
        //base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);

        SolidBrush cellBackground = new SolidBrush(cellStyle.BackColor);
        graphics.FillRectangle(cellBackground, cellBounds);
        cellBackground.Dispose();
        PaintBorder(graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
        Rectangle checkBoxArea = cellBounds;
        Rectangle buttonAdjustment = this.BorderWidths(advancedBorderStyle);
        checkBoxArea.X += buttonAdjustment.X;
        checkBoxArea.Y += buttonAdjustment.Y;

        checkBoxArea.Height -= buttonAdjustment.Height;
        checkBoxArea.Width -= buttonAdjustment.Width;
        Point drawInPoint = new Point(cellBounds.X + cellBounds.Width / 2 - 7, cellBounds.Y + cellBounds.Height / 2 - 7);

        CheckBoxRenderer.DrawCheckBox(graphics, drawInPoint,
            (bool)this.Value
                ? System.Windows.Forms.VisualStyles.CheckBoxState.CheckedDisabled
                : System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedDisabled);
    }
}
