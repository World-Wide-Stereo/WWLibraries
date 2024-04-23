#region (c) 2006 Matt Valerio
// DropDownTreeView Control
//
// 6.21.06 Matt Valerio
// (matt.valerio@gmail.com)
//
// Filename: DropDownTreeView.cs
//
// Description: Provides a DropDownTreeView control that extends the TreeView control by allowing a
// ComboBox to be displayed at specific TreeNodes to select the text of the node.
//
// ============================================================================
// This software is free software; you can modify and/or redistribute it, provided
// that the author is credited with the work.
//
// This library is distributed in the hope that it will be useful, but WITHOUT ANY
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
// PARTICULAR PURPOSE.
// ============================================================================
//
// Revisions:
//   1  6.21.06 MDV  Initial CodeProject article
//   2  7.21.06 MDV  Fixed BringToFront() suggestion in the comment by OrlandoCurioso
//                   Cleaned up the the node type determination as suggested in the comment by mpasqual
//                   Removed OnClick
//                   Overrode the OnMouseWheel function and called HideComboBox()
//                   Added handler for the DropDownChanged event of the ComboBox -- this alleviates many of the problems
//   3  7.24.06 MDV  Changed ComboBox dropdown to happen on click instead of label edit.
#endregion


using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;


namespace Controls
{
    /// <summary>
    /// Provides the usual TreeView control with the ability to edit the labels of the nodes
    /// by using a drop-down ComboBox.
    /// </summary>
    public class DropDownTreeView : TreeView
    {
        public bool ShowDropDown { get; set; }
        public bool ShowAddRemoveButtons { get; set; }

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:DropDownTreeView"/> class.
        /// </summary>
        public DropDownTreeView()
            : base()
        {
            DrawMode = TreeViewDrawMode.OwnerDrawText;
            this.MouseMove += DropDownTreeView_MouseMove;
        }
        #endregion


        // We'll use this variable to keep track of the current node that is being edited.
        // This is set to something (non-null) only if the node's ComboBox is being displayed.
        private DropDownTreeNode m_CurrentNode = null;


        /// <summary>
        /// Occurs when the <see cref="E:System.Windows.Forms.TreeView.NodeMouseClick"></see> event is fired
        /// -- that is, when a node in the tree view is clicked.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.TreeNodeMouseClickEventArgs"></see> that contains the event data.</param>
        protected override void OnNodeMouseClick(TreeNodeMouseClickEventArgs e)
        {            
            // Are we dealing with a dropdown node?
            if (e.Node is DropDownTreeNode)
            {
                this.m_CurrentNode = (DropDownTreeNode)e.Node;
                if (ShowDropDown && e.X > this.m_CurrentNode.Bounds.X)
                {

                    // Need to add the node's ComboBox to the TreeView's list of controls for it to work
                    this.Controls.Add(this.m_CurrentNode.ComboBox);

                    // Set the bounds of the ComboBox, with a little adjustment to make it look right
                    this.m_CurrentNode.ComboBox.SetBounds(
                        this.m_CurrentNode.Bounds.X - 1,
                        this.m_CurrentNode.Bounds.Y - 2,
                        Math.Min(this.m_CurrentNode.Bounds.Width - 5, this.Width - this.m_CurrentNode.Bounds.X - this.m_CurrentNode.Bounds.Width - 55),
                        this.m_CurrentNode.Bounds.Height);

                    this.m_CurrentNode.ComboBox.Text = this.m_CurrentNode.Text;

                    // Listen to the SelectedValueChanged event of the node's ComboBox
                    this.m_CurrentNode.ComboBox.SelectedValueChanged += new EventHandler(ComboBox_SelectedValueChanged);
                    this.m_CurrentNode.ComboBox.DropDownClosed += new EventHandler(ComboBox_DropDownClosed);

                    // Now show the ComboBox
                    this.m_CurrentNode.ComboBox.Show();
                    this.m_CurrentNode.ComboBox.DroppedDown = true;
                }
            }
            base.OnNodeMouseClick(e);
        }


        /// <summary>
        /// Handles the SelectedValueChanged event of the ComboBox control.
        /// Hides the ComboBox if an item has been selected in it.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
        void ComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            HideComboBox();
        }


        /// <summary>
        /// Handles the DropDownClosed event of the ComboBox control.
        /// Hides the ComboBox if the user clicks anywhere else on the TreeView or adjusts the scrollbars, or scrolls the mouse wheel.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
        void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            HideComboBox();
        }


        /// <summary>
        /// Handles the <see cref="E:System.Windows.Forms.Control.MouseWheel"></see> event.
        /// Hides the ComboBox if the user scrolls the mouse wheel.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            HideComboBox();
            base.OnMouseWheel(e);
        }


        /// <summary>
        /// Method to hide the currently-selected node's ComboBox
        /// </summary>
        private void HideComboBox()
        {
            if (ShowDropDown && this.m_CurrentNode != null)
            {
                // Unregister the event listener
                this.m_CurrentNode.ComboBox.SelectedValueChanged -= ComboBox_SelectedValueChanged;
                this.m_CurrentNode.ComboBox.DropDownClosed -= ComboBox_DropDownClosed;                                

                // Copy the selected text from the ComboBox to the TreeNode
                this.m_CurrentNode.Text = this.m_CurrentNode.ComboBox.Text;

                // Hide the ComboBox
                this.m_CurrentNode.ComboBox.Hide();
                this.m_CurrentNode.ComboBox.DroppedDown = false;

                // Remove the control from the TreeView's list of currently-displayed controls
                this.Controls.Remove(this.m_CurrentNode.ComboBox);

                // And return to the default state (no ComboBox displayed)
                this.m_CurrentNode = null;
            }
        
        }        

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            e.DrawDefault = true;
            base.OnDrawNode(e);
            ToggleButtons(e.Node);
        }

        protected void ToggleButtons(TreeNode node)
        {
            if (node is DropDownTreeNode)
            {
                ToggleButtons(node as DropDownTreeNode);
            }
        }

        internal void ToggleButtons(DropDownTreeNode node)
        {
            if (ShowAddRemoveButtons)
            {
                if (node.IsVisible)
                {
                    this.Controls.Add(node.Buttons);

                    node.Buttons.SetBounds(
                        this.Width - 50,
                        node.Bounds.Y,
                        0,
                        0,
                        BoundsSpecified.X | BoundsSpecified.Y);
                    node.Buttons.Tag = node;
                    node.Buttons.Show();
                }
                else
                {
                    this.Controls.Remove(node.Buttons);
                    node.Buttons.Hide();
                }

                foreach (var child in node.Nodes)
                {
                    ToggleButtons(child as TreeNode);
                }
            }

        }

        protected override void OnAfterCollapse(TreeViewEventArgs e)
        {
            base.OnAfterCollapse(e);
            ToggleButtons(e.Node);
        }

        protected override void OnAfterExpand(TreeViewEventArgs e)
        {
            base.OnAfterCollapse(e);
            ToggleButtons(e.Node);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // DropDownTreeView
            // 
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.DropDownTreeView_MouseMove);
            this.ResumeLayout(false);

        }

        // From http://support.microsoft.com/kb/322634
        private ToolTip toolTip = new ToolTip
                                      {
                                          InitialDelay = 1,
                                      };
        private void DropDownTreeView_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the node at the current mouse pointer location.
            TreeNode theNode = this.GetNodeAt(e.X, e.Y);

            // Set a ToolTip only if the mouse pointer is actually paused on a node.
            if ((theNode != null))
            {
                // Verify that the tag property is not "null".
                if (theNode.ToolTipText != null)
                {
                    // Change the ToolTip only if the pointer moved to a new node.
                    if (toolTip.Tag != theNode)
                    {
                        this.toolTip.SetToolTip(this, theNode.ToolTipText);
                        toolTip.Tag = theNode;
                    }
                }
                else
                {
                    this.toolTip.SetToolTip(this, "");
                    toolTip.Tag = null;
                }
            }
            else     // Pointer is not over a node so clear the ToolTip.
            {
                this.toolTip.SetToolTip(this, "");
                toolTip.Tag = null;
            }
        }

        //protected void ToggleAllButtons()
        //{
        //    foreach (var node in this.Nodes)
        //    {
        //        if (node is DropDownTreeNode)
        //        {
        //            ToggleButtons(node as DropDownTreeNode);
        //        }
        //    }
        //}



    }
}
