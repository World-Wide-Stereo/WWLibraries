    partial class frm_promptPhone
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
            this.lbAltMessage = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.lbMessage = new System.Windows.Forms.Label();
            this.lbDash1 = new System.Windows.Forms.Label();
            this.lbDash2 = new System.Windows.Forms.Label();
            this.tbAreaCode = new TextBoxExtended();
            this.tbMid = new TextBoxExtended();
            this.tbLast = new TextBoxExtended();
            this.lbExt = new System.Windows.Forms.Label();
            this.tbExt = new TextBoxExtended();
            this.SuspendLayout();
            // 
            // lbAltMessage
            // 
            this.lbAltMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbAltMessage.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbAltMessage.ForeColor = System.Drawing.Color.Black;
            this.lbAltMessage.Location = new System.Drawing.Point(9, 120);
            this.lbAltMessage.Name = "lbAltMessage";
            this.lbAltMessage.Size = new System.Drawing.Size(278, 29);
            this.lbAltMessage.TabIndex = 0;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(210, 152);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lbMessage
            // 
            this.lbMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbMessage.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbMessage.ForeColor = System.Drawing.Color.Red;
            this.lbMessage.Location = new System.Drawing.Point(9, 10);
            this.lbMessage.Name = "lbMessage";
            this.lbMessage.Size = new System.Drawing.Size(278, 110);
            this.lbMessage.TabIndex = 0;
            // 
            // lbDash1
            // 
            this.lbDash1.AutoSize = true;
            this.lbDash1.Location = new System.Drawing.Point(36, 157);
            this.lbDash1.Name = "lbDash1";
            this.lbDash1.Size = new System.Drawing.Size(10, 13);
            this.lbDash1.TabIndex = 0;
            this.lbDash1.Text = "-";
            // 
            // lbDash2
            // 
            this.lbDash2.AutoSize = true;
            this.lbDash2.Location = new System.Drawing.Point(68, 157);
            this.lbDash2.Name = "lbDash2";
            this.lbDash2.Size = new System.Drawing.Size(10, 13);
            this.lbDash2.TabIndex = 0;
            this.lbDash2.Text = "-";
            // 
            // tbAreaCode
            // 
            this.tbAreaCode.AllowNegativeNumbers = false;
            this.tbAreaCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbAreaCode.AutoSelect = true;
            this.tbAreaCode.InputType = TextBoxExtended.InputTypeEnum.Integer;
            this.tbAreaCode.Location = new System.Drawing.Point(12, 154);
            this.tbAreaCode.MaxNumberOfWholePlaces = 3;
            this.tbAreaCode.Name = "tbAreaCode";
            this.tbAreaCode.Size = new System.Drawing.Size(26, 20);
            this.tbAreaCode.TabIndex = 0;
            this.tbAreaCode.TextChanged += new System.EventHandler(this.tb_TextChanged);
            // 
            // tbMid
            // 
            this.tbMid.AllowNegativeNumbers = false;
            this.tbMid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbMid.AutoSelect = true;
            this.tbMid.InputType = TextBoxExtended.InputTypeEnum.Integer;
            this.tbMid.Location = new System.Drawing.Point(44, 154);
            this.tbMid.MaxNumberOfWholePlaces = 3;
            this.tbMid.Name = "tbMid";
            this.tbMid.Size = new System.Drawing.Size(26, 20);
            this.tbMid.TabIndex = 0;
            this.tbMid.TextChanged += new System.EventHandler(this.tb_TextChanged);
            // 
            // tbLast
            // 
            this.tbLast.AllowNegativeNumbers = false;
            this.tbLast.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLast.AutoSelect = true;
            this.tbLast.InputType = TextBoxExtended.InputTypeEnum.Integer;
            this.tbLast.Location = new System.Drawing.Point(76, 154);
            this.tbLast.MaxNumberOfWholePlaces = 4;
            this.tbLast.Name = "tbLast";
            this.tbLast.Size = new System.Drawing.Size(32, 20);
            this.tbLast.TabIndex = 0;
            this.tbLast.TextChanged += new System.EventHandler(this.tb_TextChanged);
            // 
            // lbExt
            // 
            this.lbExt.AutoSize = true;
            this.lbExt.Location = new System.Drawing.Point(114, 157);
            this.lbExt.Name = "lbExt";
            this.lbExt.Size = new System.Drawing.Size(24, 13);
            this.lbExt.TabIndex = 0;
            this.lbExt.Text = "ext.";
            // 
            // tbExt
            // 
            this.tbExt.AllowNegativeNumbers = false;
            this.tbExt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbExt.AutoSelect = true;
            this.tbExt.InputType = TextBoxExtended.InputTypeEnum.Integer;
            this.tbExt.Location = new System.Drawing.Point(138, 154);
            this.tbExt.MaxNumberOfWholePlaces = 6;
            this.tbExt.Name = "tbExt";
            this.tbExt.Size = new System.Drawing.Size(44, 20);
            this.tbExt.TabIndex = 0;
            this.tbExt.TextChanged += new System.EventHandler(this.tb_TextChanged);
            // 
            // frm_promptPhone
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(296, 184);
            this.Controls.Add(this.lbMessage);
            this.Controls.Add(this.lbAltMessage);
            this.Controls.Add(this.tbAreaCode);
            this.Controls.Add(this.lbDash1);
            this.Controls.Add(this.tbMid);
            this.Controls.Add(this.lbDash2);
            this.Controls.Add(this.tbLast);
            this.Controls.Add(this.lbExt);
            this.Controls.Add(this.tbExt);
            this.Controls.Add(this.btnOK);
            this.Name = "frm_promptPhone";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frm_phonePrompt";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbAltMessage;
        public TextBoxExtended tbAreaCode;
        private System.Windows.Forms.Label lbDash1;
        public TextBoxExtended tbMid;
        private System.Windows.Forms.Label lbDash2;
        public TextBoxExtended tbLast;
        private System.Windows.Forms.Label lbExt;
        public TextBoxExtended tbExt;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lbMessage;
    }