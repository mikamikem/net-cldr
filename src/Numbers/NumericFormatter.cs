﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sepia.Globalization.Numbers
{
    class NumericFormatter : NumberFormatter
    {
        static string[] digits = new NumberFormatInfo().NativeDigits;
        static Regex significantDigitsPattern = new Regex(@"\.0+", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        static Regex groupingPattern = new Regex(@"[\#0]+[,\.]", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public override string Format(long value)
        {
            int nDigits = (value == 0) ? 1 : ((int)Math.Floor(Math.Log10(Math.Abs(value))) + 1);
            string pattern;
            NumberFormatInfo nfi;
            NumberInfo(null, nDigits, out pattern, out nfi);
            return Transform(value.ToString(pattern, nfi), null, pattern, nfi);
        }

        public override string Format(decimal value)
        {
            int nDigits = (value == 0) ? 1 : ((int)Math.Floor(Math.Log10((double)Math.Abs(value))) + 1);
            string pattern;
            NumberFormatInfo nfi;
            NumberInfo(null, nDigits, out pattern, out nfi);
            return Transform(value.ToString(pattern, nfi), null, pattern, nfi);
        }

        public override string Format(double value)
        {
            int nDigits = (value == 0) ? 1 : ((int)Math.Floor(Math.Log10(Math.Abs(value))) + 1);
            string pattern;
            NumberFormatInfo nfi;
            NumberInfo(null, nDigits, out pattern, out nfi);
            return Transform(value.ToString(pattern, nfi), null, pattern, nfi);
        }

        public override string Format(long value, string currencyCode)
        {
            int nDigits = (value == 0) ? 1 : ((int)Math.Floor(Math.Log10(Math.Abs(value))) + 1);
            string pattern;
            NumberFormatInfo nfi;
            NumberInfo(currencyCode, nDigits, out pattern, out nfi);
            return Transform(value.ToString(pattern, nfi), currencyCode, pattern, nfi);
        }

        public override string Format(decimal value, string currencyCode)
        {
            int nDigits = (value == 0) ? 1 : ((int)Math.Floor(Math.Log10((double)Math.Abs(value))) + 1);
            string pattern;
            NumberFormatInfo nfi;
            NumberInfo(currencyCode, nDigits, out pattern, out nfi);
            return Transform(value.ToString(pattern, nfi), currencyCode, pattern, nfi);
        }

        public override string Format(double value, string currencyCode)
        {
            int nDigits = (value == 0) ? 1 : ((int)Math.Floor(Math.Log10(Math.Abs(value))) + 1);
            string pattern;
            NumberFormatInfo nfi;
            NumberInfo(currencyCode, nDigits, out pattern, out nfi);
            return Transform(value.ToString(pattern, nfi), currencyCode, pattern, nfi);
        }


        void NumberInfo(string currencyCode, int nDigits, out string pattern, out NumberFormatInfo nfi)
        {
            nfi = new NumberFormatInfo
            {
                NaNSymbol = Symbols.NotANumber,
                NegativeSign = Symbols.MinusSign,
                NegativeInfinitySymbol = Symbols.MinusSign + Symbols.Infinity,
                NumberDecimalSeparator = Symbols.Decimal,
                NumberGroupSeparator = Symbols.Group,
                // NativeDigits is NOT used the formatter!!
                //NativeDigits = NumberingSystem.Digits,
                PercentDecimalSeparator = Symbols.Decimal,
                PercentSymbol = Symbols.PercentSign,
                PerMilleSymbol = Symbols.PerMille,
                PositiveInfinitySymbol = Symbols.Infinity,
                PositiveSign = Symbols.PlusSign
            };

            pattern = null;
            if (Options.Style == NumberStyle.Decimal)
            {
                var path = $"ldml/numbers/decimalFormats[@numberSystem='{NumberingSystem.Id}']/decimalFormatLength[not(@type)]/decimalFormat/pattern";
                pattern = Locale.Find(path).Value;
            }

            else if (Options.Style == NumberStyle.Percent)
            {
                var path = $"ldml/numbers/percentFormats[@numberSystem='{NumberingSystem.Id}']/percentFormatLength[not(@type)]/percentFormat/pattern";
                pattern = Locale.Find(path).Value;
            }

            else if (Options.Style == NumberStyle.Scientific)
            {
                var path = $"ldml/numbers/scientificFormats[@numberSystem='{NumberingSystem.Id}']/scientificFormatLength[not(@type)]/scientificFormat/pattern";
                pattern = Locale.Find(path).Value;
                pattern = pattern == "#E0" ? "0.0######E0" : pattern;
            }

            else if (Options.Style == NumberStyle.CurrencyStandard || Options.Style == NumberStyle.CurrencyAccounting)
            {
                nfi.NumberDecimalSeparator = Symbols.CurrencyDecimal;
                nfi.NumberGroupSeparator = Symbols.CurrencyGroup;

                string path = null;
                if (Options.Style == NumberStyle.CurrencyStandard)
                    path = $"ldml/numbers/currencyFormats[@numberSystem='{NumberingSystem.Id}']/currencyFormatLength/currencyFormat[@type='standard']/pattern";
                else if (Options.Style == NumberStyle.CurrencyAccounting)
                    path = $"ldml/numbers/currencyFormats[@numberSystem='{NumberingSystem.Id}']/currencyFormatLength/currencyFormat[@type='accounting']/pattern";
                pattern = Locale.Find(path).Value;

                // Apply currency decimal places
                if (currencyCode == null)
                    currencyCode = Locale.CurrencyCode;
                var digits = Cldr.Instance
                    .GetDocuments("common/supplemental/supplementalData.xml")
                    .FirstElementOrDefault($"supplementalData/currencyData/fractions/info[@iso4217='{currencyCode}']/@digits");
                if (digits != null)
                {
                    int significantDigits = Int32.Parse(digits.Value);
                    pattern = significantDigitsPattern.Replace(pattern, "." + new String('0', significantDigits));
                }
            }

            else
            {
                throw new NotImplementedException();
            }

            // Grouping of digits.
            var useGrouping = Options.UseGrouping;
            if (useGrouping)
            {
                // Grouping digits, "#,##,##0" => [3, 2, 1]
                var parts = pattern.Split(';');
                nfi.NumberGroupSizes = groupingPattern
                    .Matches(parts[0])
                    .Cast<Match>()
                    .Skip(1)
                    .Reverse()
                    .Select(m => m.Length - 1)
                    .DefaultIfEmpty(3)
                    .ToArray();

                // Don't group if min grouping digits is not met.
                var minElement = Locale.FindOrDefault("ldml/numbers/minimumGroupingDigits");
                if (minElement != null)
                {
                    var minDigits = Int32.Parse(minElement.Value) + nfi.NumberGroupSizes[0];
                    useGrouping = nDigits > minDigits;
                }
            }

            if (!useGrouping)
            {
                nfi.NumberGroupSeparator = "";
            }
            nfi.PercentGroupSizes = nfi.NumberGroupSizes;
            nfi.PercentGroupSeparator = nfi.NumberGroupSeparator;
        }

        string Transform(string s, string currencyCode, string pattern, NumberFormatInfo nfi)
        {
            var sb = new StringBuilder(s);

            // Replace digits with number system digits.
            for (int i = 0; i < 10; ++i)
            {
                sb.Replace(digits[i], NumberingSystem.Digits[i]);
            }

            // Replace currency symbol
            if (s.Contains("¤"))
            {
                if (currencyCode == null)
                {
                    currencyCode = Locale.CurrencyCode;
                }

                if (s.Contains("¤¤¤¤¤"))
                {
                    throw new NotImplementedException();
                }
                else if (s.Contains("¤¤¤¤"))
                {
                    throw new NotImplementedException();
                }
                else if (s.Contains("¤¤¤"))
                {
                    throw new NotImplementedException();
                }
                else if (s.Contains("¤¤"))
                {
                    sb.Replace("¤¤", currencyCode);
                }
                else if (s.Contains("¤"))
                {
                    var symbol = $"ldml/numbers/currencies/currency[@type='{currencyCode}']/symbol";
                    sb.Replace("¤", Locale.Find(symbol).Value);
                }
            }

            return sb.ToString();
        }

    }
}
