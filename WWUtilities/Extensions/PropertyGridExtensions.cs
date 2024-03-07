using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace XmlGridControl
{
    [DebuggerStepThrough]
    public static class PropertyGridExtensions
    {
        /// <summary>
        /// Gets the private PropertyGridView instance.
        /// </summary>
        private static object GetPrivatePropertyGridView(PropertyGrid propertyGrid)
        {
            // private PropertyGridView GetPropertyGridView();
            // PropertyGridView is an internal class...
            MethodInfo methodInfo = typeof(PropertyGrid).GetMethod("GetPropertyGridView", BindingFlags.NonPublic | BindingFlags.Instance);
            return methodInfo.Invoke(propertyGrid, new object[] { });
        }

        /// <summary>
        /// Effectively gets the splitter distance by grabbing the width of the left column.
        /// </summary>
        public static int GetSplitterDistance(this PropertyGrid propertyGrid)
        {
            // System.Windows.Forms.PropertyGridInternal.PropertyGridView
            object gridView = GetPrivatePropertyGridView(propertyGrid);

            // protected int InternalLabelWidth
            PropertyInfo propInfo = gridView.GetType().GetProperty("InternalLabelWidth", BindingFlags.NonPublic | BindingFlags.Instance);
            return (int)propInfo.GetValue(gridView, new object[]{});
        }

        /// <summary>
        /// Moves the splitter to the supplied horizontal position.
        /// </summary>
        public static void SetSplitterDistance(this PropertyGrid propertyGrid, int newDistance)
        {
            // System.Windows.Forms.PropertyGridInternal.PropertyGridView
            object gridView = GetPrivatePropertyGridView(propertyGrid);

            // private void MoveSplitterTo(int xpos);
            MethodInfo methodInfo = gridView.GetType().GetMethod("MoveSplitterTo", BindingFlags.NonPublic | BindingFlags.Instance);
            methodInfo.Invoke(gridView, new object[] { newDistance });
        }
    }
}