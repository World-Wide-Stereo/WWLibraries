using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows.Forms;

namespace Controls
{
    // Modified from http://msdn.microsoft.com/en-us/library/ms996467.aspx
    public class ListViewItemComparer : IComparer
    {
        private int col;
        private SortOrder order;
        public ListViewItemComparer()
        {
            col = 0;
            order = SortOrder.Ascending;
        }
        public ListViewItemComparer(int column, SortOrder order)
        {
            col = column;
            this.order = order;
        }
        public int Compare(object x, object y)
        {
            int returnVal;
            DateTime firstDate, secondDate;
            var itemX = ((ListViewItem)x).SubItems[col].Text;
            var itemY = ((ListViewItem)y).SubItems[col].Text;
            if (DateTime.TryParse(itemX, out firstDate) && DateTime.TryParse(itemY, out secondDate))
            {
                returnVal = DateTime.Compare(firstDate, secondDate);
            }
            else
            {
                returnVal = String.Compare(itemX, itemY, StringComparison.CurrentCulture);
            }
            // Determine whether the sort order is descending.
            if (order == SortOrder.Descending)
                // Invert the value returned by String.Compare.
                returnVal *= -1;
            return returnVal;
        }
    }
}
