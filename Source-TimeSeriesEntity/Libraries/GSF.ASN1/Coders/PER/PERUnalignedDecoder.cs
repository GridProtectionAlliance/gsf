//******************************************************************************************************
//  PERUnalignedDecoder.cs - Gbtc
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
//  09/24/2013 - J. Ritchie Carroll
//       Derived original version of source code from BinaryNotes (http://bnotes.sourceforge.net).
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

/*
    Copyright 2006-2011 Abdulla Abdurakhmanov (abdulla@latestbit.com)
    Original sources are available at www.latestbit.com

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

using System;
using System.IO;
using System.Text;
using GSF.ASN1.Utilities;

namespace GSF.ASN1.Coders.PER
{
    public class PERUnalignedDecoder : PERAlignedDecoder
    {
        protected override void skipAlignedBits(Stream stream)
        {
            // Do nothing! Unaligned encoding ;)
        }

        protected override long decodeConstraintNumber(long min, long max, BitArrayInputStream stream)
        {
            long result = 0;
            long valueRange = max - min;
            // !!! int narrowedVal = value - min; !!!
            int maxBitLen = PERCoderUtils.getMaxBitLength(valueRange);

            if (valueRange == 0)
            {
                return max;
            }
            //For the UNALIGNED variant the value is always encoded in the minimum 
            // number of bits necessary to represent the range (defined in 10.5.3). 
            int currentBit = maxBitLen;
            while (currentBit > 7)
            {
                currentBit -= 8;
                result |= (uint)(stream.ReadByte() << currentBit);
            }
            if (currentBit > 0)
            {
                result |= (uint)(stream.readBits(currentBit));
            }
            result += min;
            return result;
        }

        public override DecodedObject<object> decodeString(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream)
        {
            if (!PERCoderUtils.is7BitEncodedString(elementInfo))
                return base.decodeString(decodedTag, objectClass, elementInfo, stream);
            else
            {
                DecodedObject<object> result = new DecodedObject<object>();
                int strLen = decodeLength(elementInfo, stream);

                if (strLen <= 0)
                {
                    result.Value = ("");
                    return result;
                }

                BitArrayInputStream bitStream = (BitArrayInputStream)stream;
                byte[] buffer = new byte[strLen];
                // 7-bit decoding of string
                for (int i = 0; i < strLen; i++)
                    buffer[i] = (byte)bitStream.readBits(7);
                result.Value = new string(
                    Encoding.ASCII.GetChars(buffer)
                    );
                return result;
            }
        }
    }
}