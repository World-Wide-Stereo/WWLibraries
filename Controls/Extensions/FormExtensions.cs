using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Controls.Extensions
{
    public static class FormExtensions
    {
        #region Background Tasks
        public static MainForm MainForm;
        public static string GetMainFormNullMsg(string program)
        {
            return $"This functionality does not work when running this screen outside of {program}.";
        }
        #endregion

        #region Taskbar
        private static MenuStrip Taskbar;
        private static int TaskTextMaxChars;
        private static List<ToolStripMenuItem> Tasks = new List<ToolStripMenuItem>();

        private struct TaskInfo
        {
            public Form Form { get; set; }
            public FormWindowState PreviousWindowState { get; set; }
        }


        public static void InitializeTaskbar(MenuStrip taskbar, int taskTextMaxChars = 30)
        {
            Taskbar = taskbar;
            Taskbar.CanOverflow = true;
            Taskbar.AllowDrop = true;
            Taskbar.OverflowButton.DropDown.AllowDrop = true;
            TaskTextMaxChars = taskTextMaxChars;

            Taskbar.DragOver += new DragEventHandler(Taskbar_DragOver);
            Taskbar.OverflowButton.DropDown.DragOver += new DragEventHandler(Taskbar_DragOver);
            Taskbar.GiveFeedback += new GiveFeedbackEventHandler(Taskbar_GiveFeedback);
            Taskbar.OverflowButton.DropDown.GiveFeedback += new GiveFeedbackEventHandler(Taskbar_GiveFeedback);
        }
        public static void LaunchScreen(this Control callingForm, Form formToLaunch)
        {
            if (formToLaunch.IsDisposed)
            {
                return;
            }

            if (callingForm.TopLevelControl == null)
            {
                formToLaunch.Show();
            }
            else
            {
                Form topLevelParentForm = callingForm.TopLevelControl.FindForm();
                if (topLevelParentForm == null || !topLevelParentForm.IsMdiContainer)
                {
                    formToLaunch.Show();
                }
                else
                {
                    if (Taskbar != null)
                    {
                        var task = new ToolStripMenuItem(formToLaunch.Text.Length > TaskTextMaxChars ? formToLaunch.Text.Substring(0, TaskTextMaxChars) : formToLaunch.Text, formToLaunch.Icon.ToBitmap())
                        {
                            Overflow = ToolStripItemOverflow.AsNeeded,
                            Tag = new TaskInfo { Form = formToLaunch, PreviousWindowState = formToLaunch.WindowState },
                        };
                        task.MouseUp += new MouseEventHandler(Task_MouseUp);
                        task.MouseMove += new MouseEventHandler(Task_MouseMove);
                        Tasks.Add(task);
                        Taskbar.Items.Add(task);

                        formToLaunch.Activated += new EventHandler(Form_Activated);
                        formToLaunch.Deactivate += new EventHandler(Form_Deactivate);
                    }

                    Form previouslyActiveForm = topLevelParentForm.ActiveMdiChild;
                    FormWindowState? previouslyActiveFormWindowState = previouslyActiveForm?.WindowState;

                    formToLaunch.MdiParent = topLevelParentForm;
                    formToLaunch.FormClosing += new FormClosingEventHandler(Form_FormClosing);
                    formToLaunch.Disposed += new EventHandler(Form_Disposed);
                    formToLaunch.Show();

                    if (Taskbar != null && previouslyActiveForm != null)
                    {
                        // This must be done here as the active form, if maximized when the new form was opened, will already be set to FormWindowState.Normal by the time Form_Deactivate() is called.
                        SetTaskInfoPreviousWindowState(previouslyActiveForm, previouslyActiveFormWindowState.Value);
                    }
                }
            }
        }
        private static void Form_Activated(object sender, EventArgs e)
        {
            // Maximized forms are automatically set to FormWindowState.Normal when they lose focus. Windows Forms will not allow a Normal form to be displayed ontop of a Maximized form.
            // This will once again set the Maximized state on forms that previously had it.
            var form = (Form)sender;
            ToolStripMenuItem task = Tasks.FirstOrDefault(x => ((TaskInfo)x.Tag).Form == form);
            if (task != null)
            {
                form.WindowState = ((TaskInfo)task.Tag).PreviousWindowState;
            }
        }
        private static void Form_Deactivate(object sender, EventArgs e)
        {
            var form = (Form)sender;
            SetTaskInfoPreviousWindowState(form, form.WindowState);
        }
        private static void SetTaskInfoPreviousWindowState(Form form, FormWindowState windowState)
        {
            ToolStripMenuItem task = Tasks.FirstOrDefault(x => ((TaskInfo)x.Tag).Form == form);
            if (task != null)
            {
                var taskInfo = (TaskInfo)task.Tag;
                taskInfo.PreviousWindowState = windowState;
                task.Tag = taskInfo;
            }
        }
        private static void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!e.Cancel)
            {
                // This is being done to avoid an ObjectDisposedException that is sometimes encountered when closing MDI child forms.
                // In some unknown scenario when the form is still visible, the .NET Framework will attempt to access the Icon of the already disposed form.
                // https://stackoverflow.com/a/28699815
                ((Form)sender).Hide();
            }
        }
        private static void Form_Disposed(object sender, EventArgs e)
        {
            if (Taskbar != null)
            {
                var form = (Form)sender;
                ToolStripMenuItem task = Tasks.FirstOrDefault(x => ((TaskInfo)x.Tag).Form == form);
                if (task != null)
                {
                    Tasks.Remove(task);
                    Taskbar.Items.Remove(task);
                }
            }
        }
        private static void Task_MouseUp(object sender, MouseEventArgs e)
        {
            ToolStripMenuItem stoppedOnTask = (ToolStripMenuItem)sender;
            TaskInfo stoppedOnTaskInfo = (TaskInfo)stoppedOnTask.Tag;
            switch (e.Button)
            {
                case MouseButtons.Left:
                {
                    // The same task that was clicked is the one where the mouse button was let go.
                    // Bring the coressponding window into focus and restore if minimized.
                    Form form = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x == stoppedOnTaskInfo.Form);
                    if (form != null)
                    {
                        form.Focus(); // This will maximize the form we're switching to if the form we've just left was maximized. Form_Activated() will correct this.
                        if (form.WindowState == FormWindowState.Minimized)
                        {
                            form.WindowState = FormWindowState.Normal;
                        }
                    }
                    break;
                }
                case MouseButtons.Right:
                {
                    Form form = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x == stoppedOnTaskInfo.Form);
                    if (form != null)
                    {
                        Win32.ShowSystemMenu(form.Handle, form.MdiParent.Handle, Cursor.Position);
                    }
                    break;
                }
                case MouseButtons.Middle:
                {
                    Form form = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x == stoppedOnTaskInfo.Form);
                    if (form != null)
                    {
                        form.Close();
                    }
                    break;
                }
            }
        }
        private static void Task_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Taskbar.DoDragDrop(sender, DragDropEffects.Move);
                Taskbar.OverflowButton.DropDown.DoDragDrop(sender, DragDropEffects.Move);
            }
        }
        private static void Taskbar_DragOver(object sender, DragEventArgs e)
        {
            ToolStrip senderToolStrip = (ToolStrip)sender;
            ToolStripMenuItem clickedTask = (ToolStripMenuItem)e.Data.GetData(typeof(ToolStripMenuItem));
            ToolStripItem stoppedOnItem = senderToolStrip.GetItemAt(senderToolStrip.PointToClient(new Point(e.X, e.Y)));

            if (stoppedOnItem == Taskbar.OverflowButton)
            {
                Taskbar.OverflowButton.DropDown.Visible = true;
            }
            else if (stoppedOnItem != clickedTask && Tasks.Contains(stoppedOnItem))
            {
                if (Taskbar.OverflowButton.DropDown.Visible && senderToolStrip != Taskbar.OverflowButton.DropDown)
                {
                    Taskbar.OverflowButton.DropDown.Visible = false;
                }

                int indexOfStoppedOnTask = Taskbar.Items.IndexOf(stoppedOnItem);
                Taskbar.Items.Remove(clickedTask);
                Taskbar.Items.Insert(indexOfStoppedOnTask, clickedTask);
            }
        }
        private static void Taskbar_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            // Prevent the cursor from changing during drag and drop.
            e.UseDefaultCursors = false;
            Cursor.Current = Cursors.Default;
        }

        public static void UpdateTaskbarText(this Form callingForm)
        {
            ToolStripMenuItem task = Tasks.Find(x => ((TaskInfo)x.Tag).Form == callingForm);
            if (task != null)
            {
                task.Text = callingForm.Text.Length > TaskTextMaxChars ? callingForm.Text.Substring(0, TaskTextMaxChars) : callingForm.Text;
            }
        }

        private static class Win32
        {
            private const uint TPM_LEFTBUTTON = 0x0000;
            private const uint TPM_RETURNCMD = 0x0100;
            private const uint WM_SYSCOMMAND = 0x0112;

            [DllImport("user32.dll")]
            private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

            [DllImport("user32.dll")]
            private static extern uint TrackPopupMenuEx(IntPtr hmenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

            public static void ShowSystemMenu(IntPtr windowWithSysMenu, IntPtr windowBeingClicked, Point displaySysMenuAt)
            {
                IntPtr wMenu = GetSystemMenu(windowWithSysMenu, false);
                uint command = TrackPopupMenuEx(wMenu, TPM_LEFTBUTTON | TPM_RETURNCMD, displaySysMenuAt.X, displaySysMenuAt.Y, windowBeingClicked, IntPtr.Zero);
                if (command == 0)
                {
                    return;
                }
                PostMessage(windowWithSysMenu, WM_SYSCOMMAND, (IntPtr)command, IntPtr.Zero);
            }
        }
        #endregion
    }
}
