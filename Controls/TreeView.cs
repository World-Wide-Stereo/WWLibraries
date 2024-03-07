using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Controls
{
    public partial class TreeView : System.Windows.Forms.TreeView
    {
        public TreeView()
        {
            InitializeComponent();
            Initialize();
        }
        public TreeView(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
            Initialize();
        }
        private void Initialize()
        {
            this.Validating += new CancelEventHandler(treeView_Validating);
            this.MouseDown += new MouseEventHandler(treeView_MouseDown);
            this.BeforeSelect += new TreeViewCancelEventHandler(treeView_BeforeEvents);
            this.BeforeExpand += new TreeViewCancelEventHandler(treeView_BeforeEvents);
            this.BeforeCollapse += new TreeViewCancelEventHandler(treeView_BeforeEvents);
        }


        #region ReadOnly
        private bool? ReadOnlyOrigHideSelection;
        private TreeNode ReadOnlyPrevSelectedNode;
        private List<TreeNode> ReadOnlyNodesToResetBackColors = new List<TreeNode>();

        private bool _readOnly;
        public bool ReadOnly
        {
            get { return _readOnly; }
            set
            {
                _readOnly = value;
                if (ReadOnlyOrigHideSelection == null) ReadOnlyOrigHideSelection = this.HideSelection;
                this.HideSelection = value ? !(bool)ReadOnlyOrigHideSelection : (bool)ReadOnlyOrigHideSelection; // HideSelection must be true in order to keep the selected node highlighted while the TreeView does not have focus.
                this.BackColor = value ? SystemColors.Control : Color.Empty;
                if (value)
                {
                    treeView_Validating(this, new CancelEventArgs());
                }
                else
                {
                    // While ReadOnly, the user may have fired the MouseDown event, changing the BackColor of the clicked node to SystemColors.Control.
                    // This ensures that the BackColor of nodes isn't off once the BackColor of the TreeView has been changed back from Gray to White.
                    foreach (TreeNode node in ReadOnlyNodesToResetBackColors)
                    {
                        node.BackColor = Color.Empty;
                    }
                    ReadOnlyNodesToResetBackColors.Clear();
                }
            }
        }

        // While ReadOnly and !ReadOnlyOrigHideSelection, ensure that the selected node remains SystemColors.Highlight even when the TreeView does not have focus.
        private void treeView_Validating(object sender, CancelEventArgs e)
        {
            if (ReadOnly)
            {
                ReadOnlyPrevSelectedNode = this.SelectedNode;
            }
        }

        // While ReadOnly, ensure that another node cannot be selected.
        private void treeView_MouseDown(object sender, MouseEventArgs e)
        {
            if (ReadOnly)
            {
                TreeNode node = this.GetNodeAt(e.Location);
                if (node != null)
                {
                    if (!ReadOnlyNodesToResetBackColors.Contains(node))
                    {
                        ReadOnlyNodesToResetBackColors.Add(node);
                    }
                }
            }
        }
        private void treeView_BeforeEvents(object sender, TreeViewCancelEventArgs e)
        {
            if (ReadOnly) e.Cancel = true;
        }
        #endregion
    }
}
