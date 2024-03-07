using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Controls
{
    public partial class frm_promptCopyable : Form
    {
        public frm_promptCopyable(string messageTop, string messageCopyable, string messageBottom = "", string title = "")
        {
            InitializeComponent();
            this.labelTop.Text = messageTop;
            this.textBox1.Text = messageCopyable;
            this.labelBottom.Text = messageBottom;
            if (title != null) this.Text = title;
            else this.Text = "Alert!";
        }
    }
}
