using System;
using System.Diagnostics;
using System.Text;

namespace ww.Utilities.Extensions
{
    [DebuggerStepThrough]
    public static class StringBuilderExtensions
    {
        /// <summary>
        /// Appends the default new line character(s) to the calling <see cref="StringBuilder"/>, followed by the specified <see cref="String"/>.
        /// </summary>
        public static StringBuilder AppendLineFirst(this StringBuilder input, string value)
        {
            return input.Append(Environment.NewLine).Append(value);
        }

        // Taken from https://stackoverflow.com/a/6601226
        /// <summary>
        /// Reports the index of the first occurrence of the specified string in this <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="value">The string to seek.</param>
        /// <param name="startIndex">The search starting position.</param>
        /// <param name="ignoreCase">When true it will ignore case.</param>
        /// <returns>The zero-based index position of value if that string is found, or -1 if it is not. If value is <see cref="string.Empty"/>, the return value is <see cref="startIndex"/>.</returns>
        public static int IndexOf(this StringBuilder sb, string value, int startIndex = 0, bool ignoreCase = false)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (startIndex >= value.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (value == string.Empty)
                return startIndex;

            int index;
            int maxSearchLength = sb.Length - value.Length + 1;

            if (ignoreCase)
            {
                for (int i = startIndex; i < maxSearchLength; ++i)
                {
                    if (Char.ToLower(sb[i]) == Char.ToLower(value[0]))
                    {
                        index = 1;
                        while ((index < value.Length) && (Char.ToLower(sb[i + index]) == Char.ToLower(value[index])))
                            ++index;

                        if (index == value.Length)
                            return i;
                    }
                }
            }
            else
            {
                for (int i = startIndex; i < maxSearchLength; ++i)
                {
                    if (sb[i] == value[0])
                    {
                        index = 1;
                        while ((index < value.Length) && (sb[i + index] == value[index]))
                            ++index;

                        if (index == value.Length)
                            return i;
                    }
                }
            }

            return -1;
        }
    }
}
