using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Controls
{
    public partial class frm_promptListView : Form
    {
        public object Value
        {
            get
            {
                if (lvOptions.CheckedItems.Count == 0 || this.DialogResult == DialogResult.Cancel)
                    return null;
                else
                    return lvOptions.CheckedItems;
            }
        }
        public frm_promptListView(string title, string message, Dictionary<string, int> columnHeaders, List<ListViewItem> options, bool checkedByDefault)
        {
            InitializeComponent();
            this.Text = title;
            lblMessage.Text = message;
            lvOptions.Clear();
            foreach (var columnHeader in columnHeaders)
            {
                lvOptions.Columns.Add(columnHeader.Key, columnHeader.Value);
            }
            foreach (ListViewItem option in options)
            {
                option.Checked = checkedByDefault;
                lvOptions.Items.Add(option);
            }
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void lvOptions_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                e.Item.Checked = true;
            }
        }
    }
}
