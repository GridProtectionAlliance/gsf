//******************************************************************************************************
//  TsscCodeWords.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  12/02/2016 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

namespace GSF.TimeSeries.Transport.TSSC
{
    /// <summary>
    /// The encoding commands supported by TSSC. This class is used by 
    /// <see cref="TsscDecoder"/> and <see cref="TsscEncoder"/>.
    /// </summary>
    internal static class TsscCodeWords
    {
        public const byte EndOfStream = 0;

        public const byte PointIDXOR4 = 1;
        public const byte PointIDXOR8 = 2;
        public const byte PointIDXOR12 = 3;
        public const byte PointIDXOR16 = 4;

        public const byte TimeDelta1Forward = 5;
        public const byte TimeDelta2Forward = 6;
        public const byte TimeDelta3Forward = 7;
        public const byte TimeDelta4Forward = 8;
        public const byte TimeDelta1Reverse = 9;
        public const byte TimeDelta2Reverse = 10;
        public const byte TimeDelta3Reverse = 11;
        public const byte TimeDelta4Reverse = 12;
        public const byte Timestamp2 = 13;
        public const byte TimeXOR7Bit = 14;

        public const byte Quality2 = 15;
        public const byte Quality7Bit32 = 16;

        public const byte Value1 = 17;
        public const byte Value2 = 18;
        public const byte Value3 = 19;
        public const byte ValueZero = 20;
        public const byte ValueXOR4 = 21;
        public const byte ValueXOR8 = 22;
        public const byte ValueXOR12 = 23;
        public const byte ValueXOR16 = 24;
        public const byte ValueXOR20 = 25;
        public const byte ValueXOR24 = 26;
        public const byte ValueXOR28 = 27;
        public const byte ValueXOR32 = 28;
    }
}
