using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using ww.Utilities.Extensions;

namespace ww.Utilities
{
    public class SFTP : IDisposable
    {
        private readonly SftpClient Connection;

        public enum AuthenticationMethod
        {
            PrivateAuthentication,
            KeyboardInteractive,
        }

        #region Constructors
        public SFTP(string server, string username, string password, int? serverPort = null, int connNumTries = 3, int connMilisecBtwTries = 0, MemoryStream privateKeyMemoryStream = null, AuthenticationMethod authenticationMethod = AuthenticationMethod.PrivateAuthentication)
        {
            if (serverPort == null && privateKeyMemoryStream == null)
            {
                if (authenticationMethod == AuthenticationMethod.KeyboardInteractive)
                {
                    var keybAuth = new KeyboardInteractiveAuthenticationMethod(username);
                    keybAuth.AuthenticationPrompt += (object sender, AuthenticationPromptEventArgs e) =>
                    {
                        foreach (AuthenticationPrompt prompt in e.Prompts)
                        {
                            if (prompt.Request.IndexOf("Password:", StringComparison.InvariantCultureIgnoreCase) != -1)
                            {
                                prompt.Response = password;
                            }
                        }
                    };
                    this.Connection = new SftpClient(new ConnectionInfo(server, username, keybAuth));
                }
                else
                {
                    this.Connection = new SftpClient(server, username, password);
                }
            }
            else if (serverPort == null)
            {
                using (var privateKeyFile = new PrivateKeyFile(privateKeyMemoryStream, password))
                {
                    this.Connection = new SftpClient(new ConnectionInfo(server, username, new PasswordAuthenticationMethod(username, password), new PrivateKeyAuthenticationMethod(username, privateKeyFile)));
                }
            }
            else if (privateKeyMemoryStream == null)
            {
                this.Connection = new SftpClient(server, (int)serverPort, username, password);
            }
            else
            {
                using (var privateKeyFile = new PrivateKeyFile(privateKeyMemoryStream, password))
                {
                    this.Connection = new SftpClient(new ConnectionInfo(server, (int)serverPort, username, new PasswordAuthenticationMethod(username, password), new PrivateKeyAuthenticationMethod(username, privateKeyFile)));
                }
            }

            int connTryCounter = 1;
            do
            {
                try
                {
                    this.Connection.Connect();
                    return;
                }
                catch
                {
                    if (connTryCounter >= connNumTries)
                    {
                        throw;
                    }
                    connTryCounter++;
                    Thread.Sleep(connMilisecBtwTries);
                }
            } while (true); // Returns on successful connect.
        }
        #endregion

        #region GET
        public IEnumerable<string> ListFiles(string remotePath, bool getFilesOnly = true, bool getSubdirectoriesOnly = false, bool includeZeroByteFiles = true, Func<string, bool> whereClause = null)
        {
            IEnumerable<SftpFile> list = this.Connection.ListDirectory(remotePath.Replace('\\', '/'));
            if (getFilesOnly)
            {
                list = list.Where(x => !x.IsDirectory);
            }
            if (getSubdirectoriesOnly)
            {
                list = list.Where(x => x.IsDirectory);
            }
            if (!includeZeroByteFiles)
            {
                list = list.Where(x => x.Length > 0);
            }

            IEnumerable<string> files = list.Select(x => x.Name).Where(x => !x.EqualsAnyOf(".", ".."));
            if (whereClause != null)
            {
                files = files.Where(whereClause);
            }
            return files;
        }

        public FileInfo Get(string remotePath, string remoteFileName, string localFilePath, string localFileName = "", bool overwriteExisting = true)
        {
            string localFileFullPath = Path.Combine(localFilePath, localFileName.IsNullOrBlank() ? remoteFileName : localFileName);
            if (overwriteExisting || !File.Exists(localFileFullPath))
            {
                using (FileStream destinationFile = File.OpenWrite(localFileFullPath))
                {
                    this.Connection.DownloadFile(Path.Combine(remotePath, remoteFileName).Replace('\\', '/'), destinationFile);
                }
            }
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
                    using (FileStream destinationFile = File.OpenWrite(localFileFullPath))
                    {
                        this.Connection.DownloadFile(Path.Combine(remotePath, file).Replace('\\', '/'), destinationFile);
                    }
                    fileInfos.Add(new FileInfo(localFileFullPath));
                }
            }
            return fileInfos;
        }
        #endregion

        #region PUT
        public void Put(string remotePath, string localFile, string remoteName = null)
        {
            using (FileStream file = File.OpenRead(localFile))
            {
                this.Connection.UploadFile(file, Path.Combine(remotePath, remoteName.IsNullOrBlank() ? Path.GetFileName(localFile) : remoteName).Replace('\\', '/'));
            }
        }

        public void PutAvoidDuplicate(string remotePath, string localFile, FileInfo originalFileInfo, string remoteName = null)
        {
            string uploadPath = Path.Combine(remotePath, remoteName.IsNullOrBlank() ? Path.GetFileName(localFile) : remoteName).Replace('\\', '/');
            if (!this.Connection.Exists(uploadPath))
            {
                this.Put(remotePath, localFile, remoteName);
            }
            else
            {
                SftpFileAttributes sftpFileAttributes = this.Connection.GetAttributes(uploadPath);
                if (sftpFileAttributes.Size != new FileInfo(localFile).Length || sftpFileAttributes.LastWriteTimeUtc < originalFileInfo.LastWriteTimeUtc)
                {
                    this.Put(remotePath, localFile, remoteName);
                }
            }
        }
        #endregion

        #region DELETE
        public void DeleteFile(string remotePath, string remoteName)
        {
            this.Connection.DeleteFile(Path.Combine(remotePath, remoteName).Replace('\\', '/'));
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
