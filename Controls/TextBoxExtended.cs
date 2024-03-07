using System;
using System.ComponentModel;
using System.Windows.Forms;
using ww.Utilities;
using ww.Utilities.Extensions;

public class TextBoxExtended : TextBox
{
    public TextBoxExtended()
    {
        this.GotFocus += new EventHandler(TextBoxExtended_GotFocus);
        this.LostFocus += new EventHandler(TextBoxExtended_LostFocus);
        this.Click += new EventHandler(TextBoxExtended_Click);
        this.KeyPress += new KeyPressEventHandler(TextBoxExtended_KeyPress);
        this.TextChanged += new EventHandler(TextBoxExtended_TextChanged);
    }

    #region Public Properties
    [DefaultValue(false)]
    [Description("The contents of the TextBox is selected when focus is gained.")]
    public bool AutoSelect { get; set; }
    private bool wasAutoSelected; // Toggle to let the Click handler know if we already had the focus or not.

    [DefaultValue(InputTypeEnum.String)]
    [Description("A setting other than \"String\" limits which characters can be placed into the TextBox.")]
    public InputTypeEnum InputType { get; set; }
    public enum InputTypeEnum
    {
        String,
        Integer,
        Numeric,
    }

    private bool _allowNegativeNumbers = true;
    [DefaultValue(true)]
    [Description("Only applicable when the InputType property is \"Integer\" or \"Numeric\".")]
    public bool AllowNegativeNumbers
    {
        get { return _allowNegativeNumbers; }
        set { _allowNegativeNumbers = value; }
    }

    [DefaultValue(0)]
    [Description("Only applicable when the InputType property is \"Integer\" or \"Numeric\".")]
    public int MaxNumberOfWholePlaces { get; set; }

    [DefaultValue(0)]
    [Description("Only applicable when the InputType property is \"Numeric\".")]
    public int MaxNumberOfDecimalPlaces { get; set; }
    #endregion

    #region Event Handlers
    private void TextBoxExtended_GotFocus(object sender, EventArgs e)
    {
        if (AutoSelect && !wasAutoSelected)
        {
            this.SelectAll();
        }
    }

    private void TextBoxExtended_LostFocus(object sender, EventArgs e)
    {
        if (AutoSelect)
        {
            wasAutoSelected = false;
        }
    }

    private void TextBoxExtended_Click(object sender, EventArgs e)
    {
        if (AutoSelect && !wasAutoSelected)
        {
            if (this.SelectionLength == 0) this.SelectAll();
            wasAutoSelected = true;
        }
    }

    private void TextBoxExtended_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (InputType == InputTypeEnum.Numeric || InputType == InputTypeEnum.Integer)
        {
            if (AllowNegativeNumbers)
            {
                if (this.SelectionStart != 0 && e.KeyChar == '-')
                {
                    // Disallow the negative symbol if it is not the very first character.
                    e.Handled = true;
                    return;
                }
            }
            else if (e.KeyChar == '-')
            {
                // Disallow the negative symbol.
                e.Handled = true;
                return;
            }

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '-' && (InputType == InputTypeEnum.Integer || e.KeyChar != '.'))
            {
                // Disallow non-numeric characters. Also disallow decimal points if InputIntegers is true.
                e.Handled = true;
                return;
            }
            else if (InputType == InputTypeEnum.Numeric && e.KeyChar == '.' && this.Text.IndexOf('.') > -1)
            {
                // Disallow more than one decimal point.
                e.Handled = true;
                return;
            }
        }
    }

    private void TextBoxExtended_TextChanged(object sender, EventArgs e)
    {
        if (InputType == InputTypeEnum.Numeric || InputType == InputTypeEnum.Integer)
        {
            if (MaxNumberOfWholePlaces > 0 && this.Text.Length > MaxNumberOfWholePlaces)
            {
                int indexOfDecimal = this.Text.IndexOf(".");
                bool containsDash = this.Text.Contains("-");
                bool isInteger = InputType == InputTypeEnum.Integer || indexOfDecimal == -1;
                int numWholePlaces = 0;
                if (isInteger || (numWholePlaces = this.Text.Replace("-", "").Substring(0, indexOfDecimal - (containsDash ? 1 : 0)).Length) > MaxNumberOfWholePlaces)
                {
                    this.TextChanged -= new EventHandler(TextBoxExtended_TextChanged);
                    int origSelectionStart = this.SelectionStart;
                    if (isInteger)
                    {
                        this.Text = this.Text.Substring(0, containsDash ? MaxNumberOfWholePlaces + 1 : MaxNumberOfWholePlaces);
                        this.SelectionStart = origSelectionStart;
                    }
                    else
                    {
                        bool cursorAtEnd = origSelectionStart == this.Text.Length;
                        this.Text = (containsDash ? "-" : "") + this.Text.Substring(containsDash ? 1 : 0, indexOfDecimal - (containsDash && numWholePlaces == MaxNumberOfWholePlaces + 1 ? 2 : 1)) + "." + this.Text.Substring(indexOfDecimal + 1);
                        if (this.Text.EndsWith(".") && cursorAtEnd)
                        {
                            // When the user types ".", move the cursor forward.
                            this.SelectionStart = origSelectionStart + 1;
                        }
                        else
                        {
                            // When the user is typing additional digits before the decimal place, and we're at the max number of digits, do not move the cursor beyond the decimal place.
                            string afterDecimal = this.Text.Substring(indexOfDecimal + 1);
                            this.SelectionStart = origSelectionStart > MaxNumberOfWholePlaces ? origSelectionStart - (afterDecimal.Length > MaxNumberOfDecimalPlaces || origSelectionStart == indexOfDecimal ? 1 : 0) : origSelectionStart;
                        }
                    }
                    this.TextChanged += new EventHandler(TextBoxExtended_TextChanged);
                }
            }

            if (MaxNumberOfDecimalPlaces > 0 && this.Text.Contains("."))
            {
                int indexOfDecimal = this.Text.IndexOf(".");
                string afterDecimal = this.Text.Substring(indexOfDecimal + 1);
                if (afterDecimal.Length > MaxNumberOfDecimalPlaces)
                {
                    this.TextChanged -= new EventHandler(TextBoxExtended_TextChanged);
                    int origSelectionStart = this.SelectionStart;
                    this.Text = this.Text.Substring(0, indexOfDecimal) + "." + afterDecimal.Substring(0, MaxNumberOfDecimalPlaces);
                    this.SelectionStart = origSelectionStart;
                    this.TextChanged += new EventHandler(TextBoxExtended_TextChanged);
                }
            }
        }
    }
    #endregion

    #region Keyboard Shortcuts
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        Keys keyCode = keyData & Keys.KeyCode;
        Keys modifiers = keyData & Keys.Modifiers;

        if (modifiers == Keys.Control && keyCode == Keys.A)
        {
            this.SelectAll();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }
    #endregion

    protected override void WndProc(ref Message m)
    {
        // WM_PASTE
        if (m.Msg == 0x0302 && Retry.Do(Clipboard.ContainsText, retryInterval: 50))
        {
            string clipboardText = this.Multiline ? Clipboard.GetText() : Clipboard.GetText().Split('\r')[0].Split('\n')[0].Trim();
            switch (InputType)
            {
                case InputTypeEnum.Integer:
                    if (clipboardText != clipboardText.ToInt().ToString()) return;
                    break;
                case InputTypeEnum.Numeric:
                    if (clipboardText != clipboardText.ToDecimal().ToString()) return;
                    break;
            }
            this.SelectedText = clipboardText;

            if (this.Text.Length > this.MaxLength)
            {
                int oldSelectionStart = this.SelectionStart;
                this.Text = this.Text.Substring(0, this.MaxLength);
                this.SelectionStart = oldSelectionStart;
            }

            return;
        }

        base.WndProc(ref m);
    }
}
