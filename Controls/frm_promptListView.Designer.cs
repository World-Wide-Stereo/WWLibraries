/*
 * Created by SharpDevelop.
 * User: rstahlnecker
 * Date: 2/20/2017
 * Time: 1:27 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace Controls
{
    partial class frm_promptListView
    {
        /// <summary>
        /// Designer variable used to keep track of non-visual components.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.ListView lvOptions;
        private System.Windows.Forms.Button btnOK;
        
        /// <summary>
        /// Disposes resources used by the form.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }
        
        /// <summary>
        /// This method is required for Windows Forms designer support.
        /// Do not change the method contents inside the source code editor. The Forms designer might
        /// not be able to load this method if it was changed manually.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblMessage = new System.Windows.Forms.Label();
            this.lvOptions = new System.Windows.Forms.ListView();
            this.btnOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblMessage
            // 
            this.lblMessage.Location = new System.Drawing.Point(13, 13);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(552, 23);
            this.lblMessage.TabIndex = 0;
            this.lblMessage.Text = "Message";
            // 
            // lvOptions
            // 
            this.lvOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvOptions.CheckBoxes = true;
            this.lvOptions.Location = new System.Drawing.Point(13, 40);
            this.lvOptions.Name = "lvOptions";
            this.lvOptions.Size = new System.Drawing.Size(552, 158);
            this.lvOptions.TabIndex = 1;
            this.lvOptions.UseCompatibleStateImageBehavior = false;
            this.lvOptions.View = System.Windows.Forms.View.Details;
            this.lvOptions.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lvOptions_ItemSelectionChanged);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(490, 211);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // frm_ListViewPrompt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(577, 246);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lvOptions);
            this.Controls.Add(this.lblMessage);
            this.Name = "frm_ListViewPrompt";
            this.ResumeLayout(false);

        }
    }
}
