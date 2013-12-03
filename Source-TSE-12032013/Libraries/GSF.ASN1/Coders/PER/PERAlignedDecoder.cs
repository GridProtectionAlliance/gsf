//******************************************************************************************************
//  PERAlignedDecoder.cs - Gbtc
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
    public class PERAlignedDecoder : Decoder
    {
        public override T decode<T>(Stream stream)
        {
            return base.decode<T>(new BitArrayInputStream(stream));
        }

        public override DecodedObject<object> decodeTag(Stream stream)
        {
            return null;
        }

        protected virtual void skipAlignedBits(Stream stream)
        {
            ((BitArrayInputStream)stream).skipUnreadedBits();
        }

        protected virtual long decodeIntegerValueAsBytes(int intLen, Stream stream)
        {
            long result = 0;
            for (int i = 0; i < intLen; i++)
            {
                long bt = stream.ReadByte();
                if (bt == -1)
                {
                    throw new ArgumentException("Unexpected EOF when encoding!");
                }
                if (i == 0 && (bt & (byte)0x80) != 0)
                {
                    bt = bt - 256;
                }

                result = (result << 8) | bt;
            }
            return result;

            /*int result = 0;
            for (int i = intLen - 1; i >= 0; i--)
            {
                result |= stream.ReadByte() << 8 * i;
            }
            return result;*/
        }

        /// <summary>
        ///     Decode the constraint length determinant.
        ///     ITU-T X.691. 10.9. General rules for encoding a length determinant
        /// </summary>
        protected virtual int decodeConstraintLengthDeterminant(int min, int max, BitArrayInputStream stream)
        {
            if (max <= 0xFFFF)
            {
                // 10.9. NOTE 2 – (Tutorial) In the case of the ALIGNED variant 
                // if the length count is bounded above by an upper bound that is 
                // less than 64K, then the constrained whole number encoding 
                // is used for the length.
                return (int)decodeConstraintNumber(min, max, stream); // encoding as constraint integer
            }
            else
                return decodeLengthDeterminant(stream);
        }

        /// <summary>
        ///     Decode the length determinant
        ///     ITU-T X.691. 10.9. General rules for encoding a length determinant
        /// </summary>
        protected virtual int decodeLengthDeterminant(BitArrayInputStream stream)
        {
            skipAlignedBits(stream);
            int result = stream.ReadByte();
            if ((result & 0x80) == 0)
            {
                // NOTE 2. a) ("n" less than 128)             
                // a single octet containing "n" with bit 8 set to zero;
                return result;
            }
            else
            {
                // NOTE 2. b) ("n" less than 16K) two octets 
                // containing "n" with bit 8 of the first octet 
                // set to 1 and bit 7 set to zero;        
                result = (result & 0x3f) << 8;
                result |= stream.ReadByte();
            }
            // WARNING! Large N doesn't supported NOW!
            // NOTE 2. b) (large "n") a single octet containing a count "m" 
            // with bit 8 set to 1 and bit 7 set to 1. 
            // The count "m" is one to four, and the length indicates that 
            // a fragment of the material follows (a multiple "m" of 16K items). 
            // For all values of "m", the fragment is then followed 
            // by another length encoding for the remainder of the material.        
            return result;
        }

        /// <summary>
        ///     Decode of the constrained whole number
        ///     ITU-T X.691. 10.5.
        ///     NOTE – (Tutorial) This subclause is referenced by other clauses,
        ///     and itself references earlier clauses for the production of
        ///     a nonnegative-binary-integer or a 2's-complement-binary-integer encoding.
        /// </summary>
        protected virtual long decodeConstraintNumber(long min, long max, BitArrayInputStream stream)
        {
            long result = 0;
            long valueRange = max - min;
            //!!!! int narrowedVal = value - min; !!!
            int maxBitLen = PERCoderUtils.getMaxBitLength(valueRange);

            if (valueRange == 0)
            {
                return max;
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
                skipAlignedBits(stream);
                result = stream.readBits(maxBitLen);
                result += min;
            }
            else if (valueRange > 0 && valueRange < 65536)
            {
                /* 
                * 3. Where the range is 257 to 64K, the value encodes into 
                * a two octet octet-aligned bit-field. 
                */
                skipAlignedBits(stream);
                result = stream.ReadByte() << 8;
                result = (int)result | stream.ReadByte();
                result += min;
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
                int intLen = decodeConstraintLengthDeterminant(1, CoderUtils.getPositiveIntegerLength(valueRange), stream);
                skipAlignedBits(stream);
                result = (int)decodeIntegerValueAsBytes(intLen, stream);
                result += min;
            }

            return result;
        }

        /// <summary>
        ///     Decode the semi-constrained whole number
        ///     ITU-T X.691. 10.7.
        ///     NOTE – (Tutorial) This procedure is used when a lower bound can be
        ///     identified but not an upper bound. The encoding procedure places
        ///     the offset from the lower bound into the minimum number of octets
        ///     as a non-negative-binary-integer, and requires an explicit length
        ///     encoding (typically a single octet) as specified in later procedures.
        /// </summary>
        protected virtual int decodeSemiConstraintNumber(int min, BitArrayInputStream stream)
        {
            int result = 0;
            int intLen = decodeLengthDeterminant(stream);
            skipAlignedBits(stream);
            result = (int)decodeIntegerValueAsBytes(intLen, stream);
            result += min;
            return result;
        }

        /// <summary>
        ///     Decode the normally small number
        ///     ITU-T X.691. 10.6
        ///     NOTE – (Tutorial) This procedure is used when encoding
        ///     a non-negative whole number that is expected to be small, but whose size
        ///     is potentially unlimited due to the presence of an extension marker.
        ///     An example is a choice index.
        /// </summary>
        protected virtual int decodeNormallySmallNumber(BitArrayInputStream stream)
        {
            int result = 0;
            int bitIndicator = stream.readBit();
            if (bitIndicator == 0)
            {
                /* 10.6.1 If the non-negative whole number, "n", is less than 
                * or equal to 63, then a single-bit bit-field shall be appended
                * to the field-list with the bit set to 0, and "n" shall be 
                * encoded as a non-negative-binary-integer into a 6-bit bit-field.
                */
                result = stream.readBits(6);
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
                result = decodeSemiConstraintNumber(0, stream);
            }
            return result;
        }

        /// <summary>
        ///     Decode the unconstrained whole number
        ///     ITU-T X.691. 10.8.
        ///     NOTE – (Tutorial) This case only arises in the encoding of the
        ///     value of an integer type with no lower bound. The procedure
        ///     encodes the value as a 2's-complement-binary-integer into
        ///     the minimum number of octets required to accommodate the encoding,
        ///     and requires an explicit length encoding (typically a single octet)
        ///     as specified in later procedures.
        /// </summary>
        protected virtual int decodeUnconstraintNumber(BitArrayInputStream stream)
        {
            int result = 0;
            int numLen = decodeLengthDeterminant(stream);
            skipAlignedBits(stream);
            result += (int)decodeIntegerValueAsBytes(numLen, stream);
            return result;
        }

        protected virtual int decodeLength(ElementInfo elementInfo, Stream stream)
        {
            int result = 0;
            BitArrayInputStream bitStream = (BitArrayInputStream)stream;

            if (elementInfo.hasPreparedInfo())
            {
                if (elementInfo.PreparedInfo.hasConstraint())
                {
                    IASN1ConstraintMetadata constraint = elementInfo.PreparedInfo.Constraint;
                    if (constraint is ASN1ValueRangeConstraintMetadata)
                    {
                        result = decodeConstraintLengthDeterminant(
                            (int)((ASN1ValueRangeConstraintMetadata)constraint).Min,
                            (int)((ASN1ValueRangeConstraintMetadata)constraint).Max,
                            bitStream
                            );
                    }
                    else if (constraint is ASN1SizeConstraintMetadata)
                    {
                        result = (int)((ASN1SizeConstraintMetadata)constraint).Max;
                    }
                }
                else
                    result = decodeLengthDeterminant(bitStream);
            }
            else if (elementInfo.isAttributePresent<ASN1ValueRangeConstraint>())
            {
                ASN1ValueRangeConstraint constraint = elementInfo.getAttribute<ASN1ValueRangeConstraint>();
                result = decodeConstraintLengthDeterminant((int)constraint.Min, (int)constraint.Max, bitStream);
            }
            else if (elementInfo.isAttributePresent<ASN1SizeConstraint>())
            {
                ASN1SizeConstraint constraint = elementInfo.getAttribute<ASN1SizeConstraint>();
                result = (int)constraint.Max;
            }
            else
                result = decodeLengthDeterminant(bitStream);
            CoderUtils.checkConstraints(result, elementInfo);
            return result;
        }

        public override DecodedObject<object> decodeChoice(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream)
        {
            object choice = createInstanceForElement(objectClass, elementInfo);
            skipAlignedBits(stream);
            PropertyInfo[] fields = elementInfo.getProperties(objectClass);
            int elementIndex = (int)decodeConstraintNumber(1, fields.Length, (BitArrayInputStream)stream);
            DecodedObject<object> val = null;
            for (int i = 0; i < elementIndex && i < fields.Length; i++)
            {
                if (i + 1 == elementIndex)
                {
                    PropertyInfo field = fields[i];
                    ElementInfo info = new ElementInfo();
                    info.AnnotatedClass = field;
                    if (elementInfo.hasPreparedInfo())
                    {
                        info.PreparedInfo = elementInfo.PreparedInfo.getPropertyMetadata(i);
                    }
                    else
                        info.ASN1ElementInfo = CoderUtils.getAttribute<ASN1Element>(field);
                    val = decodeClassType(decodedTag, field.PropertyType, info, stream);
                    if (val != null)
                        invokeSelectMethodForField(field, choice, val.Value, info);
                    break;
                }
                ;
            }
            if (val == null && !CoderUtils.isOptional(elementInfo))
            {
                throw new ArgumentException("The choice '" + objectClass + "' does not have a selected item!");
            }
            else
                return new DecodedObject<object>(choice);
        }


        protected virtual int getSequencePreambleBitLen(Type objectClass, ElementInfo elementInfo)
        {
            int preambleLen = 0;
            ElementInfo info = new ElementInfo();
            int fieldIdx = 0;
            foreach (PropertyInfo field in elementInfo.getProperties(objectClass))
            {
                if (elementInfo.hasPreparedInfo())
                    info.PreparedInfo = elementInfo.PreparedInfo.getPropertyMetadata(fieldIdx);

                if (CoderUtils.isOptionalField(field, info))
                {
                    preambleLen++;
                }
                fieldIdx++;
            }

            return preambleLen;
        }

        public override DecodedObject<object> decodeSequence(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream)
        {
            BitArrayInputStream bitStream = (BitArrayInputStream)stream;
            int preambleLen = getSequencePreambleBitLen(objectClass, elementInfo);
            int preamble = bitStream.readBits(preambleLen);
            int preambleCurrentBit = 32 - preambleLen;
            skipAlignedBits(stream);
            object sequence = createInstanceForElement(objectClass, elementInfo);
            initDefaultValues(sequence);
            ElementInfo info = new ElementInfo();
            int idx = 0;
            PropertyInfo[] fields = null;

            if (!CoderUtils.isSequenceSet(elementInfo) || elementInfo.hasPreparedInfo())
            {
                fields = elementInfo.getProperties(objectClass);
            }
            else
            {
                SortedList<int, PropertyInfo> fieldOrder = CoderUtils.getSetOrder(sequence.GetType());
                fields = new PropertyInfo[fieldOrder.Values.Count];
                fieldOrder.Values.CopyTo(fields, 0);
            }

            foreach (PropertyInfo field in fields)
            {
                if (elementInfo.hasPreparedInfo())
                {
                    info.PreparedInfo = elementInfo.PreparedInfo.getPropertyMetadata(idx);
                }
                if (CoderUtils.isOptionalField(field, info))
                {
                    if ((preamble & (0x80000000 >> preambleCurrentBit)) != 0)
                    {
                        decodeSequenceField(null, sequence, idx, field, stream, elementInfo, true);
                    }
                    preambleCurrentBit++;
                }
                else
                {
                    decodeSequenceField(null, sequence, idx, field, stream, elementInfo, true);
                }
                idx++;
            }
            return new DecodedObject<object>(sequence);
            /*            }
                        else
                            return decodeSet(decodedTag, objectClass, elementInfo, stream);*/
        }


        public override DecodedObject<object> decodeEnumItem(DecodedObject<object> decodedTag, Type objectClass, Type enumClass, ElementInfo elementInfo, Stream stream)
        {
            //int min = 0, max = enumClass.GetFields().Length;
            int min = 0, max = 0;
            foreach (FieldInfo enumItem in enumClass.GetFields())
            {
                if (CoderUtils.isAttributePresent<ASN1EnumItem>(enumItem))
                {
                    max++;
                }
            }

            if (max <= 0)
                throw new Exception("Unable to present any enum item!");

            int enumItemIdx = (int)decodeConstraintNumber(min, max - 1, (BitArrayInputStream)stream);
            DecodedObject<object> result = new DecodedObject<object>();
            int idx = 0;
            foreach (FieldInfo enumItem in enumClass.GetFields())
            {
                if (CoderUtils.isAttributePresent<ASN1EnumItem>(enumItem))
                {
                    if (idx++ == enumItemIdx)
                    {
                        ASN1EnumItem enumItemObj = CoderUtils.getAttribute<ASN1EnumItem>(enumItem);
                        result.Value = enumItemObj.Tag;
                        break;
                    }
                }
            }
            return result;
        }

        public override DecodedObject<object> decodeBoolean(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream)
        {
            DecodedObject<object> result = new DecodedObject<object>();
            BitArrayInputStream bitStream = (BitArrayInputStream)stream;
            result.Value = (bitStream.readBit() == 1);
            return result;
        }

        public override DecodedObject<object> decodeAny(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream)
        {
            return null;
        }

        public override DecodedObject<object> decodeNull(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream)
        {
            return new DecodedObject<object>(createInstanceForElement(objectClass, elementInfo));
        }

        public override DecodedObject<object> decodeInteger(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream)
        {
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

            DecodedObject<object> result = new DecodedObject<object>();
            BitArrayInputStream bitStream = (BitArrayInputStream)stream;
            int val = 0;
            if (hasConstraint)
            {
                ASN1ValueRangeConstraint constraint = elementInfo.getAttribute<ASN1ValueRangeConstraint>();
                val = (int)decodeConstraintNumber(min, max, bitStream);
            }
            else
                val = decodeUnconstraintNumber(bitStream);
            result.Value = val;
            return result;
        }

        public override DecodedObject<object> decodeReal(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream)
        {
            BitArrayInputStream bitStream = (BitArrayInputStream)stream;
            int len = decodeLengthDeterminant(bitStream);
            int realPreamble = stream.ReadByte();
            skipAlignedBits(stream);

            Double result = 0.0D;
            int szResult = len;
            if ((realPreamble & 0x40) == 1)
            {
                // 01000000 Value is PLUS-INFINITY
                result = Double.PositiveInfinity;
            }
            if ((realPreamble & 0x41) == 1)
            {
                // 01000001 Value is MINUS-INFINITY
                result = Double.NegativeInfinity;
                szResult += 1;
            }
            else if (len > 0)
            {
                int szOfExp = 1 + (realPreamble & 0x3);
                int sign = realPreamble & 0x40;
                int ff = (realPreamble & 0x0C) >> 2;
                long exponent = decodeIntegerValueAsBytes(szOfExp, stream);
                long mantissaEncFrm = decodeIntegerValueAsBytes(szResult - szOfExp - 1, stream);
                // Unpack mantissa & decrement exponent for base 2
                long mantissa = mantissaEncFrm << ff;
                while ((mantissa & 0x000ff00000000000L) == 0x0)
                {
                    exponent -= 8;
                    mantissa <<= 8;
                }
                while ((mantissa & 0x0010000000000000L) == 0x0)
                {
                    exponent -= 1;
                    mantissa <<= 1;
                }
                mantissa &= 0x0FFFFFFFFFFFFFL;
                long lValue = (exponent + 1023 + 52) << 52;
                lValue |= mantissa;
                if (sign == 1)
                {
                    lValue = (long)((ulong)lValue | 0x8000000000000000L);
                }
#if PocketPC
                    byte[] dblValAsBytes = System.BitConverter.GetBytes(lValue);
                    result = System.BitConverter.ToDouble(dblValAsBytes, 0);
#else
                result = BitConverter.Int64BitsToDouble(lValue);
#endif
            }
            return new DecodedObject<object>(result, szResult);
        }


        public override DecodedObject<object> decodeOctetString(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream)
        {
            DecodedObject<object> result = new DecodedObject<object>();
            int sizeOfString = decodeLength(elementInfo, stream);
            skipAlignedBits(stream);
            if (sizeOfString > 0)
            {
                byte[] val = new byte[sizeOfString];
                stream.Read(val, 0, val.Length);
                result.Value = val;
            }
            else
                result.Value = (new byte[0]);
            return result;
        }

        public override DecodedObject<object> decodeBitString(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream)
        {
            DecodedObject<object> result = new DecodedObject<object>();
            skipAlignedBits(stream);
            int sizeOfString = decodeLength(elementInfo, stream);

            BitArrayInputStream bitStream = (BitArrayInputStream)stream;
            skipAlignedBits(stream);
            int trailBits = 8 - sizeOfString % 8;
            sizeOfString = sizeOfString / 8;

            if (sizeOfString > 0 || (sizeOfString == 0 && trailBits > 0))
            {
                byte[] value = new byte[trailBits > 0 ? sizeOfString + 1 : sizeOfString];
                if (sizeOfString > 0)
                    stream.Read(value, 0, sizeOfString);
                if (trailBits > 0)
                {
                    value[sizeOfString] = (byte)(bitStream.readBits(trailBits) << (8 - trailBits));
                }

                result.Value = (new BitString(value, trailBits));
            }
            else
                result.Value = (new BitString(new byte[0]));
            return result;
        }

        protected virtual int decodeStringLength2(ElementInfo elementInfo, Stream stream)
        {
            int resultSize = 0;
            BitArrayInputStream bitStream = (BitArrayInputStream)stream;
            if (elementInfo.isAttributePresent<ASN1ValueRangeConstraint>())
            {
                ASN1ValueRangeConstraint constraint = elementInfo.getAttribute<ASN1ValueRangeConstraint>();
                resultSize = decodeConstraintLengthDeterminant((int)constraint.Min, (int)constraint.Max, bitStream);
            }
            else
                resultSize = decodeLengthDeterminant(bitStream);
            return resultSize;
        }


        public override DecodedObject<object> decodeString(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream)
        {
            DecodedObject<object> result = new DecodedObject<object>();
            int strLen = decodeLength(elementInfo, stream);
            skipAlignedBits(stream);
            if (strLen > 0)
            {
                byte[] val = new byte[strLen];
                stream.Read(val, 0, val.Length);
                result.Value = CoderUtils.bufferToASN1String(val, elementInfo);
            }
            else
                result.Value = ("");

            return result;
        }

        public override DecodedObject<object> decodeSequenceOf(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream)
        {
            Type paramType = (Type)objectClass.GetGenericArguments()[0];
            Type collectionType = typeof(List<>);
            Type genCollectionType = collectionType.MakeGenericType(paramType);
            Object param = Activator.CreateInstance(genCollectionType);

            int countOfElements = decodeLength(elementInfo, stream);

            MethodInfo method = param.GetType().GetMethod("Add");
            for (int i = 0; i < countOfElements; i++)
            {
                ElementInfo elementItemInfo = new ElementInfo();
                elementItemInfo.ParentAnnotatedClass = elementInfo.AnnotatedClass;
                elementItemInfo.AnnotatedClass = paramType;
                if (elementInfo.hasPreparedInfo())
                {
                    ASN1SequenceOfMetadata seqOfMeta = (ASN1SequenceOfMetadata)elementInfo.PreparedInfo.TypeMetadata;
                    elementItemInfo.PreparedInfo = seqOfMeta.getItemClassMetadata();
                }
                else
                    elementItemInfo.ASN1ElementInfo = null;

                DecodedObject<object> item = decodeClassType(null, paramType, elementItemInfo, stream);
                if (item != null)
                {
                    method.Invoke(param, new[] { item.Value });
                }
            }

            return new DecodedObject<object>(param);
        }

        public override DecodedObject<object> decodeObjectIdentifier(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream)
        {
            DecodedObject<int> len = BERCoderUtils.decodeLength(stream);
            byte[] byteBuf = new byte[len.value];
            stream.Read(byteBuf, 0, byteBuf.Length);
            string dottedDecimal = BERObjectIdentifier.Decode(byteBuf);
            return new DecodedObject<object>(new ObjectIdentifier(dottedDecimal));
        }
    }
}