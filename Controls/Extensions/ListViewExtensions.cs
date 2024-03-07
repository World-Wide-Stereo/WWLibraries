using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using ww.Utilities.Extensions;

namespace Controls.Extensions
{
    public static class ListViewExtensions
    {
        public static void SetColumnHeaders(this ListView lv, DataTable dt, int[] widths, string[] types)
        {
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                lv.Columns.Add(new ColumnHeader { Name = dt.Columns[i].ColumnName, Text = dt.Columns[i].ColumnName, Width = widths[i], TextAlign = types[i] == "n" ? HorizontalAlignment.Right : HorizontalAlignment.Left });
            }
        }
        public static void SetColumnHeaders(this DropDownListView lv, DataTable dt, int[] widths, string[] types)
        {
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                lv.AddColumn(dt.Columns[i].ColumnName, widths[i], types[i]);
            }
        }

        public static DataTable ToDataTable(this ListView list)
        {
            var exportTable = new DataTable();

            // add column headers
            for (int i = 0; i < list.Columns.Count; i++)
            {
                exportTable.Columns.Add(list.Columns[i].Text);
            }

            // fill datatable from listview
            for (int rowIndex = 0; rowIndex < list.Items.Count; rowIndex++)
            {
                var dRow = new List<string>();
                for (int colIndex = 0; colIndex < list.Items[rowIndex].SubItems.Count; colIndex++)
                {
                    dRow.Add(list.Items[rowIndex].SubItems[colIndex].Text);
                }
                exportTable.Rows.Add(dRow.ToArray());
            }

            return exportTable;
        }

        #region ListViewItem Manipulation
        public static void DeleteSelectedItems(this ListView lv)
        {
            lv.DeleteSelectedItems<int>(null);
        }
        public static void DeleteSelectedItems<T>(this ListView lv, List<T> companionList)
        {
            if (lv.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in lv.SelectedItems)
                {
                    if (companionList != null) companionList.RemoveAt(item.Index);
                    item.Remove();
                }
            }
        }

        public static void MoveSelectedItemsUp(this ListView lv)
        {
            lv.MoveSelectedItemsUp<int>(null);
        }
        public static void MoveSelectedItemsUp<T>(this ListView lv, List<T> companionList)
        {
            // If at least one item is selected && it isn't the very first item in the list.
            if (lv.SelectedItems.Count > 0 && lv.SelectedItems[0].Index > 0)
            {
                // If all indicies in lv.SelectedItems are consecutive.
                if (lv.SelectedItems.Cast<ListViewItem>().Select(x => x.Index).AreIntegersConsecutive())
                {
                    int indexOfItemBeforeFirstItem = lv.SelectedItems[0].Index - 1;
                    int indexOfItemAfterLastItem = lv.SelectedItems.Cast<ListViewItem>().Last().Index + 1;
                    if (companionList != null)
                    {
                        companionList.Insert(indexOfItemAfterLastItem, companionList[indexOfItemBeforeFirstItem]);
                        companionList.RemoveAt(indexOfItemBeforeFirstItem);
                    }
                    lv.Items.Insert(indexOfItemAfterLastItem, (ListViewItem)lv.Items[indexOfItemBeforeFirstItem].Clone());
                    lv.Items.RemoveAt(indexOfItemBeforeFirstItem);
                }
                else
                {
                    foreach (ListViewItem item in lv.SelectedItems)
                    {
                        int index = item.Index;
                        if (companionList != null)
                        {
                            companionList.Insert(index - 1, companionList[index]);
                            companionList.RemoveAt(index + 1);
                        }
                        lv.Items.Insert(index - 1, (ListViewItem)item.Clone());
                        lv.Items.RemoveAt(index + 1);
                        lv.Items[index - 1].Selected = true;
                    }
                }
            }
            lv.Focus();
        }

        public static void MoveSelectedItemsDown(this ListView lv)
        {
            lv.MoveSelectedItemsDown<int>(null);
        }
        public static void MoveSelectedItemsDown<T>(this ListView lv, List<T> companionList)
        {
            // If at least one item is selected && it isn't the very last item in the list.
            if (lv.SelectedItems.Count > 0 && lv.SelectedItems.Cast<ListViewItem>().Last().Index < lv.Items.Count - 1)
            {
                // If the indicies in lv.SelectedItems are consecutive.
                if (lv.SelectedItems.Cast<ListViewItem>().Select(x => x.Index).AreIntegersConsecutive())
                {
                    int indexOfFirstItem = lv.SelectedItems[0].Index;
                    int indexOfItemAfterLastItem = lv.SelectedItems.Cast<ListViewItem>().Last().Index + 1;
                    if (companionList != null)
                    {
                        companionList.Insert(indexOfFirstItem, companionList[indexOfItemAfterLastItem]);
                        companionList.RemoveAt(indexOfItemAfterLastItem + 1);
                    }
                    lv.Items.Insert(indexOfFirstItem, (ListViewItem)lv.Items[indexOfItemAfterLastItem].Clone());
                    lv.Items.RemoveAt(indexOfItemAfterLastItem + 1);
                }
                else
                {
                    foreach (ListViewItem item in lv.SelectedItems.Cast<ListViewItem>().Reverse())
                    {
                        int index = item.Index;
                        if (companionList != null)
                        {
                            companionList.Insert(index + 2, companionList[index]);
                            companionList.RemoveAt(index);
                        }
                        lv.Items.Insert(index + 2, (ListViewItem)item.Clone());
                        lv.Items.RemoveAt(index);
                        lv.Items[index + 1].Selected = true;
                    }
                }
            }
            lv.Focus();
        }
        #endregion
    }
}
