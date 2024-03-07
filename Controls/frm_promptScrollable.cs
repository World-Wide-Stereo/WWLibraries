using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ww.Tables;
using ww.Utilities.Extensions;

namespace Controls
{
    public partial class frm_promptScrollable : Form
    {
        public frm_promptScrollable(string topMessage, string scrollableMessage, Dictionary<string, DialogResult> buttonDefinitions, string bottomMessage = "", BorderStyle borderStyle = BorderStyle.Fixed3D, bool textBoxIsReadOnly = true, bool isDialog = false)
        {
            InitializeComponent();
            this.Text = Global.ProgramName;
            lbTopMsg.Text = topMessage;
            lbBottomMsg.Text = bottomMessage;
            tbScrollableMessage.Text = scrollableMessage;
            tbScrollableMessage.ReadOnly = textBoxIsReadOnly;
            tbScrollableMessage.BorderStyle = borderStyle;

            if (topMessage.IsNullOrBlank())
            {
                this.Controls.Remove(splitContainer1);
                this.Controls.Add(tbScrollableMessage);
                tbScrollableMessage.Height += splitContainer1.SplitterDistance;
                splitContainer1.Dispose();
            }

            if (bottomMessage.IsNullOrBlank())
            {
                this.Controls.Remove(lbBottomMsg);
                tbScrollableMessage.Height += lbBottomMsg.Height;
                lbBottomMsg.Dispose();
            }

            if (isDialog)
            {
                this.ControlBox = false;
                this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                this.StartPosition = FormStartPosition.CenterParent;
            }

            // Since flpButtons.FlowDirection is set to RightToLeft, buttons must be inserted in reverse order to appear in the order in which they were specified.
            foreach (KeyValuePair<string, DialogResult> buttonDefinition in buttonDefinitions.Reverse())
            {
                var button = new Button
                {
                    Text = buttonDefinition.Key,
                    DialogResult = buttonDefinition.Value,
                    TabIndex = 0,
                };
                button.Click += button_Click;
                flpButtons.Controls.Add(button);
            }

            // Reposition everything so that it matches what will be seen if the screen is later resized.
            PromptShared.SplitContainerWithLabel_Resize(splitContainer1, lbTopMsg);
        }

        private void frm_promptScrollable_Resize(object sender, EventArgs e)
        {
            PromptShared.SplitContainerWithLabel_Resize(splitContainer1, lbTopMsg);
        }

        private void button_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys keyCode = keyData & Keys.KeyCode;
            Keys modifiers = keyData & Keys.Modifiers;

            if (modifiers == Keys.None)
            {
                if (keyCode == Keys.Y)
                {
                    Button button = flpButtons.Controls.OfType<Button>().FirstOrDefault(x => x.DialogResult == DialogResult.Yes);
                    if (button != null)
                    {
                        button.PerformClick();
                        return true;
                    }
                }
                else if (keyCode == Keys.N)
                {
                    Button button = flpButtons.Controls.OfType<Button>().FirstOrDefault(x => x.DialogResult == DialogResult.No);
                    if (button != null)
                    {
                        button.PerformClick();
                        return true;
                    }
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
