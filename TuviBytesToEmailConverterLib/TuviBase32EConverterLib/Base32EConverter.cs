///////////////////////////////////////////////////////////////////////////////
//   Copyright 2025 Eppie (https://eppie.io)
//
//   Licensed under the Apache License, Version 2.0 (the "License");
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
    /// Provides methods to encode a byte array into an email-safe Base32 string and decode it back.
    /// The Base32E alphabet ([abcdefghijkmnpqrstuvwxyz23456789]) excludes visually similar characters (1, l, 0, o)
    /// to ensure compatibility with email naming conventions.
    /// </summary>
    public static class Base32EConverter
    {
        private const int MaxEmailNameSize = 64;
        private const int ByteSize = 8;
        private const int FiveBitsSize = 5;
        private const string Base32EDictionary = "abcdefghijkmnpqrstuvwxyz23456789";

        /// <summary>
        /// Returns true if the provided string consists only of Base32E alphabet characters.
        /// Does not enforce any length constraints beyond non-null/non-empty.
        /// </summary>
        /// <param name="value">String to check.</param>
        /// <param name="caseInsensitive">If true, comparison is case-insensitive.</param>
        public static bool IsEmailBase32(string value, bool caseInsensitive = true)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            var alphabet = Base32EDictionary;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (caseInsensitive)
                {
                    c = char.ToLowerInvariant(c);
                }
                if (alphabet.IndexOf(c) == -1)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Encodes a byte array into an email-safe Base32 string.
        /// </summary>
        /// <param name="array">The byte array to encode.</param>
        /// <returns>An email-safe string encoded in Base32E format.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="array"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="array"/> is empty or would produce a string longer than 64 characters.</exception>
        public static string ToEmailBase32(byte[] array)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (array.Length < 1)
            {
                throw new ArgumentException("The input array must contain at least one element.", nameof(array));
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
        /// Decodes an email-safe Base32 string into a byte array.
        /// </summary>
        /// <param name="name">The Base32E-encoded string to decode.</param>
        /// <returns>The decoded byte array.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is empty, contains only whitespace, exceeds 64 characters, or contains invalid characters.</exception>
        public static byte[] FromEmailBase32(string name)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The input string cannot be empty or consist only of whitespace.", nameof(name));
            }

            if (name.Length > MaxEmailNameSize)
            {
                throw new ArgumentException($"The input string cannot exceed {MaxEmailNameSize} characters.", nameof(name));
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
        /// Converts a byte array into an array of 5-bit groups for Base32E encoding.
        /// </summary>
        /// <param name="array">The byte array to convert.</param>
        /// <returns>An array of 5-bit values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="array"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="array"/> is empty or would produce a string longer than 64 characters.</exception>
        private static byte[] ConvertEightToFiveBits(byte[] array)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (array.Length < 1)
            {
                throw new ArgumentException("The input array must contain at least one element.", nameof(array));
            }

            var sizeInBits = array.Length * ByteSize;
            int resultSizeInBytes = sizeInBits / FiveBitsSize;
            if (sizeInBits % FiveBitsSize != 0)
            {
                ++resultSizeInBytes;
            }
            if (resultSizeInBytes > MaxEmailNameSize)
            {
                throw new ArgumentException($"The input array is too large to produce a valid email-safe string (maximum {MaxEmailNameSize} characters).", nameof(array));
            }

            int currentPosition = resultSizeInBytes - 1;
            byte[] result = new byte[resultSizeInBytes];
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
        /// Combines an array of 5-bit groups into a byte array.
        /// </summary>
        /// <param name="array">The array of 5-bit values.</param>
        /// <returns>The resulting byte array.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="array"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="array"/> exceeds 64 elements.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when any element in <paramref name="array"/> is greater than 31.</exception>
        private static byte[] ConvertFiveToEightBits(byte[] array)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (array.Length > MaxEmailNameSize)
            {
                throw new ArgumentException($"The input array cannot exceed {MaxEmailNameSize} elements.", nameof(array));
            }

            BigInteger number = 0;
            int size = array.Length * FiveBitsSize / ByteSize;
            byte[] resultArray = new byte[size];
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] >= 32)
                {
                    throw new ArgumentOutOfRangeException(nameof(array),
                        $"Element at index {i} is invalid. Values must be between 0 and 31.");
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
                    resultArray[size - i] = tempArray[tempArray.Length - i];
                }

                return resultArray;
            }
        }

        /// <summary>
        /// Converts a 5-bit value to the corresponding Base32E character.
        /// </summary>
        /// <param name="byteValue">The 5-bit value (0–31) to convert.</param>
        /// <returns>The corresponding character from the Base32E alphabet.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="byteValue"/> is greater than 31.</exception>
        private static char ConvertByteToSymbol(byte byteValue)
        {
            if (byteValue >= 32)
            {
                throw new ArgumentOutOfRangeException(nameof(byteValue), "Value must be between 0 and 31.");
            }
            return Base32EDictionary[byteValue];
        }

        /// <summary>
        /// Converts a Base32E character to its corresponding 5-bit value.
        /// </summary>
        /// <param name="symbol">The Base32E character to convert.</param>
        /// <returns>The corresponding 5-bit value (0–31).</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="symbol"/> is not in the Base32E alphabet.</exception>
        private static byte ConvertSymbolToBits(char symbol)
        {
            int value = Base32EDictionary.IndexOf(symbol);
            if (value == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(symbol), $"Character '{symbol}' is not in the Base32E alphabet.");
            }
            return (byte)value;
        }
    }
}