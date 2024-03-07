using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace ww.Utilities
{
    public class PurchaseOrderDocumentRepository
    {
        const string Root = @"\\server\PurchaseOrderDocuments\";
        DirectoryInfo DirectoryInfo;
        public string FullPath { get { return DirectoryInfo.FullName; } }
        public int PONumber { get; private set; }

        public PurchaseOrderDocumentRepository(int poNumber)
        {
            PONumber = poNumber;
            DirectoryInfo = new DirectoryInfo(Root + PONumber + @"\");
        }

        public bool AddFile(string fullFileName, out FileInfo returnFile)
        {
            FileInfo file = new FileInfo(fullFileName);
            try
            {
                DirectoryInfo.Create();
                returnFile = file.CopyTo(DirectoryInfo.FullName + file.Name, true);
            }
            catch (IOException ex)
            {
                returnFile = new FileInfo(DirectoryInfo.FullName + file.Name);
                return LogError(ex, "Error adding file to repository");
            }
            return true;
        }

        public bool DeleteFile(string fileName)
        {
            var file = new FileInfo(DirectoryInfo.FullName + fileName);
            if (!file.Exists) { return false; }
            try
            {
                file.Delete();
            }
            catch (IOException ex)
            {
                return LogError(ex, "Error deleting file from repository");
                
            }
            return true;
        }

        public bool ViewFile(string fileName)
        {
            var file = new FileInfo(DirectoryInfo.FullName + fileName);
            if (!file.Exists) { return false; }
            try
            {
                Process.Start(file.FullName);
            }
            catch (FileNotFoundException ex)
            {
                return LogError(ex, "Error viewing file from repository");
            }
            return true;
        }

        private bool LogError(Exception ex, string customError)
        {
            Logging.WriteLine(customError + Environment.NewLine + ex);
            return false;
        }
    }
}
