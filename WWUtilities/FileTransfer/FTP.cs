using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentFTP;
using ww.Utilities.Extensions;

namespace ww.Utilities
{
    public class FTP : IDisposable
    {
        private readonly FtpClient Connection;

        #region Constructors
        public FTP(string server, string username, string password, int serverPort = 21)
        {
            this.Connection = new FtpClient(server, username, password, port: serverPort);
            this.Connection.LegacyLogger = (FtpTraceLevel ftpTraceLevel, string logMessage) => Logging.WriteLine(logMessage);
            this.Connection.AutoConnect();
        }
        #endregion

        #region GET
        public IEnumerable<string> ListFiles(string remotePath, bool getFilesOnly = true, bool getSubdirectoriesOnly = false, bool includeZeroByteFiles = true, Func<string, bool> whereClause = null)
        {
            IEnumerable<FtpListItem> list = this.Connection.GetListing(remotePath);
            if (getFilesOnly)
            {
                list = list.Where(x => x.Type == FtpObjectType.File);
            }
            if (getSubdirectoriesOnly)
            {
                list = list.Where(x => x.Type == FtpObjectType.Directory);
            }
            if (!includeZeroByteFiles)
            {
                list = list.Where(x => x.Size > 0);
            }

            IEnumerable<string> files = list.Select(x => x.Name);
            if (whereClause != null)
            {
                files = files.Where(whereClause);
            }
            return files;
        }

        public FileInfo Get(string remotePath, string remoteFileName, string localFilePath, string localFileName = "", bool overwriteExisting = true)
        {
            string localFileFullPath = Path.Combine(localFilePath, localFileName.IsNullOrBlank() ? remoteFileName : localFileName);
            this.Connection.DownloadFile(localFileFullPath, Path.Combine(remotePath, remoteFileName), existsMode: overwriteExisting ? FtpLocalExists.Overwrite : FtpLocalExists.Skip);
            return new FileInfo(localFileFullPath);
        }

        public IEnumerable<FileInfo> GetFiles(string remotePath, string localPath, IEnumerable<string> customFileList = null, bool overwriteExisting = true, bool includeZeroByteFiles = true, Func<string, bool> whereClause = null)
        {
            var fileInfos = new List<FileInfo>();
            foreach (string file in customFileList ?? ListFiles(remotePath, includeZeroByteFiles: includeZeroByteFiles, whereClause: whereClause))
            {
                string localFileFullPath = Path.Combine(localPath, file);
                if (overwriteExisting || !File.Exists(localFileFullPath))
                {
                    this.Connection.DownloadFile(localFileFullPath, Path.Combine(remotePath, file), existsMode: overwriteExisting ? FtpLocalExists.Overwrite : FtpLocalExists.Skip);
                    fileInfos.Add(new FileInfo(localFileFullPath));
                }
            }
            return fileInfos;
        }
        #endregion

        #region PUT
        public void Put(string remotePath, string localFile, string remoteName = null)
        {
            FtpStatus status = this.Connection.UploadFile(localFile, Path.Combine(remotePath, remoteName.IsNullOrBlank() ? Path.GetFileName(localFile) : remoteName), createRemoteDir: true);
            if (status != FtpStatus.Success)
            {
                throw new Exception($"Failed to upload file {localFile} with status {status}.");
            }
        }
        #endregion

        #region DELETE
        public void DeleteFile(string remotePath, string remoteName)
        {
            this.Connection.DeleteFile(Path.Combine(remotePath, remoteName));
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            this.Connection.Dispose();
        }
        #endregion
    }
}
