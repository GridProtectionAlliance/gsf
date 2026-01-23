//******************************************************************************************************
//  LevenshteinDistance.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/14/2013 - Kevin D. Jones
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

// TODO: Please add comments to these classes
#pragma warning disable 1591

namespace GSF.FuzzyStrings
{
    public static partial class ComparisonMetrics
    {
        public static int LevenshteinDistance(this string source, string target)
        {
            // Optimize for wider matrix M, resulting in fewer swaps
            if (source.Length > target.Length)
                return target.LevenshteinDistance(source);

            // Given a matrix M where M[i,j] is the Levenshtein distance of the
            // first i characters in source and the first j characters in target,
            // arr0 and arr1 represent two consecutive rows of matrix M
            int rows = source.Length + 1;
            int columns = target.Length + 1;
            int[] arr0 = new int[columns];
            int[] arr1 = new int[columns];

            // This fills in M[0] of the matrix
            // If source is empty, the distance is the number of characters in target
            for (int i = 0; i < columns; i++)
                arr0[i] = i;

            // In the following loop, arr0 is M[i-1] and arr1 is M[i]
            // We fill in the values for M[i] given that M[i-1] has already been filled in
            for (int i = 1; i < rows; i++)
            {
                // Fill in M[i,0]
                // If target is empty, the distance is the number of characters in source
                arr1[0] = i;

                for (int j = 1; j < columns; j++)
                {
                    int distance = source[i - 1] == target[j - 1] ? 0 : 1;

                    // M[i,j] = min(M[i-1,j] + 1,
                    //              M[i,j-1] + 1,
                    //              M[i-1,j-1] + distance)
                    //
                    // This is the recursive case of Levenshtein using precomputed values instead of recursion
                    arr1[j] = Common.Min(arr0[j] + 1, arr1[j - 1] + 1, arr0[j - 1] + distance);
                }

                // Move M[i] into arr0 for the next iteration
                // We no longer need M[i-1] so we reuse that array for arr1 in the next iteration
                int[] temp = arr0;
                arr0 = arr1;
                arr1 = temp;
            }

            // After the final swap, arr0 has the final row of the matrix M[rows-1]
            // The last column contains the distance for all characters in source and all characters in target
            // aka M[i,j] where i == source.Length and j == target.Length
            return arr0[columns - 1];
        }

        public static double NormalizedLevenshteinDistance(this string source, string target)
        {
            int unnormalizedLevenshteinDistance = source.LevenshteinDistance(target);

            return unnormalizedLevenshteinDistance - source.LevenshteinDistanceLowerBounds(target);
        }

        public static int LevenshteinDistanceUpperBounds(this string source, string target)
        {
            // If the two strings are the same length then the Hamming Distance is the upper bounds of the Levenshtien Distance.
            if (source.Length == target.Length)
            {
                return source.HammingDistance(target);
            }

            // Otherwise, the upper bound is the length of the longer string.
            else if (source.Length > target.Length)
            {
                return source.Length;
            }
            else if (target.Length > source.Length)
            {
                return target.Length;
            }

            return 9999;
        }

        public static int LevenshteinDistanceLowerBounds(this string source, string target)
        {
            // If the two strings are the same length then the lower bound is zero.
            if (source.Length == target.Length)
            {
                return 0;
            }

            // If the two strings are different lengths then the lower bounds is the difference in length.
            else
            {
                return Math.Abs(source.Length - target.Length);
            }
        }

    }
}
