using System;
using System.Windows.Forms;

namespace Controls
{
    public partial class frm_InfoBox : Form
    {
        public frm_InfoBox(string message, string title = null)
        {
            InitializeComponent();
            this.Text = title;
            this.txtInfoWindow.Text = message;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
