using System;
using System.Windows.Forms;

public class FunctionKeyListView : ListView
{
    public event EventHandler F2Pressed;
    public event EventHandler F3Pressed;
    public event EventHandler F4Pressed;

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.F2 && F2Pressed != null)
        {
            F2Pressed(this, new EventArgs());
            return true;
        }
        else if (keyData == Keys.F3 && F3Pressed != null)
        {
            F3Pressed(this, new EventArgs());
            return true;
        }
        else if (keyData == Keys.F4 && F4Pressed != null)
        {
            F4Pressed(this, new EventArgs());
            return true;
        }
        else
        {
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}

