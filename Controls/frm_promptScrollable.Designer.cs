namespace Controls
{
    partial class frm_promptScrollable
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lbTopMsg = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tbScrollableMessage = new System.Windows.Forms.RichTextBox();
            this.lbBottomMsg = new System.Windows.Forms.Label();
            this.flpButtons = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbTopMsg
            // 
            this.lbTopMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbTopMsg.AutoSize = true;
            this.lbTopMsg.Location = new System.Drawing.Point(9, 9);
            this.lbTopMsg.Name = "lbTopMsg";
            this.lbTopMsg.Size = new System.Drawing.Size(35, 13);
            this.lbTopMsg.TabIndex = 0;
            this.lbTopMsg.Text = "label1";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lbTopMsg);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tbScrollableMessage);
            this.splitContainer1.Panel2.Controls.Add(this.lbBottomMsg);
            this.splitContainer1.Size = new System.Drawing.Size(392, 357);
            this.splitContainer1.SplitterDistance = 33;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 0;
            // 
            // tbScrollableMessage
            // 
            this.tbScrollableMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbScrollableMessage.Location = new System.Drawing.Point(12, 12);
            this.tbScrollableMessage.Name = "tbScrollableMessage";
            this.tbScrollableMessage.ReadOnly = true;
            this.tbScrollableMessage.Size = new System.Drawing.Size(368, 270);
            this.tbScrollableMessage.TabIndex = 0;
            this.tbScrollableMessage.Text = "";
            // 
            // lbBottomMsg
            // 
            this.lbBottomMsg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbBottomMsg.Location = new System.Drawing.Point(12, 295);
            this.lbBottomMsg.Name = "lbBottomMsg";
            this.lbBottomMsg.Size = new System.Drawing.Size(368, 28);
            this.lbBottomMsg.TabIndex = 0;
            this.lbBottomMsg.Text = "label1";
            // 
            // flpButtons
            // 
            this.flpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpButtons.Location = new System.Drawing.Point(0, 360);
            this.flpButtons.Name = "flpButtons";
            this.flpButtons.Size = new System.Drawing.Size(383, 34);
            this.flpButtons.TabIndex = 0;
            // 
            // frm_promptScrollable
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(392, 393);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.flpButtons);
            this.Name = "frm_promptScrollable";
            this.Text = "frm_promptScrollable";
            this.Resize += new System.EventHandler(this.frm_promptScrollable_Resize);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lbTopMsg;
        public System.Windows.Forms.RichTextBox tbScrollableMessage;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.FlowLayoutPanel flpButtons;
        private System.Windows.Forms.Label lbBottomMsg;
    }
}