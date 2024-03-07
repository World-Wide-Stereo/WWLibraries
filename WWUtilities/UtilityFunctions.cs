using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Spire.Pdf;
using ww.Utilities.Extensions;

namespace ww.Utilities
{
    [DebuggerStepThrough]
    public static class UtilityFunctions
    {
        public static ENUM EnumMin<ENUM>(ENUM a, ENUM b) where ENUM : struct, IComparable, IConvertible // i.e. an enum
        {
            if (Convert.ToInt32(a) < Convert.ToInt32(b))
                return a;
            else
                return b;
        }

        public static ENUM? EnumMin<ENUM>(ENUM? a, ENUM? b) where ENUM : struct, IComparable, IConvertible // i.e. an enum
        {
            if (b == null || Convert.ToInt32(a) < Convert.ToInt32(b))
                return a;
            else
                return b;
        }

        public static ENUM EnumMax<ENUM>(ENUM a, ENUM b) where ENUM : struct, IComparable, IConvertible // i.e. an enum
        {
            if (Convert.ToInt32(a) > Convert.ToInt32(b))
                return a;
            else
                return b;
        }

        public static ENUM? EnumMax<ENUM>(ENUM? a, ENUM? b) where ENUM : struct, IComparable, IConvertible // i.e. an enum
        {
            if (b == null || Convert.ToInt32(a) > Convert.ToInt32(b))
                return a;
            else
                return b;
        }

        public static bool ValidateCreditCard(string cardNumber)
        {
            bool isOdd = true;
            int total = 0;
            foreach (char digit in cardNumber.Replace(" ", "").Replace("-", "").Reverse())
            {
                if (isOdd)
                {
                    total += digit.ToString().ToInt();
                }
                else
                {
                    int temp = digit.ToString().ToInt() * 2;
                    if (temp > 9) temp -= 9;
                    total += temp;
                }
                isOdd = !isOdd;
            }
            return total % 10 == 0;
        }

        public static IEnumerable<int> Sequence(int low, int high, int countBy = 1)
        {
            for (int x = low; x <= high; x += countBy)
            {
                yield return x;
            }
        }

        public static string GetSafeFilename(string filename, char[] additionalInvalidCharacters = null, char replacementChar = default(char))
        {
            string replacementString = replacementChar == default(char) ? "" : replacementChar.ToString();
            IEnumerable<char> invalidCharacters = Path.GetInvalidFileNameChars();
            if (additionalInvalidCharacters != null)
            {
                invalidCharacters = invalidCharacters.Concat(additionalInvalidCharacters);
            }
            foreach (var c in invalidCharacters)
            {
                filename = filename.Replace(c.ToString(), replacementString);
            }
            return filename;
        }

        public static bool IsValidEmail(string email)
        {
            // This regex is used to validate email addresses in MVC4 models https://stackoverflow.com/a/17712290
            if (!email.IsNullOrBlank())
            {
                var emailRegex = new Regex("^((([a-z]|\\d|[!#\\$%&'\\*\\+\\-\\/=\\?\\^_`{\\|}~]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])+(\\.([a-z]|\\d|[!#\\$%&'\\*\\+\\-\\/=\\?\\^_`{\\|}~]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])+)*)|((\\x22)((((\\x20|\\x09)*(\\x0d\\x0a))?(\\x20|\\x09)+)?(([\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x7f]|\\x21|[\\x23-\\x5b]|[\\x5d-\\x7e]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])|(\\\\([\\x01-\\x09\\x0b\\x0c\\x0d-\\x7f]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF]))))*(((\\x20|\\x09)*(\\x0d\\x0a))?(\\x20|\\x09)+)?(\\x22)))@((([a-z]|\\d|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])|(([a-z]|\\d|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])([a-z]|\\d|-|\\.|_|~|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])*([a-z]|\\d|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])))\\.)+(([a-z]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])|(([a-z]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])([a-z]|\\d|-|\\.|_|~|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])*([a-z]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])))\\.?$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
                return emailRegex.IsMatch(email);
            }
            return false;
        }

        public static decimal CalculateMarginFromPrice(decimal price, decimal unitCost, decimal conversionFactor)
        {
            if (price == 0) return 0;
            return Math.Round((price / conversionFactor - unitCost) / (price / conversionFactor) * 100, 4);
        }

        public static decimal CalculatePriceFromMargin(decimal margin, decimal unitCost, decimal conversionFactor)
        {
            return Math.Round(Math.Round(100 * unitCost / (100 - margin) * conversionFactor, 4), 2);
        }

        /// <summary>
        /// Return the percentage changed from the first parameter to the second.
        /// The result will be negative if it is a decline, positive if it is an increase.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static decimal PercentageChange(decimal from, decimal to)
        {
            if (from == 0 && to == 0) return 0;
            if (to == 0) throw new ArgumentOutOfRangeException("to", "Change to 0: -Infinity");

            return ((to - from) / to);
        }

        public static string PercentageChangeAsString(decimal from, decimal to)
        {
            if (from == 0 && to == 0) return 0.ToString("p");
            if (to == 0) return "-Inf";
            return ((to - from) / to).ToString("p");
        }

        public static bool IsOverlappingDateRange(DateTime? startDate1, DateTime? endDate1, DateTime? startDate2, DateTime? endDate2)
        {
            return IsOverlappingDateRange(startDate1 ?? DateTime.MinValue,
                endDate1 ?? DateTime.MaxValue,
                startDate2 ?? DateTime.MinValue,
                endDate2 ?? DateTime.MaxValue);
        }

        public static bool IsOverlappingDateRange(DateTime startDate1, DateTime endDate1, DateTime startDate2, DateTime endDate2)
        {
            return (startDate1 <= endDate2) && (startDate2 <= endDate1);
        }

        public static DateTime DateTimeMin(DateTime one, DateTime two)
        {
            return one < two ? one : two;
        }

        public static DateTime DateTimeMax(DateTime one, DateTime two)
        {
            return one > two ? one : two;
        }

        /// <summary>
        /// Extracts the contents of a &lt;title&gt; attribute.  
        /// Returns <code>null</code> if there is none.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string ExtractTitle(string html)
        {
            if (html == null) return null;
            var match = Regex.Match(html, @"<title>(.*?)</title>");
            if (!match.Success || match.Groups.Count < 2) return null;
            return match.Groups[1].Value;
        }

        /// <summary>
        /// Compute the distance between two strings.
        /// </summary>
        /// <remarks>From https://www.dotnetperls.com/levenshtein</remarks>
        public static int LevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

        public static string ExecutableLocation
        {
            get { return Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) + "\\"; }
        }

        public static string GenerateRandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * RandomThreadSafe.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        public static string GetUnusedFullPathForFile(string fullPath)
        {
            int count = 2;
            string fileNameOnly = Path.GetFileNameWithoutExtension(fullPath);
            string extension = Path.GetExtension(fullPath);
            string path = Path.GetDirectoryName(fullPath);
            string newFullPath = fullPath;

            while (File.Exists(newFullPath))
            {
                string tempFileName = $"{fileNameOnly}({count++})";
                newFullPath = Path.Combine(path, tempFileName + extension);
            }

            return newFullPath;
        }

        public static string GetPublicIP(string proxyIP = null, int proxyPort = 0, int numTries = 4, int numSecBtwTries = 1, int timeoutInSec = 5)
        {
            string[] urls = {"http://checkip.dyndns.org/", "http://canihazip.com/s"};
            int urlToUseIndex = 0;
            int tryCounter = 1;
            do
            {
                try
                {
                    string pageAsString = HTTP.Get(urls[urlToUseIndex], timeout: 1000 * timeoutInSec, proxy: proxyIP != null && proxyPort != 0 ? new WebProxy(proxyIP, proxyPort) : null);
                    return (new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")).Matches(pageAsString)[0].ToString();
                }
                catch (WebException)
                {
                    if (tryCounter >= numTries) throw;
                    tryCounter++;
                    urlToUseIndex = urlToUseIndex + 1 < urls.Length ? urlToUseIndex + 1 : 0; //Go to next URL or loop back to the first
                    Thread.Sleep(1000 * numSecBtwTries);
                }
            } while (true); // Returns on success
        }

        public static void SendTextAlerts(TextAlertTo to, string subject, string body)
        {
            SendTextAlerts(new[] { to }, subject, body);
        }
        public static void SendTextAlerts(IEnumerable<TextAlertTo> to, string subject, string body)
        {
            var emailTo = new StringBuilder();
            foreach (TextAlertTo recipient in to)
            {
                emailTo.Append(recipient.GetLabel());
                emailTo.Append("; ");
            }
            emailTo.Length -= 2;

            Email.sendEmail(emailTo.ToString(), subject, body);
        }

        public static string GetShippingURL(string carrier, string trackingNumber)
        {
            switch (carrier.StripNonLetterCharacters().ToUpper())
            {
                case "UPS":
                case "UPSGROUND":
                case "UPSSUREPOST":
                    return $"https://wwwapps.ups.com/WebTracking/track?track=yes&trackNums={trackingNumber}";
                case "FEDEX":
                case "FEDEXGROUND":
                case "FEDEXSMARTPOST":
                case "FEDEXFREIGHT":
                case "FDEG":
                    return $"https://www.fedex.com/fedextrack/?tracknumbers={trackingNumber}";
                case "USPS":
                    return $"https://tools.usps.com/go/TrackConfirmAction_input?qtc_tLabels1={trackingNumber}";
                case "AMAZON":
                case "AMAZONLOGISTICS":
                    return $"https://track.amazon.com/tracking/{trackingNumber}";
                case "SBA":
                    return "https://www.sbaglobal.com/SBA/ShipmentTrackingPage.aspx";
                case "PILOT":
                    return "https://www.pilotdelivers.com/tracking/";
                case "HOMEDIRECT":
                    return "https://www.mxdgroup.com";
                case "CEVA":
                    return "https://www.cevalogistics.com/ceva-trak";
                case "LASERSHIP":
                    return $"https://www.trackingex.com/lasership-tracking/{trackingNumber}.html";
                case "WANNERS":
                    return "http://www.wannerstransport.e-courier.com/wannerstransport/home/Wizard_tracking.asp?UserGUID=";
                case "PRESTIGE":
                    return "http://www.prestigedelivery.com/trackpackage.aspx";
                case "ONTRAC":
                    return $"https://www.ontrac.com/trackingres.asp?tracking_number={trackingNumber}";
                case "ESTES":
                case "ESTESEXPRESS":
                    return "https://www.estes-express.com/WebApp/ShipmentTracking/";
                case "ABF":
                    return "https://arcb.com/tools/tracking.html";
                case "CNWY":
                case "CONWAY":
                case "CON-WAY":
                case "XPO":
                    return $"https://app.ltl.xpo.com/appjs/tracking/multi-results?proNumbers={trackingNumber}";
                case "NEMF":
                case "NEWENGLANDMOTORFREIGHT":
                    return $"http://nemfweb.nemf.com/shptrack.nsf/request?openagent=1&pro={trackingNumber}&submit=Track";
                case "R&LCARRIERS":
                case "RLCARRIERS":
                    return "https://www2.rlcarriers.com/shipment-tracing.aspx";
                case "AMHOMEDELIVERY":
                    return "https://www.amtrucking.com/tracking.php";
                case "SAIA":
                    return $"https://www.saia.com/track/details;pro={trackingNumber}";
                case "SEKO":
                case "SEKOLOGISTICS":
                    return $"https://harmony.myseko.com/Track/Result/{trackingNumber}";
                default:
                    return "";
            }
        }

        public static string GetCarrierPhoneNumber(string carrier)
        {
            switch (carrier)
            {
                case "SBA":
                    return "SBA Global's customer service number is 610.534.7030.";
                case "Wanners":
                    return "Wanner's Transport & Delivery's customer service number is 215.672.8060.";
                case "FedExFreight":
                    return "FedEx freight's customer service number is 1.800.463.3339.";
                case "R&L Carriers":
                    return "R&L Carriers customer service number is 1.800.543.5589.";
                case "AM HOME DELIVERY":
                case "AMHomeDelivery":
                    return "AM Trucking's customer service number is 718-272-5900";
                case "UPSFreight":
                    return "UPS freight's customer service number is 800.333.7400.";
                default:
                    return "";
            }
        }

        /// <summary>
        /// Returns true when all simple type public fields and properties of the inputted objects are equal.
        /// Skips comparison of complex type fields and properties for the purpose of avoiding circular references leading to a StackOverflowException.
        /// </summary>
        /// <param name="includePrivateMembers">Compares private members in addition to public members.</param>
        /// <param name="membersToIgnore">Skips comparison of members with these names.</param>
        /// <param name="mustHaveAttributes">Compares only members with at least one of the specified attributes.</param>
        public static bool AreMembersEqual<T>(T a, T b, bool treatNullAndWhitespaceStringsAsEqual = false, bool includePrivateMembers = false, List<string> membersToIgnore = null, List<Type> mustHaveAttributes = null) where T : class
        {
            if (a != null && b != null)
            {
                Type type = typeof(T);
                if (membersToIgnore == null) membersToIgnore = new List<string>();
                if (mustHaveAttributes == null) mustHaveAttributes = new List<Type>();
                if ((includePrivateMembers ? type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) : type.GetFields())
                    .Where(x => !membersToIgnore.Contains(x.Name) && (mustHaveAttributes.Count == 0 || mustHaveAttributes.Any(y => Attribute.IsDefined(x, y))))
                    .Any(x => !AreValuesEqual(x.GetValue(a), x.GetValue(b), treatNullAndWhitespaceStringsAsEqual)))
                {
                    return false;
                }
                if ((includePrivateMembers ? type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) : type.GetProperties())
                    .Where(x => !membersToIgnore.Contains(x.Name) && (mustHaveAttributes.Count == 0 || mustHaveAttributes.Any(y => Attribute.IsDefined(x, y))))
                    .Any(x => !AreValuesEqual(x.GetValue(a, null), x.GetValue(b, null), treatNullAndWhitespaceStringsAsEqual)))
                {
                    return false;
                }
                return true;
            }
            return a == b;
        }

        /// <summary>
        /// Returns true when both values are of the same simple type and equal.
        /// Always returns true for complex types in order to avoid circular references leading to a StackOverflowException.
        /// </summary>
        public static bool AreValuesEqual(object aValue, object bValue, bool treatNullAndWhitespaceStringsAsEqual = false)
        {
            if (aValue != bValue && (aValue == null || !aValue.Equals(bValue)))
            {
                if (aValue != null && !aValue.IsSimpleType())
                {
                    return true;
                }

                if (treatNullAndWhitespaceStringsAsEqual)
                {
                    // The types of both values must be checked because "null is string" will return false.
                    if (aValue is string || bValue is string)
                    {
                        if (!String.IsNullOrWhiteSpace((string) aValue) || !String.IsNullOrWhiteSpace((string) bValue))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsValidUPC(string upc)
        {
            try
            {
                return Regex.IsMatch(upc, "^[0-9]+$");
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void GetCityStateZipFromAddressLine(string addressLine, out string city, out string state, out string zip)
        {
            city = "";
            state = "";
            zip = "";

            int indexOfComma = addressLine.IndexOf(",");
            if (indexOfComma > -1 && addressLine.Length > 4) // Ensure there is enough text to grab something for the state.
            {
                city = addressLine.Substring(0, indexOfComma);
                state = addressLine.Substring(indexOfComma + 2, 2);
                int indexOfSpaceBeforeZipCode = addressLine.IndexOf(" ", indexOfComma + 4);
                if (indexOfSpaceBeforeZipCode > -1) zip = addressLine.Substring(indexOfSpaceBeforeZipCode + 1).ToTrimmedString().TrimStart(' ');
            }
        }

        /// <summary>
        /// Runs code utilizing a temp directory. Eliminates the need to manage creation and deletion of the directory.
        /// </summary>
        /// <param name="action">The action's parameter is the string that holds the temp directory's path.</param>
        public static void RunTempDirectoryAction(Action<string> action)
        {
            string tempDirectory = GetTempDirectory();

            try
            {
                action(tempDirectory);
            }
            catch
            {
                DeleteTempDirectory(tempDirectory);
                throw;
            }

            DeleteTempDirectory(tempDirectory);
        }
        /// <summary>
        /// Runs code utilizing a temp directory. Eliminates the need to manage creation and deletion of the directory.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="func">The func's parameter is the string that holds the temp directory's path.</param>
        public static T RunTempDirectoryFunc<T>(Func<string, T> func)
        {
            string tempDirectory = GetTempDirectory();

            T returnVal;
            try
            {
                returnVal = func(tempDirectory);
            }
            catch
            {
                DeleteTempDirectory(tempDirectory);
                throw;
            }

            DeleteTempDirectory(tempDirectory);
            return returnVal;
        }

        /// <summary>
        /// Returns the full path for a new directory in the current user's temp directory.<para/>
        /// <see cref="DeleteTempDirectory"/> should be called to delete the directory after all work involving it is complete.
        /// </summary>
        public static string GetTempDirectory(bool createDirectory = true)
        {
            string tempDirectory;
            do
            {
                tempDirectory = Path.GetTempPath() + Path.GetRandomFileName() + "\\";
            } while (Directory.Exists(tempDirectory));
            if (createDirectory)
            {
                Directory.CreateDirectory(tempDirectory);
            }
            return tempDirectory;
        }

        /// <summary>
        /// Deletes the specified temp directory and all subdirectories and files.
        /// </summary>
        public static void DeleteTempDirectory(string tempDirectory)
        {
            bool throwAway = false;
            DeleteTempDirectory(new DirectoryInfo(tempDirectory), ref throwAway);
        }
        /// <summary>
        /// Deletes the specified temp directory and all subdirectories and files.
        /// </summary>
        public static void DeleteTempDirectory(string tempDirectory, ref bool getNewTempDirectory)
        {
            DeleteTempDirectory(new DirectoryInfo(tempDirectory), ref getNewTempDirectory);
        }
        private static void DeleteTempDirectory(DirectoryInfo tempDirectory, ref bool getNewTempDirectory)
        {
            if (!tempDirectory.Exists)
            {
                return;
            }

            foreach (DirectoryInfo subdirectory in tempDirectory.EnumerateDirectories())
            {
                DeleteTempDirectory(subdirectory, ref getNewTempDirectory);
            }

            try
            {
                foreach (string file in tempDirectory.EnumerateFiles().Select(x => x.FullName))
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
                Directory.Delete(tempDirectory.FullName);
            }
            catch
            {
                getNewTempDirectory = true;
            }
        }

        /// <summary>
        /// Improves upon Directory.Delete() in several ways:
        /// 1) No exception is thrown when the directory does not exist.
        /// 2) File attributes are cleared before deletion of the files are attempted. Without this, readonly files would not be deleted.
        /// 3) If Windows Explorer has a handle to the directory, we retry the deletion to give it enough time to close the handle.
        /// </summary>
        public static void DeleteDirectory(string directory, bool recursive = false)
        {
            if (!Directory.Exists(directory))
            {
                return;
            }

            if (recursive)
            {
                foreach (string subdirectory in Directory.EnumerateDirectories(directory))
                {
                    DeleteDirectory(subdirectory, recursive: true);
                }
            }

            foreach (string file in Directory.EnumerateFiles(directory))
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            // When a folder is open in Windows Explorer and we try to delete it, Windows Explorer will slowly release
            // its handle on the folder, causing an exception in our code. Retrying ensures that it has enough time to
            // fully release the handle.
            int tryCounter = 0;
            do
            {
                try
                {
                    Directory.Delete(directory);
                }
                catch
                {
                    tryCounter++;
                    if (tryCounter > 3)
                    {
                        throw;
                    }
                    Thread.Sleep(500);
                }
            } while (Directory.Exists(directory));
        }

        public struct PersonName
        {
            public string First;
            public string Middle;
            public string Last;
        }

        public static PersonName ParsePersonName(string name)
        {
            var output = new PersonName();
            name = name.Replace(".", "").Trim().ToUpper();
            int firstSpaceIndex = name.IndexOf(' ');
            if (firstSpaceIndex != -1)
            {
                output.First = name.Substring(0, firstSpaceIndex);
                name = name.Substring(firstSpaceIndex).Trim();
                if (name.IndexOf(' ') == 1 && name.Length > 2)
                {
                    output.Middle = name.Substring(0, 1);
                    output.Last = name.Substring(2);
                }
                else
                {
                    output.Middle = "";
                    output.Last = name;
                }
            }
            else
            {
                output.First = name;
                output.Middle = "";
                output.Last = name;
            }
            return output;
        }

        public struct PhoneNumber
        {
            public string AreaCode;
            public string Prefix;
            public string LineNumber;
            public string Extension;

            public override string ToString()
            {
                return $"{AreaCode}-{Prefix}-{LineNumber}{(!Extension.IsNullOrBlank() ? $" ext. {Extension}" : string.Empty)}";
            }
        }

        public static PhoneNumber ParsePhoneNumber(string phoneNumber, bool parseAsMuchAsPossible = false)
        {
            return ParsePhoneNumber(phoneNumber, out _, parseAsMuchAsPossible: parseAsMuchAsPossible);
        }
        public static PhoneNumber ParsePhoneNumber(string phoneNumber, out bool isValid, bool parseAsMuchAsPossible = false)
        {
            var output = new PhoneNumber();

            int indexOfExt = phoneNumber.LastIndexOf("x", StringComparison.OrdinalIgnoreCase);
            if (indexOfExt != -1)
            {
                output.Extension = phoneNumber.Substring(indexOfExt).GetDigitsInStringAsString();
                phoneNumber = phoneNumber.Substring(0, indexOfExt);
            }
            else
            {
                output.Extension = "";
            }

            phoneNumber = phoneNumber.Replace("+1", "").GetDigitsInStringAsString();
            if (phoneNumber.Length == 11 && phoneNumber[0] == '1')
            {
                phoneNumber = phoneNumber.Substring(1);
            }

            if (phoneNumber.Length == 10)
            {
                isValid = true;
                output.AreaCode = phoneNumber.Substring(0, 3);
                output.Prefix = phoneNumber.Substring(3, 3);
                output.LineNumber = phoneNumber.Substring(6, 4);
            }
            else if (parseAsMuchAsPossible)
            {
                isValid = false;
                if (phoneNumber.Length > 10)
                {
                    output.AreaCode = phoneNumber.Substring(0, 3);
                    output.Prefix = phoneNumber.Substring(3, 3);
                    output.LineNumber = phoneNumber.Substring(6, 4);
                }
                else if (phoneNumber.Length > 6)
                {
                    output.AreaCode = phoneNumber.Substring(0, 3);
                    output.Prefix = phoneNumber.Substring(3, 3);
                    output.LineNumber = phoneNumber.Substring(6);
                }
                else if (phoneNumber.Length > 3)
                {
                    output.AreaCode = phoneNumber.Substring(0, 3);
                    output.Prefix = phoneNumber.Substring(3);
                    output.LineNumber = "";
                }
                else if (phoneNumber.Length > 0)
                {
                    output.AreaCode = phoneNumber.Substring(0);
                    output.Prefix = "";
                    output.LineNumber = "";
                }
                else
                {
                    output.AreaCode = "";
                    output.Prefix = "";
                    output.LineNumber = "";
                }
            }
            else
            {
                isValid = false;
                output.AreaCode = "000";
                output.Prefix = "000";
                output.LineNumber = "0000";
                output.Extension = "";
            }

            return output;
        }

        public static string FormatPhoneNumber(string phoneNumber, bool useParentheses = false, bool returnOnlyRecognizedFormat = false)
        {
            if (phoneNumber == null) return null;
            if (phoneNumber.IsNullOrBlank()) return "";

            int indexOfExt = phoneNumber.LastIndexOf("x", StringComparison.OrdinalIgnoreCase);
            long extension = 0;
            if (indexOfExt != -1)
            {
                extension = phoneNumber.Substring(indexOfExt).GetDigitsInString();
                phoneNumber = phoneNumber.Substring(0, indexOfExt);
            }

            long phoneNumberDigitsOnly = phoneNumber.GetDigitsInString();
            string returnStr = phoneNumberDigitsOnly.ToString();
            int phoneNumberDigitsOnlyLength = returnStr.Length;

            if (phoneNumberDigitsOnly != 0 && (phoneNumberDigitsOnlyLength == 10 || (phoneNumberDigitsOnlyLength == 11 && returnStr.Substring(0, 1) == "1")))
            {
                // US and Canada
                returnStr = String.Format(phoneNumberDigitsOnlyLength == 10 ? (useParentheses ? "{0:(###) ###-####}" : "{0:###-###-####}") : (useParentheses ? "{0:+# (###) ###-####}" : "{0:+#-###-###-####}"), phoneNumberDigitsOnly);
                if (indexOfExt != -1)
                {
                    returnStr += " ext. " + extension;
                }
            }
            else if (phoneNumberDigitsOnly == 0 || returnOnlyRecognizedFormat)
            {
                returnStr = "";
            }

            return returnStr;
        }

        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            return UtilityFunctions.FormatPhoneNumber(phoneNumber, returnOnlyRecognizedFormat: true).Length > 0;
        }

        // Given the first x digits of an integer that's meant to be of length y, this function returns the range
        // in which the final integer could be. Used for searching for partial phone numbers to avoid casting to string.
        public static void getRangeForPartialNumber(string partialNumber, int wholeNumberLength, out int lowerBound, out int upperBound)
        {
            lowerBound = upperBound = 0;

            int partialNumberInt = int.Parse(partialNumber);
            if (partialNumberInt < 0) return;
            
            if (partialNumber.Length > wholeNumberLength) return;

            lowerBound = partialNumberInt * (int)Math.Pow(10, wholeNumberLength - partialNumber.Length);
            upperBound = (lowerBound + (int)Math.Pow(10, wholeNumberLength - partialNumber.Length)) - 1;
        }

        /// <summary>
        /// Adds a dash to the zip code when greater than 5 digits. Only allows numeric input and a dash after the 5th character.
        /// </summary>
        public static string FormatZipCode(string zipCode, out bool isValid)
        {
            string zipCodeDigitsOnlyAsString = zipCode.GetDigitsInStringAsString();
            int zipCodeDigitsOnlyLength = zipCodeDigitsOnlyAsString.Length;
            isValid = zipCodeDigitsOnlyLength == 9 || (zipCodeDigitsOnlyLength == 5 && (zipCode.Length == 5 || (zipCode.Length > 5 && zipCode.Substring(5, 1) != "-")));
            return zipCodeDigitsOnlyLength > 5 || (zipCodeDigitsOnlyLength == 5 && zipCode.Length > 5 && zipCode.Substring(5, 1) == "-") ? zipCodeDigitsOnlyAsString.Substring(0, 5) + "-" + zipCodeDigitsOnlyAsString.Substring(5) : zipCodeDigitsOnlyAsString;
        }

        // Based on information from here: https://en.wikipedia.org/wiki/Postal_codes_in_Canada#Components_of_a_postal_code
        public static string FormatZipCodeCanada(string zipCode, out bool isValid)
        {
            isValid = false;
            int length = zipCode.Length;


            // Check the min length.
            if (length <= 0) return zipCode;

            // Insert the space when needed, even if one of the early characters is invalid.
            if (length <= 3) return zipCode;
            if (zipCode[3] != ' ')
            {
                zipCode = zipCode.Insert(3, " ");
                length++;
            }

            // Check the max length.
            if (length > 7)
            {
                zipCode = zipCode.Substring(0, 7);
                length = 7;
            }

            char[] invalidLetterChars = {'D', 'F', 'I', 'O', 'Q', 'U'};


            if (!Char.IsLetter(zipCode[0]) || zipCode[0].EqualsAnyOf(invalidLetterChars) || zipCode[0].EqualsAnyOf('W', 'Z'))
            {
                return zipCode;
            }

            if (length <= 1) return zipCode;
            if (!Char.IsDigit(zipCode[1]))
            {
                return zipCode;
            }

            if (length <= 2) return zipCode;
            if (!Char.IsLetter(zipCode[2]) || zipCode[2].EqualsAnyOf(invalidLetterChars))
            {
                return zipCode;
            }

            if (length <= 4) return zipCode;
            if (!Char.IsDigit(zipCode[4]))
            {
                return zipCode;
            }

            if (length <= 5) return zipCode;
            if (!Char.IsLetter(zipCode[5]) || zipCode[5].EqualsAnyOf(invalidLetterChars))
            {
                return zipCode;
            }

            if (length <= 6) return zipCode;
            if (!Char.IsDigit(zipCode[6]))
            {
                return zipCode;
            }

            isValid = true;
            return zipCode;
        }

        public static void PrintSFPLabel(string strLabelName, string strLabelPrinter, bool isWirelessPrinter)
        {
            if (strLabelName.EndsWith(".zpl"))
            {
                RawPrinterHelper.SendFileToPrinter(strLabelPrinter, strLabelName);
            }
            else if (strLabelName.EndsWith(".pdf"))
            {
                //uses default printer
                var pdf = new Spire.Pdf.PdfDocument();
                pdf.LoadFromFile(strLabelName);
                var newpdf = new Spire.Pdf.PdfDocument();
                foreach (PdfPageBase page in pdf.Pages)
                {
                    PdfPageBase newPage = newpdf.Pages.Add(page.ActualSize, new Spire.Pdf.Graphics.PdfMargins(0));
                    newPage.Canvas.ScaleTransform((float) .9, (float) .9);
                    if (isWirelessPrinter)
                        newPage.Canvas.DrawTemplate(page.CreateTemplate(), new PointF(0, -150));
                    else
                        newPage.Canvas.DrawTemplate(page.CreateTemplate(), new PointF(0, 100));
                }
                newpdf.CustomHandleLandscape = true;
                newpdf.PrintDocument.DefaultPageSettings.Landscape = true;
                newpdf.PageScaling = PdfPrintPageScaling.ActualSize;
                newpdf.PrintDocument.Print();
            }
            else
            {
                ExecuteCommand("mspaint /p \"" + strLabelName + "\" /pt \"" + strLabelPrinter + "\"");
            }
        }

        public static int ExecuteCommand(string command, bool visible = false)
        {
            int exitCode = 0;
            ProcessStartInfo startinfo;
            Process process = null;
            OperatingSystem os;
            string stdoutline;
            StreamReader stdoutreader;

            Console.WriteLine("Executing: {0}", command);
            try
            {
                //check windows version
                os = Environment.OSVersion;
                if (os.Platform != PlatformID.Win32NT)
                {
                    throw new PlatformNotSupportedException(
                        "Supported on Windows NT or later only"
                    );
                }

                //check arguments
                if (command == null || command.Trim().Length == 0)
                {
                    throw new ArgumentNullException(
                        "command",
                        "the command cannot be null"
                    );
                }

                startinfo = new ProcessStartInfo();

                // use command prompt
                startinfo.FileName = Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\cmd.exe";
                //MessageBox.Show(startinfo.FileName);

                // /c switch sends a command
                startinfo.Arguments = "/C " + command;

                // don't exec with shellexecute api
                startinfo.UseShellExecute = false;

                // redirect stdout to this program
                startinfo.RedirectStandardOutput = true;

                // don't open a cmd prompt window
                startinfo.CreateNoWindow = !visible;

                // start cmd prompt, execute command
                process = Process.Start(startinfo);

                // retrieve stdout line by line
                stdoutreader = process.StandardOutput;
                //StringBuilder errormessage = new StringBuilder("");
                //errormessage.Append( "** " + target.Name + " FAILED\n* executing: " + command +"\n" );
                while ((stdoutline = stdoutreader.ReadLine()) != null)
                {
                    Console.WriteLine("> " + stdoutline);
                    //errormessage.Append( "> " + stdoutline.ToString() + "\n" );
                }
                stdoutreader.Close();

                /*if ( process.ExitCode != 0 )
                    {
                        exitCode = process.ExitCode;
                        _localerrors++;
                        _errors++;

                        StringBuilder exitCodeString = new StringBuilder(""); 
                        exitCodeString.Append("** Bldmod " + target.Name + " failed with Exit Code = " + process.ExitCode + "\n" );
                         
                        Log.WriteLine( exitCodeString.ToString() );
                        errormessage.Append( "** ExitCode=" + process.ExitCode );

                        _errorlog.Append(errormessage.ToString() );
                        _errorlog.Append("\n");
                    }*/
            }
            finally
            {
                if (process != null)
                {
                    // close process handle
                    process.Close();
                }
            }

            return exitCode;
        }

        public static void ClearConsoleLine(int? consoleLineNum = null)
        {
            if (consoleLineNum == null) consoleLineNum = Console.CursorTop;
            Console.SetCursorPosition(0, (int) consoleLineNum);
            Console.Write(new String(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, (int) consoleLineNum);
        }

        public static T GetAttribute<T>(Type classType, string memberName)
        {
            MemberInfo[] memberInfos = classType.GetMember(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (memberInfos.Length == 0) return default(T);
            T[] attrs = memberInfos[0].GetCustomAttributes(typeof(T), false) as T[];
            if (attrs != null && attrs.Length > 0)
            {
                return attrs[0];
            }
            return default(T);
        }

        public static Form GetAndFocusForm(string formName)
        {
            Form form = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x.Name == formName);
            if (form != null)
            {
                form.Focus();
                return form;
            }
            return null;
        }

        public static void CloseFormsByName(string formName, bool closeForcibly = false)
        {
            foreach (Form form in Application.OpenForms.Cast<Form>().Where(x => x.Name == formName).ToList())
            {
                if (closeForcibly)
                {
                    form.Dispose();
                }
                else
                {
                    form.Close();
                }
            }
        }

        public static void PrintBoxLabel(string strPrinter, int intCount, int intTotal = 0)
        {
            var objBuilder = new StringBuilder("\n" + "N\n");
            if (intTotal == 0)
                objBuilder.AppendLine($"A150,80,0,5,1,1,N,\"BOX {intCount}\"");
            else
                objBuilder.AppendLine($"A50,80,0,5,1,1,N,\"BOX {intCount} OF {intTotal}\"");
            objBuilder.AppendLine("P1");
            PrintCustomBarCode(strPrinter, objBuilder.ToString());
        }

        public static void PrintCustomBarCode(string strPrinter, string strText)
        {
            string printingTaskFileName = Path.GetTempPath() + Path.GetRandomFileName(); // file in %temp%
            var printingTaskFile = new FileStream(printingTaskFileName, FileMode.Append);
            var printingTaskStream = new StreamWriter(printingTaskFile, System.Text.Encoding.Default);
            printingTaskStream.Write(strText);

            //printingTaskStream.Write();
            printingTaskStream.Flush();
            printingTaskStream.Close();

            if (strPrinter.StartsWith(@"\\"))
            {
                File.Copy(printingTaskFileName, strPrinter, true); // also can be "LPT1", @"\\127.0.0.1\PNT5", etc
            }
            else
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    socket.Connect(strPrinter, 9100);
                    socket.SendFile(printingTaskFileName);
                    socket.Close();
                }
            }
            File.Delete(printingTaskFileName);
        }

        public struct SumCombination
        {
            public object Obj;
            public int Num;
        }
        /// <summary>
        /// Returns all possible combinations of the number in numberFieldName that sum to a value equal to the sum variable.
        /// </summary>
        /// <param name="objects">Objects to look for sum combinations in.</param>
        /// <param name="numberFieldName">The field whose combinations accross the objects is equal to the sum variable.</param>
        /// <param name="sum">The target number that the values in numberFieldName must sum to.</param>
        public static List<List<TObj>> GetSumCombinations<TObj>(List<TObj> objects, string numberFieldName, int sum)
        {
            if (objects.Count == 0)
            {
                return new List<List<TObj>>();
            }
            Type type = objects[0].GetType();
            SumCombination[] set = objects.Select(x =>
            {
                MemberInfo mi = type.GetField(numberFieldName) ?? (MemberInfo)type.GetProperty(numberFieldName);
                object value = null;
                switch (mi.MemberType)
                {
                    case MemberTypes.Field:
                        value = ((FieldInfo)mi).GetValue(x);
                        break;
                    case MemberTypes.Property:
                        value = ((PropertyInfo)mi).GetValue(x, null);
                        break;
                }
                return new SumCombination { Obj = x, Num = value.ToInt() };
            }).ToArray();
            return GetSumCombinations(set, sum, null).Select(x => x.Select(y => (TObj)y.Obj).ToList()).ToList();
        }
        /// <summary>
        /// Returns all possible combinations within the set variable that sum to a value equal to the sum variable.
        /// Taken from https://stackoverflow.com/a/10739219
        /// </summary>
        /// <param name="values">Always leave null. This is just used during recurrsion.</param>
        public static List<List<SumCombination>> GetSumCombinations(SumCombination[] set, int sum, List<SumCombination> values)
        {
            var returnValue = new List<List<SumCombination>>();
            if (values == null) values = new List<SumCombination>();
            for (int i = 0; i < set.Length; i++)
            {
                int left = sum - set[i].Num;
                var vals = new List<SumCombination> { set[i] };
                vals.AddRange(values);
                if (left == 0)
                {
                    returnValue.Add(vals);
                }
                else
                {
                    SumCombination[] possible = set.Take(i).Where(x => x.Num <= sum).ToArray();
                    if (possible.Length > 0)
                    {
                        foreach (List<SumCombination> list in GetSumCombinations(possible, left, values: vals))
                        {
                            returnValue.Add(list);
                        }
                    }
                }
            }
            return returnValue;
        }

        public static string GetExternalLink(ExternalLinkType linkType, string orderID)
        {
            switch (linkType)
            {
                case ExternalLinkType.AmazonInboundOrder:
                    return $"https://sellercentral.amazon.com/gp/fba/inbound-shipment-workflow/index.html/ref=au_fbaisw_name_fbasqs#{orderID.Trim()}/prepare";
                case ExternalLinkType.IngramOrder:
                    return $"https://merchant.shipwire.com/merchants/store/tracking/orderId/{orderID.Trim()}";
                case ExternalLinkType.FBATransfer:
                    return $"https://sellercentral.amazon.com/gp/fba/inbound-shipment-workflow/index.html/ref=au_fbaisw_name_fbasqs#{orderID.Trim()}/summary/tracking";
                default:
                    return "";
            }
        }

        public static string FormatHtmlForEmail(StringBuilder objEmailBody, string strTitle, string strColumns)
        {
            StringBuilder templateBuilder = new StringBuilder();
            templateBuilder.AppendLine("<p>&nbsp</p>");
            templateBuilder.AppendLine("<table>");
            templateBuilder.AppendLine($"<tr><td align=\"center\"><h2>{strTitle}</h2></td></tr>");
            templateBuilder.AppendLine("<tr><td><table>");
            templateBuilder.AppendLine(strColumns);
            templateBuilder.AppendLine(objEmailBody.ToString());
            templateBuilder.AppendLine("</table></td></tr>");
            templateBuilder.AppendLine("</table>");
            templateBuilder.AppendLine("<p>&nbsp</p>");

            return templateBuilder.ToString();
        }

        public static List<string> GetEmptyActiveDirectoryDistributionGroups()
        {
            List<string> lstEmptyGroups = new List<string>();
            using (PrincipalContext ctx = new PrincipalContext(ContextType.Domain, "domain.example.com"))
            {
                foreach (var found in new PrincipalSearcher(new GroupPrincipal(ctx)).FindAll())
                {
                    GroupPrincipal group = GroupPrincipal.FindByIdentity(ctx, found.Sid.ToString());
                    if (group != null && (!group.IsSecurityGroup.HasValue || group.IsSecurityGroup == false))
                    {
                        if (!group.GetMembers().Any())
                        {
                            lstEmptyGroups.Add(group.Name);
                        }
                    }
                }
            }
            return lstEmptyGroups;
        }

        public static Dictionary<string, decimal> GetFolderSizes(string path)
        {
            var dctFoldersAndSizes = new Dictionary<string, decimal>();
            foreach (DirectoryInfo folder in new DirectoryInfo(path).EnumerateDirectories())
            {
                long size = folder.EnumerateFiles().Sum(x => x.Length);

                foreach (DirectoryInfo sub in folder.EnumerateDirectories("*", SearchOption.AllDirectories))
                {
                    size += sub.EnumerateFiles().Sum(x => x.Length);
                }
                //Store it in MB
                dctFoldersAndSizes.Add(folder.Name, Math.Round(size / 1048576m));                                                                     
            }
            return dctFoldersAndSizes;
        }
        public static string getBarcodeCellText(int codeCount)
        {
            switch (codeCount)
            {
                case 0:
                    return "";
                case 1:
                    return "[1 Code]...";
                default:
                    return $"[{codeCount} Codes]...";
            }
        }

        /// <summary>
        /// UPS can only handle 4 digit extensions that is only digits. If the extension is more than 4 digits we need to remove it.
        /// </summary>
        /// <param name="telephoneNumber"></param>
        /// <returns></returns>
        public static string FormatPhoneNumberForUPS(string telephoneNumber)
        {
            string formatedTelephoneNumber = telephoneNumber.StripNonNumericCharacters();
            if (formatedTelephoneNumber.Length > 14) formatedTelephoneNumber = formatedTelephoneNumber.SubstringSafe(0, 10).Trim();
            return formatedTelephoneNumber;
        }

        public static void PrintTextBarcodeLabel(string text, string barcodePrinter)
        {
            // ZPL Barcode Format
            // For in-depth info on the format, see https://www.servopack.de/support/zebra/ZPLII-Prog.pdf
            /*
             * Formatting barcode label:
             * [Text(A) or Barcode(B) with X starting position on label],
             * [Y position on label],
             * [don't know],
             * [Font size],
             * [Width of row],
             * [Height of row],
             * [Horizontal Line (N is default)],
            */

            var label = new StringBuilder("\nN\n");
            label.AppendLine($"B10,20,0,1,2,7,100,N,\"{text}\"");
            label.AppendLine($"A10,150,0,4,1,1,N,\"{text}\"");
            label.AppendLine("P1");

            UtilityFunctions.PrintCustomBarCode(barcodePrinter, label.ToString());
        }

        public delegate TResult Func<T1, T2, T3, T4, TResult>(T1 a, T2 b, T3 c, out T4 d);

        public static string InsightlyOportunityUrl(long insightlyID)
        {
            return $"https://crm.na1.insightly.com/list/Opportunity/?blade=/details/opportunity/{insightlyID}";
        }

        // https://stackoverflow.com/a/16407272
        public static string HtmlToPlainText(string input)
        {
            const string tagWhiteSpace = @"(?<=>)\s+(?=<)"; // matches one or more (white space or line breaks) between '>' and '<'
            const string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>"; // matches: <br>,<br/>,<br />,<BR>,<BR/>,<BR />
            const string stripFormatting = @"<[^>]*(>|$)"; // match any character between '<' and '>', even when end tag is missing

            // Replace tag whitespace/line breaks with a single space
            input = new Regex(tagWhiteSpace, RegexOptions.Multiline).Replace(input, " ");
            // Replace <br /> with line breaks
            input = new Regex(lineBreak, RegexOptions.Multiline).Replace(input, Environment.NewLine);
            // Strip formatting
            input = new Regex(stripFormatting, RegexOptions.Multiline).Replace(input, "");

            // Decode html specific characters
            input = WebUtility.HtmlDecode(input);

            return input.Trim();
        }

        /// <summary>
        /// Returns the value of a field or property on a dynamic object.
        /// </summary>
        public static object GetDynamicValue(dynamic obj, string memeberName)
        {
            var site = CallSite<Func<CallSite, object, object>>.Create(Microsoft.CSharp.RuntimeBinder.Binder.GetMember(0, memeberName, obj.GetType(), new[] { Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo.Create(0, null) }));
            return site.Target(site, obj);
        }

        /// <summary>
        /// Pulled from https://stackoverflow.com/questions/27372816/how-to-read-the-value-for-an-enummember-attribute
        /// </summary>
        public static String GetEnumMemberValue<T>(T value) where T : Enum
        {
            return typeof(T)
                .GetTypeInfo()
                .DeclaredMembers
                .SingleOrDefault(x => x.Name == value.ToString())
                ?.GetCustomAttribute<EnumMemberAttribute>(false)
                ?.Value;
    }
    }

    public enum TextAlertTo
    {
        [EnumLabel("5555555555@vtext.com")] Example = 1,
    }

    public class RawPrinterHelper
    {
        // Structure and API declarions:
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)] public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)] public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)] public string pDataType;
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

        // SendBytesToPrinter()
        // When the function is given a printer name and an unmanaged array
        // of bytes, the function sends those bytes to the print queue.
        // Returns true on success, false on failure.
        public static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, Int32 dwCount)
        {
            Int32 dwError = 0, dwWritten = 0;
            IntPtr hPrinter = new IntPtr(0);
            DOCINFOA di = new DOCINFOA();
            bool bSuccess = false; // Assume failure unless you specifically succeed.

            di.pDocName = "My C#.NET RAW Document";
            di.pDataType = "RAW";

            // Open the printer.
            if (OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
            {
                // Start a document.
                if (StartDocPrinter(hPrinter, 1, di))
                {
                    // Start a page.
                    if (StartPagePrinter(hPrinter))
                    {
                        // Write your bytes.
                        bSuccess = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
                        EndPagePrinter(hPrinter);
                    }
                    EndDocPrinter(hPrinter);
                }
                ClosePrinter(hPrinter);
            }
            // If you did not succeed, GetLastError may give more information
            // about why not.
            if (bSuccess == false)
            {
                dwError = Marshal.GetLastWin32Error();
            }
            return bSuccess;
        }

        public static bool SendFileToPrinter(string szPrinterName, string szFileName)
        {
            bool bSuccess;

            using (var fs = new FileStream(szFileName, FileMode.Open))
            {
                // Create a BinaryReader on the file.
                using (var br = new BinaryReader(fs))
                {
                    // Dim an array of bytes big enough to hold the file's contents.
                    Byte[] bytes = new Byte[fs.Length];
                    // Your unmanaged pointer.
                    IntPtr pUnmanagedBytes = new IntPtr(0);
                    int nLength;

                    nLength = Convert.ToInt32(fs.Length);
                    // Read the contents of the file into the array.
                    bytes = br.ReadBytes(nLength);
                    // Allocate some unmanaged memory for those bytes.
                    pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
                    // Copy the managed byte array into the unmanaged array.
                    Marshal.Copy(bytes, 0, pUnmanagedBytes, nLength);
                    // Send the unmanaged bytes to the printer.
                    bSuccess = SendBytesToPrinter(szPrinterName, pUnmanagedBytes, nLength);
                    // Free the unmanaged memory that you allocated earlier.
                    Marshal.FreeCoTaskMem(pUnmanagedBytes);
                }

            }

            return bSuccess;
        }

        public static bool SendStringToPrinter(string szPrinterName, string szString)
        {
            IntPtr pBytes;
            Int32 dwCount;
            // How many characters are in the string?
            dwCount = szString.Length;
            // Assume that the printer is expecting ANSI text, and then convert
            // the string to ANSI text.
            pBytes = Marshal.StringToCoTaskMemAnsi(szString);
            // Send the converted ANSI string to the printer.
            SendBytesToPrinter(szPrinterName, pBytes, dwCount);
            Marshal.FreeCoTaskMem(pBytes);
            return true;
        }
    }

    public enum ExternalLinkType
    {
        AmazonInboundOrder,
        IngramOrder,
        FBATransfer
    }

    public class SingleOrArrayConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(List<T>));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Array)
            {
                return token.ToObject<List<T>>();
            }
            if (token.Type == JTokenType.Null)
            {
                return null;
            }
            return new List<T> { token.ToObject<T>() };
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}