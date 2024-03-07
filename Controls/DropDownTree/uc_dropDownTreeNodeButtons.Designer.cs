namespace Controls
{
    partial class uc_dropDownTreeNodeButtons
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnRemoveThis = new System.Windows.Forms.Button();
            this.btnAddChild = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnRemoveThis
            // 
            this.btnRemoveThis.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveThis.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnRemoveThis.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRemoveThis.Location = new System.Drawing.Point(20, -1);
            this.btnRemoveThis.Name = "btnRemoveThis";
            this.btnRemoveThis.Size = new System.Drawing.Size(16, 16);
            this.btnRemoveThis.TabIndex = 16;
            this.btnRemoveThis.Text = "-";
            this.btnRemoveThis.UseVisualStyleBackColor = false;
            this.btnRemoveThis.Click += new System.EventHandler(this.btnRemoveThis_Click);
            // 
            // btnAddChild
            // 
            this.btnAddChild.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddChild.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnAddChild.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddChild.Location = new System.Drawing.Point(1, -1);
            this.btnAddChild.Name = "btnAddChild";
            this.btnAddChild.Size = new System.Drawing.Size(16, 16);
            this.btnAddChild.TabIndex = 15;
            this.btnAddChild.Text = "+";
            this.btnAddChild.UseVisualStyleBackColor = false;
            this.btnAddChild.Click += new System.EventHandler(this.btnAddChild_Click);
            // 
            // DropDownTreeNodeButtons
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.btnRemoveThis);
            this.Controls.Add(this.btnAddChild);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "DropDownTreeNodeButtons";
            this.Size = new System.Drawing.Size(38, 14);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnRemoveThis;
        private System.Windows.Forms.Button btnAddChild;
    }
}
