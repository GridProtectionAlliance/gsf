//******************************************************************************************************
//  FuzzyStringComparisonOptions.cs - Gbtc
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
    [Flags]
    public enum FuzzyStringComparisonOptions
    {
        UseHammingDistance = (int)Bits.Bit00,
        UseJaccardDistance = (int)Bits.Bit01,
        UseJaroDistance = (int)Bits.Bit02,
        UseJaroWinklerDistance = (int)Bits.Bit03,
        UseLevenshteinDistance = (int)Bits.Bit04,
        UseLongestCommonSubsequence = (int)Bits.Bit05,
        UseLongestCommonSubstring = (int)Bits.Bit06,
        UseNormalizedLevenshteinDistance = (int)Bits.Bit07,
        UseOverlapCoefficient = (int)Bits.Bit08,
        UseRatcliffObershelpSimilarity = (int)Bits.Bit09,
        UseSorensenDiceDistance = (int)Bits.Bit10,
        UseTanimotoCoefficient = (int)Bits.Bit11,
        CaseSensitive = (int)Bits.Bit12
    }
}
