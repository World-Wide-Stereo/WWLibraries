using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ww.Tables;

public class frm_promptButton : Form
{
    private Label message;
    private Label altMessage;
    private FlowLayoutPanel buttonPanel;
    public TextBox textInput;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public frm_promptButton(string Message, IDictionary<string, DialogResult> buttons, string redMessage = null, bool showTextBox = false)
    {
        InitializeComponent();
        this.Text = Global.ProgramName;

        if (redMessage != null)
        {
            altMessage.Text = Message;
            message.Text = redMessage;
        }
        else
        {
            message.Text = Message;
        }

        textInput.Visible = showTextBox;
        altMessage.Visible = !showTextBox;

        var maxWidth = buttons.Keys.Max(x => 10 + (int)this.CreateGraphics().MeasureString(x, this.Font).Width);
        int buttonWidth = 0;
        foreach (var button in buttons)
        {
            Button b = new Button
            {
                Text = button.Key,
                DialogResult = button.Value,
                Width = maxWidth,
            };
            buttonPanel.Controls.Add(b);
            buttonWidth = b.Width + b.Margin.Horizontal * 2 + b.Padding.Horizontal * 2;
        }

        int totalWidth = buttons.Count * buttonWidth;
        if (totalWidth < 650)
        {
            totalWidth += 50;
            this.Width = totalWidth < 312 ? 312 : totalWidth;
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
            this.altMessage = new System.Windows.Forms.Label();
            this.buttonPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.textInput = new System.Windows.Forms.TextBox();
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
            this.message.Size = new System.Drawing.Size(278, 119);
            this.message.TabIndex = 0;
            // 
            // altMessage
            // 
            this.altMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.altMessage.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.altMessage.ForeColor = System.Drawing.Color.Black;
            this.altMessage.Location = new System.Drawing.Point(8, 127);
            this.altMessage.Name = "altMessage";
            this.altMessage.Size = new System.Drawing.Size(278, 27);
            this.altMessage.TabIndex = 0;
            // 
            // buttonPanel
            // 
            this.buttonPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPanel.AutoSize = true;
            this.buttonPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.buttonPanel.Location = new System.Drawing.Point(13, 152);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(271, 31);
            this.buttonPanel.TabIndex = 0;
            // 
            // textInput
            // 
            this.textInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textInput.Location = new System.Drawing.Point(13, 127);
            this.textInput.Name = "textInput";
            this.textInput.Size = new System.Drawing.Size(172, 20);
            this.textInput.TabIndex = 0;
            // 
            // frm_promptButton
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(296, 193);
            this.Controls.Add(this.message);
            this.Controls.Add(this.altMessage);
            this.Controls.Add(this.textInput);
            this.Controls.Add(this.buttonPanel);
            this.Name = "frm_promptButton";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frm_prompt";
            this.ResumeLayout(false);
            this.PerformLayout();

    }
    #endregion
}
