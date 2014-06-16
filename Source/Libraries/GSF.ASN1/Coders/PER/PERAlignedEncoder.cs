//******************************************************************************************************
//  PERAlignedEncoder.cs - Gbtc
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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GSF.ASN1.Attributes;
using GSF.ASN1.Attributes.Constraints;
using GSF.ASN1.Coders.BER;
using GSF.ASN1.Metadata;
using GSF.ASN1.Metadata.Constraints;
using GSF.ASN1.Types;
using GSF.ASN1.Utilities;

namespace GSF.ASN1.Coders.PER
{
    public class PERAlignedEncoder : Encoder
    {
        public override void encode<T>(T obj, Stream stream)
        {
            BitArrayOutputStream bitStream = new BitArrayOutputStream();
            base.encode(obj, bitStream);
            bitStream.WriteTo(stream);
        }

        protected virtual int encodeIntegerValueAsBytes(long val, Stream stream)
        {
            int integerSize = CoderUtils.getIntegerLength(val);
            for (int i = integerSize - 1; i >= 0; i--)
            {
                long valueTmp = val >> (8 * i);
                stream.WriteByte((byte)valueTmp);
            }
            return integerSize;
        }

        /// <summary>
        ///     Encoding constraint length determinant procedure.
        ///     ITU-T X.691. 10.9. General rules for encoding a length determinant
        /// </summary>
        protected virtual int encodeConstraintLengthDeterminant(int length, int min, int max, BitArrayOutputStream stream)
        {
            if (max <= 0xFFFF)
            {
                // 10.9. NOTE 2 – (Tutorial) In the case of the ALIGNED variant 
                // if the length count is bounded above by an upper bound that is 
                // less than 64K, then the constrained whole number encoding 
                // is used for the length.
                return encodeConstraintNumber(length, min, max, stream); // encoding as constraint integer
            }
            else
                return encodeLengthDeterminant(length, stream);
        }

        /// <summary>
        ///     Encoding length determinant procedure.
        ///     ITU-T X.691. 10.9. General rules for encoding a length determinant
        /// </summary>
        protected virtual int encodeLengthDeterminant(int length, BitArrayOutputStream stream)
        {
            int result = 0;
            doAlign(stream);
            if (length >= 0 && length < 0x80)
            {
                // NOTE 2. a) ("n" less than 128)             
                stream.WriteByte(length); // a single octet containing "n" with bit 8 set to zero;
                result = 1;
            }
            else if (length < 0x4000)
            {
                // NOTE 2. b) ("n" less than 16K) two octets 
                // containing "n" with bit 8 of the first octet 
                // set to 1 and bit 7 set to zero;
                stream.WriteByte((length >> 8) & 0x3f | 0x80);
                stream.WriteByte(length);
                result = 2;
            }
            else
            {
                // NOTE 2. b) (large "n") a single octet containing a count "m" 
                // with bit 8 set to 1 and bit 7 set to 1. 
                // The count "m" is one to four, and the length indicates that 
                // a fragment of the material follows (a multiple "m" of 16K items). 
                // For all values of "m", the fragment is then followed 
                // by another length encoding for the remainder of the material.        
                throw new ApplicationException("Not supported for this version. Length too big!");
            }
            return result;
        }

        /// <summary>
        ///     Encoding of a constrained whole number
        ///     ITU-T X.691. 10.5.
        ///     NOTE – (Tutorial) This subclause is referenced by other clauses,
        ///     and itself references earlier clauses for the production of
        ///     a nonnegative-binary-integer or a 2's-complement-binary-integer encoding.
        /// </summary>
        protected virtual int encodeConstraintNumber(long val, long min, long max, BitArrayOutputStream stream)
        {
            int result = 0;
            long valueRange = max - min;
            long narrowedVal = val - min;
            int maxBitLen = PERCoderUtils.getMaxBitLength(valueRange);

            if (valueRange == 0)
            {
                return result;
            }

            // The rest of this Note addresses the ALIGNED variant. 
            if (valueRange > 0 && valueRange < 256)
            {
                /*
                * 1. Where the range is less than or equal to 255, the value encodes 
                * into a bit-field of the minimum size for the range. 
                * 2. Where the range is exactly 256, the value encodes 
                * into a single octet octet-aligned bit-field. 
                */
                doAlign(stream);
                for (int i = maxBitLen - 1; i >= 0; i--)
                {
                    int bitValue = (int)((narrowedVal >> i) & 0x1);
                    stream.writeBit(bitValue);
                }
                result = 1;
            }
            else if (valueRange > 0 && valueRange < 65536)
            {
                /* 
                * 3. Where the range is 257 to 64K, the value encodes into 
                * a two octet octet-aligned bit-field. 
                */
                doAlign(stream);
                stream.WriteByte((byte)(narrowedVal >> 8));
                stream.WriteByte((byte)(narrowedVal & 0xFF));
                result = 2;
            }
            else
            {
                /*
                * 4. Where the range is greater than 64K, the range is ignored 
                * and the value encodes into an  octet-aligned bit-field 
                * which is the minimum number of octets for the value. 
                * In this latter case, later procedures (see 10.9)
                * also encode a length field (usually a single octet) to indicate 
                * the length of the encoding. For the other cases, the length 
                * of the encoding is independent of the value being encoded, 
                * and is not explicitly encoded.
                */
                result = encodeConstraintLengthDeterminant(CoderUtils.getIntegerLength(narrowedVal), 1, CoderUtils.getPositiveIntegerLength(valueRange), stream);
                doAlign(stream);
                result += encodeIntegerValueAsBytes(narrowedVal, stream);
            }
            return result;
        }

        /// <summary>
        ///     Encoding of a semi-constrained whole number
        ///     ITU-T X.691. 10.7.
        ///     NOTE – (Tutorial) This procedure is used when a lower bound can be
        ///     identified but not an upper bound. The encoding procedure places
        ///     the offset from the lower bound into the minimum number of octets
        ///     as a non-negative-binary-integer, and requires an explicit length
        ///     encoding (typically a single octet) as specified in later procedures.
        /// </summary>
        protected virtual int encodeSemiConstraintNumber(int val, int min, BitArrayOutputStream stream)
        {
            int result = 0;
            int narrowedVal = val - min;
            int intLen = CoderUtils.getIntegerLength(narrowedVal);
            result += encodeLengthDeterminant(intLen, stream);
            doAlign(stream);
            result += encodeIntegerValueAsBytes(narrowedVal, stream);
            return result;
        }


        /// <summary>
        ///     Encode normally small number
        ///     ITU-T X.691. 10.6
        ///     NOTE – (Tutorial) This procedure is used when encoding
        ///     a non-negative whole number that is expected to be small, but whose size
        ///     is potentially unlimited due to the presence of an extension marker.
        ///     An example is a choice index.
        /// </summary>
        protected virtual int encodeNormallySmallNumber(int val, BitArrayOutputStream stream)
        {
            int result = 0;
            if (val > 0 && val < 64)
            {
                /* 10.6.1 If the non-negative whole number, "n", is less than 
                * or equal to 63, then a single-bit bit-field shall be appended
                * to the field-list with the bit set to 0, and "n" shall be 
                * encoded as a non-negative-binary-integer into a 6-bit bit-field.
                */
                stream.writeBit(0);
                for (int i = 0; i < 6; i++)
                {
                    int bitValue = (val >> 6 - i) & 0x1;
                    stream.writeBit(bitValue);
                }
                result = 1;
            }
            else
            {
                /* If "n" is greater than or equal to 64, a single-bit 
                * bit-field with the bit set to 1 shall be appended to the field-list.
                * The value "n" shall then be encoded as a semi-constrained 
                * whole number with "lb" equal to 0 and the procedures of 
                * 10.9 shall be invoked to add it to the field-list preceded 
                * by a length determinant.
                */
                stream.writeBit(1);
                result += encodeSemiConstraintNumber(val, 0, stream);
            }
            return result;
        }

        /// <summary>
        ///     Encoding of a unconstrained whole number
        ///     ITU-T X.691. 10.8.
        ///     NOTE – (Tutorial) This case only arises in the encoding of the
        ///     value of an integer type with no lower bound. The procedure
        ///     encodes the value as a 2's-complement-binary-integer into
        ///     the minimum number of octets required to accommodate the encoding,
        ///     and requires an explicit length encoding (typically a single octet)
        ///     as specified in later procedures.
        /// </summary>
        protected virtual int encodeUnconstraintNumber(long val, BitArrayOutputStream stream)
        {
            int result = 0;
            int intLen = CoderUtils.getIntegerLength(val);
            result += encodeLengthDeterminant(intLen, stream);
            doAlign(stream);
            result += encodeIntegerValueAsBytes(val, stream);
            return result;
        }

        public override int encodeInteger(object obj, Stream stream, ElementInfo elementInfo)
        {
            int result = 0;
            bool hasConstraint = false;
            long min = 0, max = 0;

            if (elementInfo.hasPreparedInfo())
            {
                if (elementInfo.PreparedInfo.hasConstraint()
                    && elementInfo.PreparedInfo.Constraint is ASN1ValueRangeConstraintMetadata)
                {
                    IASN1ConstraintMetadata constraint = elementInfo.PreparedInfo.Constraint;
                    hasConstraint = true;
                    min = ((ASN1ValueRangeConstraintMetadata)constraint).Min;
                    max = ((ASN1ValueRangeConstraintMetadata)constraint).Max;
                }
            }
            else if (elementInfo.isAttributePresent<ASN1ValueRangeConstraint>())
            {
                hasConstraint = true;
                ASN1ValueRangeConstraint constraint = elementInfo.getAttribute<ASN1ValueRangeConstraint>();
                min = constraint.Min;
                max = constraint.Max;
            }

            if (obj.GetType().Equals(typeof(int)))
            {
                int val = (int)obj;
                BitArrayOutputStream bitStream = (BitArrayOutputStream)stream;
                if (hasConstraint)
                {
                    result += encodeConstraintNumber(val, min, max, bitStream);
                }
                else
                    result += encodeUnconstraintNumber(val, bitStream);
            }
            else
            {
                long val = (long)obj;
                BitArrayOutputStream bitStream = (BitArrayOutputStream)stream;
                if (hasConstraint)
                {
                    result += encodeConstraintNumber(val, min, max, bitStream);
                }
                else
                    result += encodeUnconstraintNumber(val, bitStream);
            }
            return result;
        }

        public override int encodeReal(object obj, Stream stream, ElementInfo elementInfo)
        {
            int result = 0;
            BitArrayOutputStream bitStream = (BitArrayOutputStream)stream;
            Double value = (Double)obj;
            //CoderUtils.checkConstraints(value,elementInfo);

#if PocketPC
            byte[] dblValAsBytes =  System.BitConverter.GetBytes(value);
            long asLong = System.BitConverter.ToInt64(dblValAsBytes, 0);
#else
            long asLong = BitConverter.DoubleToInt64Bits(value);
#endif
            if (value == Double.PositiveInfinity)
            {
                // positive infinity
                result += encodeLengthDeterminant(1, bitStream);
                doAlign(stream);
                stream.WriteByte(0x40); // 01000000 Value is PLUS-INFINITY
                result += 1;
            }
            else if (value == Double.NegativeInfinity)
            {
                // negative infinity
                result += encodeLengthDeterminant(1, bitStream);
                doAlign(stream);
                stream.WriteByte(0x41); // 01000001 Value is MINUS-INFINITY
                result += 1;
            }
            else if (asLong != 0x0)
            {
                long exponent = ((0x7ff0000000000000L & asLong) >> 52) - 1023 - 52;
                long mantissa = 0x000fffffffffffffL & asLong;
                mantissa |= 0x10000000000000L; // set virtual delimiter

                // pack mantissa for base 2
                while ((mantissa & 0xFFL) == 0x0)
                {
                    mantissa >>= 8;
                    exponent += 8; //increment exponent to 8 (base 2)
                }
                while ((mantissa & 0x01L) == 0x0)
                {
                    mantissa >>= 1;
                    exponent += 1; //increment exponent to 1
                }

                int szOfExp = CoderUtils.getIntegerLength(exponent);
                encodeLengthDeterminant(CoderUtils.getIntegerLength(mantissa) + szOfExp + 1, bitStream);
                doAlign(stream);
                byte realPreamble = 0x80;

                realPreamble |= (byte)(szOfExp - 1);
                if ((((ulong)asLong) & 0x8000000000000000L) == 1)
                {
                    realPreamble |= 0x40; // Sign
                }
                stream.WriteByte(realPreamble);
                result += 1;


                result += encodeIntegerValueAsBytes(exponent, stream);
                result += encodeIntegerValueAsBytes(mantissa, stream);
            }
            return result;
        }

        protected int encodeLength(int val, ElementInfo elementInfo, Stream stream)
        {
            CoderUtils.checkConstraints(val, elementInfo);
            int resultSize = 0;
            BitArrayOutputStream bitStream = (BitArrayOutputStream)stream;
            if (elementInfo.hasPreparedInfo())
            {
                if (elementInfo.PreparedInfo.hasConstraint())
                {
                    IASN1ConstraintMetadata constraint = elementInfo.PreparedInfo.Constraint;
                    if (constraint is ASN1ValueRangeConstraintMetadata)
                    {
                        resultSize += encodeConstraintLengthDeterminant(
                            val,
                            (int)((ASN1ValueRangeConstraintMetadata)constraint).Min,
                            (int)((ASN1ValueRangeConstraintMetadata)constraint).Max,
                            bitStream
                            );
                    }
                    else if (constraint is ASN1SizeConstraintMetadata)
                    {
                    }
                }
                else
                    resultSize += encodeLengthDeterminant(val, bitStream);
            }
            else
            {
                if (elementInfo.isAttributePresent<ASN1ValueRangeConstraint>())
                {
                    ASN1ValueRangeConstraint constraint = elementInfo.getAttribute<ASN1ValueRangeConstraint>();
                    resultSize += encodeConstraintLengthDeterminant(val, (int)constraint.Min, (int)constraint.Max, bitStream);
                }
                else if (elementInfo.isAttributePresent<ASN1SizeConstraint>())
                {
                    ASN1SizeConstraint constraint = elementInfo.getAttribute<ASN1SizeConstraint>();
                }
                else
                    resultSize += encodeLengthDeterminant(val, bitStream);
            }
            return resultSize;
        }

        public virtual int encodeSequencePreamble(object obj, PropertyInfo[] fields, Stream stream, ElementInfo elementInfo)
        {
            int resultBitSize = 0;
            ElementInfo info = new ElementInfo();
            int fieldIdx = 0;
            foreach (PropertyInfo field in fields) // obj.GetType().GetProperties()
            {
                if (elementInfo.hasPreparedInfo())
                    info.PreparedInfo = elementInfo.PreparedInfo.getPropertyMetadata(fieldIdx);
                if (CoderUtils.isOptionalField(field, info))
                {
                    object invokeObjResult = invokeGetterMethodForField(field, obj, info);
                    ((BitArrayOutputStream)stream).writeBit(invokeObjResult != null);
                    resultBitSize += 1;
                }
                fieldIdx++;
            }
            doAlign(stream);
            return (resultBitSize / 8) + (resultBitSize % 8 > 0 ? 1 : 0);
        }

        public override int encodeSequence(object obj, Stream stream, ElementInfo elementInfo)
        {
            int resultSize = 0;
            if (!CoderUtils.isSequenceSet(elementInfo))
            {
                resultSize += encodeSequencePreamble(obj, elementInfo.getProperties(obj.GetType()), stream, elementInfo);
                resultSize += base.encodeSequence(obj, stream, elementInfo);
            }
            else
            {
                resultSize += encodeSet(obj, stream, elementInfo);
            }
            return resultSize;
        }

        public virtual int encodeChoicePreamble(object obj, Stream stream, int elementIndex, ElementInfo elementInfo)
        {
            return encodeConstraintNumber(elementIndex, 1, elementInfo.getProperties(obj.GetType()).Length, (BitArrayOutputStream)stream);
        }

        /// <summary>
        ///     Encoding of the choice structure
        ///     ITU-T X.691. 22.
        ///     NOTE – (Tutorial) A choice type is encoded by encoding an index specifying
        ///     the chosen alternative. This is encoded as for a constrained integer
        ///     (unless the extension marker is present in the choice type,
        ///     in which case it is a normally small non-negative whole number)
        ///     and would therefore typically occupy a fixed length bit-field of the
        ///     minimum number of bits needed to encode the index. (Although it could
        ///     in principle be arbitrarily large.) This is followed by the encoding
        ///     of the chosen alternative, with alternatives that are extension
        ///     additions encoded as if they were the value of an open type field.
        ///     Where the choice has only one alternative, there is no encoding
        ///     for the index.
        /// </summary>
        public override int encodeChoice(object obj, Stream stream, ElementInfo elementInfo)
        {
            int resultSize = 0;
            doAlign(stream);
            ElementInfo info = null;

            int elementIndex = 0;
            int fieldIdx = 0;
            foreach (PropertyInfo field in elementInfo.getProperties(obj.GetType()))
            {
                info = new ElementInfo();
                info.AnnotatedClass = field;

                elementIndex++;
                if (elementInfo.hasPreparedInfo())
                    info.PreparedInfo = elementInfo.PreparedInfo.getPropertyMetadata(fieldIdx);
                else
                    info.ASN1ElementInfo = CoderUtils.getAttribute<ASN1Element>(field);

                if (invokeSelectedMethodForField(field, obj, info))
                {
                    break;
                }
                else
                    info = null;
                fieldIdx++;
            }
            if (info == null)
            {
                throw new ArgumentException("The choice '" + obj + "' does not have a selected item!");
            }
            object invokeObjResult = invokeGetterMethodForField((PropertyInfo)info.AnnotatedClass, obj, info);
            resultSize += encodeChoicePreamble(obj, stream, elementIndex, elementInfo);
            resultSize += encodeClassType(invokeObjResult, stream, info);
            return resultSize;
        }

        public override int encodeEnumItem(object enumConstant, Type enumClass, Stream stream, ElementInfo elementInfo)
        {
            ASN1EnumItem enumObj = elementInfo.getAttribute<ASN1EnumItem>();
            //int min = 0, max = enumClass.GetFields().Length, val = 0;
            int min = 0, max = 0, val = 0;
            foreach (FieldInfo enumItem in enumClass.GetFields())
            {
                if (CoderUtils.isAttributePresent<ASN1EnumItem>(enumItem))
                {
                    ASN1EnumItem enumItemObj = CoderUtils.getAttribute<ASN1EnumItem>(enumItem);
                    if (enumItemObj.Tag == enumObj.Tag)
                        val = max;
                    max++; //val++;
                }
            }
            if (max > 0)
                return encodeConstraintNumber(val, min, max, (BitArrayOutputStream)stream);
            else
                throw new Exception("Unable to present any enum item!");
        }

        public override int encodeBoolean(object obj, Stream stream, ElementInfo elementInfo)
        {
            int resultSize = 1;
            BitArrayOutputStream bitStream = (BitArrayOutputStream)stream;
            bitStream.writeBit((Boolean)obj);
            return resultSize;
        }

        public override int encodeAny(object obj, Stream stream, ElementInfo elementInfo)
        {
            int resultSize = 0, sizeOfString = 0;
            byte[] buffer = (byte[])obj;
            stream.Write(buffer, 0, buffer.Length);
            sizeOfString = buffer.Length;
            resultSize += sizeOfString;
            return resultSize;
        }

        public override int encodeOctetString(object obj, Stream stream, ElementInfo elementInfo)
        {
            int resultSize = 0, sizeOfString = 0;
            byte[] buffer = (byte[])obj;
            sizeOfString = buffer.Length;
            resultSize += encodeLength(sizeOfString, elementInfo, stream);
            doAlign(stream);
            if (sizeOfString > 0)
            {
                stream.Write(buffer, 0, buffer.Length);
            }
            return resultSize;
        }

        public override int encodeString(object obj, Stream stream, ElementInfo elementInfo)
        {
            int resultSize = 0;

            byte[] val = CoderUtils.ASN1StringToBuffer(obj, elementInfo);
            resultSize = encodeLength(val.Length, elementInfo, stream);
            doAlign(stream);
            resultSize += val.Length;
            if (val.Length > 0)
            {
                stream.Write(val, 0, val.Length);
            }
            return resultSize;
        }

        public override int encodeSequenceOf(object obj, Stream stream, ElementInfo elementInfo)
        {
            IList collection = (IList)obj;
            int resultSize = encodeLength(collection.Count, elementInfo, stream);

            for (int i = 0; i < collection.Count; i++)
            {
                object itemObj = collection[i];
                ElementInfo info = new ElementInfo();
                info.AnnotatedClass = itemObj.GetType();
                info.ParentAnnotatedClass = elementInfo.AnnotatedClass;

                if (elementInfo.hasPreparedInfo())
                {
                    ASN1SequenceOfMetadata seqOfMeta = (ASN1SequenceOfMetadata)elementInfo.PreparedInfo.TypeMetadata;
                    info.PreparedInfo = (seqOfMeta.getItemClassMetadata());
                }

                resultSize += encodeClassType(itemObj, stream, info);
            }
            return resultSize;
        }

        public override int encodeNull(object obj, Stream stream, ElementInfo elementInfo)
        {
            return 0;
        }

        protected virtual void doAlign(Stream stream)
        {
            ((BitArrayOutputStream)stream).align();
        }

        public virtual int encodeSet(object obj, Stream stream, ElementInfo elementInfo)
        {
            int resultSize = 0;
            PropertyInfo[] fields = null;
            if (elementInfo.hasPreparedInfo())
            {
                fields = elementInfo.getProperties(obj.GetType());
            }
            else
            {
                SortedList<int, PropertyInfo> fieldOrder = CoderUtils.getSetOrder(obj.GetType());
                //TO DO Performance optimization need (unnecessary copy)
                fields = new PropertyInfo[fieldOrder.Count];
                fieldOrder.Values.CopyTo(fields, 0);
            }

            resultSize += encodeSequencePreamble(obj, fields, stream, elementInfo);
            int fieldIdx = 0;
            foreach (PropertyInfo field in fields)
            {
                resultSize += encodeSequenceField(obj, fieldIdx++, field, stream, elementInfo);
            }
            return resultSize;
        }

        public override int encodeBitString(Object obj, Stream stream, ElementInfo elementInfo)
        {
            /*NOTE – (Tutorial) Bitstrings constrained to a fixed length less than or equal to 16 bits do not cause octet alignment. Larger
            bitstrings are octet-aligned in the ALIGNED variant. If the length is fixed by constraints and the upper bound is less than 64K,
            there is no explicit length encoding, otherwise a length encoding is included which can take any of the forms specified earlier for
            length encodings, including fragmentation for large bit strings.*/

            int resultSize = 0, sizeOfString = 0;
            BitString str = (BitString)obj;
            byte[] buffer = str.Value;
            sizeOfString = str.getLengthInBits(); // buffer.Length*8 - str.TrailBitsCnt;
            resultSize += encodeLength(sizeOfString, elementInfo, stream);
            doAlign(stream);

            BitArrayOutputStream bitStream = (BitArrayOutputStream)stream;

            if (sizeOfString > 0)
            {
                //doAlign(stream);
                if (str.TrailBitsCnt == 0)
                    stream.Write(buffer, 0, buffer.Length);
                else
                {
                    bitStream.Write(buffer, 0, buffer.Length - 1);
                    for (int i = 0; i < str.TrailBitsCnt; i++)
                    {
                        int bitValue = (buffer[buffer.Length - 1] >> (7 - i)) & 0x1;
                        bitStream.writeBit(bitValue);
                    }
                }
            }
            return resultSize;
        }

        public override int encodeObjectIdentifier(Object obj, Stream stream, ElementInfo elementInfo)
        {
            ObjectIdentifier oid = (ObjectIdentifier)obj;
            int[] ia = oid.getIntArray();
            byte[] buffer = BERObjectIdentifier.Encode(ia);
            if (buffer.Length < 1)
                return 0;
            int resultSize = 0; // size of tag 
            resultSize += BERCoderUtils.encodeLength(buffer.Length, stream);
            stream.Write(buffer, 0, buffer.Length);
            resultSize += buffer.Length; // size of buffer         
            return resultSize;
        }
    }
}