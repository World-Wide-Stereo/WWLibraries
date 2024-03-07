using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ww.Utilities
{
    public static class BarcodeParser
    {
        public enum BarcodeType
        {
            BoxPlaceHolder,
            Demo,
            FinishedPlaceHolder,
            FNSku,
            Generic,
            OpenBox,
            QuickPick,
            RA,
            Repair,
            RMA,
            Standard,
            StandardWithPrice,
            Used,
            Wire,
        }

        private const string _receipt = @"\d{6,7}";

        public static Tuple<BarcodeType, string, string> ParseForValue(string input)
        {
            //get UPC
            var lastNumber = input.Substring(input.LastIndexOf('-') + 1);

            //get middle component if applicable
            var start = input.IndexOf('-') + 1;
            var length = input.LastIndexOf('-') - start;
            if (length < 0)
                length = 0;
            var specialInfo = input.Substring(start, length);

            if (input.StartsWith("O-", StringComparison.OrdinalIgnoreCase))
                return Tuple.Create(BarcodeType.OpenBox, lastNumber, specialInfo);
            if (input.StartsWith("D-", StringComparison.OrdinalIgnoreCase))
                return Tuple.Create(BarcodeType.Demo, lastNumber, specialInfo);
            if (input.StartsWith("W-", StringComparison.OrdinalIgnoreCase))
                return Tuple.Create(BarcodeType.Wire, lastNumber, specialInfo);
            if (input.StartsWith("RA-", StringComparison.OrdinalIgnoreCase))
                return Tuple.Create(BarcodeType.RA, lastNumber, specialInfo);
            if (input.StartsWith("U-", StringComparison.OrdinalIgnoreCase))
                return Tuple.Create(BarcodeType.Used, lastNumber, specialInfo);
            if (input.StartsWith("R#", StringComparison.OrdinalIgnoreCase) || input.StartsWith("R ", StringComparison.OrdinalIgnoreCase) || input.StartsWith("R_", StringComparison.OrdinalIgnoreCase))
                return Tuple.Create(BarcodeType.Repair, input.TrimStart("R# _".ToArray()), specialInfo);
            if (input.StartsWith("RMA-", StringComparison.OrdinalIgnoreCase))
                return Tuple.Create(BarcodeType.RMA, lastNumber.TrimStart('0'), specialInfo);
            if (Regex.IsMatch(input, _receipt + @"-\d{6}"))
                return Tuple.Create(BarcodeType.RMA, lastNumber.TrimStart('0'), specialInfo);
            if (Regex.IsMatch(input, _receipt + @"-\d{5}"))
                return Tuple.Create(BarcodeType.QuickPick, lastNumber, specialInfo);

            return Tuple.Create(BarcodeType.Generic, input, specialInfo);
        }
    }
}
