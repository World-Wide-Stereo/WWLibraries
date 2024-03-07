using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Controls
{
    public partial class ListViewOfTextBoxes : Panel
    {
        private const int TextBoxHeight = 20;

        private Size _textBoxDefaultSize;
        private int _nextTextBoxIndex;
        private int _nextTextBoxPositionY;
        public TextBoxExtended SelectedTextBox;

        public ListWithCustomEvents<TextBoxExtended> Items { get; set; }
        public bool? EditMode { get; set; }
        public bool LoadingData { get; set; }

        private bool _autoSelect = true;
        [DefaultValue(true)]
        [Description("The contents of a TextBox is selected when focus is gained.")]
        public bool AutoSelect
        {
            get { return _autoSelect; }
            set
            {
                _autoSelect = value;
                foreach (TextBoxExtended tb in Items)
                {
                    tb.AutoSelect = value;
                }
            }
        }

        public Func<TextBoxExtended> GetCustomNewTextBox;

        #region Constructors
        public ListViewOfTextBoxes()
        {
            InitializeComponent();
            ConstructorShared();
        }
        public ListViewOfTextBoxes(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
            ConstructorShared();
        }
        private void ConstructorShared()
        {
            this.AutoScroll = true;

            this.Resize += Panel_Resize;
            //this.ControlRemoved += Panel_ControlRemoved;

            Items = new ListWithCustomEvents<TextBoxExtended>();
            Items.OnAdd += Items_OnAdd;
            Items.OnRemove += Items_OnRemove;
            Items.OnClear += Items_OnClear;
        }
        #endregion

        #region Panel Events
        private void Panel_Resize(object sender, EventArgs e)
        {
            // Occurs when initializing the panel's size in the Designer and on resizes.
            _textBoxDefaultSize = new Size(this.Width - SystemInformation.VerticalScrollBarWidth, TextBoxHeight);
        }

        // Would be necessary if ListViewOfTextBoxes.Controls.Clear() was called instead of ListViewOfTextBoxes.Items.Clear().
        //private void Panel_ControlRemoved(object sender, ControlEventArgs e)
        //{
        //    if (e.Control.GetType() == typeof(TextBoxExtended) && !((ListViewOfTextBoxes)sender).Controls.OfType<TextBoxExtended>().Any())
        //    {
        //        _nextTextBoxIndex = 0;
        //        _nextTextBoxPositionY = 0;
        //    }
        //}
        #endregion

        #region Items Events
        private void Items_OnAdd(object sender, ListWithCustomEvents<TextBoxExtended>.ListEventArgs e)
        {
            foreach (TextBoxExtended tb in e.Items)
            {
                tb.Size = _textBoxDefaultSize;
                tb.Location = new Point(0, _nextTextBoxPositionY + this.AutoScrollPosition.Y);
                tb.Name = _nextTextBoxIndex.ToString();

                tb.TextChanged += TextBox_TextChanged;
                tb.Enter += TextBox_Enter;
                tb.Leave += TextBox_Leave;
                tb.KeyDown += TextBox_KeyDown;

                this.Controls.Add(tb);
                _nextTextBoxIndex++;
                _nextTextBoxPositionY += tb.Height;

                TextBox_TextChanged(tb, new EventArgs());
            }
        }

        private void Items_OnRemove(object sender, ListWithCustomEvents<TextBoxExtended>.ListEventArgs e)
        {
            if (SelectedTextBox != null)
            {
                int heightOfSelectedTb = SelectedTextBox.Height;
                this.Controls.Remove(this.Controls.OfType<TextBoxExtended>().Single(x => x.Name == SelectedTextBox.Name));
                foreach (var tb in this.Controls.OfType<TextBoxExtended>().Where(x => x.Location.Y > SelectedTextBox.Location.Y))
                {
                    tb.Location = new Point(tb.Location.X, tb.Location.Y - heightOfSelectedTb);
                }
                _nextTextBoxPositionY -= heightOfSelectedTb;
                SelectedTextBox = null;
            }
        }

        private void Items_OnClear(object sender, EventArgs e)
        {
            foreach (var tb in this.Items) this.Controls.Remove(tb);
            _nextTextBoxIndex = 0;
            _nextTextBoxPositionY = 0;
        }
        #endregion

        #region TextBox Events
        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            var tb = (TextBoxExtended)sender;
            Size size = TextRenderer.MeasureText(tb.Text, tb.Font);
            if (LoadingData || size.Width > _textBoxDefaultSize.Width)
            {
                // Expand the width of the textbox if the text goes beyond the normally visiable area.
                if (tb.Width < size.Width)
                {
                    tb.Width = size.Width;
                    if (!LoadingData) this.HorizontalScroll.Value = tb.Width - _textBoxDefaultSize.Width;
                }

                // Make all textboxes match the size of the longest one.
                if (this.Items.Any(x => x.Width > _textBoxDefaultSize.Width))
                {
                    Size longestTextBoxTextSize = this.Items.Select(x => TextRenderer.MeasureText(x.Text, x.Font)).OrderByDescending(x => x.Width).First();
                    if (this.Items.Select(x => x.Width).Any(x => x != longestTextBoxTextSize.Width))
                    {
                        foreach (var textBox in this.Items)
                        {
                            textBox.Size = longestTextBoxTextSize;
                        }
                    }
                }
            }
        }

        private void TextBox_Enter(object sender, EventArgs e)
        {
            // When a TextBox gains focus, place it into _selectedTextBox. Knowing which TextBox was selected last will allow us to manipulate it.
            // Modifying the colors so that it's clear which item is selected for manipulation by the buttons. Mimicking ListView colors.
            if (EditMode == null || (bool)EditMode)
            {
                ResetColorsOfTextBox(this.SelectedTextBox);
                SelectedTextBox = (TextBoxExtended)sender;
                SelectedTextBox.BackColor = SystemColors.MenuHighlight;
                SelectedTextBox.ForeColor = SystemColors.Window;
            }
        }
        private void TextBox_Leave(object sender, EventArgs e)
        {
            // Mimick ListView colors for when the ListView has lost focus.
            if ((EditMode == null || (bool)EditMode) && SelectedTextBox.Name == ((TextBoxExtended)sender).Name)
            {
                SelectedTextBox.BackColor = SystemColors.ControlLight;
                SelectedTextBox.ForeColor = Color.Empty;
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var tb = GetCustomNewTextBox == null ? new TextBoxExtended { AutoSelect = this.AutoSelect } : GetCustomNewTextBox();
                this.Items.Add(tb);
                tb.Focus();
            }
        }
        #endregion

        #region Public Functions
        public void ResetColorsOfTextBox(TextBoxExtended tb)
        {
            if (tb != null)
            {
                tb.BackColor = Color.Empty;
                tb.ForeColor = Color.Empty;
            }
        }

        public void MoveTextBoxUp(TextBoxExtended tb)
        {
            if (tb != null)
            {
                TextBoxExtended tbAboveOneToMove = this.Items.Where(x => x.Location.Y < tb.Location.Y).OrderByDescending(x => x.Location.Y).FirstOrDefault();
                if (tbAboveOneToMove != null)
                {
                    int heightOfSelectedTb = tb.Height;
                    tbAboveOneToMove.Location = new Point(tbAboveOneToMove.Location.X, tbAboveOneToMove.Location.Y + heightOfSelectedTb);
                    tb.Location = new Point(tb.Location.X, tb.Location.Y - heightOfSelectedTb);
                    this.ScrollControlIntoView(tb);
                }
                TextBox_Enter(tb, new EventArgs());
            }
        }
        public void MoveTextBoxDown(TextBoxExtended tb)
        {
            if (tb != null)
            {
                TextBoxExtended tbBelowOneToMove = this.Items.Where(x => x.Location.Y > tb.Location.Y).OrderBy(x => x.Location.Y).FirstOrDefault();
                if (tbBelowOneToMove != null)
                {
                    int heightOfSelectedTb = tb.Height;
                    tbBelowOneToMove.Location = new Point(tbBelowOneToMove.Location.X, tbBelowOneToMove.Location.Y - heightOfSelectedTb);
                    tb.Location = new Point(tb.Location.X, tb.Location.Y + heightOfSelectedTb);
                    this.ScrollControlIntoView(tb);
                }
                TextBox_Enter(tb, new EventArgs());
            }
        }
        #endregion
    }
}
