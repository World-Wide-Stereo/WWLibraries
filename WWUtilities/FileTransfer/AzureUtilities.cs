using System;
using System.IO;
using System.Web;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using KeePassLib.Security;
using ww.Utilities.Extensions;

namespace ww.Utilities
{
    public static class AzureUtilities
    {
        private static Lazy<ProtectedString> _storageConnectionString = new Lazy<ProtectedString>(() => KeePass.GetCredential("Azure", "Storage Connection String").Password);
        private static string StorageConnectionString => _storageConnectionString.Value.ReadString();

        public static void UploadImage(string remotePath, string localFile, string remoteName = null, FileInfo originalFileInfo = null)
        {
            UploadFile("images", remotePath, localFile, remoteName, originalFileInfo);
        }

        public static void UploadDocument(string remotePath, string localFile, string remoteName = null, FileInfo originalFileInfo = null)
        {
            UploadFile("documents", remotePath, localFile, remoteName, originalFileInfo);
        }

        private static void UploadFile(string blobContainerName, string remotePath, string localFile, string remoteName = null, FileInfo originalFileInfo = null)
        {
            var blobContainerClient = new BlobServiceClient(StorageConnectionString).GetBlobContainerClient(blobContainerName);
            BlobClient blobClient = blobContainerClient.GetBlobClient(remotePath + "/" + (remoteName.IsNullOrBlank() ? Path.GetFileName(localFile) : remoteName));
            if (!blobClient.Exists())
            {
                UploadFile(localFile, blobClient);
                return;
            }

            Response<BlobProperties> properties = blobClient.GetProperties();
            var localFileInfo = new FileInfo(localFile);
            if (properties.Value.ContentLength != localFileInfo.Length || properties.Value.LastModified < (originalFileInfo ?? localFileInfo).LastWriteTimeUtc)
            {
                UploadFile(localFile, blobClient);
                return;
            }
        }
        private static void UploadFile(string localFile, BlobClient blobClient)
        {
            byte[] buffer;
            using (FileStream fileStream = File.OpenRead(localFile))
            {
                buffer = new byte[fileStream.Length];
                using (var reader = new BinaryReader(fileStream))
                {
                    reader.Read(buffer, 0, buffer.Length);
                }
            }

            blobClient.Upload(new BinaryData(buffer), new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = MimeMapping.GetMimeMapping(localFile),
                }
            });
        }
    }
}
