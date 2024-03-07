using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ww.Utilities.Extensions
{
    public class FileExtensions
    {
        public static void MoveTo(FileInfo file, string destFullFileName, bool overwrite)
        {
            if (file == null) { throw new ArgumentNullException("file is null or does not exist"); }
            if (destFullFileName.IsNullOrBlank()) { throw new ArgumentNullException("Destination file name is invalid"); }

            try
            {
            if (File.Exists(destFullFileName) && overwrite == false) { return; }
            

                if (overwrite)
                {
                    File.Delete(destFullFileName);
                    file.MoveTo(destFullFileName);
                }
                else
                {
                    file.MoveTo(destFullFileName);
                }
            }
            catch (IOException)
            {
                throw;
            }
        }
    }
}
