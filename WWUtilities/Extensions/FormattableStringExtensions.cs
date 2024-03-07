using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace ww.Utilities.Extensions
{
    [DebuggerStepThrough]
    public static class FormattableStringExtensions
    {
        [Obsolete("This function should only be used for debugging. The query it returns is otherwise vulnerable to SQL injection.")]
        public static string ToUnparameterizedSQL(this FormattableString input, bool booleanToBit = false)
        {
            string query = input.Format;
            object[] inputArgs = input.GetArguments();

            for (int i = 0; i < inputArgs.Length; i++)
            {
                string value;
                Type type = inputArgs[i].GetType();

                switch (type.Name)
                {
                    case nameof(String):
                        value = "'" + inputArgs[i] + "'";
                        break;
                    case nameof(DateTime):
                        value = "'" + ((DateTime)inputArgs[i]).ToMilitaryDateTimeString() + "'";
                        break;
                    case nameof(Boolean):
                        value = booleanToBit ? ((bool)inputArgs[i] ? "1" : "0") : inputArgs[i].ToString().ToLower();
                        break;
                    default:
                        value = type.IsSubclassOf(typeof(Enum)) ? ((int)inputArgs[i]).ToString() : inputArgs[i].ToString();
                        break;
                }

                query = query.Replace("{" + i + "}", value);
            }

            return query;
        }

        /// <summary>
        /// Retrieves a substring from this instance. The substring starts at a specified character position
        /// and uses the length variable, if greater than 0, otherwise it continues to the end of the string.
        /// </summary>
        public static FormattableString Substring(this FormattableString input, int startIndex, int length = 0)
        {
            string format = length <= 0 ? input.Format.Substring(startIndex) : input.Format.Substring(startIndex, length);

            List<object> arguments = null;
            if (input.ArgumentCount > 0)
            {
                List<int> indexes = Regex.Matches(format, FormattableStringBuilder.IndexRegexPattern).Cast<Match>().Select(x => x.Value.ToInt()).ToList();
                if (indexes.Count > 0)
                {
                    arguments = input.GetArguments().ToList().GetRange(indexes[0], indexes.Count);

                    int i = 0;
                    foreach (int index in indexes)
                    {
                        format = format.Replace("{" + index + "}", "{" + i + "}");
                        i++;
                    }
                }
            }

            return arguments == null ? FormattableStringFactory.Create(format) : FormattableStringFactory.Create(format, arguments.ToArray());
        }
    }
}
