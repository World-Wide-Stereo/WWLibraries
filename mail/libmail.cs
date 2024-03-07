using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

public class libmail
{
    private static Microsoft.Office.Interop.Outlook.Application app;
    private static Microsoft.Office.Interop.Outlook.NameSpace ns;

    public void startoutlook()
    {
        app = new Microsoft.Office.Interop.Outlook.Application();
        ns = app.GetNamespace("MAPI");
        ns.Logon("MS Exchange Settings", null, true, false);
    }

    public bool removeapt(DateTime startDate, string receipt, string folder)
    {
        foreach (Microsoft.Office.Interop.Outlook.AppointmentItem apt in GetAllPublicFolders()[folder].Items.Restrict($@"@SQL=""urn:schemas:httpmail:subject"" like '%{receipt}%''").OfType<Microsoft.Office.Interop.Outlook.AppointmentItem>())
        {
            if (apt.Start == startDate && apt.Subject.Contains(receipt))
            {
                apt.Delete();
                return true;
            }
        }
        return false;
    }

    public bool updateapt(DateTime olddate, DateTime newdate, string receipt, string folder, string subject, string body, int durationInMinutes = 30)
    {
        foreach (Microsoft.Office.Interop.Outlook.AppointmentItem apt in GetAllPublicFolders()[folder].Items.Restrict($@"@SQL=""urn:schemas:httpmail:subject"" like '%{receipt}%'").OfType<Microsoft.Office.Interop.Outlook.AppointmentItem>())
        {
            if (apt.Start == olddate && apt.Subject.Contains(receipt))
            {
                if (body.Length > 1) apt.Body = body;
                if (subject.Length > 1) apt.Subject = subject;
                apt.Start = newdate;
                apt.Duration = durationInMinutes;
                apt.Save();
                return true;
            }
        }
        return false;
    }

    public void appointment(DateTime sdate, DateTime edate, string subject, string body)
    {
        Microsoft.Office.Interop.Outlook.AppointmentItem apt = (Microsoft.Office.Interop.Outlook.AppointmentItem)app.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olAppointmentItem);
        apt.Start = sdate;
        apt.End = edate;
        apt.Subject = subject;
        apt.Body = body;
        apt.ReminderSet = true;
        apt.Display(null);
    }

    public void appointment(DateTime sdate, DateTime edate, string subject, string body, List<string> invite, string folder, bool open)
    {
        var apt = (Microsoft.Office.Interop.Outlook.AppointmentItem)app.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olAppointmentItem);
        apt.Start = sdate;
        apt.End = edate;
        apt.Subject = subject;
        apt.Body = body;
        apt.ReminderSet = true;

        invite.ForEach(x => apt.Recipients.Add(x));
        apt.Recipients.ResolveAll();

        Microsoft.Office.Interop.Outlook.Folders folders = GetAllPublicFolders();
        var myFolder = (Microsoft.Office.Interop.Outlook.Folder)folders[folder];
        try
        {
            apt.Save();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unable to save appointment.\n{ex}");
            throw;
        }
        try
        {
            apt.Move(myFolder);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unable to move appointment.\n{ex}");
            throw;
        }
        if (open) apt.Display(null);
    }

    public static Microsoft.Office.Interop.Outlook.Folders GetAllPublicFolders()
    {
        Microsoft.Office.Interop.Outlook.Folders fldrs = ns.Folders;
        Microsoft.Office.Interop.Outlook.MAPIFolder pub;
        try
        {
            pub = fldrs["Public Folders"];
        }
        catch // Outlook 2010 has the embedded user name
        {
            try
            {
                pub = fldrs[$"Public Folders - {Environment.UserName.ToLower()}"];
            }
            catch
            {
                pub = fldrs[$"Public Folders - {Environment.UserName.ToLower()}@example.com"];
            }
        }
        fldrs = pub.Folders;
        fldrs = fldrs["All Public Folders"].Folders;
        Microsoft.Office.Interop.Outlook.MAPIFolder allpub = fldrs["Example"];
        return allpub.Folders;
    }
}