using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Controls;
using ww.Tables;
using ww.Utilities.Extensions;

public static class UserMessage
{
    private static string GetName(string title = null)
    {
        return String.IsNullOrWhiteSpace(title) ? Global.ProgramName : Global.ProgramName + " - " + title;
    }

    public static void Info(string message, string title = null)
    {
        MessageBox.Show(message, GetName(title));
    }

    public static void InfoBox(string message, string title = null)
    {
        var form = new frm_InfoBox(message, title);
        form.ShowDialog();
        form.Dispose();
    }
    public static void InfoBox(ListViewItem messageList, string title = null)
    {
        var message = new StringBuilder();
        foreach (ListViewItem.ListViewSubItem item in messageList.SubItems)
        {
            message.AppendFormat("{0}: {1}\r\n", item.GetType().Name, item.Text);
        }
        InfoBox(message.ToString(), title);
    }

    public static bool YesNo(string message)
    {
        return MessageBox.Show(message, GetName(), MessageBoxButtons.YesNo) == DialogResult.Yes;
    }
    public static bool YesNo(string message, string title)
    {
        return MessageBox.Show(message, GetName(title), MessageBoxButtons.YesNo) == DialogResult.Yes;
    }
    public static bool YesNo(string message, string title, int useForDefaultNo)
    {
        return MessageBox.Show(message, GetName(title), MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
    }

    public static DialogResult YesNoCancel(string message)
    {
        return MessageBox.Show(message, GetName(), MessageBoxButtons.YesNoCancel);
    }

    public static bool OkCancel(string message, string title = null)
    {
        return MessageBox.Show(message, GetName(title), MessageBoxButtons.OKCancel) == DialogResult.OK;
    }

    public static string Prompt(string message, string defaultValue = "", bool forceResponse = false, bool allowCancel = true, bool maskInput = false, int maxLength = 0)
    {
        var prompt = new frm_prompt(message, defaultValue, maskInput, maxLength);
        DialogResult result = prompt.ShowDialog();
        if (result == DialogResult.Cancel && allowCancel) return null;
        while (forceResponse && prompt.field.Text == "")
        {
            result = prompt.ShowDialog();
            if (result == DialogResult.Cancel && allowCancel) return null;
        }
        string returnVal = prompt.field.Text;
        prompt.Dispose();
        return returnVal;
    }
    public static string Prompt(string redMessage, string blackMessage, string defaultValue, bool forceResponse = false, bool allowCancel = true, bool maskInput = false, int fontSize = 0, int formHeight = 0, int formWidth = 0, string makeRedMessageThisColor = "", string title = "", int maxLength = 0)
    {
        string fieldValue;
        var prompt = new frm_prompt(redMessage, blackMessage, defaultValue, maskInput, maxLength);
        PromptCommonCode(prompt, out _, out fieldValue, forceResponse, allowCancel, fontSize, formHeight, formWidth, makeRedMessageThisColor, title);
        prompt.Dispose();
        return fieldValue;
    }

    public static void PromptForCredentials(string redMessage, string blackMessage, out string username, out string password, bool forceResponse = false, bool allowCancel = true, int fontSize = 0, int formHeight = 0, int formWidth = 0, string makeRedMessageThisColor = "", string title = "")
    {
        var prompt = new frm_promptForCredentials(redMessage, blackMessage, "", "");
        PromptCommonCode(prompt, out username, out password, forceResponse, allowCancel, fontSize, formHeight, formWidth, makeRedMessageThisColor, title);
        prompt.Dispose();
    }

    private static void PromptCommonCode(dynamic prompt, out string username, out string fieldValue, bool forceResponse, bool allowCancel, int fontSize, int formHeight, int formWidth, string makeRedMessageThisColor, string title)
    {
        username = null;
        fieldValue = null;

        if (title != "") prompt.Text = title;

        // Change the font size.
        if (fontSize > 0)
        {
            prompt.tbMessage.Font = new System.Drawing.Font(prompt.tbMessage.Font.FontFamily, fontSize);
        }

        // Resize the form.
        if (formHeight > 0 && formWidth > 0)
        {
            prompt.Size = new System.Drawing.Size(formWidth, formHeight);
        }
        else if (formWidth > 0)
        {
            prompt.Size = new System.Drawing.Size(formWidth, prompt.Size.Height);
        }
        else if (formHeight > 0)
        {
            prompt.Size = new System.Drawing.Size(prompt.Size.Width, formHeight);
        }

        // Change the red/main message font color to inputed color.
        if (makeRedMessageThisColor != "")
        {
            System.Drawing.Color col = System.Drawing.Color.FromName(makeRedMessageThisColor);
            if (col.IsKnownColor)
            {
                prompt.tbMessage.ForeColor = col;
            }
        }

        var result = prompt.ShowDialog();
        if (result == DialogResult.Cancel && allowCancel) return;

        var credentialsPrompt = prompt as frm_promptForCredentials;
        if (credentialsPrompt != null) username = credentialsPrompt.Username;
        fieldValue = credentialsPrompt == null ? prompt.field.Text : credentialsPrompt.Password;
        while (forceResponse && fieldValue == "")
        {
            result = prompt.ShowDialog();
            if (result == DialogResult.Cancel && allowCancel) return;
            fieldValue = credentialsPrompt == null ? prompt.field.Text : credentialsPrompt.Password;
        }
    }

    public static bool CopyablePrompt(string messageTop, string messageCopyable, string messageBottom = "", string title = "")
    {
        var prompt = new frm_promptCopyable(messageTop, messageCopyable, messageBottom, title);
        DialogResult result = prompt.ShowDialog();
        prompt.Dispose();
        return result == DialogResult.OK;
    }

    public static T ComboPrompt<T>(string message, IDictionary<T, string> options, ComboBoxStyle dropDownStyle = ComboBoxStyle.DropDown, string defaultValueText = null)
    {
        using (var prompt = new frm_promptCombo(message, options.ToDictionary(x => (object)x.Key, y => y.Value).ToList(), dropDownStyle: dropDownStyle, defaultValueText: defaultValueText))
        {
            DialogResult result = prompt.ShowDialog();
            return result == DialogResult.Cancel ? default(T) : prompt.Value == null ? default(T) : (T)prompt.Value;
        }
    }

    public static DialogResult ButtonPrompt(string message, IDictionary<string, DialogResult> buttons, string title = null, bool showTextBox = false)
    {
        return ButtonPrompt(message, buttons, out _, title: title, showTextBox: showTextBox);
    }
    public static DialogResult ButtonPrompt(string message, IDictionary<string, DialogResult> buttons, out string userTextInput, string title = null, bool showTextBox = false)
    {
        using (var prompt = new frm_promptButton(message, buttons, showTextBox: showTextBox))
        {
            if (title != null)
            {
                prompt.Text = title;
            }
            DialogResult result = prompt.ShowDialog();
            userTextInput = prompt.textInput.Text;
            return result;
        }
    }

    public static string RadioButtonPrompt(string message, IEnumerable<string> buttonLabels, bool hideFirstOption = false) //, bool forceResponse = false, bool allowCancel = true)
    {
        var prompt = new frm_promptRadio(message, buttonLabels.ToDictionary(x => (object)x, x => x), hideFirstOption: hideFirstOption);
        prompt.ShowDialog();
        string returnVal = prompt.DialogResult == DialogResult.OK ? prompt.Value.ToString() : "";
        prompt.Dispose();
        return returnVal;
    }
    public static T RadioButtonPrompt<T>(string message, IDictionary<T, string> buttons, bool hideDefault = false, bool preventCancel = true, int promptWidth = 0) //, bool forceResponse = false, bool allowCancel = true)
    {
        var prompt = new frm_promptRadio(message, buttons.ToDictionary(x => (object)x.Key, y => y.Value), hideFirstOption: hideDefault, preventCancel: preventCancel);
        if (promptWidth != 0)
        {
            prompt.Width = promptWidth;
        }
        prompt.ShowDialog();
        var returnVal = (T)prompt.Value;
        prompt.Dispose();
        return returnVal;
    }
    public static T? RadioButtonPromptNullable<T>(string message, IDictionary<T, string> buttons, bool hideDefault = false, bool preventCancel = true, int promptWidth = 0) where T : struct //, bool forceResponse = false, bool allowCancel = true)
    {
        var prompt = new frm_promptRadio(message, buttons.ToDictionary(x => (object)x.Key, y => y.Value), hideFirstOption: hideDefault, preventCancel: preventCancel);
        if (promptWidth != 0)
        {
            prompt.Width = promptWidth;
        }
        prompt.ShowDialog();
        var returnVal = (T?)prompt.Value;
        prompt.Dispose();
        return returnVal;
    }

    public static DialogResult ScrollBarPrompt(string messageTop, string messageScrollable, string messageBottom, Dictionary<string, DialogResult> buttons, string title = null, int width = 0, int height = 0, BorderStyle borderStyle = BorderStyle.None, bool isDialog = true)
    {
        using (var prompt = new frm_promptScrollable(messageTop, messageScrollable, buttons, bottomMessage: messageBottom, borderStyle: borderStyle, isDialog: isDialog))
        {
            if (title != null) prompt.Text = title;
            if (width != 0) prompt.Width = width;
            if (height != 0) prompt.Height = height;
            return prompt.ShowDialog();
        }
    }

    public static List<ListViewItem> ListViewPrompt(string title, string message, Dictionary<string, int> columnHeaders, List<ListViewItem> options, bool checkedByDefault = false)
    {
        using (var prompt = new frm_promptListView(title, message, columnHeaders, options, checkedByDefault))
        {
            DialogResult result = prompt.ShowDialog();
            if (result == DialogResult.OK && prompt.Value != null)
            {
                return new List<ListViewItem>(((ListView.CheckedListViewItemCollection)prompt.Value).Cast<ListViewItem>());
            }
            return null;
        }
    }

    public static List<DataGridViewRow> DataGridViewPrompt(string title, string message, Dictionary<string, int> columnHeaders, IEnumerable<IEnumerable<string>> rowValues)
    {
        using (var prompt = new frm_promptDataGridView(title, message, columnHeaders, rowValues))
        {
            prompt.ShowDialog();
            return prompt.DialogResult == DialogResult.OK ? prompt.RowCollection.Cast<DataGridViewRow>().ToList() : new List<DataGridViewRow>();
        }
    }

    public static DateTime? DatePrompt(string title, string message, string fieldName, DateTime? defaultDate = null)
    {
        using (var prompt = new frm_promptDate(title, message, fieldName, defaultDate))
        {
            prompt.ShowDialog();
            return prompt.DialogResult == DialogResult.OK ? prompt.dtpDate.Value : (DateTime?)null;
        }
    }

    public static string NoteWithHistoryPrompt(string title, string history)
    {
        using (var prompt = new frm_promptNoteWithHistory(title, history))
        {
            return prompt.ShowDialog() == DialogResult.Cancel ? null : prompt.NoteText;
        }
    }
}
