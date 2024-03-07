using System.Collections.Generic;
using System.Windows.Forms;
using ww.Tables;

public partial class frm_promptCheckbox : Form
{
    public frm_promptCheckbox(string message, IEnumerable<CheckBox> checkboxes)
    {
        InitializeComponent();
        this.Text = Global.ProgramName;
        this.message.Text = message;
        foreach (var checkbox in checkboxes)
        {
            checkbox.AutoSize = true;
            checkboxPanel.Controls.Add(checkbox);
        }
    }

    private void btnOK_Click(object sender, System.EventArgs e)
    {
        this.DialogResult = DialogResult.OK;
    }
}
