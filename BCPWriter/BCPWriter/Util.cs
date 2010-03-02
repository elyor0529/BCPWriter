﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCPWriter
{
    /// <summary>
    /// Internal utility class.
    /// </summary>
    public class Util
    {
        /// <summary>
        /// Concats 2 byte[]
        /// </summary>
        /// <param name="array1">First array to concat</param>
        /// <param name="array2">Second array to concat</param>
        /// <returns>byte[] resulting from the concatenation</returns>
        public static byte[] ConcatByteArrays(byte[] array1, byte[] array2)
        {
            byte[] bytes = new byte[array1.Length + array2.Length];
            Buffer.BlockCopy(array1, 0, bytes, 0, array1.Length);
            Buffer.BlockCopy(array2, 0, bytes, array1.Length, array2.Length);
            return bytes;
        }

        /// <summary>
        /// Encode text using OEM code page, see http://en.wikipedia.org/wiki/Windows_code_page
        /// </summary>
        /// <param name="text">text to encode</param>
        /// <returns>text encoded using OEM code page</returns>
        public static byte[] EncodeToOEMCodePage(string text)
        {
            Encoding enc = Encoding.GetEncoding(System.Globalization.CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
            return enc.GetBytes(text);
        }

        /// <summary>
        /// Converts a byte[] to hexadecimal.
        /// </summary>
        /// <param name="data">data to convert</param>
        /// <returns>string containing hexadecimal</returns>
        public static string ToHexString(byte[] data)
        {
            StringBuilder hex = new StringBuilder();
            foreach (byte b in data)
            {
                hex.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "{0:x2}", b);
            }
            return hex.ToString();
        }

        /// <summary>
        /// Converts a string to a byte[]
        /// </summary>
        /// <param name="text">text to convert</param>
        /// <returns>byte[]</returns>
        public static byte[] StringToByteArray(string text)
        {
            byte[] bytes = new byte[text.Length];
            int i = 0;
            foreach (char c in text.ToCharArray())
            {
                bytes[i] = (byte)c;
                i++;
            }
            return bytes;
        }

        /// <summary>
        /// See <a href="http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa-in-c">How do you convert Byte Array to Hexadecimal String, and vice versa, in C#?</a>
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] HexToByteArray(string hex)
        {
            int nbChars = hex.Length;
            byte[] bytes = new byte[nbChars / 2];
            for (int i = 0; i < nbChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
    }
}
