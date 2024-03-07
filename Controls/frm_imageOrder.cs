using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Controls.Extensions;
using ww.Tables;
using ww.Utilities;
using ww.Utilities.Extensions;

namespace Controls
{
    public partial class frm_imageOrder : Form
    {
        public bool OrderChanged { get; private set; }
        public List<FileInfo> OrderedImages { get; private set; }
        public List<FileInfo> DeletedImages { get; } = new List<FileInfo>();
        public List<FileInfo> AddedImages { get; } = new List<FileInfo>();

        private bool ViewOnly;
        private uc_image ImageControl;
        private List<FileInfo> OriginalImageOrder = new List<FileInfo>();
        private List<FileInfo> ReplacedImages = new List<FileInfo>();
        private ContextMenuStrip mnuContext = new ContextMenuStrip();
        private PictureBox RightClickedPictureBox;
        private bool OutOfMemoryExceptionEncountered;

        public delegate bool OkToDeletePrimaryImageHandler(object sender, uc_image.ImageControlEventArgs e);
        public event uc_image.OkToDeletePrimaryImageHandler OkToDeletePrimaryImage;


        public frm_imageOrder(string imagesForItem, IEnumerable<FileInfo> images, uc_image imageControl = null, bool viewOnly = false)
        {
            InitializeComponent();
            this.ViewOnly = viewOnly;
            this.ImageControl = imageControl;

            try
            {
                foreach (FileInfo image in images)
                {
                    OriginalImageOrder.Add(image);
                    AddImageToFlowLayoutPanel(image);
                }
            }
            catch (OutOfMemoryException)
            {
                foreach (PictureBox pictureBox in flpImages.Controls.Cast<PictureBox>().ToList())
                {
                    flpImages.Controls.Remove(pictureBox);
                    pictureBox.Dispose();
                }
                OutOfMemoryExceptionEncountered = true;
            }

            if (viewOnly)
            {
                this.Text = "Images" + (imagesForItem.IsNullOrBlank() ? "" : " - " + imagesForItem);
                lbInstructions.Visible = false;
                btnCancel.Text = "Close";
                btnCancel.Location = btnOK.Location;
                btnOK.Visible = false;
                btnAdd.Visible = false;
            }
            else
            {
                flpImages.DragOver += new DragEventHandler(flpImages_DragOver);
                flpImages.AllowDrop = true;

                var mniDelete = new ToolStripMenuItem { Text = "Delete" };
                mniDelete.Click += new EventHandler(mniDelete_Click);
                mnuContext.Items.Add(mniDelete);
            }
        }

        private void frm_imageOrder_Shown(object sender, EventArgs e)
        {
            if (OutOfMemoryExceptionEncountered)
            {
                UserMessage.Info($"Failed to load the images due to a lack of RAM. Please close {Global.ProgramName} and perhaps some other programs, or restart your PC, and then try again.");
                this.Close();
            }
        }

        private void AddImageToFlowLayoutPanel(FileInfo image, int insertAtIndex = -1)
        {
            var pictureBox = new PictureBox
            {
                Size = new Size(128, 128),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Image.FromFile(image.FullName),
                Tag = image,
            };
            if (!ViewOnly)
            {
                pictureBox.MouseDown += new MouseEventHandler(PictureBox_MouseDown);
                pictureBox.MouseUp += new MouseEventHandler(PictureBox_MouseUp);
            }
            flpImages.Controls.Add(pictureBox);
            if (insertAtIndex != -1)
            {
                flpImages.Controls.SetChildIndex(pictureBox, insertAtIndex);
            }
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                flpImages.DoDragDrop(sender, DragDropEffects.Move);
            }
        }
        private void flpImages_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move; // Changes the cursor to the drag and drop move cursor.
            PictureBox clickedPictureBox = (PictureBox)e.Data.GetData(typeof(PictureBox));
            PictureBox stoppedOnPictureBox = (PictureBox)flpImages.GetChildAtPoint(flpImages.PointToClient(new Point(e.X, e.Y)));
            if (clickedPictureBox != stoppedOnPictureBox)
            {
                int indexOfStoppedOnPictureBox = flpImages.Controls.IndexOf(stoppedOnPictureBox);
                flpImages.Controls.SetChildIndex(clickedPictureBox, indexOfStoppedOnPictureBox);
            }
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                RightClickedPictureBox = (PictureBox)flpImages.GetChildAtPoint(flpImages.PointToClient(Cursor.Position));
                mnuContext.Show(Cursor.Position);
            }
        }
        private void mniDelete_Click(object sender, EventArgs e)
        {
            if (flpImages.Controls.IndexOf(RightClickedPictureBox) == 0)
            {
                var okToDeleteArgs = new uc_image.ImageControlEventArgs();
                if (OkToDeletePrimaryImage == null || !OkToDeletePrimaryImage(this, okToDeleteArgs))
                {
                    UserMessage.Info("Cannot delete the Primary Image" + okToDeleteArgs.Message + ".");
                    return;
                }
            }

            flpImages.Controls.Remove(flpImages.Controls.Cast<PictureBox>().First(x => x == RightClickedPictureBox));
            FileInfo fileInfo = (FileInfo)RightClickedPictureBox.Tag;
            this.AddedImages.RemoveAll(x => x.Name.EqualsIgnoreCase(fileInfo.Name));
            this.DeletedImages.Add(fileInfo);
            this.DeletedImages.AddRange(ReplacedImages.Where(x => x.Name.EqualsIgnoreCase(fileInfo.Name)));
            RightClickedPictureBox.Image.Dispose();
            RightClickedPictureBox = null;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (ImageControl == null)
            {
                UserMessage.Info("Adding has not been implemented. Please contact IT.");
                return;
            }

            this.SuspendRedraw();

            List<string> files = FileDialogs.OpenFiles(null, uc_image.FileDialogFilter).ToList();
            if (files.Any(x => x.Length > 0))
            {
                int i = 0;
                var fileInfos = files.Select(x => new FileInfo(x)).ToDictionary(x => i++, x => x);
                i = 0;
                var safeFilenames = fileInfos.Values.Select(x => UtilityFunctions.GetSafeFilename(x.Name.ToURLSafe().Replace("+", ""), additionalInvalidCharacters: ImageControl.InvalidFilenameCharacters)).ToDictionary(x => i++, x => x);
                for (i = 0; i < files.Count; i++)
                {
                    FileInfo fi = fileInfos[i];
                    List<FileInfo> existingImages = flpImages.Controls.Cast<PictureBox>().Select(x => (FileInfo)x.Tag).ToList();

                    // Remove the existing PictureBox, if there is one, so that its file can be replaced.
                    int existingPictureBoxIndex = -1;
                    PictureBox existingPictureBox = flpImages.Controls.Cast<PictureBox>().FirstOrDefault(x => ((FileInfo)x.Tag).Name.EqualsIgnoreCase(fi.Name));
                    if (existingPictureBox != null)
                    {
                        existingPictureBoxIndex = flpImages.Controls.IndexOf(existingPictureBox);
                        flpImages.Controls.Remove(existingPictureBox);
                        existingPictureBox.Image.Dispose();
                    }

                    string fullPath;
                    ImageControl.SaveImage(existingImages, ImageControl.FolderPath, files, i, fi, safeFilenames, out fullPath);

                    if (fullPath.IsNullOrBlank())
                    {
                        // If we cancel or fail at an attempt to replace an existing image, that original image must be readded.
                        if (existingPictureBox != null)
                        {
                            AddImageToFlowLayoutPanel((FileInfo)existingPictureBox.Tag, insertAtIndex: existingPictureBoxIndex);
                        }
                    }
                    else
                    {
                        var addedFi = new FileInfo(fullPath);

                        DeletedImages.RemoveAll(x => x.Name.EqualsIgnoreCase(addedFi.Name));
                        AddedImages.RemoveAll(x => x.Name.EqualsIgnoreCase(addedFi.Name));
                        AddedImages.Add(addedFi);

                        if (existingPictureBox != null)
                        {
                            ReplacedImages.Add((FileInfo)existingPictureBox.Tag);
                        }

                        AddImageToFlowLayoutPanel(addedFi, insertAtIndex: existingPictureBoxIndex);
                    }
                }
            }

            this.ResumeRedraw();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.OrderedImages = flpImages.Controls.Cast<PictureBox>().Select(x => (FileInfo)x.Tag).ToList();
            this.OrderChanged = OriginalImageOrder.Any(x => OriginalImageOrder.FindIndex(y => y == x) != OrderedImages.FindIndex(y => y == x)) || (OriginalImageOrder.Count == 0 && OrderedImages.Count > 0);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void frm_imageOrder_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (PictureBox pictureBox in flpImages.Controls)
            {
                pictureBox.Image.Dispose();
            }
        }
    }
}
