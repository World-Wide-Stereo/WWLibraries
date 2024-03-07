using System.Drawing;
using System.Windows.Forms;

namespace Controls
{
    public static class PromptShared
    {
        public static void SplitContainerWithLabel_Resize(SplitContainer splitContainer, Label label)
        {
            if (splitContainer.IsDisposed)
            {
                return;
            }

            if (splitContainer.ParentForm != null)
            {
                label.MaximumSize = new Size(splitContainer.ParentForm.Width - label.Location.X * 4, label.MaximumSize.Height);
            }

            int splitterDistance = label.Height + 5;
            int minSplitterDistance = splitContainer.Panel1MinSize;
            if (splitterDistance > minSplitterDistance
                && splitterDistance <= splitContainer.Width - splitContainer.Panel2MinSize // Max splitter distance
                && splitContainer.Height > splitterDistance)
            {
                splitContainer.SplitterDistance = splitterDistance;
            }
        }
    }
}
