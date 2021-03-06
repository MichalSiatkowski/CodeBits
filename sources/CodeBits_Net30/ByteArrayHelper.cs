﻿#region --- License & Copyright Notice ---
/*
Useful code blocks that can included in your C# projects through NuGet
Copyright (c) 2012-2019 Jeevan James
All rights reserved.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
#endregion

/* Documentation: https://github.com/JeevanJames/CodeBits/wiki/ByteArrayHelper */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CodeBits
{
    /// <summary>
    ///     Set of utility extension methods for byte arrays.
    /// </summary>
    public static class ByteArrayHelper
    {
        /// <summary>
        ///     Populates each item in a byte array with a specific value.
        /// </summary>
        /// <param name="source"> The byte array to be populated. </param>
        /// <param name="value"> The value to populate the byte array with. </param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void Fill(this byte[] source, byte value)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (source.Length == 0)
                return;
            for (int i = 0; i < source.Length; i++)
                source[i] = value;
        }

        /// <summary>
        ///     Checks whether two byte arrays are equal based on their content.
        /// </summary>
        /// <param name="source"> The first byte array to check. </param>
        /// <param name="other"> The second byte array to check. </param>
        /// <returns> True if the contents of the two byte arrays are equal. </returns>
        public static bool IsEqualTo(this byte[] source, params byte[] other)
        {
            if (ReferenceEquals(source, other))
                return true;
            if (source == null || other == null)
                return false;
            if (source.Length != other.Length)
                return false;
            for (int byteIdx = 0; byteIdx < source.Length; byteIdx++)
            {
                if (source[byteIdx] != other[byteIdx])
                    return false;
            }
            return true;
        }

        /// <summary>
        ///     Indicates whether the specified byte array is null or does not contain any elements.
        /// </summary>
        /// <param name="source"> The byte array to test. </param>
        /// <returns> True if the byte array is either null or empty; other False. </returns>
        public static bool IsNullOrEmpty(this byte[] source)
        {
            return source == null || source.Length == 0;
        }

        /// <summary>
        ///     Indicates whether the specified byte array is null, does not contain any elements or consists
        ///     of only zero value items.
        /// </summary>
        /// <param name="source"> The byte array to test. </param>
        /// <returns> True if the byte array is null, does not contain any elements or consists exclusively of zero value items. </returns>
        public static bool IsNullOrZeroed(this byte[] source)
        {
            if (source == null || source.Length == 0)
                return true;
            return source.All(b => b == 0);
        }

        /// <summary>
        ///     Creates a string from a byte array that concatenates each item in the array, separated by
        ///     the specified delimiter.
        /// </summary>
        /// <param name="source"> The byte array from which to create the string. </param>
        /// <param name="delimiter"> The optional delimiter to separate each item in the array. Default to ',' if not specified. </param>
        /// <returns> The combined string </returns>
        public static string ToString(this byte[] source, string delimiter)
        {
            if (source == null)
                return null;
            if (source.Length == 0)
                return string.Empty;

            if (delimiter == null)
                throw new ArgumentNullException("delimiter");
            bool useDelimiter = delimiter.Length > 0;

            var result = new StringBuilder(source[0].ToString(CultureInfo.InvariantCulture),
                (source.Length * 3) + ((source.Length - 1) * delimiter.Length));
            for (int i = 1; i < source.Length; i++)
            {
                if (useDelimiter)
                    result.Append(delimiter);
                result.Append(source[i]);
            }
            return result.ToString();
        }

        /// <summary>
        /// Retrieves the bytes from a byte array upto a specific sequence.
        /// </summary>
        /// <param name="source">The byte array</param>
        /// <param name="start">The index in the array to start checking from</param>
        /// <param name="sequence">The sequence to search for</param>
        /// <returns>An array of bytes from the starting index to the matching sequence.</returns>
        public static byte[] GetBytesUptoSequence(this byte[] source, int start, params byte[] sequence)
        {
            int sequenceIndex = IndexOfSequence(source, start, source.Length - start + 1, sequence);
            if (sequenceIndex == -1)
                return null;
            var result = new byte[sequenceIndex - start];
            Array.Copy(source, start, result, 0, result.Length);
            return result;
        }

        public static int IndexOfSequence(this byte[] source, params byte[] sequence)
        {
            return IndexOfSequence(source, 0, source.Length, sequence);
        }

        public static int IndexOfSequence(this byte[] source, int start, int count, byte[] sequence)
        {
            var sequenceIndex = 0;
            int endIndex = Math.Min(source.Length, start + count);
            for (int byteIdx = start; byteIdx < endIndex; byteIdx++)
            {
                if (source[byteIdx] == sequence[sequenceIndex])
                {
                    sequenceIndex++;
                    if (sequenceIndex >= sequence.Length)
                        return byteIdx - sequence.Length + 1;
                }
                else
                {
                    if (sequenceIndex > 0)
                        byteIdx = byteIdx - sequenceIndex;
                    sequenceIndex = 0;
                }
            }
            return -1;
        }

        public static int[] IndexOfSequences(this byte[] source, params byte[] sequence)
        {
            return IndexOfSequences(source, 0, source.Length, sequence);
        }

        public static int[] IndexOfSequences(this byte[] source, int start, int count, byte[] sequence)
        {
            var locations = new List<int>();

            int sequenceLocation = IndexOfSequence(source, start, count, sequence);
            while (sequenceLocation >= 0)
            {
                locations.Add(sequenceLocation);
                count -= sequenceLocation - start + 1;
                start = sequenceLocation + sequence.Length;
                sequenceLocation = IndexOfSequence(source, start, count, sequence);
            }

            return locations.ToArray();
        }

        public static byte[][] SplitBySequence(this byte[] source, params byte[] sequence)
        {
            return SplitBySequence(source, 0, source.Length, sequence);
        }

        public static byte[][] SplitBySequence(this byte[] source, int start, int count, byte[] sequence)
        {
            if (start + count > source.Length)
                count = source.Length - start;

            int[] locations = IndexOfSequences(source, start, count, sequence);
            if (locations.Length == 0)
                return new[] { source };

            var results = new List<byte[]>(locations.Length + 1);
            for (int locationIdx = 0; locationIdx < locations.Length; locationIdx++)
            {
                int startIndex = locationIdx > 0 ? locations[locationIdx - 1] + sequence.Length : start;
                int endIndex = locations[locationIdx] - 1;
                if (endIndex < startIndex)
                    results.Add(new byte[0]);
                else
                {
                    var splitBytes = new byte[endIndex - startIndex + 1];
                    Array.Copy(source, startIndex, splitBytes, 0, splitBytes.Length);
                    results.Add(splitBytes);
                }
            }

            if (locations[locations.Length - 1] + sequence.Length > start + count - 1)
                results.Add(new byte[0]);
            else
            {
                var splitBytes = new byte[start + count - locations[locations.Length - 1] - sequence.Length];
                Array.Copy(source, locations[locations.Length - 1] + sequence.Length, splitBytes, 0, splitBytes.Length);
                results.Add(splitBytes);
            }

            return results.ToArray();
        }
    }
}
