using System.Windows.Forms;
using ww.Tables;

namespace Controls
{
    public partial class frm_prompt : Form
    {
        public frm_prompt(string message, string fieldValue, bool maskInput = false, int maxLength = 0)
        {
            ConstructorShared(message, fieldValue, maskInput, maxLength);
        }
        public frm_prompt(string redMessage, string blackMessage, string fieldValue, bool maskInput = false, int maxLength = 0)
        {
            ConstructorShared(redMessage, fieldValue, maskInput, maxLength);
            tbAltMessage.Text = blackMessage;
        }
        private void ConstructorShared(string message, string fieldValue, bool maskInput, int maxLength)
        {
            InitializeComponent();
            this.Text = Global.ProgramName;
            this.tbMessage.Text = message;
            field.Text = fieldValue;
            field.UseSystemPasswordChar = maskInput;
            if (maxLength != 0)
            {
                field.MaxLength = maxLength;
            }
        }
    }
}
