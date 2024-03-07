using System;
using System.Windows.Forms;

namespace Controls
{
    public partial class uc_dropDownTreeNodeButtons : UserControl
    {
        public uc_dropDownTreeNodeButtons()
        {
            InitializeComponent();
        }

        internal bool ShowAddButton
        {
            get { return btnAddChild.Visible; }
            set { btnAddChild.Visible = value;}
        }

        internal bool ShowRemoveButton
        {
            get { return btnRemoveThis.Visible; }
            set { btnRemoveThis.Visible = value; }
        }

        private void btnAddChild_Click(object sender, EventArgs e)
        {
            var parent = ((Control)sender).Parent as uc_dropDownTreeNodeButtons;
            var node = parent.Tag as DropDownTreeNode;

            var newNode = new DropDownTreeNode();
            node.AddChild(newNode);
        }

        private void btnRemoveThis_Click(object sender, EventArgs e)
        {
            var parent = ((Control)sender).Parent as uc_dropDownTreeNodeButtons;
            var node = parent.Tag as DropDownTreeNode;

            node.RemoveSelf();
        }
    }
}
