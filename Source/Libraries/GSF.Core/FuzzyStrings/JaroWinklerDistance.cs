//******************************************************************************************************
//  JaroWinklerDistance.cs - Gbtc
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

// TODO: Please add comments to these classes
#pragma warning disable 1591

namespace GSF.FuzzyStrings
{
    public static partial class ComparisonMetrics
    {
        public static double JaroWinklerDistance(this string source, string target)
        {
            double jaroDistance = source.JaroDistance(target);
            double commonPrefixLength = CommonPrefixLength(source, target);

            return jaroDistance + (commonPrefixLength * 0.1 * (1 - jaroDistance));
        }

        public static double JaroWinklerDistanceWithPrefixScale(string source, string target, double p)
        {
            double prefixScale = 0.1;

            if (p > 0.25)
            {
                prefixScale = 0.25;
            } // The maximu value for distance to not exceed 1
            else if (p < 0)
            {
                prefixScale = 0;
            } // The Jaro Distance
            else
            {
                prefixScale = p;
            }

            double jaroDistance = source.JaroDistance(target);
            double commonPrefixLength = CommonPrefixLength(source, target);

            return jaroDistance + (commonPrefixLength * prefixScale * (1 - jaroDistance));
        }

        private static double CommonPrefixLength(string source, string target)
        {
            int maximumPrefixLength = 4;
            int commonPrefixLength = 0;
            if (source.Length <= 4 || target.Length <= 4)
            {
                maximumPrefixLength = Math.Min(source.Length, target.Length);
            }

            for (int i = 0; i < maximumPrefixLength; i++)
            {
                if (source[i].Equals(target[i]))
                {
                    commonPrefixLength++;
                }
                else
                {
                    return commonPrefixLength;
                }
            }

            return commonPrefixLength;
        }
    }
}
