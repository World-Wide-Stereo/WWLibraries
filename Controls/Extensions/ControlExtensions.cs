using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Controls.Extensions
{
	public static class ControlExtensions
	{
		public static void EnableEditableControls(this Control callingControl, bool enable, bool? enableLinkLabels = null, bool textBoxesReadOnly = true, bool changeButtonEnabledStatus = true)
		{
			// LinkLabels
			if (enableLinkLabels == null) foreach (var lb in callingControl.Controls.OfType<LinkLabel>()) { lb.Enabled = enable; }
			else foreach (var lb in callingControl.Controls.OfType<LinkLabel>()) { lb.Enabled = (bool)enableLinkLabels; }

			// TextBoxes
			if (textBoxesReadOnly) foreach (var tb in callingControl.Controls.OfType<TextBox>()) { tb.ReadOnly = !enable; }
			else foreach (var tb in callingControl.Controls.OfType<TextBox>()) { tb.Enabled = enable; }

			// RicherTextBoxes
			if (textBoxesReadOnly) foreach (var tb in callingControl.Controls.OfType<RicherTextBox.RicherTextBox>()) { tb.ReadOnly = !enable; }
			else foreach (var tb in callingControl.Controls.OfType<RicherTextBox.RicherTextBox>()) { tb.Enabled = enable; }

			// CheckBoxes, ComboBoxes, RadioButtons, and DateTimePickers
			foreach (var control in callingControl.Controls.Cast<Control>().Where(x => x is CheckBox || x is ComboBox || x is RadioButton || x is DateTimePicker)) { control.Enabled = enable; }

			// ListViews
			foreach (var lv in callingControl.Controls.OfType<ListView>()) { lv.BackColor = enable ? Color.Empty : SystemColors.Control; }

			// DataGridViews
			// You must call DataGridViewExtensions.EnableEditableControls() when a DataGridView is refreshed.

			// TreeViews
			foreach (var tv in callingControl.Controls.OfType<TreeView>()) { tv.ReadOnly = !enable; }

			// Buttons
			if (changeButtonEnabledStatus) foreach (var btn in callingControl.Controls.OfType<Button>()) { btn.Enabled = enable; }

			// Sub Controls
			foreach (var control in callingControl.Controls.Cast<Control>().Where(x => x.HasChildren)) control.EnableEditableControls(enable, enableLinkLabels, textBoxesReadOnly, changeButtonEnabledStatus);
		}

		public static void ResetEditableControls(this Control callingControl)
		{
			// Labels
			foreach (var lb in callingControl.Controls.OfType<DataLabel>()) { lb.Text = ""; }

			// LinkLabels
			foreach (var lb in callingControl.Controls.OfType<DataLinkLabel>()) { lb.Text = ""; }

			// TextBoxes
			foreach (var tb in callingControl.Controls.OfType<TextBox>()) { tb.Text = ""; }

			// CheckBoxes
			foreach (var cb in callingControl.Controls.OfType<CheckBox>()) { cb.Checked = false; }

			// ComboBoxes
			foreach (var cb in callingControl.Controls.OfType<ComboBox>().Where(x => !(x is ComboBoxOfCheckBoxes.ComboBoxOfCheckBoxes) && !(x is EnumComboBoxUnresettable))) { cb.SelectedIndex = -1; }

			// ComboBoxOfCheckBoxes
			foreach (var cb in callingControl.Controls.OfType<ComboBoxOfCheckBoxes.ComboBoxOfCheckBoxes>().SelectMany(x => x.CheckBoxItems)) { cb.Checked = false; }

			// ListViews
			foreach (var lv in callingControl.Controls.OfType<ListView>()) { lv.Items.Clear(); }

			// DataGridViews
			foreach (var dgv in callingControl.Controls.OfType<DataGridView>()) { dgv.Rows.Clear(); }

			// PictureBoxes
			foreach (var pb in callingControl.Controls.OfType<PictureBox>()) { pb.Visible = false; }

			// Sub Controls
			foreach (var control in callingControl.Controls.Cast<Control>().Where(x => x.HasChildren)) control.ResetEditableControls();
		}

		public static bool HasData(this Control callingControl, bool defaultValue = false)
		{
			switch (callingControl)
			{
				case ComboBoxOfCheckBoxes.ComboBoxOfCheckBoxes comboBoxOfCheckBoxes:
					return comboBoxOfCheckBoxes.CheckBoxItems.Any(x => x.Checked);
				case DataLabel _:
				case DataLinkLabel _:
				case TextBox _:
				case ComboBox _:
					return callingControl.Text.Trim().Length > 0;
				case CheckBox checkBox:
					return checkBox.Checked;
				case RadioButton radioButton:
					return radioButton.Checked;
			}
			return defaultValue;
		}

		#region Redraw Manipulation
		/// <summary>
		/// An application sends the WM_SETREDRAW message to a window to allow changes in that
		/// window to be redrawn or to prevent changes in that window from being redrawn.
		/// </summary>
		private const int WM_SETREDRAW = 11;

		/// <summary>
		/// Suspends painting for the target control. *** IMPORTANT - DO NOT forget to call ResumeRedraw(). ***
		/// </summary>
		public static void SuspendRedraw(this Control control)
		{
			Message msgSuspendUpdate = Message.Create(control.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);

			NativeWindow window = NativeWindow.FromHandle(control.Handle);
			window.DefWndProc(ref msgSuspendUpdate);
		}

		/// <summary>
		/// Resumes painting for the target control. Intended to be called following a call to SuspendRedraw().
		/// </summary>
		public static void ResumeRedraw(this Control control)
		{
			// Create a C "true" boolean as an IntPtr.
			var wparam = new IntPtr(1);
			Message msgResumeUpdate = Message.Create(control.Handle, WM_SETREDRAW, wparam, IntPtr.Zero);

			NativeWindow window = NativeWindow.FromHandle(control.Handle);
			window.DefWndProc(ref msgResumeUpdate);
			control.Invalidate();
			control.Refresh();
		}
		#endregion
	}
}
