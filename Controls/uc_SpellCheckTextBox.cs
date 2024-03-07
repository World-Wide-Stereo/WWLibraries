using NetSpell.SpellChecker;
using System;
using System.Windows.Forms;

namespace Controls
{
    public partial class uc_SpellCheckTextBox : UserControl
    {
        Spelling spellChecker = new Spelling();

        public uc_SpellCheckTextBox()
        {
            InitializeComponent();
        }

        public bool ReadOnly
        {
            get
            {
                return textBox.ReadOnly && tsbSpellCheck.Enabled;
            }
            set
            {
                textBox.ReadOnly = value;
                tsbSpellCheck.Enabled = !value;
            }
        }

        public TextBox TextBox => textBox;


        #region Spell Checker

        private void tsbSpellCheck_Click(object sender, EventArgs e)
        {
            spellChecker.Text = textBox.Text;
            spellChecker.ReplacedWord += SpellChecker_ReplacedWord;
            spellChecker.DeletedWord += SpellChecker_DeletedWord;
            spellChecker.SpellCheck();
        }
        private void SpellChecker_ReplacedWord(object sender, ReplaceWordEventArgs e)
        {
            SpellCheckerReplace(e.TextIndex, e.Word.Length, e.ReplacementWord);
        }

        private void SpellChecker_DeletedWord(object sender, SpellingEventArgs e)
        {
            SpellCheckerReplace(e.TextIndex, e.Word.Length, string.Empty);
        }

        private void SpellCheckerReplace(int textIndex, int wordLength, string replacementText)
        {
            int start = textBox.SelectionStart;
            int length = textBox.SelectionLength;

            textBox.Select(textIndex, wordLength);
            textBox.SelectedText = replacementText;

            if (start > textBox.Text.Length)
                start = textBox.Text.Length;

            if ((start + length) > textBox.Text.Length)
                length = 0;

            textBox.Select(start, length);
        }

        #endregion
    }
}
