using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Controls
{
    public partial class frm_promptDate : Form
    {
        public frm_promptDate(string title, string message, string dateFieldName, DateTime? defaultDate = null)
        {
            InitializeComponent();
            this.Text = title;
            label2.Text = message;
            label1.Text = dateFieldName;
            dtpDate.Value = defaultDate ?? DateTime.Today;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
