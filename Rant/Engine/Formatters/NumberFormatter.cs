﻿using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Rant.Engine.Formatters
{
    internal class NumberFormatter
    {
        private static readonly NumberFormatInfo CommaGroupFormat = new NumberFormatInfo()
        {
            NumberGroupSizes = new[] { 3 },
            NumberGroupSeparator = ",",
            NumberDecimalSeparator = "."
        };

        private static readonly NumberFormatInfo DotGroupFormat = new NumberFormatInfo()
        {
            NumberGroupSizes = new[] { 3 },
            NumberGroupSeparator = ".",
            NumberDecimalSeparator = ","
        };

        public Endianness Endianness { get; set; } = Endianness.Default;
        public BinaryFormat BinaryFormat { get; set; } = BinaryFormat.Normal;

        public NumberFormat NumberFormat { get; set; } = NumberFormat.Normal;

        public int BinaryFormatDigits { get; set; } = 8;

        public string FormatNumber(double number)
        {
            switch (NumberFormat)
            {
                case NumberFormat.Normal:
                    return number.ToString(CultureInfo.InvariantCulture);
                case NumberFormat.Group:
                    return String.Format("{0:n0}", number);
                case NumberFormat.GroupCommas:
                    return number.ToString("n0", CommaGroupFormat);
                case NumberFormat.GroupDots:
                    return number.ToString("n0", DotGroupFormat);
                case NumberFormat.Roman:
                case NumberFormat.RomanUpper:
                    return Numerals.ToRoman(number);
                case NumberFormat.RomanLower:
                    return Numerals.ToRoman(number, true);
                case NumberFormat.VerbalEn:
                    return number % 1 > 0 ? "?" : Numerals.ToVerbal((long)number);
                case NumberFormat.Hex:
                case NumberFormat.HexUpper:
                    return GetHex((long)number, true);
                case NumberFormat.HexLower:
                    return GetHex((long)number, false);
                case NumberFormat.Binary:
                    return GetBinary((long)number);
            }
            return number.ToString(CultureInfo.InvariantCulture);
        }

        private string GetHex(long number, bool uppercase)
        {
            bool needsReverse = Endianness != Endianness.Default && (BitConverter.IsLittleEndian != (Endianness == Endianness.Little));
            var hexString = Convert.ToString((long)number, 16);
            hexString = uppercase ? hexString.ToUpper() : hexString.ToLower();
            if (needsReverse && hexString.Length % 2 != 0) hexString = "0\{hexString}";

            int origLength = hexString.Length;
            int finalLength =
                BinaryFormat == BinaryFormat.Pad
                ? (origLength < BinaryFormatDigits ? BinaryFormatDigits : origLength)
                : BinaryFormat == BinaryFormat.Truncate
                    ? (origLength < BinaryFormatDigits ? origLength : BinaryFormatDigits)
                    : origLength;

            var chars = new char[finalLength];
            for (int i = 0; i < finalLength; i++) chars[i] = '0';

            if (needsReverse)
            {
                for (int i = 0; i < origLength; i += 2)
                {
                    chars[finalLength - i - 2] = hexString[i];
                    chars[finalLength - i - 1] = hexString[i + 1];
                }
            }
            else
            {
                int truncatedOrigin = origLength > finalLength ? origLength - finalLength : 0;
                int truncatedLength = origLength > finalLength ? finalLength : origLength;
                hexString.CopyTo(truncatedOrigin, chars, finalLength - truncatedLength, truncatedLength);
            }
            return new string(chars);
        }

        private string GetBinary(long number)
        {
            bool needsReverse = Endianness != Endianness.Default && (BitConverter.IsLittleEndian != (Endianness == Endianness.Little));
            var hexString = Convert.ToString((long)number, 2);
            if (needsReverse && hexString.Length % 8 != 0) hexString = new string('0', hexString.Length % 8) + hexString;

            int origLength = hexString.Length;
            int finalLength =
                BinaryFormat == BinaryFormat.Pad
                ? (origLength < BinaryFormatDigits * 4 ? BinaryFormatDigits * 4 : origLength)
                : BinaryFormat == BinaryFormat.Truncate
                    ? (origLength < BinaryFormatDigits * 4 ? origLength : BinaryFormatDigits * 4)
                    : origLength;

            var chars = new char[finalLength];
            for (int i = 0; i < finalLength; i++) chars[i] = '0';

            if (needsReverse)
            {
                for (int i = 0; i < origLength; i += 8)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        chars[finalLength - i - (8 - j)] = hexString[i + j];
                    }
                }
            }
            else
            {
                int truncatedOrigin = origLength > finalLength ? origLength - finalLength : 0;
                int truncatedLength = origLength > finalLength ? finalLength : origLength;
                hexString.CopyTo(truncatedOrigin, chars, finalLength - truncatedLength, truncatedLength);
            }
            return new string(chars);
        }
    }
}