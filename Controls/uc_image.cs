using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Controls.Extensions;
using ww.Tables;
using ww.Utilities;
using ww.Utilities.Extensions;

namespace Controls
{
    public partial class uc_image : UserControl
    {
        private bool readonlyMode;
        private bool editMode;
        private string[] folders;
        public string primaryImageName { get; private set; }
        private string imagesForItem;
        private List<FileInfo> images = new List<FileInfo>();
        private List<string> filesPendingSave = new List<string>();
        private List<string> filesPendingDeletion = new List<string>();
        private int imgindex = -1;
        public event ImageEventHandler ImageAdded;
        public event ImageEventHandler ImageDeleted;
        public event ImageEventHandler ImagesChanged;
        public event ImageEventHandler ImagesScrolled;
        private readonly string root = (Global.TestMode ? @"\\server\test" : @"\\server") + @"\images\";
        public const string FileDialogFilter = "All Images|*.jpg;*.gif;*.tif;*.png;*.bmp;*.jpeg;*.jpe;*.jfif|JPEG|*.jpg;*.jpeg;*.jpe;*.jfif|GIF|*.gif|TIFF|*.tif;*.tiff|PNG|*.png|BMP|*.bmp";
        private const int MinimumImageSize = 800;

        public delegate void UserControlEditHandler(object sender, ImageControlEventArgs e);
        public event UserControlEditHandler InnerEdit;
        public delegate void UserControlCancelHandler(object sender, EventArgs e);
        public event UserControlCancelHandler InnerCancel;
        public delegate DataRequirementException UserControlGetDataRequirementExceptionHandler(object sender, ImageControlEventArgs e);
        public event UserControlGetDataRequirementExceptionHandler InnerGetDataRequirementException;
        public delegate void UserControlUpdateHandler(object sender, ImageControlEventArgs e);
        public event UserControlUpdateHandler InnerSave;
        public delegate bool OkToDeletePrimaryImageHandler(object sender, ImageControlEventArgs e);
        public event OkToDeletePrimaryImageHandler OkToDeletePrimaryImage;

        public int Count { get { return images == null ? 0 : images.Count; } }
        public string CurrentImage { get { return images != null && images.Count > 0 && imgindex != -1 && images[imgindex].Exists ? images[imgindex].Name : ""; } }
        public bool BlockSmallImages { get; set; }
        public bool UseEditButton { get; set; }
        public bool EnableOrdering { get; set; }
        public bool OrderChanged { get; set; }
        public char[] InvalidFilenameCharacters { get; set; }

        public string FolderPath
        {
            get { return folders.Length == 0 ? "" : UseEditButton ? TempDirectory : root + folders[0]; }
        }

        private List<string> filenamesInOrder;
        public ReadOnlyCollection<string> FilenamesInOrder
        {
            get { return filenamesInOrder.AsReadOnly(); }
        }

        private bool getNewTempDirectory;
        private string _tempDirectory;
        private string TempDirectory
        {
            get
            {
                if (_tempDirectory == null || getNewTempDirectory)
                {
                    _tempDirectory = UtilityFunctions.GetTempDirectory(createDirectory: false);
                }
                return _tempDirectory;
            }
        }


        #region Init
        public uc_image()
        {
            InitializeComponent();
        }

        private void uc_image_Load(object sender, EventArgs e)
        {
            btnCancel.Visible = UseEditButton;
            btnEdit.Visible = UseEditButton;
            btnSave.Visible = UseEditButton;

            // Resize Buttons
            if (!UseEditButton)
            {
                const int btnWidth = 35;
                const int btnLocHeight = 140;
                btnPrev.Width = btnWidth;
                btnDelete.Width = btnWidth;
                btnSaveAs.Width = btnWidth;
                btnNext.Width = btnWidth;
                btnPrev.Location = new Point(3, btnLocHeight);
                btnDelete.Location = new Point(39, btnLocHeight);
                btnSaveAs.Location = new Point(76, btnLocHeight);
                btnNext.Location = new Point(112, btnLocHeight);
            }

            // Resize Button Images
            // The Button.BackgroundImage property was originally used, which didn't require this, but it does not grey out images when the button is disabled.
            foreach (var btn in this.Controls.OfType<Button>().Where(x => x.Image != null))
            {
                btn.Image = new Bitmap(btn.Image, new Size(btn.Width - 5, btn.Height - 5));
            }
        }

        public void Initialize(string[] folders, string primaryImageName, List<string> filenamesInOrder = null, bool enable = true, bool readonlyMode = false, string imagesForItem = null)
        {
            this.readonlyMode = readonlyMode;
            this.folders = folders;
            this.primaryImageName = primaryImageName;
            this.filenamesInOrder = filenamesInOrder;
            this.imagesForItem = imagesForItem;
            this.OrderChanged = false;
            ReloadImages(startingImageFilename: primaryImageName);

            if (enable)
            {
                if (!UseEditButton && !readonlyMode)
                {
                    pbPhoto.Enabled = true;
                    btnDelete.Enabled = true;
                }
                btnPrev.Enabled = true;
                btnEdit.Enabled = !readonlyMode;
                btnSaveAs.Enabled = !readonlyMode;
                btnNext.Enabled = true;
            }
            else
            {
                EnableAllControls(false, changeVisible: false);
            }
        }
        #endregion

        #region Enable/Disable Controls
        private void EnableAllControls(bool enable, bool changeVisible = true)
        {
            btnPrev.Enabled = enable;
            EnableEditing(enable, changeVisible);
            btnSaveAs.Enabled = !readonlyMode && enable;
            btnNext.Enabled = enable;
        }
        public void EnableEditing(bool enable, bool changeVisible = true)
        {
            enable = !readonlyMode && enable;

            if (enable && EnableOrdering)
            {
                llView.Text = "Order";
                llView.Size = new Size(33, llView.Height);
                llView.Location = new Point(104, llView.Location.Y);
            }
            else
            {
                llView.Text = "View";
                llView.Size = new Size(30, llView.Height);
                llView.Location = new Point(107, llView.Location.Y);
            }

            pbPhoto.Enabled = enable;
            pasteMenuItem.Enabled = enable;
            btnDelete.Enabled = enable;
            btnCancel.Enabled = enable;
            btnEdit.Enabled = enable;
            btnSave.Enabled = enable;
            if (changeVisible)
            {
                btnEdit.Visible = !enable;
                btnSave.Visible = enable;
            }
        }

        public void ProposalEditorMode()
        {
            btnDelete.Visible = false;
            btnSaveAs.Visible = false;
        }
        #endregion

        #region Close/Cancel/Save Helpers
        protected override void OnHandleDestroyed(EventArgs e)
        {
            pbPhoto.Image?.Dispose();
            // In case the user control is closed while in Edit mode.
            if (editMode) UtilityFunctions.DeleteTempDirectory(TempDirectory, ref getNewTempDirectory);
            base.OnHandleDestroyed(e);
        }
        #endregion

        #region Click Events
        private void llView_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            bool viewOnly = !editMode || !EnableOrdering;
            var form = new frm_imageOrder(imagesForItem, images, this, viewOnly: viewOnly);

            try
            {
                if (viewOnly)
                {
                    this.LaunchScreen(form);
                }
                else
                {
                    form.OkToDeletePrimaryImage += OkToDeletePrimaryImage;
                    form.ShowDialog();
                }

                if (form.DialogResult == DialogResult.OK)
                {
                    foreach (FileInfo fileInfo in form.AddedImages)
                    {
                        AddImage(fileInfo.FullName);
                    }

                    foreach (FileInfo fileInfo in form.DeletedImages)
                    {
                        if (imgindex != -1 && images[imgindex].Name.EqualsIgnoreCase(fileInfo.Name))
                        {
                            pbPhoto.Image?.Dispose();
                            pbPhoto.Image = null;
                            pbPhoto.Refresh();
                            imgindex--;
                            if (imgindex < 0)
                            {
                                imgindex = 0;
                            }
                        }
                        DeleteImage(fileInfo.FullName);
                    }

                    if (form.OrderChanged)
                    {
                        OrderChanged = true;
                        filenamesInOrder = form.OrderedImages.Select(x => x.Name).ToList();
                        primaryImageName = filenamesInOrder.Count == 0 ? "" : filenamesInOrder[0];
                    }

                    ReloadImages(startingImageFilename: imgindex == -1 ? null : images[imgindex].Name);
                }
            }
            catch
            {
                if (!viewOnly)
                {
                    form.Dispose();
                }
                throw;
            }

            if (!viewOnly)
            {
                form.Dispose();
            }
            this.Cursor = Cursors.Default;
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (folders.Length > 0 && folders[0].Length > 0)
            {
                imgindex--;
                if (imgindex < -1) imgindex = images.Count - 1;
                if (imgindex != -1 && ImagesScrolled != null) ImagesScrolled(this, new ImageEventArgs(images[imgindex].Name));
                TryImage();
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (folders.Length > 0 && folders[0].Length > 0)
            {
                imgindex++;
                if (imgindex == images.Count) imgindex = -1;
                if (imgindex != -1 && ImagesScrolled != null) ImagesScrolled(this, new ImageEventArgs(images[imgindex].Name));
                TryImage();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            editMode = true;
            filesPendingSave.Clear();
            filesPendingDeletion.Clear();
            string origImageFilename = imgindex == -1 ? "" : images[imgindex].Name;
            if (InnerEdit != null)
            {
                var imageControlEventArgs = new ImageControlEventArgs();
                InnerEdit(this, imageControlEventArgs);
                if (!imageControlEventArgs.RecordLockedSuccessfully)
                {
                    this.Cursor = Cursors.Default;
                    return;
                }
            }
            ShowSelectedImageAfterReload(origImageFilename);
            this.EnableEditing(true);
            this.Cursor = Cursors.Default;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            editMode = false;
            UtilityFunctions.DeleteTempDirectory(TempDirectory, ref getNewTempDirectory);
            filesPendingSave.Clear();
            filesPendingDeletion.Clear();
            this.EnableEditing(false);
            string origImageFilename = imgindex == -1 ? "" : images[imgindex].Name;
            if (InnerCancel != null) InnerCancel(this, e);
            ShowSelectedImageAfterReload(origImageFilename);
            this.Cursor = Cursors.Default;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            if (InnerGetDataRequirementException != null)
            {
                DataRequirementException dataReqEx = InnerGetDataRequirementException(this, new ImageControlEventArgs { FilesSaved = filesPendingSave.ToList(), FilesDeleted = filesPendingDeletion.ToList() });
                if (dataReqEx != null)
                {
                    this.Cursor = Cursors.Default;
                    UserMessage.Info(dataReqEx.Message);
                    return;
                }
            }

            editMode = false;
            this.EnableEditing(false);


            List<string> origFilesPendingSave = filesPendingSave.ToList();
            foreach (string file in origFilesPendingSave)
            {
                var fi = new FileInfo(file);
                string filename = Path.GetFileName(file);
                string path = root + folders[0];
                Directory.CreateDirectory(path); // Not necessary to check if the folder already exists. That check is built-in.
                string destinationFile = path + fi.Name;
                if (File.Exists(destinationFile))
                {
                    File.SetAttributes(destinationFile, FileAttributes.Normal);
                }
                fi.CopyTo(destinationFile, true);
                File.SetAttributes(destinationFile, FileAttributes.Normal);
                images[images.FindIndex(x => x.Name.EqualsIgnoreCase(filename))] = new FileInfo(destinationFile);
                File.SetAttributes(file, FileAttributes.Normal);
                fi.Delete();
                filesPendingSave.Remove(file);
            }

            List<string> origFilesPendingDeletion = filesPendingDeletion.ToList();
            foreach (string file in origFilesPendingDeletion)
            {
                // Do not delete files that were deleted and then reuploaded.
                string filename = Path.GetFileName(file);
                if (!origFilesPendingSave.Any(x => x.EndsWith(filename)))
                {
                    try
                    {
                        File.SetAttributes(file, FileAttributes.Normal);
                        File.Delete(file);
                        filesPendingDeletion.Remove(file);
                    }
                    catch
                    {
                        UserMessage.Info("Could not delete image.");
                    }
                }
            }

            UtilityFunctions.DeleteTempDirectory(TempDirectory, ref getNewTempDirectory);
            if (InnerSave != null) InnerSave(this, new ImageControlEventArgs { FilesSaved = origFilesPendingSave, FilesDeleted = origFilesPendingDeletion, });
            llView.Enabled = editMode || images.Count > 0;
            btnEdit.Enabled = true;
            this.Cursor = Cursors.Default;
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            if (imgindex > -1 && folders.Length > 0 && folders[0].Length > 0)
            {
                string dest = FileDialogs.SaveFile("", "Image|*" + images[imgindex].Extension, "Save As");
                if (dest.Length > 0)
                {
                    images[imgindex].CopyTo(dest, true);
                    File.SetAttributes(dest, FileAttributes.Normal);
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            if (imgindex > -1 && folders.Length > 0 && folders[0].Length > 0)
            {
                if (imgindex == 0)
                {
                    var okToDeleteArgs = new ImageControlEventArgs();
                    if (OkToDeletePrimaryImage == null || !OkToDeletePrimaryImage(this, okToDeleteArgs))
                    {
                        UserMessage.Info("Cannot delete the Primary Image" + okToDeleteArgs.Message + ".");
                        this.Cursor = Cursors.Default;
                        return;
                    }
                }
                if (UserMessage.YesNo("Are you sure?"))
                {
                    pbPhoto.Image?.Dispose();
                    pbPhoto.Image = null;
                    pbPhoto.Refresh();

                    DeleteImage(images[imgindex].FullName);

                    if (filenamesInOrder == null)
                    {
                        filenamesInOrder = images.Select(x => x.Name).ToList();
                    }
                    filenamesInOrder.RemoveAll(x => x.EqualsIgnoreCase(images[imgindex].Name));
                    OrderChanged = true;

                    ReloadImages(startingImageIndex: imgindex - 1);
                }
            }
            this.Cursor = Cursors.Default;
        }
        private void DeleteImage(string file)
        {
            if (Path.GetFileName(file).EqualsIgnoreCase(primaryImageName))
            {
                primaryImageName = "";
                OrderChanged = true;
            }

            if (UseEditButton)
            {
                if (!filesPendingDeletion.Contains(file))
                {
                    filesPendingDeletion.Add(file);
                }
                filesPendingSave.Remove(file);
            }
            else
            {
                try
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                    if (ImageDeleted != null)
                    {
                        ImageDeleted(this, new ImageEventArgs(file));
                    }
                }
                catch
                {
                    for (int j = 0; j < 4; j++)
                    {
                        try
                        {
                            Thread.Sleep(1000);
                            File.SetAttributes(file, FileAttributes.Normal);
                            File.Delete(file);
                            break;
                        }
                        catch { }
                    }
                    UserMessage.Info("Could not delete image.");
                }
            }

            ImagesChanged(this, new ImageEventArgs(Path.GetFileName(file)));
        }
        #endregion

        private void TryImage()
        {
            if (images.Count > 0 && imgindex != -1)
            {
                if (images[imgindex].Exists)
                {
                    try
                    {
                        var image = new Bitmap(images[imgindex].FullName);
                        lbSize.Text = image.Width + " x " + image.Height;
                        lbSize.ForeColor = image.Width < MinimumImageSize && image.Height < MinimumImageSize ? Color.Red : Color.Empty;
                        double factor = image.Height > image.Width ? (double)image.Height / pbPhoto.Height : (double)image.Width / pbPhoto.Width;
                        var sizedimage = new Bitmap(image, (int)(image.Width / factor), (int)(image.Height / factor));
                        image.Dispose();
                        pbPhoto.Image?.Dispose();
                        pbPhoto.Image = sizedimage;
                        lbPictureCount.Text = (imgindex + 1) + " / " + images.Count;
                    }
                    catch
                    {
                        SwitchToDefault();
                    }
                }
                else
                {
                    SwitchToDefault();
                }
            }
            else
            {
                SwitchToDefault();
            }
        }

        private void SwitchToDefault()
        {
            pbPhoto.Image?.Dispose();
            pbPhoto.Image = new Bitmap(@"\\server\images\default.jpg");
            imgindex = -1;
            lbSize.Text = "0 x 0";
            lbSize.ForeColor = Color.Empty;
            lbPictureCount.Text = "0 / " + images.Count;
        }

        private void ShowSelectedImageAfterReload(string origImageFilename)
        {
            // Show the same image that was being shown before the reload, if available.
            int origImgIndex = images.FindIndex(x => x.Name.EqualsIgnoreCase(origImageFilename));
            if (imgindex != origImgIndex)
            {
                imgindex = origImgIndex;
                TryImage();
            }
        }

        private void pbPhoto_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                rightClickMenu.Show(Cursor.Position);
            }
            else if (e.Button == MouseButtons.Left && imgindex == -1)
            {
                if (folders[0].Length > 0)
                {
                    List<string> files = FileDialogs.OpenFiles(null, FileDialogFilter).ToList();
                    if (files.Any(x => x.Length > 0))
                    {
                        int i = 0;
                        var fileInfos = files.Select(x => new FileInfo(x)).ToDictionary(x => i++, x => x);
                        i = 0;
                        var safeFilenames = fileInfos.Values.Select(x => UtilityFunctions.GetSafeFilename(x.Name.ToURLSafe().Replace("+", ""), additionalInvalidCharacters: InvalidFilenameCharacters)).ToDictionary(x => i++, x => x);
                        for (i = 0; i < files.Count; i++)
                        {
                            FileInfo fi = fileInfos[i];
                            SaveImage(images, FolderPath, files, i, fi, safeFilenames, out string fullPath);
                            if (!fullPath.IsNullOrBlank())
                            {
                                AddImage(fullPath);
                            }
                        }
                        ReloadImages();
                    }
                }
            }
        }
        public void SaveImage(IEnumerable<FileInfo> existingImages, string path, List<string> files, int i, FileInfo fi, Dictionary<int, string> safeFilenames, out string fullPath)
        {
            fullPath = "";
            string newFilename = safeFilenames[i];
            string newFilenameWithoutExtension = Path.GetFileNameWithoutExtension(newFilename);
            // If there are any pre-existing files with the same name as this file OR multiple new files cleaned up to the point where they have identical filenames...
            if ((existingImages.Any(x => Path.GetFileNameWithoutExtension(x.Name).EqualsIgnoreCase(newFilenameWithoutExtension)) || safeFilenames.Values.Count(x => Path.GetFileNameWithoutExtension(x).EqualsIgnoreCase(newFilenameWithoutExtension)) > 1)
                && !UserMessage.YesNo("An image with the filename " + newFilename + " already exists or multiple images with the same name are being uploaded. Are you sure you want to overwrite an image?"))
            {
                return;
            }

            Bitmap image;
            try
            {
                image = new Bitmap(files[i]);
            }
            catch (ArgumentException)
            {
                MessageBox.Show($"{Path.GetFileName(files[i])} is not a valid image file.");
                return;
            }
            catch (OutOfMemoryException)
            {
                MessageBox.Show($"{Path.GetFileName(files[i])} is too large or you are low on RAM. If the image still cannot be added after restarting your PC, it is too large.");
                return;
            }

            if (BlockSmallImages && (image.Width < MinimumImageSize || image.Height < MinimumImageSize))
            {
                MessageBox.Show("Images have to be at least " + MinimumImageSize + " x " + MinimumImageSize + " pixels.");
                return;
            }

            Directory.CreateDirectory(path); // Not necessary to check if the folder already exists. That check is built-in.
            bool isCMYK = ImageUtilities.IsCMYK(image);
            if (isCMYK)
            {
                try
                {
                    image = ImageUtilities.ConvertCMYK(image);
                }
                catch
                {
                    MessageBox.Show("Failed to convert CMYK to RGB. CMYK image will be uploaded.");
                }
            }

            fullPath = path + newFilename;
            if (!image.RawFormat.Equals(ImageFormat.Jpeg) || isCMYK)
            {
                fullPath = path + Path.GetFileNameWithoutExtension(newFilename) + ".jpg";
                if (File.Exists(fullPath))
                {
                    File.SetAttributes(fullPath, FileAttributes.Normal);
                    File.Delete(fullPath);
                }
                image.Save(fullPath, ImageFormat.Jpeg);
            }
            else
            {
                fi.CopyTo(fullPath, true);
                File.SetAttributes(fullPath, FileAttributes.Normal);
            }

            image.Dispose();
        }
        private void AddImage(string fullPath)
        {
            if (UseEditButton)
            {
                if (!filesPendingSave.Contains(fullPath))
                {
                    filesPendingSave.Add(fullPath);
                }
                filesPendingDeletion.Remove(fullPath);
            }
            else if (ImageAdded != null)
            {
                ImageAdded(this, new ImageEventArgs(fullPath));
            }

            string filename = Path.GetFileName(fullPath);
            if (images.Count == 0 && (!UseEditButton || filesPendingSave.Count == 1))
            {
                primaryImageName = filename;
                OrderChanged = true;
            }

            ImagesChanged(this, new ImageEventArgs(filename));
        }

        private void ReloadImages(int? startingImageIndex = null, string startingImageFilename = null)
        {
            images.Clear();
            if (folders != null)
            {
                var di = new DirectoryInfo(root + folders[0]);
                if (di.Exists)
                {
                    string thumbsFile = root + folders[0] + @"\thumbs.db";
                    if (File.Exists(thumbsFile))
                    {
                        File.SetAttributes(thumbsFile, FileAttributes.Normal);
                        File.Delete(thumbsFile);
                    }
                    images = UseEditButton
                        ? di.EnumerateFiles().Where(x => !filesPendingDeletion.Contains(x.FullName) && filesPendingSave.All(y => !Path.GetFileName(y).EqualsIgnoreCase(x.Name))).ToList()
                        : di.EnumerateFiles().ToList();
                }
                for (int i = 1; i < folders.Length; i++)
                {
                    di = new DirectoryInfo(root + folders[i]);
                    if (di.Exists)
                    {
                        images.AddRange(UseEditButton
                            ? di.EnumerateFiles().Where(x => !filesPendingDeletion.Contains(x.FullName) && filesPendingSave.All(y => !Path.GetFileName(y).EqualsIgnoreCase(x.Name)))
                            : di.EnumerateFiles());
                    }
                }
                if (editMode)
                {
                    di = new DirectoryInfo(TempDirectory);
                    if (di.Exists)
                    {
                        int i = 0;
                        var origImages = images.ToList();
                        while (true)
                        {
                            // Images may be in the process of being deleted from the temp directory, which can cause an UnauthorizedAccessException.
                            // Wait and try again for a total of 3 tries.
                            try
                            {
                                images.AddRange(di.EnumerateFiles().Where(x => !filesPendingDeletion.Contains(x.FullName)));
                                break;
                            }
                            catch
                            {
                                images = origImages.ToList();
                                Thread.Sleep(1000);
                                di = new DirectoryInfo(TempDirectory);
                                if (!di.Exists) break;
                                if (i == 2) throw;
                                i++;
                            }
                        }
                        images = images.OrderBy(x => x.Name).ToList();
                    }
                }

                if (filenamesInOrder != null && filenamesInOrder.Count > 0)
                {
                    List<string> imagesFilenames = images.Select(x => x.Name).ToList();
                    var imagesOrdered = new List<FileInfo>();
                    // Add images in their specified order.
                    imagesOrdered.AddRange(filenamesInOrder.Where(x => imagesFilenames.Contains(x)).Select(x => images.Find(y => y.Name.EqualsIgnoreCase(x))));
                    // Add images whose order was not specified.
                    imagesOrdered.AddRange(imagesFilenames.Where(x => !filenamesInOrder.Contains(x)).Select(x => images.Find(y => y.Name.EqualsIgnoreCase(x))));
                    images = imagesOrdered;
                }

                if (startingImageIndex != null && images.Count > startingImageIndex)
                {
                    imgindex = (int)startingImageIndex;
                }
                else if (!startingImageFilename.IsNullOrBlank())
                {
                    imgindex = 0;
                    while (imgindex < images.Count - 1 && !images[imgindex].Name.EqualsIgnoreCase(startingImageFilename))
                    {
                        imgindex++;
                    }
                }
                else
                {
                    imgindex = images.Count > 0 ? 0 : -1;
                }
            }

            llView.Enabled = editMode || images.Count > 0;
            TryImage();
        }

        private void Control_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) rightClickMenu.Show(Cursor.Position);
        }

        private void pasteMenuItem_Click(object sender, EventArgs e)
        {
            if (Clipboard.GetDataObject().GetDataPresent(DataFormats.Bitmap))
            {
                var image = (Bitmap)Clipboard.GetDataObject().GetData(DataFormats.Bitmap);
                if (BlockSmallImages && (image.Width < MinimumImageSize || image.Height < MinimumImageSize))
                {
                    image.Dispose();
                    MessageBox.Show("Images have to be at least " + MinimumImageSize + " x " + MinimumImageSize + " pixels.");
                    return;
                }
                string fileName = Path.GetRandomFileName() + ".jpg";
                try
                {
                    string path = UseEditButton ? TempDirectory : root + folders[0];
                    string fullPath = path + fileName;
                    Directory.CreateDirectory(path); // Not necessary to check if the folder already exists. That check is built-in.
                    image.Save(fullPath, ImageFormat.Jpeg);
                    image.Dispose();
                    if (UseEditButton && !filesPendingSave.Contains(fullPath))
                    {
                        filesPendingSave.Add(fullPath);
                    }
                    if (images.Count == 0 && (!UseEditButton || filesPendingSave.Count == 1))
                    {
                        primaryImageName = fileName;
                        OrderChanged = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Paste failed because " + ex.Message);
                }
                ImagesChanged(this, new ImageEventArgs(fileName));
                ReloadImages(startingImageFilename: fileName);
            }
            else
            {
                MessageBox.Show("Please copy an image first.");
            }
        }

        #region EventArgs
        public class ImageControlEventArgs : EventArgs
        {
            public bool RecordLockedSuccessfully;
            public List<string> FilesSaved;
            public List<string> FilesDeleted;
            public string Message;
        }
        #endregion
    }
}
