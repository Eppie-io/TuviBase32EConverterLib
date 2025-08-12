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
using System.Collections.Generic;
using System.Numerics;

namespace Tuvi.Base32EConverterLib
{
    /// <summary>
    /// Extra function to work with BigInteger.
    /// </summary>
    public static class BigIntegerExtension
    {
        /// <summary>
        /// Concatenate byte array to a BigInteger number with big-endian format.
        /// Can be used as "BigInteger ctor" with big-endian format.
        /// </summary>
        /// <param name="number">Initial BigInteger.</param>
        /// <param name="array">Byte array.</param>
        /// <returns>Resulting BigInteger.</returns>
        public static BigInteger BigEndianConcatBytes(this BigInteger number, byte[] array)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            for (int i = 0; i < array.Length; i++)
            {
                number = number << 8;
                number |= array[i];
            }

            return number;
        }

        /// <summary>
        /// Converting BigInteger into byte array with big-endian format.
        /// </summary>
        /// <param name="number">BigInteger number.</param>
        /// <returns>Byte array with big-endian format.</returns>
        public static byte[] ToBigEndianByteArray(this BigInteger number)
        {
            BigInteger temp = number;
            List<byte> result = new List<byte>();
            while (temp > 0)
            {
                byte currentByte = (byte)(temp & 255);
                result.Add(currentByte);
                temp = temp >> 8;
            }

            result.Reverse();
            return result.ToArray();
        }
    }
}
