using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ww.Tables;

public partial class frm_promptRadio : Form
{
    public object Value
    {
        get
        {
            var button = buttonPanel.Controls.OfType<RadioButton>().FirstOrDefault(x => x.Checked);
            if (button == null || this.DialogResult == DialogResult.Cancel) return null;
            else return button.Tag;
        }
    }

    public frm_promptRadio(string message, IDictionary<object, string> buttons, bool hideFirstOption = false, bool preventCancel = true)
    {
        InitializeComponent();
        this.Text = Global.ProgramName;
        this.ControlBox = !preventCancel;
        this.message.Text = message;
        foreach (var button in buttons)
        {
            buttonPanel.Controls.Add(new RadioButton { Text = button.Value, AutoSize = true, Tag = button.Key });
        }
        var first = buttonPanel.Controls.OfType<RadioButton>().First();
        first.Select();
        if (hideFirstOption) first.Visible = false;
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        var button = buttonPanel.Controls.OfType<RadioButton>().FirstOrDefault(x => x.Checked);
        if (button != null && button.Visible)
        {
            this.DialogResult = DialogResult.OK;
        }
        else
        {
            UserMessage.Info("Invalid selection.");
        }
    }
}
