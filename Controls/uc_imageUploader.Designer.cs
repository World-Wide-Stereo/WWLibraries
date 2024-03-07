namespace Controls
{
    partial class uc_imageUploader
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(uc_imageUploader));
            this.photo = new System.Windows.Forms.PictureBox();
            this.llbDelete = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.photo)).BeginInit();
            this.SuspendLayout();
            // 
            // photo
            // 
            this.photo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.photo.Image = ((System.Drawing.Image)(resources.GetObject("photo.Image")));
            this.photo.InitialImage = ((System.Drawing.Image)(resources.GetObject("photo.InitialImage")));
            this.photo.Location = new System.Drawing.Point(1, 1);
            this.photo.Name = "photo";
            this.photo.Size = new System.Drawing.Size(100, 100);
            this.photo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.photo.TabIndex = 522;
            this.photo.TabStop = false;
            // 
            // llbDelete
            // 
            this.llbDelete.AutoSize = true;
            this.llbDelete.Font = new System.Drawing.Font("Arial Narrow", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.llbDelete.Location = new System.Drawing.Point(19, 102);
            this.llbDelete.Name = "llbDelete";
            this.llbDelete.Size = new System.Drawing.Size(60, 15);
            this.llbDelete.TabIndex = 523;
            this.llbDelete.TabStop = true;
            this.llbDelete.Text = "Delete Image";
            this.llbDelete.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llbDelete_LinkClicked);
            // 
            // ImageUploader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.llbDelete);
            this.Controls.Add(this.photo);
            this.Name = "ImageUploader";
            this.Size = new System.Drawing.Size(102, 117);
            ((System.ComponentModel.ISupportInitialize)(this.photo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox photo;
        private System.Windows.Forms.LinkLabel llbDelete;
    }
}
