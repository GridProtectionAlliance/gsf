//******************************************************************************************************
//  PERUnalignedEncoder.cs - Gbtc
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
    public class PERUnalignedEncoder : PERAlignedEncoder
    {
        protected override int encodeConstraintNumber(long val, long min, long max, BitArrayOutputStream stream)
        {
            int result = 0;
            long valueRange = max - min;
            long narrowedVal = val - min;
            int maxBitLen = PERCoderUtils.getMaxBitLength(valueRange);

            if (valueRange == 0)
            {
                return result;
            }

            //For the UNALIGNED variant the value is always encoded in the minimum 
            // number of bits necessary to represent the range (defined in 10.5.3). 
            int currentBit = maxBitLen;
            while (currentBit > 8)
            {
                currentBit -= 8;
                result++;
                stream.WriteByte((byte)(narrowedVal >> currentBit));
            }
            if (currentBit > 0)
            {
                for (int i = currentBit - 1; i >= 0; i--)
                {
                    int bitValue = (int)((narrowedVal >> i) & 0x1);
                    stream.writeBit(bitValue);
                }
                result += 1;
            }
            return result;
        }

        public override int encodeString(Object obj, Stream stream, ElementInfo elementInfo)
        {
            if (!PERCoderUtils.is7BitEncodedString(elementInfo))
                return base.encodeString(obj, stream, elementInfo);
            else
            {
                int resultSize = 0;
                byte[] val = Encoding.ASCII.GetBytes((string)obj);
                resultSize = encodeLength(val.Length, elementInfo, stream);
                if (val.Length == 0)
                    return resultSize;


                BitArrayOutputStream bitStream = (BitArrayOutputStream)stream;
                // 7-bit encoding of string
                for (int i = 0; i < val.Length; i++)
                {
                    bitStream.writeBits(val[i], 7);
                }
                return resultSize;
            }
        }

        protected override void doAlign(Stream stream)
        {
            // Do nothing! Unaligned encoding ;)
        }
    }
}