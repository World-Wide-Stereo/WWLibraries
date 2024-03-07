using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Controls
{
    public class ListViewItemByColIndex : ListViewItem
    {
        public ListViewItemByColIndex() { }

        /// <summary>
        /// Essentially creates a ListViewItem. Allows the columns of the parent ListView to be rearranged
        /// without requiring modification to the code that creates its ListViewItems.
        /// </summary>
        /// <param name="dict">Dictionary&lt;columnHeaderIndex, listViewItemText&gt;</param>
        public ListViewItemByColIndex(Dictionary<int, string> dict) // <columnHeaderIndex, listViewItemText>
        {
            var orderedKeyValuePair = dict.OrderBy(x => x.Key).ToList();
            Text = orderedKeyValuePair[0].Value;
            SubItems.AddRange(orderedKeyValuePair
                .Skip(1)
                .Select(keyValuePair => new ListViewSubItem(this, keyValuePair.Value))
                .ToArray());
        }
    }
}
