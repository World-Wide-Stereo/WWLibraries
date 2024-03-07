using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ww.Utilities.Extensions;

namespace ww.Utilities
{
    public class SensitiveDataUtilities
    {
        private bool Initialized;
        private RegexOptions regexOptions;
        
        // Luhn algorithm pattern matching
        internal Regex regexVisa;                //# Visa
        internal Regex regexMasterCard;          //# MasterCard
        internal Regex regexAmericanExpress;     //# American Express
        internal Regex regexDinersClub;          //# Diners Club
        internal Regex regexDiscover;            //# Discover

        // Specific pattern matching from WWS data analysis
        internal Regex regexFinancing;           //# Financing Accounts
        internal Regex regexAmericanGeneral;     //# American General
        internal Regex regexGiftCard;            //# Gift Cards
        
        // *** Specific pattern matching to exclude from removal ***
        internal Regex regexComcastAcct;         //# Comcast Accounts
        
        // General pattern matching to be used with ***extreme caution*** sequentially
        private Regex regexPatternMatch16;
        private Regex regexPatternMatch15;
        
        private SensitiveDataUtilities()
        {
            if (!Initialized) 
            {
                InitializeRegexValues();
            }
        }
        private static readonly Lazy<SensitiveDataUtilities> creditCardUtilities = new Lazy<SensitiveDataUtilities>(() => new SensitiveDataUtilities());
        public static SensitiveDataUtilities Instance => creditCardUtilities.Value;

        private void InitializeRegexValues()
        {
            regexOptions = RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace;
            regexVisa = new Regex(@"\b(4[0-9]{12}(?:[0-9]{3})?)|(4[0-9]{3}[\-\ ][0-9]{4}[\-\ ][0-9]{4}[\-\ ][0-9]{4})\b", regexOptions);
            regexMasterCard = new Regex(@"\b((?:5[1-5][0-9]{2}|222[1-9]|22[3-9][0-9]|2[3-6][0-9]{2}|27[01][0-9]|2720)[0-9]{12})|(5[1-5][0-9]{2}[\-\ ][0-9]{4}[\-\ ][0-9]{4}[\-\ ][0-9]{4})\b", regexOptions);
            regexAmericanExpress = new Regex(@"\b3[47][0-9]{13}\b", regexOptions);
            regexDinersClub = new Regex(@"\b3(?:0[0-5]|[68][0-9])[0-9]{11}\b", regexOptions);
            regexDiscover = new Regex(@"\b6(?:011|5[0-9]{2})[0-9]{12}\b", regexOptions);
            regexFinancing = new Regex(@"\b603(([0-9]{13})|([0-9]{1}[\-\ ][0-9]{4}[\-\ ][0-9]{4}[\-\ ][0-9]{4}))\b", regexOptions);
            regexAmericanGeneral = new Regex(@"\b(00200300|0200300)([0-9]{7}|[0-9]{8}|[0-9]{11})\b", regexOptions);
            regexGiftCard = new Regex(@"\b627([0-9]{12})\b", regexOptions);
            regexComcastAcct = new Regex(@"\b8499([0-9]{12})\b", regexOptions);
            regexPatternMatch16 = new Regex(@"\b(([0-9]{16})|([0-9]{4}[\-\ ][0-9]{4}[\-\ ][0-9]{4}[\-\ ][0-9]{4}))\b", regexOptions);
            regexPatternMatch15 = new Regex(@"\b([0-9]{15})|([0-9]{4}[\-\ ][0-9]{6}[\-\ ][0-9]{5})\b", regexOptions);
            Initialized = true;
        }

        private Regex[] GetCreditCardRegexPatterns()
        {
            var regexList = new[] { regexVisa, regexMasterCard, regexAmericanExpress, regexDinersClub, regexDiscover, regexFinancing, regexAmericanGeneral, regexGiftCard };
            return regexList;
        }

        internal bool MatchPattern(string input, Regex pattern = null)
        {
            if (input.IsNullOrBlank()) { return false; }
            if (pattern == null) { return Instance.GetCreditCardRegexPatterns().Any(x => x.IsMatch(input)); }
            return pattern.IsMatch(input);
        }

        internal string ExtractPattern(string input, Regex pattern = null)
        {
            if (input.IsNullOrBlank()) { return string.Empty; }
            StringBuilder cleanInput = new StringBuilder(input);
            StringBuilder extractedOutput = new StringBuilder();
            if (pattern != null)
            {
                while (pattern.IsMatch(cleanInput.ToString()))
                {
                    extractedOutput.Append($"{(extractedOutput.Length == 0 ? string.Empty : "|")}{pattern.Match(cleanInput.ToString())}");
                    CleanInput(pattern, cleanInput);
                }
            }
            else
            {
                foreach (var regex in GetCreditCardRegexPatterns().Where(regex => regex.IsMatch(input)))
                {
                    while (regex.IsMatch(cleanInput.ToString()))
                    {
                        extractedOutput.Append($"{(extractedOutput.Length == 0 ? string.Empty : "|")}{regex.Match(cleanInput.ToString())}");
                        CleanInput(regex, cleanInput);
                    }
                }
            }
            return extractedOutput.ToString();
        }

        private static void CleanInput(Regex pattern, StringBuilder cleanInput)
        {
            cleanInput.Replace(pattern.Match(cleanInput.ToString()).Value, "[-#-]");
        }

        internal string PatternType(string input)
        {
            StringBuilder regMatch = new StringBuilder();
            if (regexVisa.IsMatch(input)) { regMatch.Append($"{(regMatch.Length == 0 ? "" : "|")}{nameof(CCDataType.Visa)}"); }
            if (regexMasterCard.IsMatch(input)) { regMatch.Append($"{(regMatch.Length == 0 ? "" : "|")}{nameof(CCDataType.MasterCard)}"); }
            if (regexAmericanExpress.IsMatch(input)) { regMatch.Append($"{(regMatch.Length == 0 ? "" : "|")}{nameof(CCDataType.AmericanExpress)}"); }
            if (regexDinersClub.IsMatch(input)) { regMatch.Append($"{(regMatch.Length == 0 ? "" : "|")}{nameof(CCDataType.DinersClub)}"); }
            if (regexDiscover.IsMatch(input)) { regMatch.Append($"{(regMatch.Length == 0 ? "" : "|")}{nameof(CCDataType.Discover)}"); }
            if (regexFinancing.IsMatch(input)) { regMatch.Append($"{(regMatch.Length == 0 ? "" : "|")}{nameof(CCDataType.Financing)}"); }
            if (regexAmericanGeneral.IsMatch(input)) { regMatch.Append($"{(regMatch.Length == 0 ? "" : "|")}{nameof(CCDataType.AmericanGeneral)}"); }
            if (regexGiftCard.IsMatch(input)) { regMatch.Append($"{(regMatch.Length == 0 ? "" : "|")}{nameof(CCDataType.GiftCard)}"); }
            if (regexComcastAcct.IsMatch(input)) { regMatch.Append($"{(regMatch.Length == 0 ? "" : "|")}{nameof(CCDataType.ComCast)}"); }
            return regMatch.Length == 0 ? "No Match" : regMatch.ToString();
        }

        internal string SanitizePattern(string input, Regex pattern = null)
        {
            if (input.IsNullOrBlank()) { return string.Empty; }
            StringBuilder cleanInput = new StringBuilder(input);
            if (pattern != null)
            {
                while (pattern.IsMatch(cleanInput.ToString()))
                {
                    CleanInput(pattern, cleanInput);
                }
            }
            else
            {
                foreach (var allPattern in GetCreditCardRegexPatterns())
                {
                    while (allPattern.IsMatch(cleanInput.ToString()))
                    {
                        CleanInput(allPattern, cleanInput);
                    }
                }
            }
            return cleanInput.ToString();
        }

        public enum CCDataType 
        {
            Unknown = 0,
            Visa,
            MasterCard,
            AmericanExpress,
            DinersClub,
            Discover,
            Financing,
            AmericanGeneral,
            GiftCard,
            ComCast
        }
    }
}
