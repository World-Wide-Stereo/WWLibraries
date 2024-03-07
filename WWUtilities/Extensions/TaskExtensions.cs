using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Win32.TaskScheduler;

namespace ww.Utilities.Extensions
{
    [DebuggerStepThrough]
    public static class TaskExtensions
    {
        /// <summary>
        /// Get the author of this Task.
        /// </summary>
        public static string GetAuthor(this Task task)
        {
            string author = null;
            Regex rgx = new Regex(@"(?:<Author>)([a-zA-Z\\]*)(?:</Author>)");
            if (rgx.IsMatch(task.Xml)) author = rgx.Match(task.Xml).Groups[1].Value;
            return author ?? "";
        }
    }
}
