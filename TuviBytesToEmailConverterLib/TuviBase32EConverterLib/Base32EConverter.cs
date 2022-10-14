///////////////////////////////////////////////////////////////////////////////
//   Copyright 2022 Eppie (https://eppie.io)
//
//   Licensed under the Apache License, Version 2.0(the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;

namespace Tuvi.Base32EConverterLib
{
    /// <summary>
    /// Allows to convert byte array to a string of symbols that can be an email name and vice versa. 
    /// Symbols dictionary is [abcdefghijkmnpqrstuvwxyz23456789]. 1 and l, 0 and o looks similar so they were deleted.
    /// </summary>
    public static class Base32EConverter
    {
        private const int MaxEmailNameSize = 64;
        private const int ByteSize = 8;
        private const int FiveBitsSize = 5;
        private const string Base32EDictionary = "abcdefghijkmnpqrstuvwxyz23456789";

        /// <summary>
        /// Convert byte array to a string
        /// </summary>
        /// <param name="array">Converting byte array.</param>
        /// <returns>String that can be an email's name.</returns>
        public static string ConvertBytesToEmailName(byte[] array)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (array.Length < 1)
            {
                throw new ArgumentException("Array should contain at least 1 element.");
            }

            byte[] fiveBitsArray = ConvertEightToFiveBits(array);
            char[] symbolsArray = new char[fiveBitsArray.Length];
            for (int i = 0; i < symbolsArray.Length; i++)
            {
                symbolsArray[i] = ConvertByteToSymbol(fiveBitsArray[i]);
            }
            return new string(symbolsArray);
        }

        /// <summary>
        /// Converts string to an byte array.
        /// </summary>
        /// <param name="name">String of allowed symbols.</param>
        /// <returns>Array of bytes.</returns>
        public static byte[] ConvertStringToByteArray(string name)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Email's name can not be empty or whitespace.");
            }

            if (name.Length > MaxEmailNameSize)
            {
                throw new ArgumentException($"Email's name can not be longer than {MaxEmailNameSize} symbols.", nameof(name));
            }

            char[] symbolsArray = name.ToCharArray();
            byte[] fiveBitsArray = new byte[symbolsArray.Length];

            for (int i = 0; i < fiveBitsArray.Length; i++)
            {
                fiveBitsArray[i] = ConvertSymbolToBits(symbolsArray[i]);
            }

            return ConvertFiveToEightBits(fiveBitsArray);
        }

        /// <summary>
        /// Divides sequence of bits (initial array) into groups of 5 bits.
        /// </summary>
        /// <param name="array">Initial array.</param>
        /// <returns>Array of 5-bit groups.</returns>
        private static byte[] ConvertEightToFiveBits(byte[] array)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (array.Length < 1)
            {
                throw new ArgumentException("Array should contain at least 1 element.");
            }

            int size = array.Length * ByteSize % FiveBitsSize == 0 ? array.Length * ByteSize / FiveBitsSize : array.Length * ByteSize / FiveBitsSize + 1;
            if (size > MaxEmailNameSize)
            {
                throw new ArgumentException($"Initial array is too big to create correct email's name (name can not be longer than {MaxEmailNameSize} symbols).", nameof(array));
            }

            int currentPosition = size - 1;
            byte[] result = new byte[size];
            BigInteger bitSequence = new BigInteger(0);
            bitSequence = bitSequence.BigEndianConcatBytes(array);
            while (bitSequence != 0 && currentPosition >= 0)
            {
                byte lastFiveBits = (byte)(bitSequence & 31);
                result[currentPosition] = lastFiveBits;
                bitSequence = bitSequence >> FiveBitsSize;
                currentPosition--;
            }

            return result;
        }

        /// <summary>
        /// Concats groups of 5 bits into a sequense. Represents it as an byte array (groups of 8 bits).
        /// </summary>
        /// <param name="array">Array of 5-bit groups.</param>
        /// <returns>Array of bytes (8-bit groups).</returns>
        private static byte[] ConvertFiveToEightBits(byte[] array)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (array.Length > MaxEmailNameSize)
            {
                throw new ArgumentException($"Email's name can not be longer than {MaxEmailNameSize} symbols.", nameof(array));
            }

            BigInteger number = 0;
            int size = array.Length * FiveBitsSize / ByteSize;
            byte[] resultArray = new byte[size];
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] >= 32)
                {
                    throw new ArgumentOutOfRangeException(nameof(array),
                        $"Array at index {i} has wrong value. Allowed values are from 0 to 31.");
                }
                number = number << FiveBitsSize;
                number |= array[i];
            }

            byte[] tempArray = number.ToBigEndianByteArray();
            if (tempArray.Length >= resultArray.Length)
            {
                return tempArray;
            }
            else
            {
                for (int i = 1; i <= tempArray.Length; i++)
                {
                    resultArray[size - i] = tempArray[tempArray.Length  -  i];
                }

                return resultArray;
            }
        }

        private static char ConvertByteToSymbol(byte byteValue)
        {
            if (byteValue >= 32)
            {
                throw new ArgumentOutOfRangeException(nameof(byteValue));
            }
            return Base32EDictionary[byteValue];
        }

        private static byte ConvertSymbolToBits(char symbol)
        {
            int value = Base32EDictionary.IndexOf(symbol);
            if (value == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(symbol));
            }
            else
            {
                return (byte)value;
            }
        }
    }
}
