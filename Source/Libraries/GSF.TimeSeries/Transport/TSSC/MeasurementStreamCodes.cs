//******************************************************************************************************
//  MeasurementStreamCodes.cs - Gbtc
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
    internal class MeasurementStreamCodes
    {
        /// <summary>
        /// A new measurement has been registered.
        /// <para>byte MeasurementTypeCode</para>
        /// <para>7BitUInt MetadataLength</para>
        /// <para>byte[] Metadata</para>
        /// </summary>
        public const byte NewPointId = 0;

        public const byte FlushBits = 3;

        public const byte PointIDXOR4 = 4;
        public const byte PointIDXOR8 = 5;
        public const byte PointIDXOR12 = 6;
        public const byte PointIDXOR16 = 7;
        public const byte PointIDXOR20 = 8;
        public const byte PointIDXOR24 = 9;
        public const byte PointIDXOR32 = 10;

        public const byte TimeDelta1Forward = 11;
        public const byte TimeDelta2Forward = 12;
        public const byte TimeDelta3Forward = 13;
        public const byte TimeDelta4Forward = 14;

        public const byte TimeDelta1Reverse = 15;
        public const byte TimeDelta2Reverse = 16;
        public const byte TimeDelta3Reverse = 17;
        public const byte TimeDelta4Reverse = 18;

        public const byte Timestamp2 = 19;
        public const byte TimeXOR7Bit = 20;

        public const byte Quality2 = 21;
        public const byte Quality7Bit32 = 22;


        public const byte Value1 = 32;
        public const byte Value2 = 33;
        public const byte Value3 = 34;
        public const byte ValueZero = 35;
        public const byte ValueXOR4 = 36;
        public const byte ValueXOR8 = 37;
        public const byte ValueXOR12 = 38;
        public const byte ValueXOR16 = 39;
        public const byte ValueXOR20 = 40;
        public const byte ValueXOR24 = 41;
        public const byte ValueXOR28 = 42;
        public const byte ValueXOR32 = 43;
    }
}
