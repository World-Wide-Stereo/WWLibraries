using System;
using System.Linq;
using System.Windows.Forms;
using ww.Tables;
using ww.Utilities.Extensions;

public partial class frm_promptPhone : Form
{
    public frm_promptPhone(string message)
    {
        InitializeComponent();
        this.Text = Global.ProgramName;
        this.lbMessage.Text = message;
    }

    private void tb_TextChanged(object sender, EventArgs e)
    {
        var tb = (TextBoxExtended)sender;
        if (tb.TextLength == tb.MaxNumberOfWholePlaces)
        {
            this.SelectNextControl(tb, true, true, true, true);
        }
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        TextBoxExtended tbIncomplete = this.Controls.OfType<TextBoxExtended>().Where(x => x != tbExt).FirstOrDefault(x => x.TextLength != x.MaxNumberOfWholePlaces);
        if (tbIncomplete != null)
        {
            UserMessage.Info("A portion of the phone number has not been entered.");
            tbIncomplete.Focus();
            this.DialogResult = DialogResult.None;
            return;
        }
        string phoneNumber = this.Controls.OfType<TextBoxExtended>().Select(x => x.Text).Join("");
        if (this.Controls.OfType<TextBoxExtended>().Select(x => x.Text).Join("").All(x => x == phoneNumber[0]))
        {
            UserMessage.Info("A phone number cannot be composed entirely of the same digit.");
            this.DialogResult = DialogResult.None;
            return;
        }

        this.DialogResult = DialogResult.OK;
    }
}
