using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Controls
{
    public class MultiTypeListViewColumnSorter : ListViewColumnSorter
    {
        private bool columnClicked;
        private int sortColumnsIndex;
        public bool IgnoreCase = true;

        public List<int> SortColumns = new List<int>();
        public override int SortColumn
        {
            set
            {
                if (SortColumns.Count > 0)
                {
                    SortColumns[0] = value;
                }
                else
                {
                    SortColumns.Add(value);
                }
            }
            get
            {
                return SortColumns.Count > 0 ? SortColumns[0] : 0;
            }
        }

        private List<SortOrder> _orders = new List<SortOrder>();
        public List<SortOrder> Orders
        {
            set
            {
                _orders = value;
                if (value.Count > 0)
                {
                    Order = value[0];
                }
            }
            get
            {
                return _orders;
            }
        }

        public override int Compare(object x, object y)
        {
            string lhs, rhs;
            if (SortColumns.Count > 0)
            {
                lhs = ((ListViewItem)x).SubItems[SortColumns[sortColumnsIndex]].Text;
                rhs = ((ListViewItem)y).SubItems[SortColumns[sortColumnsIndex]].Text;
            }
            else
            {
                lhs = ((ListViewItem)x).SubItems[0].Text;
                rhs = ((ListViewItem)y).SubItems[0].Text;
            }
            int direction = (Orders.Count > 0 && !columnClicked ? Orders[sortColumnsIndex] : Order) == SortOrder.Descending ? -1 : 1;

            if (DateTime.TryParse(lhs, out DateTime dt_x) && DateTime.TryParse(rhs, out DateTime dt_y))
            {
                int result = DateTime.Compare(dt_x, dt_y) * direction;
                if (!columnClicked && result == 0 && sortColumnsIndex < SortColumns.Count - 1)
                {
                    sortColumnsIndex++;
                    return Compare(x, y);
                }
                sortColumnsIndex = 0;
                return result;
            }
            if (decimal.TryParse(lhs, out decimal dec_x) && decimal.TryParse(rhs, out decimal dec_y))
            {
                int result = decimal.Compare(dec_x, dec_y) * direction;
                if (!columnClicked && result == 0 && sortColumnsIndex < SortColumns.Count - 1)
                {
                    sortColumnsIndex++;
                    return Compare(x, y);
                }
                sortColumnsIndex = 0;
                return result;
            }
            int compareResult = String.Compare(lhs, rhs, IgnoreCase) * direction;
            if (!columnClicked && compareResult == 0 && sortColumnsIndex < SortColumns.Count - 1)
            {
                sortColumnsIndex++;
                return Compare(x, y);
            }
            sortColumnsIndex = 0;
            return compareResult;
        }

        public static void ColumnClick(object sender, ColumnClickEventArgs e, IComparer listSorter)
        {
            var myListSorter = (MultiTypeListViewColumnSorter)listSorter;
            var myListView = (ListView)sender;
            if (e.Column == myListSorter.SortColumn)
            {
                myListSorter.Order = myListSorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                myListSorter.SortColumn = e.Column;
                myListSorter.Order = SortOrder.Ascending;
            }
            // If the user selected a specific column to sort by, we want to only sort by that column and not the additional ones.
            myListSorter.columnClicked = true;
            myListView.Sort();
            myListSorter.columnClicked = false;
        }
    }
}
