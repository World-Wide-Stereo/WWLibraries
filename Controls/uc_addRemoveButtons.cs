using System;
using System.ComponentModel;
using System.Windows.Forms;
using ww.Utilities.Extensions;

    [DefaultEvent("Add")]
    public partial class uc_addRemoveButtons : UserControl
    {
        private ButtonStyles _buttonStyle;

        public ButtonStyles ButtonStyle
        {
            get { return _buttonStyle; }
            set
            {
                _buttonStyle = value;
                switch (_buttonStyle)
                {
                    case ButtonStyles.Symbol:
                        this.Size = new System.Drawing.Size(53, 23);
                        this.btnRemove.Location = new System.Drawing.Point(29, 0);
                        this.btnRemove.Size = new System.Drawing.Size(23, 23);
                        this.btnRemove.Text = "-";
                        this.btnAdd.Location = new System.Drawing.Point(0, 0);
                        this.btnAdd.Size = new System.Drawing.Size(23, 23);
                        this.btnAdd.Text = "+";
                        break;
                    case ButtonStyles.StackedSymbol:
                        this.Size = new System.Drawing.Size(23, 53);
                        this.btnRemove.Location = new System.Drawing.Point(0, 29);
                        this.btnRemove.Size = new System.Drawing.Size(23, 23);
                        this.btnRemove.Text = "-";
                        this.btnAdd.Location = new System.Drawing.Point(0, 0);
                        this.btnAdd.Size = new System.Drawing.Size(23, 23);
                        this.btnAdd.Text = "+";
                        break;
                    case ButtonStyles.Text:
                        this.Size = new System.Drawing.Size(136, 23);
                        this.btnRemove.Location = new System.Drawing.Point(87, 0);
                        this.btnRemove.Size = new System.Drawing.Size(47, 23);
                        this.btnRemove.Text = "Delete";
                        this.btnAdd.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        this.btnAdd.Location = new System.Drawing.Point(0, 0);
                        this.btnAdd.Size = new System.Drawing.Size(81, 23);
                        this.btnAdd.Text = (AddText.IsNullOrBlank() ? "Add" : AddText);
                        break;
                }
            }
        }

        public string AddText
        {
            get { return btnAdd.Text; }
            set
            {
                if (ButtonStyle == ButtonStyles.Symbol)
                    btnAdd.Text = "+";
                else if (string.IsNullOrEmpty(value))
                    btnAdd.Text = "Add";
                else
                {
                    btnAdd.Text = value;
                }
            }
        }

        public enum ButtonStyles
        {
            Text,
            Symbol,
            StackedSymbol,
        }

        public uc_addRemoveButtons()
        {
            InitializeComponent();
        }

        public bool EnableAdd
        {
            get { return btnAdd.Enabled; }
            set { btnAdd.Enabled = value; }
        }
        public bool EnableRemove
        {
            get { return btnRemove.Enabled; }
            set { btnRemove.Enabled = value; }
        }

        public delegate void AddClickHandler(object sender, EventArgs e);
        public event AddClickHandler Add;

        public delegate void RemoveClickHandler(object sender, EventArgs e);
        public event RemoveClickHandler Remove;

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (Add != null) Add(sender, e);
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (Remove != null) Remove(sender, e);
        }

    }

