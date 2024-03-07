using System.Collections.Generic;
using System.Windows.Forms;

namespace Controls.Extensions
{
    public static class TreeNodeExtensions
    {
        public static IEnumerable<TreeNode> Flatten(this TreeNode node)
        {
            yield return node;
            foreach (TreeNode child in node.Nodes)
            {
                yield return child;
                foreach (TreeNode childOfChild in Flatten(child))
                {
                    yield return childOfChild;
                }
            }
        }

        public static void MoveUp(this TreeNode node)
        {
            TreeNode parent = node.Parent;
            if (parent != null)
            {
                int index = parent.Nodes.IndexOf(node);
                if (index > 0)
                {
                    parent.Nodes.RemoveAt(index);
                    parent.Nodes.Insert(index - 1, node);
                    node.TreeView.SelectedNode = node;
                }
            }
        }

        public static void MoveDown(this TreeNode node)
        {
            TreeNode parent = node.Parent;
            if (parent != null)
            {
                int index = parent.Nodes.IndexOf(node);
                if (index < parent.Nodes.Count - 1)
                {
                    parent.Nodes.RemoveAt(index);
                    parent.Nodes.Insert(index + 1, node);
                    node.TreeView.SelectedNode = node;
                }
            }
        }
    }
}
