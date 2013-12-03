//******************************************************************************************************
//  BERObjectIdentifier.cs - Gbtc
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
using System.Collections;
using System.Text;

namespace GSF.ASN1.Coders.BER
{
    /**
     * BER OID Encoding
     * Implemented by Alan Gutzeit.
     */

    public static class BERObjectIdentifier
    {
        public static byte[] Encode(int[] oidArcArray)
        {
            int arcLength = oidArcArray.Length;
            if (arcLength < 2)
                throw new Exception("Object id must contain at least 2 arcs");
            byte[] result = new byte[(arcLength * 5)]; // 32-bit encoding cannot exceed 5 bytes each 
            int nextAvailable = 0;
            nextAvailable += EncodeFirstTwoArcs(oidArcArray[0], oidArcArray[1], result, nextAvailable);
            for (int i = 2; i < arcLength; i++)
            {
                nextAvailable += EncodeOneArc(oidArcArray[i], result, nextAvailable);
            }
            if (nextAvailable > 255)
                throw new Exception("Encoded length of object id exceeded 255 bytes");
            return Truncate(result, nextAvailable);
        }

        private static int EncodeFirstTwoArcs(int topArc, int secondArc, byte[] result, int nextAvailable)
        {
            if (topArc < 0 || topArc > 2)
                throw new Exception("Top-level arc must be 0, 1 or 2");
            if (secondArc < 0 || secondArc > 39)
                throw new Exception("Second-level arc must be less than 40");
            int combinedArc = topArc * 40 + secondArc;
            return EncodeOneArc(combinedArc, result, nextAvailable);
        }

        // Encoding: 
        //         11112222333344445555666677778888 
        // 00001111x2222333x3444455x5566667x7778888

        /// <summary>
        ///     Adds encoding to passed result array. Note that result array must already have adequate capacity.
        /// </summary>
        /// <returns>length of result</returns>
        private static int EncodeOneArc(int arc, byte[] result, int nextAvailable)
        {
            long arc1 = (arc & 0x7f);
            long arc2 = (arc & 0x3f80) << 1;
            long arc3 = (arc & 0x1fc000) << 2;
            long arc4 = (arc & 0xfe00000) << 3;
            long arc5 = (arc & 0xf0000000) << 4;
            long all = arc1 | arc2 | arc3 | arc4 | arc5;

            byte[] temp = new byte[5];
            temp[4] = (byte)((all & 0xff));
            temp[3] = (byte)((all & 0xff00) >> 8);
            temp[2] = (byte)((all & 0xff0000) >> 16);
            temp[1] = (byte)((all & 0xff000000) >> 24);
            temp[0] = (byte)((all & 0xff00000000) >> 32);

            int resultLength = 1;
            if (temp[0] > 0)
                resultLength = 5;
            else if (temp[1] > 0)
                resultLength = 4;
            else if (temp[2] > 0)
                resultLength = 3;
            else if (temp[3] > 0)
                resultLength = 2;
            // temp[4] can be zero if arc = 0 so resultLength defaults to 1 

            // all bytes have high-order bit one except last byte has high-order bit zero 
            temp[0] |= 0x80; // high-bit set
            temp[1] |= 0x80; // high-bit set
            temp[2] |= 0x80; // high-bit set
            temp[3] |= 0x80; // high-bit set
            temp[4] &= 0x7f; // high-bit reset

            int sourceIndex = 5 - resultLength;
            Array.Copy(temp, sourceIndex, result, nextAvailable, resultLength);
            return resultLength;
        }

        // return new array by truncating passed array.
        private static byte[] Truncate(byte[] b1, int nextAvailable)
        {
            byte[] b2 = new byte[nextAvailable];
            Array.Copy(b1, b2, nextAvailable);
            return b2;
        }

        // =========================================================================

        public static string Decode(byte[] oidBytes)
        {
            int[] intArray1 = BerByteArrayToIntArray(oidBytes);
            if (intArray1.Length < 1)
                throw new Exception("Object id must contain at least 2 arcs");
            int[] intArray2 = new int[intArray1.Length + 1];
            int combinedArc = intArray1[0];
            int arc1, arc2;
            if (combinedArc < 40)
            {
                arc1 = 0;
                arc2 = combinedArc;
            }
            else if (combinedArc < 80)
            {
                arc1 = 1;
                arc2 = combinedArc - 40;
            }
            else
            {
                arc1 = 2;
                arc2 = combinedArc - 80;
            }
            intArray2[0] = arc1;
            intArray2[1] = arc2;
            Array.Copy(intArray1, 1, intArray2, 2, intArray2.Length - 2);
            return IntArrayToDottedDecimal(intArray2);
        }

        public static string IntArrayToDottedDecimal(int[] oidIntArray)
        {
            StringBuilder sb = new StringBuilder(oidIntArray.Length * 4);
            for (int i = 0; i < oidIntArray.Length; i++)
            {
                if (sb.Length > 0)
                    sb.Append('.');
                sb.Append(oidIntArray[i].ToString().Trim());
            }
            return sb.ToString();
        }

        // Decoding: 
        // xxxx1111x2222333x3444455x5566667x7778888
        //         11112222333344445555666677778888 
        //

        public static int[] BerByteArrayToIntArray(byte[] berBytes)
        {
            ArrayList intArrayList = new ArrayList();
            ArrayList oneArcSequence = new ArrayList();
            int byteCount = 0;
            for (int i = 0; i < berBytes.Length; i++)
            {
                if ((berBytes[i] & 0x80) == 0) // last byte in arc
                {
                    oneArcSequence.Add(berBytes[i]);
                    intArrayList.Add(DecodeOneArc(oneArcSequence));
                    oneArcSequence = new ArrayList();
                    byteCount = 0;
                }
                else // not last byte in arc
                {
                    if (byteCount == 5)
                        throw new Exception("Conversion can only handle 5 bytes");
                    oneArcSequence.Add(berBytes[i]);
                    byteCount++;
                }
            }
            return (int[])intArrayList.ToArray(typeof(int));
        }

        // returns an single integer arc from an list of BER encoded bytes.  Note that the byte array must 
        // already contain one and only one encoded arc sequence. 
        private static int DecodeOneArc(ArrayList berByteList)
        {
            if (berByteList.Count < 1 || berByteList.Count > 5)
                throw new Exception("Conversion requires from 1 to 5 bytes");
            long all = 0;

            int startByteIndex = berByteList.Count - 1;
            byte[] berBytes = (byte[])berByteList.ToArray(typeof(byte));
            for (int i = 0; i <= startByteIndex; i++)
            {
                berBytes[i] = (byte)(berBytes[i] & 0x7f); // make continuation bit zero so it doesn't affect the result
                if (i != 0)
                    all = all << 7;
                all = all | berBytes[i];
            }
            return (int)all;
        }
    }
}