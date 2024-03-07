using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Drawing.Imaging;
using System.IO;
using ww.Utilities.Extensions;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace ww.Utilities
{
    public class ImageUtilities
    {
        private const string WebTempPath = @"\\server\images\webtemp\";

        private static ImageCodecInfo GetEncoderInfo(string mime)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mime)
                    return codecs[i];
            return null;
        }

        private static string GetImageFlags(Image image)
        {
            ImageFlags flags = (ImageFlags)image.Flags;
            return flags.ToString();
        }

        // Taken from https://stackoverflow.com/questions/5065371/
        public static bool IsCMYK(Image image)
        {
            const int pixelFormat32bppCMYK = 0x200F;

            var pixelFormat = (int)image.PixelFormat;
            if (pixelFormat == pixelFormat32bppCMYK)
            {
                return true;
            }
            if ((GetImageFlags(image).IndexOf("Ycck") > -1) ||
                (GetImageFlags(image).IndexOf("Cmyk") > -1))
            {
                return true;
            }
            else
                return false;
        }

        public static Bitmap ConvertCMYK(Bitmap bitmap)
        {
            Bitmap newBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb);

            Graphics graph = Graphics.FromImage(newBitmap);
            graph.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graph.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            graph.DrawImage(bitmap, rect);

            Bitmap returnBitmap = new Bitmap(newBitmap);

            graph.Dispose();
            newBitmap.Dispose();
            bitmap.Dispose();

            return returnBitmap;
        }

        public static string ConvertToImageAndCompress(FileInfo file, int length = 102400) // this needs to save
        {
            string guidWebTempPath = $@"{WebTempPath}{Guid.NewGuid()}\";
            Directory.CreateDirectory(guidWebTempPath);
            if (file.Length > length)
            {
                using (Image image = Image.FromFile(file.FullName))
                using (EncoderParameter qualityParam = new EncoderParameter(Encoder.Quality, 80L))
                using (EncoderParameters qualityParams = new EncoderParameters(1))
                {
                    ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");
                    qualityParams.Param[0] = qualityParam;
                    image.Save(guidWebTempPath + file.Name, jpegCodec, qualityParams);
                    file = new FileInfo(guidWebTempPath + file.Name);
                }
            }
            else
            {
                // Still need to create an image just don't compress it.
                using (Image image = Image.FromFile(file.FullName))
                {
                    image.Save(guidWebTempPath + file.Name, ImageFormat.Jpeg);
                }
            }

            return guidWebTempPath + file.Name;
        }

        public static string ConvertToImageAndResizeAndSaveToTemp(Image image, int width, int height, string fileName, bool dimenisonsAreMinimums = false, bool padWithWhitespaceToSquare = true)
        {
            using (Image resizedImage = ResizeImage(image, width, height, dimenisonsAreMinimums, padWithWhitespaceToSquare))
            {
                string guidWebTempPath = $@"{WebTempPath}{Guid.NewGuid()}\";
                Directory.CreateDirectory(guidWebTempPath);
                string newPath = guidWebTempPath + fileName;
                resizedImage.Save(newPath, ImageFormat.Jpeg);
                return newPath;
            }
        }

        public static Image ConvertToImageAndResize(string path, int width, int height, bool dimenisonsAreMinimums = false, bool padWithWhitespaceToSquare = true)
        {
            return ResizeImage(Image.FromFile(path), width, height, dimenisonsAreMinimums, padWithWhitespaceToSquare);
        }

        public static Image ResizeImage(Image imgToResize, int width, int height, bool dimenisonsAreMinimums = false, bool padWithWhitespaceToSquare = true, bool disposeImgToResize = true)
        {
            // Don't resize the image if it's smaller than the specified dimensions, unless caught by the next if statement.
            if (imgToResize.Width > width || imgToResize.Height > height)
            {
                int destX = 0;
                int destY = 0;

                float percent;
                float percentW = (float)width / (float)imgToResize.Width;
                float percentH = (float)height / (float)imgToResize.Height;

                if (percentH < percentW)
                {
                    percent = percentH;
                    destX = (int)((width - (imgToResize.Width * percent)) / 2);
                }
                else
                {
                    percent = percentW;
                    destY = (int)((height - (imgToResize.Height * percent)) / 2);
                }

                int destWidth = (int)(imgToResize.Width * percent);
                int destHeight = (int)(imgToResize.Height * percent);

                var bmPhoto = padWithWhitespaceToSquare ? new Bitmap(width, height) : new Bitmap(destWidth, destHeight);
                bmPhoto.SetResolution(imgToResize.HorizontalResolution, imgToResize.VerticalResolution);

                Graphics grPhoto = Graphics.FromImage(bmPhoto);
                SetGraphicsProperties(grPhoto);
                grPhoto.DrawImage(imgToResize,
                    padWithWhitespaceToSquare
                        ? new Rectangle(destX, destY, destWidth, destHeight)
                        : new Rectangle(0, 0, destWidth, destHeight), // Drawn from start - no whitespace.
                    new Rectangle(0, 0, imgToResize.Width, imgToResize.Height), GraphicsUnit.Pixel);
                grPhoto.Dispose();

                if (disposeImgToResize) imgToResize.Dispose();
                return bmPhoto;
            }

            // If the dimensions specified are minimums and the image is smaller than them, pad the image with whitespace to bring it up to the minimums.
            if (dimenisonsAreMinimums && (imgToResize.Width < width || imgToResize.Height < height))
            {
                int destX = imgToResize.Width < width ? (width - imgToResize.Width) / 2 : 0;
                int destY = imgToResize.Height < height ? (height - imgToResize.Height) / 2 : 0;

                var bmPhoto = new Bitmap(width, height);
                bmPhoto.SetResolution(imgToResize.HorizontalResolution, imgToResize.VerticalResolution);

                Graphics grPhoto = Graphics.FromImage(bmPhoto);
                SetGraphicsProperties(grPhoto);
                grPhoto.DrawImage(imgToResize, new Rectangle(destX, destY, imgToResize.Width, imgToResize.Height), new Rectangle(0, 0, imgToResize.Width, imgToResize.Height), GraphicsUnit.Pixel);
                grPhoto.Dispose();

                if (disposeImgToResize) imgToResize.Dispose();
                return bmPhoto;
            }

            return imgToResize;
        }

        //This will perform a high quality resize https://stackoverflow.com/questions/1922040/how-to-resize-an-image-c-sharp
        public static Bitmap HighQualityImageResize(Image image, int width, int height)
        {
            var destImage = new Bitmap(width, height);
            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, new Rectangle(0, 0, width, height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

        private static void SetGraphicsProperties(Graphics grPhoto)
        {
            grPhoto.Clear(Color.White);
            grPhoto.CompositingMode = CompositingMode.SourceCopy;
            grPhoto.CompositingQuality = CompositingQuality.HighQuality;
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
            grPhoto.SmoothingMode = SmoothingMode.HighQuality;
            grPhoto.PixelOffsetMode = PixelOffsetMode.HighQuality;
        }

        public static void CreateThumbs(string partno, int usedListingID = 0)
        {
            Thumbs(@"\\server\images\" + (usedListingID > 0 ? $@"_usedListings\{usedListingID}" : partno));
        }

        public static void CreateThumbs()
        {
            string path = @"\\server\images\";
            DirectoryInfo directory = new DirectoryInfo(path);
            foreach (var dir in directory.EnumerateDirectories())
            {
                Thumbs(dir.FullName);
            }
        }

        private static void Thumbs(string path)
        {
            string s = path + "\\images";
            if (Directory.Exists(s)) //okay we are inside the part and images now check if there are any files in there
            {
                DirectoryInfo d = new DirectoryInfo(s);
                FileInfo[] images = d.GetFiles();
                if (images.Any()) // great have files in the folder lets create 2 new folders now
                {
                    try
                    {
                        var d140 = d.CreateSubdirectory("Thumb140");
                        foreach (var image in images.Where(x => x.Extension.ToLower().EqualsAnyOf(".jpg", ".jpeg", ".png", ".gif", ".bmp")))
                        {
                            Console.WriteLine("Working on " + d.FullName + ". File:" + image.Name);
                            var comprImage = ImageUtilities.ConvertToImageAndCompress(image); // compress images
                            var i140 = ImageUtilities.ConvertToImageAndResize(comprImage, 140, 140); //resize
                            i140.Save(d140 + "\\" + image.Name); //save both to appropriate folders
                            long fileSize140 = new FileInfo(d140 + "\\" + image.Name).Length;
                            if (fileSize140 > image.Length) // if new file is larger than old one, copy old one in place of new one
                            {
                                image.CopyTo(d140.FullName + "//" + image.Name, true);
                            }

                            i140.Dispose();
                            try
                            {
                                Directory.Delete(Path.GetDirectoryName(comprImage), true);
                            }
                            catch
                            {
                                // No need to take action. EOD will cleanup old temp folders.
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public static Image SaveBytesToImage(byte[] byteArray)
        {
            using (var ms = new MemoryStream(byteArray))
            {
                return Image.FromStream(ms);
            }
        }

        public static void SaveImageAsPdf(string imageFullPath, string pdfFullPath, int width = 0)
        {
            using (XImage img = XImage.FromFile(imageFullPath))
            {
                SaveImageAsPdf(img, pdfFullPath, width);
            }
        }

        public static void SaveImageAsPdf(Image image, string pdfFullPath, int width = 0)
        {
            XImage img;
            using (var memoryStream = new MemoryStream())
            {
                image.Save(memoryStream, ImageFormat.Png);
                img = XImage.FromStream(memoryStream);
            }
            SaveImageAsPdf(img, pdfFullPath, width);
        }

        private static void SaveImageAsPdf(XImage img, string pdfFullPath, int width)
        {
            using (var pdf = new PdfDocument())
            {
                PdfPage page = pdf.AddPage();

                if (width == 0)
                {
                    page.Width = img.PixelWidth;
                    page.Height = img.PixelHeight;
                }
                else
                {
                    // Calculate the new height to keep the aspect ratio.
                    page.Width = width;
                    page.Height = (int)(width / (double)img.PixelWidth * img.PixelHeight);
                }

                using (XGraphics gfx = XGraphics.FromPdfPage(page))
                {
                    gfx.DrawImage(img, 0, 0, page.Width, page.Height);
                }
                pdf.Save(pdfFullPath);
            }
        }
    }
}
