using System;

namespace Controls
{
    public class RunJobEventArgs : EventArgs
    {
        public RunJobEventArgsFunction function;
        public object parm;
        public RunJobEventArgs(RunJobEventArgsFunction f, object p)
        {
            this.function = f;
            this.parm = p;
        }
    }

    public enum RunJobEventArgsFunction
    {
        ExampleJob,
    }

    public delegate void ImageEventHandler(object sender, ImageEventArgs e);
    public class ImageEventArgs : EventArgs
    {
        public string fileName;
        public ImageEventArgs(string fileName)
        {
            this.fileName = fileName;
        }
    }
}
