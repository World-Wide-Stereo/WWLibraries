using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ww.Tables;
using ww.Utilities.Extensions;

namespace Controls
{
    public partial class uc_imageUploader : UserControl
    {
        public string filename = "";
        private string folderName = "";
        private FileInfo image;
        public event ImageEventHandler ImagesChanged;
        private readonly string root = (Global.TestMode ? @"\\server\test" : @"\\server") + @"\images\";


        public uc_imageUploader()
        {
            InitializeComponent();
            this.photo.Click += new System.EventHandler(this.photo_Click);
        }

        private void tryimage()
        {
            if (image != null && image.Exists)
            {
                try
                {
                    double factor = 1.0;
                    Bitmap bitmap = new Bitmap(image.FullName);
                    if (bitmap.Height > bitmap.Width) factor = bitmap.Height / (double)photo.Height;
                    else factor = bitmap.Width / (double)photo.Width;
                    Bitmap sizedimage = new Bitmap(bitmap, (int)(bitmap.Width / factor), (int)(bitmap.Height / factor));
                    bitmap.Dispose();
                    photo.Image = sizedimage;
                }
                catch (Exception ex)
                {
                    UserMessage.Info("Failed to open image with error: " + ex.Message);
                }
            }
            else { photo.LoadAsync(@"\\server\images\default.jpg"); }
        }

        private void photo_Click(object sender, EventArgs e)
        {
            if (folderName.Length > 0)
            {
                string file = FileDialogs.OpenFile(null, uc_image.FileDialogFilter);

                if (file.Length > 0)
                {
                    FileInfo sourceFileInfo = new FileInfo(file);
                    string filenameURLSafe = sourceFileInfo.Name.ToURLSafe();
                    string destinationPath = root + folderName.Replace("/", "-").Trim();
                    DirectoryInfo destinationDirInfo = new DirectoryInfo(destinationPath);
                    if (!destinationDirInfo.Exists) destinationDirInfo.Create();
                    sourceFileInfo.CopyTo(destinationPath + "\\" + filenameURLSafe, true);
                    ImagesChanged(this, new ImageEventArgs(filenameURLSafe));
                    image = new FileInfo(root + folderName + "\\" + filenameURLSafe);
                    tryimage();
                }
            }
        }

        public void Initialize(string folderName, string imageName)
        {
            //this needs to change, use part.primaryImagePath and remove root
            this.folderName = folderName;
            this.filename = imageName;
            image = new FileInfo(root + folderName + "\\" + imageName);
            tryimage();
        }

        private void llbDelete_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            image = null;
            ImagesChanged(this, new ImageEventArgs(""));
            tryimage();
        }
    }
}
