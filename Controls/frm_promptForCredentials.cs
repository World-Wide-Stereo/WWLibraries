using System.Windows.Forms;
using ww.Tables;

namespace Controls
{
    public partial class frm_promptForCredentials : Form
    {
        public string Username { get { return tbUsername.Text; } }
        public string Password { get { return tbPassword.Text; } }


        public frm_promptForCredentials(string message, string username = "", string password = "")
        {
            ConstructorShared(message, username, password);
        }
        public frm_promptForCredentials(string redMessage, string blackMessage, string username = "", string password = "")
        {
            ConstructorShared(redMessage, username, password);
            tbAltMessage.Text = blackMessage;
        }
        private void ConstructorShared(string message, string username, string password)
        {
            InitializeComponent();
            this.Text = Global.ProgramName;
            this.tbMessage.Text = message;
            tbUsername.Text = username;
            tbPassword.Text = password;
        }
    }
}
