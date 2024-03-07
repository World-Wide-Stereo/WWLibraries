using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using HtmlAgilityPack;
using ww.Utilities.Security;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace ww.Utilities.Extensions
{
    [DebuggerStepThrough]
    public static class StringExtensions
    {
        public static string AsNullIfBlank(this string input)
        {
            if (String.IsNullOrWhiteSpace(input)) return null;
            else return input;
        }

        public static bool ContainsAnyOf(this char[] testString, params char[] tests)
        {
            return ContainsAnyOf(testString, (IEnumerable<char>)tests);
        }
        public static bool ContainsAnyOf(this char[] testString, IEnumerable<char> tests)
        {
            return tests.Any(x => testString.Contains(x));
        }

        public static bool ContainsAnyOf(this string testString, IEnumerable<string> tests, bool ignoreCase = false)
        {
            return tests.Any(x => testString.Contains(x, ignoreCase));
        }
        public static bool ContainsAnyOf(this string testString, params string[] tests)
        {
            return ContainsAnyOf(testString, (IEnumerable<string>)tests);
        }
        public static bool ContainsAnyOf(this string testString, bool ignoreCase = false, params string[] tests)
        {
            return ContainsAnyOf(testString, (IEnumerable<string>)tests, ignoreCase: ignoreCase);
        }

        public static bool ContainsAllOf(this string testString, IEnumerable<string> tests, bool ignoreCase = false)
        {
            return tests.All(x => testString.Contains(x, ignoreCase));
        }
        public static bool ContainsAllOf(this string testString, params string[] tests)
        {
            return ContainsAllOf(testString, (IEnumerable<string>)tests);
        }
        public static bool ContainsAllOf(this string testString, bool ignoreCase = false, params string[] tests)
        {
            return ContainsAllOf(testString, (IEnumerable<string>)tests, ignoreCase: ignoreCase);
        }

        public static string DefaultIfEmpty(this string testString, object defaultValue)
        {
            if (testString == null || testString.Length == 0)
                return defaultValue.ToString();
            else
                return testString;
        }

        public static string Encrypt(this string input, byte[] key = null)
        {
            return Encryption.EncryptUsingAESGCM(input, key: key);
        }

        public static string Decrypt(this string input, byte[] key = null)
        {
            return Encryption.DecryptUsingAESGCM(input, key: key);
        }

        public static bool StartsWithAnyOf(this string testString, IEnumerable<string> tests)
        {
            return tests.Any(x => testString.StartsWith(x));
        }
        public static bool StartsWithAnyOf(this string testString, params string[] tests)
        {
            return StartsWithAnyOf(testString, (IEnumerable<string>)tests);
        }
        public static bool StartsWithAnyOf(this string testString, IEnumerable<string> tests, StringComparison stringComparison)
        {
            return tests.Any(x => testString.StartsWith(x, stringComparison));
        }
        public static bool StartsWithAnyOf(this string testString, IEnumerable<char> tests)
        {
            return tests.Any(x => testString.StartsWith(x.ToString()));
        }
        public static bool StartsWithAnyOf(this string testString, params char[] tests)
        {
            return StartsWithAnyOf(testString, (IEnumerable<char>)tests);
        }

        public static bool EndsWithAnyOf(this string testString, IEnumerable<string> tests)
        {
            return tests.Any(x => testString.EndsWith(x));
        }
        public static bool EndsWithAnyOf(this string testString, params string[] tests)
        {
            return EndsWithAnyOf(testString, (IEnumerable<string>)tests);
        }
        public static bool EndsWithAnyOf(this string testString, IEnumerable<string> tests, StringComparison stringComparison)
        {
            return tests.Any(x => testString.EndsWith(x, stringComparison));
        }
        public static bool EndsWithAnyOf(this string testString, IEnumerable<char> tests)
        {
            return tests.Any(x => testString.EndsWith(x.ToString()));
        }
        public static bool EndsWithAnyOf(this string testString, params char[] tests)
        {
            return EndsWithAnyOf(testString, (IEnumerable<char>)tests);
        }

        public static long GetDigitsInString(this string value)
        {
            return value == null ? 0 : new Regex(@"[^\d]").Replace(value, "").ToLong();
        }
        public static string GetDigitsInStringAsString(this string value)
        {
            return value == null ? "" : new Regex(@"[^\d]").Replace(value, "");
        }

        public static int IndexOfNonNumericChar(this string input, int startIndex = 0)
        {
            for (int i = startIndex; i < input.Length; i++)
            {
                if (!Char.IsNumber(input[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public static bool EqualsAnyOf(this string testString, IEnumerable<string> tests)
        {
            return tests.Contains(testString);
        }
        public static bool EqualsAnyOf(this string testString, params string[] tests)
        {
            return EqualsAnyOf(testString, (IEnumerable<string>)tests);
        }
        public static bool EqualsAnyOf(this string testString, IEnumerable<string> tests, StringComparison stringComparison)
        {
            return tests.Any(x => x.Equals(testString, stringComparison));
        }

        public static bool IsInteger(this string number)
        {
            long useless;
            return long.TryParse(number, out useless);
        }

        public static bool IsNullOrBlank(this string input)
        {
            return String.IsNullOrWhiteSpace(input);
        }

        public static string MaxLength(this string text, int max)
        {
            if (text != null && text.Length > max)
            {
                return text.Substring(0, max);
            }
            else
            {
                return text;
            }
        }

        public static string RemoveAll(this string input, IEnumerable<string> removals)
        {
            foreach (string rem in removals)
            {
                input = input.Replace(rem, ""); //.Replace(rem.ToUpper(), "").Replace(rem.ToLower(), "");
            }
            return input;
        }

        public static string ReplaceAt(this string input, int index, char newChar)
        {
            char[] chars = input.ToCharArray();
            chars[index] = newChar;
            return new String(chars);
        }

        public static string ReplaceFirst(this string input, string match, string replace, bool caseSensitive = true)
        {
            int startPosition;
            if (caseSensitive)
                startPosition = input.IndexOf(match);
            else
                startPosition = input.IndexOf(match, StringComparison.CurrentCultureIgnoreCase);

            if (startPosition >= 0)
            {
                return input.Substring(0, startPosition) + replace + input.Substring(startPosition + match.Length);
            }
            else
            {
                return input;
            }
        }

        public static string ReplaceLast(this string input, string match, string replace, bool caseSensitive = true)
        {
            int startPosition;
            if (caseSensitive)
                startPosition = input.LastIndexOf(match);
            else
                startPosition = input.LastIndexOf(match, StringComparison.CurrentCultureIgnoreCase);

            if (startPosition >= 0)
            {
                return input.Substring(0, startPosition) + replace + input.Substring(startPosition + match.Length);
            }
            else
            {
                return input;
            }
        }

        public static string Reverse(this string input)
        {
            return new string(input.ToCharArray().Reverse().ToArray());
        }

        public static string RevertXMLString(this string text)
        {
            return text.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Trim();
        }

        public static string SafeURL(this string input, bool stripSlashes = false, bool replaceAmpersands = false)
        {
            if (stripSlashes)
                input = input.Replace('/', '+');

            if (replaceAmpersands)
                input = input.Replace("  ", "```").Replace(" & ", " and ").Replace("  ", " ").Replace("```", "  ").Replace("&", "_and_");

            return input.Replace(' ', '-').Replace('?', '.');
        }

        public static string[] Split(this string toSplit, string splitOnString, StringSplitOptions options = StringSplitOptions.None)
        {
            return toSplit == null ? null : toSplit.Split(new[] { splitOnString }, options);
        }

        /// <summary>
        /// Splits a string into two parts at the last splitChar before maxLength.  Will always return a two-element string[].
        /// </summary>
        /// <param name="toSplit"></param>
        /// <param name="maxLength"></param>
        /// <param name="splitChar"></param>
        /// <param name="includeSplitDivider"></param>
        /// <returns></returns>
        public static string[] SplitAt(this string toSplit, int maxLength, char splitChar, bool includeSplitDivider = true)
        {
            if (maxLength > toSplit.Length) return new string[] { toSplit, String.Empty };

            int splitPoint = toSplit.Substring(0, maxLength).LastIndexOf(splitChar) + 1;
            if (splitPoint == -1) splitPoint = maxLength;
            var firstSplit = toSplit.Substring(0, splitPoint);
            if (!includeSplitDivider) firstSplit = firstSplit.TrimEnd(splitChar);
            return new string[] { firstSplit, toSplit.Substring(splitPoint) };
        }

        /*
        /// <summary>
        /// Splits a string so that each part is no more than maxLength characters long, breaking on the last possible separator.
        /// </summary>
        /// <param name="toSplit"></param>
        /// <param name="separator">Characters to break the string on.</param>
        /// <param name="maxLength">Maximum length on one line</param>
        /// <returns></returns>
        public static string[] SplitLinesByLength(this string toSplit, int maxLength, char[] separator)
        {
            List<string> split = new List<string>();
            while (toSplit.Length > maxLength)
            {
                int splitPoint = toSplit.Substring(0, maxLength).LastIndexOfAny(separator);
                if (splitPoint == -1)
                {
                    split.Add(toSplit.Substring(0, maxLength));
                    toSplit = toSplit.Substring(maxLength + 1);
                }
                else if (splitPoint == 0)
                {
                }
                else
                {
                    split.Add(toSplit.Substring(0, splitPoint));
                    toSplit = toSplit.Substring(splitPoint);
                }
            }
            split.Add(toSplit);
            return split.ToArray();
        }
        /// <summary>
        /// Splits a string so that each part is no more than maxLength characters long, breaking on the last possible separator.
        /// </summary>
        /// <param name="toSplit"></param>
        /// <param name="separator">Characters to break the string on.</param>
        /// <param name="maxLength">Maximum length on one line</param>
        /// <returns></returns>
        public static string[] SplitLinesByLength(this string toSplit, int maxLength, string separator)
        {
            return toSplit.SplitLinesByLength(maxLength, separator.ToCharArray());
        }
        */

        /// <summary>
        /// Splits a string so that each part is no more than maxLength characters long.
        /// </summary>
        /// <param name="toSplit"></param>
        /// <param name="maxLength">Maximum length on one line</param>
        /// <returns></returns>
        public static string[] SplitLinesByLength(this string toSplit, int maxLength)
        {
            List<string> split = new List<string>();
            while (toSplit.Length > maxLength)
            {
                split.Add(toSplit.Substring(0, maxLength));
                toSplit = toSplit.Substring(maxLength);
            }
            split.Add(toSplit);
            return split.ToArray();
        }

        public static string StripHTMLAttributes(this string input, int skip = 1)
        {
            var html = new HtmlDocument();
            html.LoadHtml(input);
            var nodes = html.DocumentNode.SelectNodes("//*");
            if (nodes != null)
            {
                foreach (var node in nodes.Skip(skip/*skip 1 to keep "text-content" outer class*/).Where(x => !x.Name.EqualsIgnoreCase("iframe") && !x.Name.EqualsIgnoreCase("span"))) //the outer div should stay untouched
                {
                    if (node.InnerText?.Length == 0 || node.InnerHtml?.Length == 0)
                    { node.Remove(); continue; }
                    else if (node.Name.Contains(':'))
                    { node.Remove(); continue; }

                    //if we are here the node is still in the document
                    node.Attributes.RemoveAll();
                }
            }

            return html.DocumentNode.OuterHtml;
        }

        public static HtmlNode StripHTMLAttributesReturnHTML(this string input, int skip = 1)
        {
            var html = new HtmlDocument();
            html.LoadHtml(input);
            var nodes = html.DocumentNode.SelectNodes("//*");
            if (nodes != null)
            {
                foreach (var node in nodes.Skip(skip/*skip 1 to keep "text-content" outer class*/).Where(x => !x.Name.EqualsIgnoreCase("iframe"))) //the outer div should stay untouched
                {
                    if (node.InnerText?.Length == 0 || node.InnerHtml?.Length == 0)
                    { node.Remove(); continue; }
                    else if (node.Name.Contains(':'))
                    { node.Remove(); continue; }

                    //if we are here the node is still in the document
                    node.Attributes.RemoveAll();
                }
            }

            return html.DocumentNode;
        }

        /// <summary>
        /// Replaces the appropriate tags with plain-text new line characters and &nbsp; with a space, then removes all remaining HTML.
        /// </summary>
        public static string StripAllHTML(this string input)
        {
            return input.StripHTMLEntities().StripHTMLTags();
        }

        public static string StripHTMLEntities(this string input)
        {
            return Regex.Replace(input.Replace("&nbsp;", " "), "&.*?;", "", RegexOptions.IgnoreCase);
        }
        public static string SanitizeHTMLEntities(this string input)
        {
            return input.Replace("½", "&frac12;").Replace("¼", "&frac14;").Replace("¾", "&frac34;").Replace("·", "&middot; ")
                .Replace("™", "&trade;").Replace("©", "&copy;").Replace("®", "&reg;")
                .Replace("”", "&rdquo;").Replace("“", "&ldquo;").Replace("’", "&rsquo;");
        }

        public static string StripNewLineCharacters(this string input)
        {
            return Regex.Replace(input, @"\n|\r", "");
        }

        public static string SanitizeCreditCardInfo(this string input)
        {
            return SensitiveDataUtilities.Instance.SanitizePattern(input);
        }
        public static string ExtractCreditCardInfo(this string input)
        {
            return SensitiveDataUtilities.Instance.ExtractPattern(input);
        }
        public static bool HasCreditCardInfo(this string input)
        {
            return SensitiveDataUtilities.Instance.MatchPattern(input);
        }
        public static string GetCreditCardMatchTypes(this string input)
        {
            return SensitiveDataUtilities.Instance.PatternType(input);   
        }

        

        /// <summary>
        /// Replaces the appropriate tags with plain-text new line characters before removing all remaining HTML tags.
        /// </summary>
        public static string StripHTMLTags(this string input)
        {
            input = input.Replace("\r", "").Replace("\n", "");
            input = Regex.Replace(input, "</p>", Environment.NewLine + Environment.NewLine, RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"<br\s*/?>", Environment.NewLine, RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"<h[0-6]{1}>", Environment.NewLine, RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"</h[0-6]{1}>", Environment.NewLine + Environment.NewLine, RegexOptions.IgnoreCase);
            input = Regex.Replace(input, "</li>", Environment.NewLine, RegexOptions.IgnoreCase);
            input = Regex.Replace(input, "<.*?>", "", RegexOptions.IgnoreCase);
            return input.Trim();
        }

        public static string StripHTMLFormatting(this string input)
        {
            var result = Regex.Replace(input, "</?font.*?>", string.Empty, RegexOptions.IgnoreCase);
            result = Regex.Replace(result, "style=\".*?\"", string.Empty, RegexOptions.IgnoreCase);
            result = Regex.Replace(result, "<link type=\"text/css\".*?>", string.Empty, RegexOptions.IgnoreCase);
            return result;
        }

        public static string StripHTMLImages(this string input)
        {
            var result = Regex.Replace(input, "<img .*/>", string.Empty, RegexOptions.IgnoreCase);
            return Regex.Replace(result, "<img.*>.*</img>", string.Empty, RegexOptions.IgnoreCase);
        }

        public static string StripHTMLTables(this string input)
        {
            var result = Regex.Replace(input, "</?table.*?>", string.Empty, RegexOptions.IgnoreCase);
            result = Regex.Replace(result, "</?tr.*?>", string.Empty, RegexOptions.IgnoreCase);
            result = Regex.Replace(result, "</?thead.*?>", string.Empty, RegexOptions.IgnoreCase);
            result = Regex.Replace(result, "</?td.*?>", string.Empty, RegexOptions.IgnoreCase);
            result = Regex.Replace(result, "</?tbody.*?>", string.Empty, RegexOptions.IgnoreCase);
            return result;
        }

        public static string StripInvalidFileNameChars(this string input, char[] additionalInvalidCharacters = null, char replacementChar = default(char))
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            if (additionalInvalidCharacters != null)
            {
                regexSearch += new string(additionalInvalidCharacters);
            }
            return new Regex($"[{Regex.Escape(regexSearch)}]").Replace(input, replacementChar == default(char) ? "" : replacementChar.ToString());
        }

        public static string StripInvalidXMLChars(this string input)
        {
            return Regex.Replace(input, @"[\u0000-\u0008,\u000B,\u000C,\u000E-\u001F]", "");
        }

        public static string StripNonNumericCharacters(this string input, string replace = "")
        {
            return Regex.Replace(input, @"[^0-9]", replace);
        }

        public static string StripNonLetterCharacters(this string input, string replace = "")
        {
            return Regex.Replace(input, @"[^a-zA-Z]", replace);
        }

        public static string StripNonAlphaNumericCharacters(this string input, string replace = "")
        {
            return Regex.Replace(input, @"[^a-zA-Z0-9 -]", replace);
        }

        /// <summary>
        /// Reduces multiple consecutive characters down to one.
        /// </summary>
        /// <param name="input">The string to modify.</param>
        /// <param name="charsToReduce">The characters to reduce multiples of. If null, all characters with consecutive multiples will be reduced.</param>
        public static string ReduceMultipleConsecutiveCharacters(this string input, IEnumerable<char> charsToReduce = null)
        {
            return Regex.Replace(input, $@"({(charsToReduce == null ? "." : charsToReduce.Join("|"))})\1+", "$1");
        }

        public static string StripAccentMarks(this string input)
        {
            var normalizedOutputBuilder = new StringBuilder();
            foreach (char c in input.Normalize(NormalizationForm.FormD).Where(x => CharUnicodeInfo.GetUnicodeCategory(x) != UnicodeCategory.NonSpacingMark))
            {
                normalizedOutputBuilder.Append(c);
            }
            return normalizedOutputBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Replaces Unicode characters with their ASCII equivalents when possible.
        /// Strips all other characters or replaces them with the specified replacementChar.
        /// </summary>
        public static string StripUnicode(this string input, char replacementChar = default(char))
        {
            if (input.Any(x => x > 255))
            {
                var normalizedOutputBuilder = new StringBuilder();
                foreach (char c in input)
                {
                    if (c <= 255)
                    {
                        // ASCII character.
                        normalizedOutputBuilder.Append(c);
                    }
                    else
                    {
                        switch (CharUnicodeInfo.GetUnicodeCategory(c))
                        {
                            case UnicodeCategory.SpaceSeparator:
                                // The rough equivilant of ' '.
                                normalizedOutputBuilder.Append(' ');
                                break;
                            case UnicodeCategory.OpenPunctuation:
                                // The rough equivilant of '(', '[', or '{'.
                                normalizedOutputBuilder.Append('(');
                                break;
                            case UnicodeCategory.ClosePunctuation:
                                // The rough equivilant of ')', ']', or '}'.
                                normalizedOutputBuilder.Append(')');
                                break;
                            default:
                                // Unknown character.
                                if (replacementChar != default(char))
                                {
                                    normalizedOutputBuilder.Append(replacementChar);
                                }
                                break;
                        }
                    }
                }
                return normalizedOutputBuilder.ToString();
            }
            return input;
        }

        public static string SubstringFromEnd(this string input, int length)
        {
            var inputLength = input.Length;
            if (inputLength <= length)
            {
                return input;
            }
            else
            {
                return input.Substring(inputLength - length);
            }
        }

        /// <summary>
        /// Returns a substring safely, regardless of string length
        /// </summary>
        /// <param name="input"></param>
        /// <param name="startPosition">Start position for the substring.
        /// If this is greater than length, returns the empty string.
        /// If this is negative, starts at the first character.</param>
        /// <param name="maxLength">Maximum number of characters to return.
        /// If this would go past the end of the string, returns to the end of the string.
        /// If this is negative, returns to the end of the string.</param>
        /// <returns></returns>
        public static string SubstringSafe(this string input, int startPosition, int maxLength)
        {
            if (startPosition < 0) startPosition = 0;

            if (input == null)
            {
                return "";
            }
            else if (startPosition > input.Length)
            {
                return "";
            }
            else if (maxLength < 0)
            {
                return input.Substring(startPosition);
            }
            else if (input.Length > (startPosition + maxLength))
            {
                return input.Substring(startPosition, maxLength);
            }
            else
            {
                return input.Substring(startPosition);
            }
        }

        public static string SubstringSafe(this string input, int startPosition)
        {
            if (startPosition < 0) startPosition = 0;

            if (input == null)
            {
                return "";
            }
            else if (startPosition > input.Length)
            {
                return "";
            }
            else
            {
                return input.Substring(startPosition);
            }
        }
        public static byte[] ToByteArray(this string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }

        public static string ToCSVSafe(this string input)
        {
            return "\"" + input.Replace("\"", "\"\"").Replace("\n", "").Replace("\r", "").Replace("\t", "    ").Replace("\v", "") + "\"";
        }

        public static string ToTitleCase(this string input)
        {
            // We need .ToLower() because .ToTitleCase() won't un-cap letters if the whole word is in caps.
            // Normally, that's good behavior, but because of how we store data, it's not useful here.
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower()).Replace("And", "and");
        }

        public static string ToURLSafe(this string input)
        {
            input = Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(input)); //Remove accents
            input = Regex.Replace(input, @"[^a-zA-Z0-9_.-~:\\\s-]", ""); // Remove all non valid chars
            input = Regex.Replace(input, @"\s+", " ").Trim(); // convert multiple spaces into one space
            input = Regex.Replace(input, @"\s", "-"); // //Replace spaces by dashes
            input = Regex.Replace(input, @"\-+", "-"); // //Replace multiple dashes into one dash
            return input.Replace("'", "''");
        }

        /// <summary>
        /// Utility method to ensure that the string is a valid filename/path.
        /// </summary>
        /// <param name="replacementChar">The character with which to replace invalid characters in the string, if any exist.</param>
        /// <returns>A string containing only characters valid for use in filenames/paths.</returns>
        /// <exception cref="ArgumentException">Thrown if replacementChar is an invalid filename/path character.</exception>
        public static string ToValidFilename(this string s, char replacementChar = '_')
        {
            return Path.GetInvalidFileNameChars().Aggregate(s, (current, c) => current.Replace(c, replacementChar));
        }

        public static string ToValidXsdNamespace(this string s)
        {
            return s.ToValidFilename().Replace(' ', '_').Replace(".", string.Empty);
        }

        public static string ToXMLString(this string text)
        {
            return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").StripInvalidXMLChars().Trim();
        }

        public static string UnsafeURL(this string input, bool unstripSlashes = false, bool unreplaceAmpersands = false)
        {
            if (unreplaceAmpersands)
                input = input.Replace("-and-", " & ").Replace("-AND-", " & ").Replace("-And-", " & ").Replace("_And_", "&").Replace("_and_", "&").Replace("_AND_", "&");
            if (unstripSlashes)
                input = input.Replace('+', '/');
            return input.Replace("---", "~~~").Replace('-', ' ').Replace("~~~", "-");
        }

        public static string URLEncode(this string input)
        {
            if (input == null) { return string.Empty; }
            return Uri.EscapeUriString(input);
        }
        
        public static bool IsDigits(this string input)
        {
            return input.All(x => Char.IsDigit(x));
        }

        public static bool ContainsDigit(this string input)
        {
            return input.Any(x => Char.IsDigit(x));
        }

        public static bool IsHTML(this string input)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(input);
            return doc.DocumentNode.Descendants().Any(x => x.NodeType != HtmlNodeType.Text);
        }

        public static string ToPlainText(this string input)
        {
            if (input == null) { return string.Empty; }
            try
            {
                return (input.IsRTF()) ? new RichTextBox { Rtf = input }.Text : input;
            }
            catch
            {
                return input;
            }
        }

        public static string ToRichText(this string input)
        {
            if (input == null) { return string.Empty; }
            try
            {
                return (input.IsRTF()) ? new RichTextBox { Rtf = input }.Rtf : new RichTextBox { Text = input }.Rtf;
            }
            catch
            {
                return new RichTextBox { Text = input }.Rtf;
            }
        }

        public static bool IsRTF(this string input)
        {
            return input?.Length > 6 && input.StartsWith(@"{\rtf", StringComparison.Ordinal);
        }

        public static string TrimTrailingNewLinesRTF(this string input)
        {
            if (input.IsRTF())
            {
                while (input.Length > 2 && input.SubstringFromEnd(2) == "\r\n")
                    input = input.Substring(0, input.Length - 2);

                while (input.Length > 7 && input.SubstringFromEnd(7) == "\\par\r\n}")
                    input = input.Substring(0, input.Length - 7) + "}";
            }

            return input;
        }

        public static string AppendStringToRTF(this string strRTF, string strToAppend, bool blnIncludeNewLine)
        {
            if (strRTF.IsRTF())
            {
                var rtfTarget = new RichTextBox()
                {
                    Rtf = strRTF
                };

                if (blnIncludeNewLine)
                {
                    rtfTarget.Select(rtfTarget.TextLength, 0);
                    rtfTarget.AppendText(Environment.NewLine);
                }

                rtfTarget.Select(rtfTarget.TextLength, 0);

                if (strToAppend.IsRTF())
                {
                    var rtfSource = new RichTextBox()
                    {
                        Rtf = strToAppend
                    };
                    rtfTarget.SelectedRtf = rtfSource.Rtf;
                }
                else
                {
                    rtfTarget.SelectedText = strToAppend;
                }

                return rtfTarget.Rtf;
            }

            return strRTF;
        }

        public static bool Contains(this string input, string test, bool ignoreCase)
        {
            return ignoreCase ? input.IndexOf(test, StringComparison.OrdinalIgnoreCase) > -1 : input.Contains(test);
        }

        public static bool Contains(this string input, char test)
        {
            return input.IndexOf(test) > -1;
        }

        public static bool EqualsIgnoreCase(this string input, string test)
        {
            return input.Equals(test, StringComparison.OrdinalIgnoreCase);
        }

        public static bool SubstringEquals(this string input, int startIndex, int length, string test)
        {
            return string.CompareOrdinal(input, startIndex, test, 0, length) == 0;
        }

        public static string ToXMLSafe(this string StrInput)
        {
            // Retrieved from https://stackoverflow.com/questions/29301248/hexadecimal-value-0x0b-is-an-invalid-character-issue-in-xml
            //Returns same value if the value is empty.
            if (string.IsNullOrWhiteSpace(StrInput))
            {
                return StrInput;
            }
            // From xml spec valid chars:
            // #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]    
            // any Unicode character, excluding the surrogate blocks, FFFE, and FFFF.
            const string RegularExp = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
            return Regex.Replace(StrInput, RegularExp, String.Empty);
        }

        public static string ToBase64String(this string input)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
        }

        public static string DecodeBase64String(this string input)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(input));
        }

        public static string Center(this string str, int length)
        {
            return str.PadLeft(((length - str.Length) / 2) + str.Length).PadRight(length);
        }
    }
}
