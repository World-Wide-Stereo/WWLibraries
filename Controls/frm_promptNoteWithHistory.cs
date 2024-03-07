using System.Windows.Forms;
using ww.Utilities.Extensions;

namespace Controls
{
    public partial class frm_promptNoteWithHistory : Form
    {
        public frm_promptNoteWithHistory()
        {
            InitializeComponent();
        }

        public frm_promptNoteWithHistory(string title, string history)
        {
            InitializeComponent();
            if (!title.IsNullOrBlank())
            {
                this.Text = title;
            }
            this.txtHistory.Text = history;
        }
        public string NoteText => txtNote.Text;
    }
}
