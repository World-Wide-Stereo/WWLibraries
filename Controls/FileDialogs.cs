using System;
using System.IO;
using System.Windows.Forms;
using ww.Utilities.Extensions;

namespace Controls
{
    public class FileDialogs
    {
        public static string OpenFile(string startdir, string filter, string title = "")
        {
            var open = new OpenFileDialog();
            if (startdir != null) open.InitialDirectory = startdir;
            if (filter.Length > 0) open.Filter = filter;
            if (title.Length > 0) open.Title = title;

            if (open.ShowDialog() == DialogResult.OK)
            {
                string filename = open.FileName;
                open.Dispose();
                return filename;
            }
            open.Dispose();
            return "";
        }
        public static string[] OpenFiles(string startdir, string filter, string title = "")
        {
            var open = new OpenFileDialog();
            if (startdir != null) open.InitialDirectory = startdir;
            if (filter.Length > 0) open.Filter = filter;
            if (title.Length > 0) open.Title = title;
            open.Multiselect = true;

            if (open.ShowDialog() == DialogResult.OK)
            {
                string[] filenames = open.FileNames;
                open.Dispose();
                return filenames;
            }
            open.Dispose();
            return new[] { "" };
        }

        public static string SaveFile(string startdir, string filter, string title)
        {
            var save = new SaveFileDialog();
            if (title.Length > 0) save.Title = title;
            if (startdir != null) save.InitialDirectory = startdir;
            if (filter.Length > 0) save.Filter = filter;

            if (save.ShowDialog() == DialogResult.OK)
            {
                string filename = save.FileName;
                save.Dispose();
                return filename;
            }
            save.Dispose();
            return "";
        }
        public static string SaveFile(string startdir, string filter, string title, string filename, string extension)
        {
            var save = new SaveFileDialog();
            if (title.Length > 0) save.Title = title;
            if (startdir != null) save.InitialDirectory = startdir;
            if (filter.Length > 0) save.Filter = filter;
            if (filename.Length > 0) save.FileName = filename;
            if (extension.Length > 0)
            {
                save.DefaultExt = extension;
                save.AddExtension = true;
            }

            if (save.ShowDialog() == DialogResult.OK)
            {
                filename = save.FileName;
                save.Dispose();
                return filename;
            }
            save.Dispose();
            return "";
        }
        public static string SaveFile(string filename, string extension)
        {
            return SaveFile("", "", "", filename, extension);
        }

        public static void SaveToFlatFile(Func<string> getFlatFileString, string fileExtension = ".csv", bool openFile = false, string dialogWindowTitle = "Export Current Table")
        {
            string file = SaveFile("", (fileExtension == ".csv" ? "Comma Separated Value" : "Flat File") + "|*" + fileExtension, dialogWindowTitle, "*" + fileExtension, "*" + fileExtension);
            if (file.Length > 0)
            {
                try
                {
                    string exportData = getFlatFileString();
                    if (exportData.Length > 0)
                    {
                        using (var sw = new StreamWriter(file))
                        {
                            sw.Write(exportData);
                        }
                    }

                    if (openFile) System.Diagnostics.Process.Start(file);
                }
                catch (Exception ex)
                {
                    UserMessage.Info("Failed to export.\n\n" + ex.Message);
                }
            }
        }

        public static string OpenFolder(string title)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog() { ShowNewFolderButton = false };
            if (!title.IsNullOrBlank()) folder.Description = title;
            return folder.ShowDialog() == DialogResult.OK ? folder.SelectedPath : "";
        }
    }
}
