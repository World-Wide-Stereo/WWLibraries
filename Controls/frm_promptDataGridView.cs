using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ww.Utilities.Extensions;

namespace Controls
{
    public partial class frm_promptDataGridView : Form
    {
        public DataGridViewRowCollection RowCollection => dataGridView1.Rows;
        public frm_promptDataGridView(string title, string message, Dictionary<string, int> columnHeaders, IEnumerable<IEnumerable<string>> rowValues)
        {
            InitializeComponent();
            this.Text = title;
            lblMessage.Text = message;
            foreach (var columnHeader in columnHeaders)
            {
                var columnIndex = dataGridView1.Columns.Add(columnHeader.Key, columnHeader.Key);
                dataGridView1.Columns[columnIndex].Width = columnHeader.Value;
            }
            foreach (var row in rowValues)
            {
                dataGridView1.Rows.Add(row.ToArray());
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}
