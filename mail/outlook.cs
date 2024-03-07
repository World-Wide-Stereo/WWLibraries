using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ww.Tables;
using ww.Utilities.Extensions;

public class Outlook
{
    private static Microsoft.Office.Interop.Outlook.Application app;
    private static Microsoft.Office.Interop.Outlook.NameSpace ns;

    private static void startoutlook()
    {
        try
        {
            app = new Microsoft.Office.Interop.Outlook.Application();
            ns = app.GetNamespace("MAPI");
            ns.Logon("MS Exchange Settings", null, true, false);
        }
        catch
        {
            //if outlook fails to open, we will try later
        }
    }

    public static void OpenMessage(string to, string subject, IEnumerable<string> attachmentPaths)
    {
        if (Global.TestMode) { return; }

        startoutlook();
        Microsoft.Office.Interop.Outlook.MailItem message = (Microsoft.Office.Interop.Outlook.MailItem)app.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);

        message.To = to;
        message.Subject = subject;

        foreach (string attachmentPath in attachmentPaths.Where(x => !x.IsNullOrBlank() && new FileInfo(x).Exists))
        {
            message.Attachments.Add(attachmentPath, Type.Missing, Type.Missing, Type.Missing);
        }

        message.Display(null);
    }

    public static void OpenMessage(string to, string subject, string body, string attachmentPath, string format, string sentOnBehalfOfName, string bcc = "")
    {
        if (Global.TestMode) { return; }

        startoutlook();
        Microsoft.Office.Interop.Outlook.MailItem message = (Microsoft.Office.Interop.Outlook.MailItem)app.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);

        message.To = to;
        message.Subject = subject;

        if (format == "HTML")
        {
            message.BodyFormat = Microsoft.Office.Interop.Outlook.OlBodyFormat.olFormatHTML;
            message.HTMLBody = body;
        }
        else
        {
            message.Body = body + message.Body;
        }

        if (!attachmentPath.IsNullOrBlank() && new FileInfo(attachmentPath).Exists)
        {
            message.Attachments.Add(attachmentPath, Type.Missing, Type.Missing, Type.Missing);
        }

        if (!sentOnBehalfOfName.IsNullOrBlank())
        {
            message.SentOnBehalfOfName = sentOnBehalfOfName;
        }

        if (!bcc.IsNullOrBlank())
        {
            message.BCC = bcc;
        }

        message.Display(null);
    }
}