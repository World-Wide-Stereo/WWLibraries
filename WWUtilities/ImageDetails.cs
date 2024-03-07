namespace ww.Utilities
{
	public class ImageDetails
	{
		public string Filename { get; set; }
		public string FullPath { get; set; }
		public string URL { get; set; }

		public ImageDetails() { }
		public ImageDetails(string filename, string localPath, string url)
		{
			Filename = filename;
			FullPath = localPath;
			URL = url;
		}
	}
}
