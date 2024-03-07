using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using ww.Utilities.Extensions;

namespace ww.Utilities
{
    [DebuggerStepThrough]
    public class FormattableStringBuilder
    {
        #region Fields
        private readonly StringBuilder FormatBuilder;
        private readonly List<object> Arguments;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="FormattableStringBuilder"/> class.
        /// </summary>
        public FormattableStringBuilder()
        {
            this.FormatBuilder = new StringBuilder();
            this.Arguments = new List<object>();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="FormattableStringBuilder"/> class using the specified <see cref="FormattableString"/>.
        /// </summary>
        public FormattableStringBuilder(FormattableString value)
        {
            this.FormatBuilder = new StringBuilder(value.Format);
            this.Arguments = value.GetArguments().ToList();
        }
        #endregion

        #region Constants
        /// <summary>
        /// Used to locate each index in the Format string of a <see cref="FormattableString"/>.
        /// </summary>
        public const string IndexRegexPattern = @"(?<=\{).+?(?=\})";
        #endregion

        #region Properties
        /// <summary>
        /// Returns true when the Format string contains text.
        /// </summary>
        public bool ContainsAnyText
        {
            get { return this.FormatBuilder.Length > 0; }
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Converts the value of this instance to a <see cref="FormattableString"/>.
        /// </summary>
        public FormattableString ToFormattableString()
        {
            return FormattableStringFactory.Create(this.FormatBuilder.ToString(), this.Arguments.ToArray());
        }
        /// <summary>
        /// Converts the value of this instance to a <see cref="String"/>.
        /// </summary>
        public new string ToString()
        {
            return this.ToFormattableString().ToString();
        }

        [Obsolete("This function should only be used for debugging. The query it returns is otherwise vulnerable to SQL injection.")]
        public string ToUnparameterizedSQL(bool booleanToBit = false)
        {
            return this.ToFormattableString().ToUnparameterizedSQL(booleanToBit: booleanToBit);
        }

        /// <summary>
        /// Removes all text and arguments from the current instance.
        /// </summary>
        public FormattableStringBuilder Clear()
        {
            this.FormatBuilder.Clear();
            this.Arguments.Clear();
            return this;
        }

        /// <summary>
        /// Appends the specified <see cref="FormattableString"/> to this instance.
        /// </summary>
        public FormattableStringBuilder Append(FormattableString value)
        {
            string format = value.Format;
            if (value.ArgumentCount > 0)
            {
                foreach (string indexAsString in Regex.Matches(value.Format, IndexRegexPattern).Cast<Match>().Select(x => x.Value).Reverse())
                {
                    format = format.Replace("{" + indexAsString + "}", "{" + (indexAsString.ToInt() + this.Arguments.Count) + "}");
                }
                this.Arguments.AddRange(value.GetArguments());
            }
            this.FormatBuilder.Append(format);
            return this;
        }
        /// <summary>
        /// Appends the specified <see cref="FormattableString"/> to this instance, followed by the default new line character(s).
        /// </summary>
        public FormattableStringBuilder AppendLine(FormattableString value)
        {
            this.Append(value);
            this.FormatBuilder.Append(Environment.NewLine);
            return this;
        }
        /// <summary>
        /// Appends the default new line character(s) to this instance, followed by the specified <see cref="FormattableString"/>.
        /// </summary>
        public FormattableStringBuilder AppendLineFirst(FormattableString value)
        {
            this.FormatBuilder.Append(Environment.NewLine);
            this.Append(value);
            return this;
        }

        /// <summary>
        /// Inserts a <see cref="FormattableString"/> into this instance at the specified character position.
        /// </summary>
        public FormattableStringBuilder Insert(int index, FormattableString value)
        {
            string format = this.FormatBuilder.ToString();
            string formatBeforeInsert = format.Substring(0, index);
            format = value.Format + format.Substring(index);

            if (value.ArgumentCount > 0)
            {
                List<string> formatIndexesAsStrings = Regex.Matches(format, IndexRegexPattern).Cast<Match>().Select(x => x.Value).Reverse().ToList();
                int numArgsBeforeInsert = Regex.Matches(formatBeforeInsert, IndexRegexPattern).Count;
                int numArgsTotal = numArgsBeforeInsert + formatIndexesAsStrings.Count;

                int i = 1;
                foreach (string indexAsString in formatIndexesAsStrings)
                {
                    int lastIndexOfIndex = format.LastIndexOf("{" + indexAsString + "}", StringComparison.OrdinalIgnoreCase);
                    format = format.Substring(0, lastIndexOfIndex) + "{" + (numArgsTotal - i) + "}" + format.Substring(lastIndexOfIndex + 2 + indexAsString.Length);
                    i++;
                }

                this.Arguments.InsertRange(numArgsBeforeInsert, value.GetArguments());
            }

            this.FormatBuilder.Clear();
            this.FormatBuilder.Append(formatBeforeInsert).Append(format);

            return this;
        }

        /// <summary>
        /// Replaces all occurrences of the first string with the second, but only within the Format string. Arguments cannot be replaced.
        /// </summary>
        public FormattableStringBuilder Replace(string oldValue, string newValue)
        {
            this.FormatBuilder.Replace(oldValue, newValue);
            return this;
        }
        #endregion
    }
}
