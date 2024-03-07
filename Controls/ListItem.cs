using System;

namespace Controls
{
    public class ListItem
    {
        public string Text { get; set; }
        public object Value { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
    // Creating a ListItem of a specific type means you do not have to cast the item's value every time you use it.
    public class ListItem<T>
    {
        public string Text { get; set; }
        public virtual T Value { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}
