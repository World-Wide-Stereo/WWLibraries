using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using ww.Utilities.Extensions;

namespace Controls.ComboBoxOfCheckBoxes
{
    /// <summary>
    /// Martin Lottering : 2007-10-27
    /// --------------------------------
    /// This is a usefull control in Filters. Allows you to save space and can replace a Grouped Box of CheckBoxes.
    /// Currently used on the TasksFilter for TaskStatusses, which means the user can select which Statusses to include
    /// in the "Search".
    /// This control does not implement a CheckBoxListBox, instead it adds a wrapper for the normal ComboBox and Items. 
    /// See the CheckBoxItems property.
    /// ----------------
    /// ALSO IMPORTANT: In Data Binding when setting the DataSource. The ValueMember must be a bool type property, because it will 
    /// be binded to the Checked property of the displayed CheckBox. Also see the DisplayMemberSingleItem for more information.
    /// ----------------
    /// Extends the CodeProject PopupComboBox "Simple pop-up control" "http://www.codeproject.com/cs/miscctrl/simplepopup.asp"
    /// by Lukasz Swiatkowski.
    /// </summary>
    public partial class ComboBoxOfCheckBoxes : PopupComboBox
    {
        #region CONSTRUCTOR

        public ComboBoxOfCheckBoxes()
            : base()
        {
            InitializeComponent();
            _CheckBoxProperties = new CheckBoxProperties();
            _CheckBoxProperties.PropertyChanged += new EventHandler(_CheckBoxProperties_PropertyChanged);
            // Dumps the ListControl in a(nother) Container to ensure the ScrollBar on the ListControl does not
            // Paint over the Size grip. Setting the Padding or Margin on the Popup or host control does
            // not work as I expected. I don't think it can work that way.
            ComboBoxOfCheckBoxesListControlContainer ContainerControl = new ComboBoxOfCheckBoxesListControlContainer();
            _ComboBoxOfCheckBoxesListControl = new ComboBoxOfCheckBoxesListControl(this);
            _ComboBoxOfCheckBoxesListControl.CheckBoxCheckedChanged += new EventHandler(Items_CheckBoxCheckedChanged);
            ContainerControl.Controls.Add(_ComboBoxOfCheckBoxesListControl);
            // This padding spaces neatly on the left-hand side and allows space for the size grip at the bottom.
            ContainerControl.Padding = new Padding(4, 0, 0, 14);
            // The ListControl FILLS the ListContainer.
            _ComboBoxOfCheckBoxesListControl.Dock = DockStyle.Fill;
            // The DropDownControl used by the base class. Will be wrapped in a popup by the base class.
            DropDownControl = ContainerControl;
            // Must be set after the DropDownControl is set, since the popup is recreated.
            // NOTE: I made the dropDown protected so that it can be accessible here. It was private.
            dropDown.Resizable = true;
        }

        #endregion

        #region PRIVATE FIELDS

        /// <summary>
        /// The checkbox list control. The public CheckBoxItems property provides a direct reference to its Items.
        /// </summary>
        internal ComboBoxOfCheckBoxesListControl _ComboBoxOfCheckBoxesListControl;
        /// <summary>
        /// In DataBinding operations, this property will be used as the DisplayMember in the ComboBoxOfCheckBoxesListBox.
        /// The normal/existing "DisplayMember" property is used by the TextBox of the ComboBox to display 
        /// a concatenated Text of the items selected. This concatenation and its formatting however is controlled 
        /// by the Binded object, since it owns that property.
        /// </summary>
        private string _DisplayMemberSingleItem = null;
        internal bool _AllItemAdded;
        internal bool _MustAddHiddenItem = false;
        internal bool _HiddenItemAdded;

        #endregion

        #region PRIVATE OPERATIONS

        /// <summary>
        /// Builds a CSV string of the items selected.
        /// </summary>
        internal string GetCSVText(bool skipFirstItem)
        {
            string ListText = String.Empty;
            int StartIndex =
                DropDownStyle == ComboBoxStyle.DropDownList 
                && DataSource == null
                && skipFirstItem
                    ? 1
                    : 0;
            if (!ShowAllItemInTextProperty && AddAllItem)
                StartIndex++;
            for (int Index = StartIndex; Index <= _ComboBoxOfCheckBoxesListControl.Items.Count - 1; Index++)
            {
                ComboBoxOfCheckBoxesItem Item = _ComboBoxOfCheckBoxesListControl.Items[Index];
                if (Item.Checked)
                    ListText += string.IsNullOrEmpty(ListText) ? Item.Text : $", {Item.Text}";
            }
            return ListText;
        }

        #endregion

        #region PUBLIC PROPERTIES

        /// <summary>
        /// A direct reference to the Items of ComboBoxOfCheckBoxesListControl.
        /// You can use it to Get or Set the Checked status of items manually if you want.
        /// But do not manipulate the List itself directly, e.g. Adding and Removing, 
        /// since the list is synchronised when shown with the ComboBox.Items. So for changing 
        /// the list contents, use Items instead.
        /// </summary>
        [Browsable(false)]
        public ComboBoxOfCheckBoxesItemList CheckBoxItems
        {
            get 
            { 
                // Added to ensure the CheckBoxItems are ALWAYS
                // available for modification via code.
                if (_ComboBoxOfCheckBoxesListControl.Items.Count != Items.Count)
                    _ComboBoxOfCheckBoxesListControl.SynchroniseControlsWithComboBoxItems();
                return _ComboBoxOfCheckBoxesListControl.Items; 
            }
        }
        /// <summary>
        /// The DataSource of the combobox. Refreshes the CheckBox wrappers when this is set.
        /// </summary>
        public new object DataSource
        {
            get { return base.DataSource; }
            set
            {
                base.DataSource = value;
                if (!string.IsNullOrEmpty(ValueMember))
                    // This ensures that at least the checkboxitems are available to be initialised.
                    _ComboBoxOfCheckBoxesListControl.SynchroniseControlsWithComboBoxItems();
            }
        }
        /// <summary>
        /// The ValueMember of the combobox. Refreshes the CheckBox wrappers when this is set.
        /// </summary>
        public new string ValueMember
        {
            get { return base.ValueMember; }
            set
            {
                base.ValueMember = value;
                if (!string.IsNullOrEmpty(ValueMember))
                    // This ensures that at least the checkboxitems are available to be initialised.
                    _ComboBoxOfCheckBoxesListControl.SynchroniseControlsWithComboBoxItems();
            }
        }
        /// <summary>
        /// In DataBinding operations, this property will be used as the DisplayMember in the ComboBoxOfCheckBoxesListBox.
        /// The normal/existing "DisplayMember" property is used by the TextBox of the ComboBox to display 
        /// a concatenated Text of the items selected. This concatenation however is controlled by the Binded 
        /// object, since it owns that property.
        /// </summary>
        public string DisplayMemberSingleItem
        {
            get { if (string.IsNullOrEmpty(_DisplayMemberSingleItem)) return DisplayMember; else return _DisplayMemberSingleItem; }
            set { _DisplayMemberSingleItem = value; }
        }
        /// <summary>
        /// Made this property Browsable again, since the Base Popup hides it. This class uses it again.
        /// Gets an object representing the collection of the items contained in this 
        /// System.Windows.Forms.ComboBox.
        /// </summary>
        /// <returns>A System.Windows.Forms.ComboBox.ObjectCollection representing the items in 
        /// the System.Windows.Forms.ComboBox.
        /// </returns>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public new ObjectCollection Items
        {
            get { return base.Items; }
        }

        [Browsable(true)]
        public new int DropDownWidth
        {
            get { return dropDown.Width; }
            set { dropDown.Width = value; }
        }

        [Browsable(true)]
        public new int DropDownHeight
        {
            get { return dropDown.Height; }
            set { dropDown.Height = value; }
        }

        [Browsable(true)]
        private bool _showAllItemInTextProperty = true;
        public bool ShowAllItemInTextProperty
        {
            get { return _showAllItemInTextProperty; }
            set { _showAllItemInTextProperty = value; }
        }

        [Browsable(true)]
        private bool _addAllItem = true;
        public bool AddAllItem
        {
            get { return _addAllItem; }
            set { _addAllItem = value; }
        }

        [Browsable(true)]
        private string _allItemText = "[All]";
        public string AllItemText
        {
            get
            {
                return AllItemListItem == null ? _allItemText : AllItemListItem.Text;
            }
            set
            {
                _allItemText = value;
            }
        }
        /// <summary>
        /// Overrides AllItemText.
        /// </summary>
        public ListItem AllItemListItem { get; set; }

        private bool _allItemSet;
        private CheckBox _allItem;
        public CheckBox AllItem
        {
            get
            {
                if (!_allItemSet && AddAllItem)
                {
                    _allItem = this.CheckBoxItems.FirstOrDefault(x => x.Text == AllItemText);
                    _allItemSet = _allItem != null;
                }
                return _allItem;
            }
        }

        #endregion

        #region EVENTS & EVENT HANDLERS

        public event EventHandler CheckBoxCheckedChanged;

        private void Items_CheckBoxCheckedChanged(object sender, EventArgs e)
        {
            OnCheckBoxCheckedChanged(sender, e);
        }

        #endregion

        #region EVENT CALLERS and OVERRIDES e.g. OnResize()

        protected void OnCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            string ListText = GetCSVText(true);
            // The DropDownList style seems to require that the text
            // part of the "textbox" should match a single item.
            if (DropDownStyle != ComboBoxStyle.DropDownList)
                Text = ListText;
            // This refreshes the Text of the first item (which is not visible)
            else if (DataSource == null)
            {
                Items[0] = ListText;
                // Keep the hidden item and first checkbox item in 
                // sync in order to ensure the Synchronise process
                // can match the items.
                CheckBoxItems[0].ComboBoxItem = ListText;
            }

            EventHandler handler = CheckBoxCheckedChanged;
            if (handler != null)
                handler(sender, e);
        }

        /// <summary>
        /// Will add an invisible item when the style is DropDownList,
        /// to help maintain the correct text in main TextBox.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDropDownStyleChanged(EventArgs e)
        {
            base.OnDropDownStyleChanged(e);

            if (DropDownStyle == ComboBoxStyle.DropDownList
                && DataSource == null
                && !DesignMode)
                _MustAddHiddenItem = true;
        }

        protected override void OnResize(EventArgs e)
        {
            // When the ComboBox is resized, the width of the dropdown 
            // is also resized to match the width of the ComboBox. I think it looks better.
            Size Size = new Size(Width, DropDownControl.Height);
            dropDown.Size = Size;
            base.OnResize(e);
        }

        #endregion

        #region PUBLIC OPERATIONS

        /// <summary>
        /// A function to clear/reset the list.
        /// (Ubiklou : http://www.codeproject.com/KB/combobox/extending_combobox.aspx?msg=2526813#xx2526813xx)
        /// </summary>
        public void Clear()
        {
            if (_AllItemAdded)
            {
                if (AllItem != null)
                {
                    // This will uncheck every CheckBox in the list.
                    // Without this, we would uncheck them twice in the loop below.
                    AllItem.Checked = false;
                }
                _AllItemAdded = false;
                _allItemSet = false;
            }
            foreach (ComboBoxOfCheckBoxesItem item in _ComboBoxOfCheckBoxesListControl.Items.Where(x => x.Checked))
            {
                item.Checked = false;
            }
            _HiddenItemAdded = false;
            this.Items.Clear();
            if (DropDownStyle == ComboBoxStyle.DropDownList && DataSource == null)
                _MustAddHiddenItem = true;
        }

        /// <summary>
        /// Uncheck all items.
        /// </summary>
        public void ClearSelection()
        {
            foreach (ComboBoxOfCheckBoxesItem Item in CheckBoxItems)
                if (Item.Checked)
                    Item.Checked = false;
        }

        /// <summary>
        /// Sets the DropDownHeight to show all items without requiring scrolling.
        /// </summary>
        public void SetDropDownHeightToShowAllItems()
        {
            this.DropDownHeight = 27 * (this.Items.Count + (this.AddAllItem ? 1 : 0));
        }

        /// <summary>
        /// Populates the dropdown with the values in the specified enum type.
        /// </summary>
        public void PopulateWithEnum<T>(IEnumerable<T> valuesToAdd = null, bool orderByName = false, bool setDropDownHeightToShowAllItems = true) where T : struct, IConvertible
        {
            IEnumerable<Enum> enumValues = valuesToAdd == null ? Enum.GetValues(typeof(T)).Cast<Enum>() : valuesToAdd.Select(x => (Enum)(object)x);
            if (orderByName)
            {
                enumValues = enumValues.OrderBy(x => x.GetLabel());
            }
            this.Items.AddRange(enumValues.Select(x => new ListItem { Text = x.GetLabel(), Value = x }).ToArray());

            if (setDropDownHeightToShowAllItems)
            {
                SetDropDownHeightToShowAllItems();
            }
        }

        #endregion

        #region CHECKBOX PROPERTIES (DEFAULTS)

        private CheckBoxProperties _CheckBoxProperties;

        /// <summary>
        /// The properties that will be assigned to the checkboxes as default values.
        /// </summary>
        [Description("The properties that will be assigned to the checkboxes as default values.")]
        [Browsable(true)]
        public CheckBoxProperties CheckBoxProperties
        {
            get { return _CheckBoxProperties; }
            set { _CheckBoxProperties = value; _CheckBoxProperties_PropertyChanged(this, EventArgs.Empty); }
        }

        private void _CheckBoxProperties_PropertyChanged(object sender, EventArgs e)
        {
            foreach (ComboBoxOfCheckBoxesItem Item in CheckBoxItems)
                Item.ApplyProperties(CheckBoxProperties);
        }

        #endregion

        protected override void WndProc(ref Message m)
        {
            // 323 : Item Added
            // 331 : Clearing
            if (m.Msg == 331
                && DropDownStyle == ComboBoxStyle.DropDownList
                && DataSource == null)
            {
                _MustAddHiddenItem = true;
            }
            
            base.WndProc(ref m);
        }
    }

    /// <summary>
    /// A container control for the ListControl to ensure the ScrollBar on the ListControl does not
    /// Paint over the Size grip. Setting the Padding or Margin on the Popup or host control does
    /// not work as I expected.
    /// </summary>
    [ToolboxItem(false)]
    public class ComboBoxOfCheckBoxesListControlContainer : UserControl
    {
        #region CONSTRUCTOR

        public ComboBoxOfCheckBoxesListControlContainer()
            : base()
        {
            BackColor = SystemColors.Window;
            BorderStyle = BorderStyle.FixedSingle;
            AutoScaleMode = AutoScaleMode.Inherit;
            ResizeRedraw = true;
            // If you don't set this, then resize operations cause an error in the base class.
            MinimumSize = new Size(1, 1);
            MaximumSize = new Size(500, 500);
        }
        #endregion

        #region RESIZE OVERRIDE REQUIRED BY THE POPUP CONTROL

        /// <summary>
        /// Prescribed by the Popup class to ensure Resize operations work correctly.
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            if ((Parent as Popup).ProcessResizing(ref m))
            {
                return;
            }
            base.WndProc(ref m);
        }
        #endregion
    }

    /// <summary>
    /// This ListControl that pops up to the User. It contains the ComboBoxOfCheckBoxesItems. 
    /// The items are docked DockStyle.Top in this control.
    /// </summary>
    [ToolboxItem(false)]
    public class ComboBoxOfCheckBoxesListControl : ScrollableControl
    {
        #region CONSTRUCTOR

        public ComboBoxOfCheckBoxesListControl(ComboBoxOfCheckBoxes owner)
            : base()
        {
            DoubleBuffered = true;
            _ComboBoxOfCheckBoxes = owner;
            _Items = new ComboBoxOfCheckBoxesItemList(_ComboBoxOfCheckBoxes);
            BackColor = SystemColors.Window;
            // AutoScaleMode = AutoScaleMode.Inherit;
            AutoScroll = true;
            ResizeRedraw = true;
            // if you don't set this, a Resize operation causes an error in the base class.
            MinimumSize = new Size(1, 1);
            MaximumSize = new Size(500, 500);
        }

        #endregion

        #region PRIVATE PROPERTIES

        /// <summary>
        /// Simply a reference to the ComboBoxOfCheckBoxes.
        /// </summary>
        private ComboBoxOfCheckBoxes _ComboBoxOfCheckBoxes;
        /// <summary>
        /// A Typed list of ComboBoxCheckBoxItems.
        /// </summary>
        private ComboBoxOfCheckBoxesItemList _Items;

        #endregion

        public ComboBoxOfCheckBoxesItemList Items { get { return _Items; } }

        #region RESIZE OVERRIDE REQUIRED BY THE POPUP CONTROL

        /// <summary>
        /// Prescribed by the Popup control to enable Resize operations.
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            if ((Parent.Parent as Popup).ProcessResizing(ref m))
            {
                return;
            }
            base.WndProc(ref m);
        }

        #endregion

        #region PROTECTED MEMBERS

        protected override void OnVisibleChanged(EventArgs e)
        {
            // Synchronises the CheckBox list with the items in the ComboBox.
            SynchroniseControlsWithComboBoxItems();
            base.OnVisibleChanged(e);
        }
        /// <summary>
        /// Maintains the controls displayed in the list by keeping them in sync with the actual 
        /// items in the combobox. (e.g. removing and adding as well as ordering)
        /// </summary>
        public void SynchroniseControlsWithComboBoxItems()
        {
            if (_ComboBoxOfCheckBoxes.Items.Count == 0 && _Items.Count == 0)
            {
                // Avoids adding the all item and hidden item when there are no other actual items to add.
                // Also avoids unnecessary work.
                return;
            }

            SuspendLayout();

            if (_ComboBoxOfCheckBoxes.AddAllItem && !_ComboBoxOfCheckBoxes._AllItemAdded && _ComboBoxOfCheckBoxes.Items.Count > 0 && _ComboBoxOfCheckBoxes.DataSource == null)
            {
                if (_ComboBoxOfCheckBoxes.AllItemListItem == null)
                {
                    _ComboBoxOfCheckBoxes.Items.Insert(0, _ComboBoxOfCheckBoxes.AllItemText);
                }
                else
                {
                    _ComboBoxOfCheckBoxes.Items.Insert(0, _ComboBoxOfCheckBoxes.AllItemListItem);
                }
                _ComboBoxOfCheckBoxes._AllItemAdded = true;
            }
            if (_ComboBoxOfCheckBoxes._MustAddHiddenItem && _ComboBoxOfCheckBoxes.Items.Count > (_ComboBoxOfCheckBoxes._AllItemAdded ? 1 : 0))
            {
                _ComboBoxOfCheckBoxes.Items.Insert(0, _ComboBoxOfCheckBoxes.GetCSVText(false)); // INVISIBLE ITEM
                _ComboBoxOfCheckBoxes.SelectedIndex = 0;
                _ComboBoxOfCheckBoxes._HiddenItemAdded = true;
                _ComboBoxOfCheckBoxes._MustAddHiddenItem = false;
            }
            Controls.Clear();
            #region Disposes all items that are no longer in the combo box list

            for (int Index = _Items.Count - 1; Index >= 0; Index--)
            {
                ComboBoxOfCheckBoxesItem Item = _Items[Index];
                if (!_ComboBoxOfCheckBoxes.Items.Contains(Item.ComboBoxItem))
                {
                    _Items.Remove(Item);
                    Item.Dispose();
                }
            }

            #endregion
            #region Recreate the list in the same order of the combo box items

            bool HasHiddenItem = _ComboBoxOfCheckBoxes._HiddenItemAdded && !DesignMode;

            ComboBoxOfCheckBoxesItemList NewList = new ComboBoxOfCheckBoxesItemList(_ComboBoxOfCheckBoxes);
            for (int Index0 = 0; Index0 <= _ComboBoxOfCheckBoxes.Items.Count - 1; Index0++)
            {
                object Object = _ComboBoxOfCheckBoxes.Items[Index0];
                ComboBoxOfCheckBoxesItem Item = null;
                // The hidden item could match any other item when only
                // one other item was selected.
                if (Index0 == 0 && HasHiddenItem && _Items.Count > 0)
                {
                    Item = _Items[0];
                }
                else
                {
                    int StartIndex = HasHiddenItem
                        ? 1 // Skip the hidden item, it could match 
                        : 0;
                    for (int Index1 = StartIndex; Index1 <= _Items.Count - 1; Index1++)
                    {
                        if (_Items[Index1].ComboBoxItem == Object)
                        {
                            Item = _Items[Index1];
                            break;
                        }
                    }
                }
                if (Item == null)
                {
                    Item = new ComboBoxOfCheckBoxesItem(_ComboBoxOfCheckBoxes, Object);
                    Item.ApplyProperties(_ComboBoxOfCheckBoxes.CheckBoxProperties);

                    if (_ComboBoxOfCheckBoxes.AddAllItem && Item.Text == _ComboBoxOfCheckBoxes.AllItemText)
                    {
                        Item.CheckedChanged += allItem_CheckedChanged;
                    }
                    else
                    {
                        Item.CheckedChanged += item_CheckedChanged;
                    }
                }
                NewList.Add(Item);
                Item.Dock = DockStyle.Top;
            }
            _Items.Clear();
            _Items.AddRange(NewList);

            #endregion
            #region Add the items to the controls in reversed order to maintain correct docking order

            if (NewList.Count > 0)
            {
                // This reverse helps to maintain correct docking order.
                NewList.Reverse();
                // If you get an error here that "Cannot convert to the desired 
                // type, it probably means the controls are not binding correctly.
                // The Checked property is binded to the ValueMember property. 
                // It must be a bool for example.
                Controls.AddRange(NewList.ToArray());
            }

            #endregion

            // Keep the first item invisible
            if (HasHiddenItem)
                _ComboBoxOfCheckBoxes.CheckBoxItems[0].Visible = false; 
            
            ResumeLayout();
        }

        #endregion

        #region EVENTS

        public event EventHandler CheckBoxCheckedChanged;
        protected void OnCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            EventHandler handler = CheckBoxCheckedChanged;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        public void allItem_CheckedChanged(object sender, EventArgs e)
        {
            // Check all other items when the [All] item is checked.
            bool isChecked = ((CheckBox)sender).Checked;
            for (int i = (_ComboBoxOfCheckBoxes._HiddenItemAdded ? 2 : 1); i < _ComboBoxOfCheckBoxes.CheckBoxItems.Count; i++)
            {
                _ComboBoxOfCheckBoxes.CheckBoxItems[i].CheckedChanged -= item_CheckedChanged;
                _ComboBoxOfCheckBoxes.CheckBoxItems[i].Checked = isChecked;
                _ComboBoxOfCheckBoxes.CheckBoxItems[i].CheckedChanged += item_CheckedChanged;
            }

            OnCheckBoxCheckedChanged(sender, e);
        }
        public void item_CheckedChanged(object sender, EventArgs e)
        {
            if (_ComboBoxOfCheckBoxes.AllItem != null)
            {
                if (((CheckBox)sender).Checked)
                {
                    // Check the [All] item when all other items have been checked.
                    if (!_ComboBoxOfCheckBoxes.AllItem.Checked && _ComboBoxOfCheckBoxes.CheckBoxItems.Skip(_ComboBoxOfCheckBoxes._HiddenItemAdded ? 2 : 1).All(x => x.Checked))
                    {
                        _ComboBoxOfCheckBoxes.AllItem.CheckedChanged -= allItem_CheckedChanged;
                        _ComboBoxOfCheckBoxes.AllItem.Checked = true;
                        _ComboBoxOfCheckBoxes.AllItem.CheckedChanged += allItem_CheckedChanged;
                    }
                }
                else
                {
                    // Uncheck the [All] item when another item is unchecked.
                    _ComboBoxOfCheckBoxes.AllItem.CheckedChanged -= allItem_CheckedChanged;
                    _ComboBoxOfCheckBoxes.AllItem.Checked = false;
                    _ComboBoxOfCheckBoxes.AllItem.CheckedChanged += allItem_CheckedChanged;
                }
            }

            OnCheckBoxCheckedChanged(sender, e);
        }

        #endregion
    }

    /// <summary>
    /// The CheckBox items displayed in the Popup of the ComboBox.
    /// </summary>
    [ToolboxItem(false)]
    public class ComboBoxOfCheckBoxesItem : CheckBox
    {
        #region CONSTRUCTOR

        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner">A reference to the ComboBoxOfCheckBoxes.</param>
        /// <param name="comboBoxItem">A reference to the item in the ComboBox.Items that this object is extending.</param>
        public ComboBoxOfCheckBoxesItem(ComboBoxOfCheckBoxes owner, object comboBoxItem)
            : base()
        {
            DoubleBuffered = true;
            _ComboBoxOfCheckBoxes = owner;
            _ComboBoxItem = comboBoxItem;
            if (_ComboBoxOfCheckBoxes.DataSource != null)
                AddBindings();
            else
                Text = comboBoxItem.ToString();
        }
        #endregion

        #region PRIVATE PROPERTIES

        /// <summary>
        /// A reference to the ComboBoxOfCheckBoxes.
        /// </summary>
        private ComboBoxOfCheckBoxes _ComboBoxOfCheckBoxes;
        /// <summary>
        /// A reference to the Item in ComboBox.Items that this object is extending.
        /// </summary>
        private object _ComboBoxItem;

        #endregion

        #region PUBLIC PROPERTIES

        /// <summary>
        /// A reference to the Item in ComboBox.Items that this object is extending.
        /// </summary>
        public object ComboBoxItem
        {
            get { return _ComboBoxItem; }
            internal set { _ComboBoxItem = value; }
        }

        #endregion

        #region BINDING HELPER

        /// <summary>
        /// When using Data Binding operations via the DataSource property of the ComboBox. This
        /// adds the required Bindings for the CheckBoxes.
        /// </summary>
        public void AddBindings()
        {
            // Note, the text uses "DisplayMemberSingleItem", not "DisplayMember" (unless its not assigned)
            DataBindings.Add(
                "Text",
                _ComboBoxItem,
                _ComboBoxOfCheckBoxes.DisplayMemberSingleItem
                );
            // The ValueMember must be a bool type property usable by the CheckBox.Checked.
            DataBindings.Add(
                "Checked",
                _ComboBoxItem,
                _ComboBoxOfCheckBoxes.ValueMember,
                false,
                // This helps to maintain proper selection state in the Binded object,
                // even when the controls are added and removed.
                DataSourceUpdateMode.OnPropertyChanged,
                false, null, null);
            // Helps to maintain the Checked status of this
            // checkbox before the control is visible
            if (_ComboBoxItem is INotifyPropertyChanged)
                ((INotifyPropertyChanged)_ComboBoxItem).PropertyChanged += 
                    new PropertyChangedEventHandler(
                        ComboBoxOfCheckBoxesItem_PropertyChanged);
        }

        #endregion

        #region PROTECTED MEMBERS

        protected override void OnCheckedChanged(EventArgs e)
        {
            // Found that when this event is raised, the bool value of the binded item is not yet updated.
            if (_ComboBoxOfCheckBoxes.DataSource != null)
            {
                PropertyInfo PI = ComboBoxItem.GetType().GetProperty(_ComboBoxOfCheckBoxes.ValueMember);
                PI.SetValue(ComboBoxItem, Checked, null);
            }
            base.OnCheckedChanged(e);
            // Forces a refresh of the Text displayed in the main TextBox of the ComboBox,
            // since that Text will most probably represent a concatenation of selected values.
            // Also see DisplayMemberSingleItem on the ComboBoxOfCheckBoxes for more information.
            if (_ComboBoxOfCheckBoxes.DataSource != null)
            {
                string OldDisplayMember = _ComboBoxOfCheckBoxes.DisplayMember;
                _ComboBoxOfCheckBoxes.DisplayMember = null;
                _ComboBoxOfCheckBoxes.DisplayMember = OldDisplayMember;
            }
        }

        #endregion

        #region HELPER MEMBERS

        internal void ApplyProperties(CheckBoxProperties properties)
        {
            this.Appearance = properties.Appearance;
            this.AutoCheck = properties.AutoCheck;
            this.AutoEllipsis = properties.AutoEllipsis;
            this.AutoSize = properties.AutoSize;
            this.CheckAlign = properties.CheckAlign;
            this.FlatAppearance.BorderColor = properties.FlatAppearanceBorderColor;
            this.FlatAppearance.BorderSize = properties.FlatAppearanceBorderSize;
            this.FlatAppearance.CheckedBackColor = properties.FlatAppearanceCheckedBackColor;
            this.FlatAppearance.MouseDownBackColor = properties.FlatAppearanceMouseDownBackColor;
            this.FlatAppearance.MouseOverBackColor = properties.FlatAppearanceMouseOverBackColor;
            this.FlatStyle = properties.FlatStyle;
            this.ForeColor = properties.ForeColor;
            this.RightToLeft = properties.RightToLeft;
            this.TextAlign = properties.TextAlign;
            this.ThreeState = properties.ThreeState;
        }

        #endregion

        #region EVENT HANDLERS - ComboBoxItem (DataSource)

        /// <summary>
        /// Added this handler because the control doesn't seem 
        /// to initialize correctly until shown for the first
        /// time, which also means the summary text value
        /// of the combo is out of sync initially.
        /// </summary>
        private void ComboBoxOfCheckBoxesItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _ComboBoxOfCheckBoxes.ValueMember)
                Checked = 
                    (bool)_ComboBoxItem
                        .GetType()
                        .GetProperty(_ComboBoxOfCheckBoxes.ValueMember)
                        .GetValue(_ComboBoxItem, null);
        }

        #endregion
    }

    /// <summary>
    /// A Typed List of the CheckBox items.
    /// Simply a wrapper for the ComboBoxOfCheckBoxes.Items. A list of ComboBoxOfCheckBoxesItem objects.
    /// This List is automatically synchronised with the Items of the ComboBox and extended to
    /// handle the additional boolean value. That said, do not Add or Remove using this List, 
    /// it will be lost or regenerated from the ComboBox.Items.
    /// </summary>
    [ToolboxItem(false)]
    public class ComboBoxOfCheckBoxesItemList : List<ComboBoxOfCheckBoxesItem>
    {
        #region CONSTRUCTORS

        public ComboBoxOfCheckBoxesItemList(ComboBoxOfCheckBoxes comboBoxOfCheckBoxes)
        {
            _ComboBoxOfCheckBoxes = comboBoxOfCheckBoxes;
        }

        #endregion

        #region PRIVATE FIELDS

        private ComboBoxOfCheckBoxes _ComboBoxOfCheckBoxes;

        #endregion

        #region LIST MEMBERS & OBSOLETE INDICATORS

        [Obsolete("Do not add items to this list directly. Use the ComboBox items instead.", false)]
        public new void Add(ComboBoxOfCheckBoxesItem item)
        {
            base.Add(item);
        }

        [Obsolete("Do not remove items from this list directly. Use the ComboBox items instead.", false)]
        public new bool Remove(ComboBoxOfCheckBoxesItem item)
        {
            return base.Remove(item);
        }

        #endregion

        #region DEFAULT PROPERTIES

        /// <summary>
        /// Returns the item with the specified displayName or Text.
        /// </summary>
        public ComboBoxOfCheckBoxesItem this[string displayName]
        {
            get
            {
                int StartIndex =
                    // An invisible item exists in this scenario to help 
                    // with the Text displayed in the TextBox of the Combo
                    _ComboBoxOfCheckBoxes._HiddenItemAdded
                        ? 1 // Ubiklou : 2008-04-28 : Ignore first item. (http://www.codeproject.com/KB/combobox/extending_combobox.aspx?fid=476622&df=90&mpp=25&noise=3&sort=Position&view=Quick&select=2526813&fr=1#xx2526813xx)
                        : 0;
                for(int Index = StartIndex; Index <= Count - 1; Index ++)
                {
                    ComboBoxOfCheckBoxesItem Item = this[Index];

                    string Text;
                    // The binding might not be active yet
                    if (string.IsNullOrEmpty(Item.Text)
                        // Ubiklou : 2008-04-28 : No databinding
                        && Item.DataBindings != null 
                        && Item.DataBindings["Text"] != null
                        )
                    {
                        PropertyInfo PropertyInfo
                            = Item.ComboBoxItem.GetType().GetProperty(
                                Item.DataBindings["Text"].BindingMemberInfo.BindingMember);
                        Text = (string)PropertyInfo.GetValue(Item.ComboBoxItem, null);
                    }
                    else
                        Text = Item.Text;
                    if (Text.CompareTo(displayName) == 0)
                        return Item;
                }
                throw new ArgumentOutOfRangeException($"\"{displayName}\" does not exist in this combo box.");
            }
        }
        
        #endregion
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class CheckBoxProperties
    {
        public CheckBoxProperties() { }

        #region PRIVATE PROPERTIES

        private Appearance _Appearance = Appearance.Normal;
        private bool _AutoSize = false;
        private bool _AutoCheck = true;
        private bool _AutoEllipsis = false;
        private ContentAlignment _CheckAlign = ContentAlignment.MiddleLeft;
        private Color _FlatAppearanceBorderColor = Color.Empty;
        private int _FlatAppearanceBorderSize = 1;
        private Color _FlatAppearanceCheckedBackColor = Color.Empty;
        private Color _FlatAppearanceMouseDownBackColor = Color.Empty;
        private Color _FlatAppearanceMouseOverBackColor = Color.Empty;
        private FlatStyle _FlatStyle = FlatStyle.Standard;
        private Color _ForeColor = SystemColors.ControlText;
        private RightToLeft _RightToLeft = RightToLeft.No;
        private ContentAlignment _TextAlign = ContentAlignment.MiddleLeft;
        private bool _ThreeState = false;

        #endregion

        #region PUBLIC PROPERTIES

        [DefaultValue(Appearance.Normal)]
        public Appearance Appearance
        {
            get { return _Appearance; }
            set { _Appearance = value; OnPropertyChanged(); }
        }
        [DefaultValue(true)]
        public bool AutoCheck
        {
            get { return _AutoCheck; }
            set { _AutoCheck = value; OnPropertyChanged(); }
        }
        [DefaultValue(false)]
        public bool AutoEllipsis
        {
            get { return _AutoEllipsis; }
            set { _AutoEllipsis = value; OnPropertyChanged(); }
        }
        [DefaultValue(false)]
        public bool AutoSize
        {
            get { return _AutoSize; }
            set { _AutoSize = true; OnPropertyChanged(); }
        }
        [DefaultValue(ContentAlignment.MiddleLeft)]
        public ContentAlignment CheckAlign
        {
            get { return _CheckAlign; }
            set { _CheckAlign = value; OnPropertyChanged(); }
        }
        [DefaultValue(typeof(Color), "")]
        public Color FlatAppearanceBorderColor
        {
            get { return _FlatAppearanceBorderColor; }
            set { _FlatAppearanceBorderColor = value; OnPropertyChanged(); }
        }
        [DefaultValue(1)]
        public int FlatAppearanceBorderSize
        {
            get { return _FlatAppearanceBorderSize; }
            set { _FlatAppearanceBorderSize = value; OnPropertyChanged(); }
        }
        [DefaultValue(typeof(Color), "")]
        public Color FlatAppearanceCheckedBackColor
        {
            get { return _FlatAppearanceCheckedBackColor; }
            set { _FlatAppearanceCheckedBackColor = value; OnPropertyChanged(); }
        }
        [DefaultValue(typeof(Color), "")]
        public Color FlatAppearanceMouseDownBackColor
        {
            get { return _FlatAppearanceMouseDownBackColor; }
            set { _FlatAppearanceMouseDownBackColor = value; OnPropertyChanged(); }
        }
        [DefaultValue(typeof(Color), "")]
        public Color FlatAppearanceMouseOverBackColor
        {
            get { return _FlatAppearanceMouseOverBackColor; }
            set { _FlatAppearanceMouseOverBackColor = value; OnPropertyChanged(); }
        }
        [DefaultValue(FlatStyle.Standard)]
        public FlatStyle FlatStyle
        {
            get { return _FlatStyle; }
            set { _FlatStyle = value; OnPropertyChanged(); }
        }
        [DefaultValue(typeof(SystemColors), "ControlText")]
        public Color ForeColor
        {
            get { return _ForeColor; }
            set { _ForeColor = value; OnPropertyChanged(); }
        }
        [DefaultValue(RightToLeft.No)]
        public RightToLeft RightToLeft
        {
            get { return _RightToLeft; }
            set { _RightToLeft = value; OnPropertyChanged(); }
        }
        [DefaultValue(ContentAlignment.MiddleLeft)]
        public ContentAlignment TextAlign
        {
            get { return _TextAlign; }
            set { _TextAlign = value; OnPropertyChanged(); }
        }
        [DefaultValue(false)]
        public bool ThreeState
        {
            get { return _ThreeState; }
            set { _ThreeState = value; OnPropertyChanged(); }
        }

        #endregion

        #region EVENTS AND EVENT CALLERS

        /// <summary>
        /// Called when any property changes.
        /// </summary>
        public event EventHandler PropertyChanged;

        protected void OnPropertyChanged()
        {
            EventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #endregion
    }
}
