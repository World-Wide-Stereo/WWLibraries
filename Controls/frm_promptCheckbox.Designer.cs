partial class frm_promptCheckbox
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
            this.field = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.checkboxPanel = new System.Windows.Forms.TableLayoutPanel();
            this.message = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // field
            // 
            this.field.Location = new System.Drawing.Point(13, 222);
            this.field.Name = "field";
            this.field.Size = new System.Drawing.Size(172, 20);
            this.field.TabIndex = 1;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(211, 220);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(72, 23);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // checkboxPanel
            // 
            this.checkboxPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkboxPanel.AutoScroll = true;
            this.checkboxPanel.ColumnCount = 1;
            this.checkboxPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.checkboxPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.checkboxPanel.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.AddColumns;
            this.checkboxPanel.Location = new System.Drawing.Point(13, 60);
            this.checkboxPanel.Name = "checkboxPanel";
            this.checkboxPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.checkboxPanel.Size = new System.Drawing.Size(270, 154);
            this.checkboxPanel.TabIndex = 4;
            // 
            // message
            // 
            this.message.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.message.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.message.ForeColor = System.Drawing.Color.Red;
            this.message.Location = new System.Drawing.Point(12, 8);
            this.message.Name = "message";
            this.message.Size = new System.Drawing.Size(271, 49);
            this.message.TabIndex = 1;
            // 
            // frm_checkboxPrompt
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(295, 247);
            this.Controls.Add(this.field);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.checkboxPanel);
            this.Controls.Add(this.message);
            this.Name = "frm_checkboxPrompt";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frm_prompt";
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    public System.Windows.Forms.TextBox field;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.TableLayoutPanel checkboxPanel;
    private System.Windows.Forms.Label message;

}