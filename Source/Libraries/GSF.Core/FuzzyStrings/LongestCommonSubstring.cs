//******************************************************************************************************
//  LongestCommonSubstring.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
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
using System.Diagnostics.CodeAnalysis;
using System.Text;

// TODO: Please add comments to these classes
#pragma warning disable 1591

namespace GSF.FuzzyStrings
{
    public static partial class ComparisonMetrics
    {
        [SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
        public static string LongestCommonSubstring(this string source, string target)
        {
            if (String.IsNullOrEmpty(source) || String.IsNullOrEmpty(target))
            {
                return null;
            }

            int[,] L = new int[source.Length, target.Length];
            int maximumLength = 0;
            int lastSubsBegin = 0;
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < source.Length; i++)
            {
                for (int j = 0; j < target.Length; j++)
                {
                    if (source[i] != target[j])
                    {
                        L[i, j] = 0;
                    }
                    else
                    {
                        if ((i == 0) || (j == 0))
                            L[i, j] = 1;
                        else
                            L[i, j] = 1 + L[i - 1, j - 1];

                        if (L[i, j] > maximumLength)
                        {
                            maximumLength = L[i, j];
                            int thisSubsBegin = i - L[i, j] + 1;
                            if (lastSubsBegin == thisSubsBegin)
                            {//if the current LCS is the same as the last time this block ran
                                stringBuilder.Append(source[i]);
                            }
                            else //this block resets the string builder if a different LCS is found
                            {
                                lastSubsBegin = thisSubsBegin;
                                stringBuilder.Length = 0; //clear it
                                stringBuilder.Append(source.Substring(lastSubsBegin, (i + 1) - lastSubsBegin));
                            }
                        }
                    }
                }
            }

            return stringBuilder.ToString();
        }

    }
}
