using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ww.Utilities.Extensions;

namespace Controls.Extensions
{
    public static class DataGridViewExtensions
    {
        /// <summary>
        /// Makes a DataGridView readonly and gives it a disabled apperance.
        /// Must be called when the DataGridView is refreshed as the existing rows must be directly manipulated.
        /// </summary>
        public static void EnableEditableControls(this DataGridView dgv, bool enable, List<int> readonlyColumnIndexes = null)
        {
            dgv.ReadOnly = !enable;
            dgv.DefaultCellStyle.BackColor = enable ? SystemColors.Window : SystemColors.Control;
            dgv.DefaultCellStyle.SelectionBackColor = enable ? SystemColors.Highlight : SystemColors.Control;
            dgv.DefaultCellStyle.SelectionForeColor = enable ? SystemColors.HighlightText : SystemColors.ControlText;
            foreach (DataGridViewRow row in dgv.Rows)
            {
                row.HeaderCell.Style.SelectionBackColor = enable ? SystemColors.Highlight : SystemColors.Control;
                if (readonlyColumnIndexes != null && enable)
                {
                    // Setting the dgv.ReadOnly property changes the ReadOnly property on all rows and cells.
                    // The loop below ensures that the appropriate columns remain ReadOnly.
                    foreach (int columnIndex in readonlyColumnIndexes)
                    {
                        row.Cells[columnIndex].ReadOnly = true;
                    }
                }

                // Enable CheckBox Cells
                foreach (var cell in row.Cells.OfType<DataGridViewCheckBoxCell>().ToList())
                {
                    dynamic checkBoxCell = enable ? cell as DataGridViewDisableCheckBoxCell : cell;
                    if (checkBoxCell != null)
                    {
                        bool isChecked = checkBoxCell.Value != null && (bool)checkBoxCell.Value;
                        int columnIndex = checkBoxCell.ColumnIndex;
                        row.Cells[columnIndex] = enable ? new DataGridViewCheckBoxCell() : new DataGridViewDisableCheckBoxCell();
                        row.Cells[columnIndex].Value = isChecked;
                    }
                }
            }
        }

        #region DataGridViewRow Manipulation
        public static void MoveSelectedRowsUp(this DataGridView dgv)
        {
            dgv.MoveSelectedRowsUp<int>(null);
        }
        public static void MoveSelectedRowsUp<T>(this DataGridView dgv, List<T> companionList)
        {
            // If at least one row is selected && the new row isn't selected && the first row isn't selected.
            if (dgv.SelectedRows.Count > 0 && !dgv.SelectedRows.Cast<DataGridViewRow>().Select(x => x.Index).Contains(dgv.NewRowIndex) && dgv.SelectedRows.Cast<DataGridViewRow>().OrderBy(x => x.Index).First().Index > 0)
            {
                // If all indicies in dgv.SelectedRows are consecutive.
                if (dgv.SelectedRows.Cast<DataGridViewRow>().Select(x => x.Index).AreIntegersConsecutive())
                {
                    int indexOfRowBeforeFirstRow = dgv.SelectedRows[0].Index - 1;
                    int indexOfRowAfterLastRow = dgv.SelectedRows.Cast<DataGridViewRow>().Last().Index + (dgv.AllowUserToAddRows ? 0 : 1);
                    if (companionList != null)
                    {
                        companionList.Insert(indexOfRowAfterLastRow, companionList[indexOfRowBeforeFirstRow]);
                        companionList.RemoveAt(indexOfRowBeforeFirstRow);
                    }
                    DataGridViewRow row = dgv.Rows[indexOfRowBeforeFirstRow];
                    dgv.Rows.RemoveAt(indexOfRowBeforeFirstRow);
                    dgv.Rows.Insert(indexOfRowAfterLastRow, row);
                }
                else
                {
                    foreach (DataGridViewRow row in dgv.SelectedRows.Cast<DataGridViewRow>().OrderBy(x => x.Index))
                    {
                        int index = row.Index;
                        if (companionList != null)
                        {
                            companionList.Insert(index - 1, companionList[index]);
                            companionList.RemoveAt(index + 1);
                        }
                        dgv.Rows.RemoveAt(index);
                        dgv.Rows.Insert(index - 1, row);
                        dgv.Rows[index - 1].Selected = true;
                    }
                }
            }
            dgv.Focus();
        }

        public static void MoveSelectedRowsDown(this DataGridView dgv)
        {
            dgv.MoveSelectedRowsDown<int>(null);
        }
        public static void MoveSelectedRowsDown<T>(this DataGridView dgv, List<T> companionList)
        {
            // If at least one row is selected && the new row isn't selected && the last row real row isn't selected.
            if (dgv.SelectedRows.Count > 0 && !dgv.SelectedRows.Cast<DataGridViewRow>().Select(x => x.Index).Contains(dgv.NewRowIndex) && dgv.SelectedRows.Cast<DataGridViewRow>().OrderBy(x => x.Index).Last().Index < dgv.Rows.Count - (dgv.AllowUserToAddRows ? 2 : 1))
            {
                // If the indicies in dgv.SelectedRows are consecutive.
                if (dgv.SelectedRows.Cast<DataGridViewRow>().Select(x => x.Index).AreIntegersConsecutive())
                {
                    int indexOfFirstRow = dgv.SelectedRows[0].Index;
                    int indexOfRowAfterLastRow = dgv.SelectedRows.Cast<DataGridViewRow>().Last().Index + 1;
                    if (companionList != null)
                    {
                        companionList.Insert(indexOfFirstRow, companionList[indexOfRowAfterLastRow]);
                        companionList.RemoveAt(indexOfRowAfterLastRow + 1);
                    }
                    DataGridViewRow row = dgv.Rows[indexOfRowAfterLastRow];
                    dgv.Rows.RemoveAt(indexOfRowAfterLastRow);
                    dgv.Rows.Insert(indexOfFirstRow, row);
                }
                else
                {
                    foreach (DataGridViewRow row in dgv.SelectedRows.Cast<DataGridViewRow>().OrderByDescending(x => x.Index))
                    {
                        int index = row.Index;
                        if (companionList != null)
                        {
                            companionList.Insert(index + 2, companionList[index]);
                            companionList.RemoveAt(index);
                        }
                        dgv.Rows.RemoveAt(index);
                        dgv.Rows.Insert(index + 1, row);
                        dgv.Rows[index + 1].Selected = true;
                    }
                }
            }
            dgv.Focus();
        }
        #endregion
    }

    public static class DataGridViewRowExtensions
    {
        /// <summary>
        /// Allows the columns of the parent DataGridView to be rearranged without
        /// requiring modification to the code that creates its DataGridViewRows.
        /// </summary>
        /// <param name="row">DataGridViewRow</param>
        /// <param name="dict">Dictionary&lt;columnIndex, value&gt;</param>
        public static bool SetValuesByColIndex(this DataGridViewRow row, Dictionary<int, object> dict) // <columnIndex, value>
        {
            return row.SetValues(dict.OrderBy(x => x.Key).Select(x => x.Value).ToArray());
        }
    }
}
