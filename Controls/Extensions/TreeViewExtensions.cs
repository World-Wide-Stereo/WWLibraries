using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Controls.Extensions
{
    public static class TreeViewExtensions
    {
        public static IEnumerable<TreeNode> Flatten(this TreeView treeView)
        {
            return treeView.Nodes.Cast<TreeNode>().SelectMany(x => x.Flatten());
        }
    }
}
