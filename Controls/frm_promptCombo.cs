using System.Collections.Generic;
using System.Windows.Forms;
using ww.Tables;

public class frm_promptCombo : Form
{
    public Button okbutton;
    private Label message;
    private Label altMessage;
    private ComboBox options;
    public object Value { get { return options.SelectedValue; } }
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public frm_promptCombo(string message, IList<KeyValuePair<object, string>> optionValues, ComboBoxStyle dropDownStyle = ComboBoxStyle.DropDown, string defaultValueText = null)
    {
        InitializeComponent();
        this.Text = Global.ProgramName;
        this.message.Text = message;
        options.DataSource = optionValues;
        options.DisplayMember = "Value";
        options.ValueMember = "Key";
        options.DropDownStyle = dropDownStyle;
        if (defaultValueText != null)
        {
            options.Text = defaultValueText;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (components != null)
            {
                components.Dispose();
            }
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
            this.message = new System.Windows.Forms.Label();
            this.okbutton = new System.Windows.Forms.Button();
            this.altMessage = new System.Windows.Forms.Label();
            this.options = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // message
            // 
            this.message.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.message.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.message.ForeColor = System.Drawing.Color.Red;
            this.message.Location = new System.Drawing.Point(8, 8);
            this.message.Name = "message";
            this.message.Size = new System.Drawing.Size(278, 110);
            this.message.TabIndex = 1;
            // 
            // okbutton
            // 
            this.okbutton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okbutton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okbutton.Location = new System.Drawing.Point(209, 150);
            this.okbutton.Name = "okbutton";
            this.okbutton.Size = new System.Drawing.Size(75, 23);
            this.okbutton.TabIndex = 2;
            this.okbutton.Text = "OK";
            // 
            // altMessage
            // 
            this.altMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.altMessage.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.altMessage.ForeColor = System.Drawing.Color.Black;
            this.altMessage.Location = new System.Drawing.Point(8, 118);
            this.altMessage.Name = "altMessage";
            this.altMessage.Size = new System.Drawing.Size(278, 29);
            this.altMessage.TabIndex = 3;
            // 
            // options
            // 
            this.options.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.options.FormattingEnabled = true;
            this.options.Location = new System.Drawing.Point(13, 151);
            this.options.Name = "options";
            this.options.Size = new System.Drawing.Size(171, 21);
            this.options.TabIndex = 4;
            // 
            // frm_promptCombo
            // 
            this.AcceptButton = this.okbutton;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(296, 184);
            this.Controls.Add(this.options);
            this.Controls.Add(this.altMessage);
            this.Controls.Add(this.okbutton);
            this.Controls.Add(this.message);
            this.Name = "frm_promptCombo";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frm_prompt";
            this.ResumeLayout(false);

    }
    #endregion
}
