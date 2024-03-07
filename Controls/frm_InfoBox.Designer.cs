namespace Controls
{
    partial class frm_InfoBox
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
            this.btnClose = new System.Windows.Forms.Button();
            this.txtInfoWindow = new TextBoxExtended();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(362, 244);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // txtInfoWindow
            // 
            this.txtInfoWindow.AllowNegativeNumbers = true;
            this.txtInfoWindow.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInfoWindow.AutoSelect = true;
            this.txtInfoWindow.InputType = TextBoxExtended.InputTypeEnum.String;
            this.txtInfoWindow.Location = new System.Drawing.Point(12, 12);
            this.txtInfoWindow.Multiline = true;
            this.txtInfoWindow.Name = "txtInfoWindow";
            this.txtInfoWindow.ReadOnly = true;
            this.txtInfoWindow.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtInfoWindow.Size = new System.Drawing.Size(425, 210);
            this.txtInfoWindow.TabIndex = 1;
            // 
            // frm_InfoBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(449, 279);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.txtInfoWindow);
            this.Name = "frm_InfoBox";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "frm_InfoBox";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBoxExtended txtInfoWindow;
        private System.Windows.Forms.Button btnClose;
    }
}