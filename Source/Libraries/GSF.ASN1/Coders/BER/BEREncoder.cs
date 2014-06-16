//******************************************************************************************************
//  BEREncoder.cs - Gbtc
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
using System.IO;
using System.Reflection;
using GSF.ASN1.Attributes;
using GSF.ASN1.Metadata;
using GSF.ASN1.Types;
using GSF.ASN1.Utilities;

namespace GSF.ASN1.Coders.BER
{
    public class BEREncoder : Encoder
    {
        public override void encode<T>(T obj, Stream stream)
        {
            ReverseByteArrayOutputStream reverseStream = new ReverseByteArrayOutputStream();
            base.encode(obj, reverseStream);
            reverseStream.WriteTo(stream);
        }

        public override int encodeSequence(object obj, Stream stream, ElementInfo elementInfo)
        {
            int resultSize = 0;
            PropertyInfo[] fields = elementInfo.getProperties(obj.GetType());
            for (int i = 0; i < fields.Length; i++)
            {
                PropertyInfo field = fields[fields.Length - 1 - i];
                resultSize += encodeSequenceField(obj, fields.Length - 1 - i, field, stream, elementInfo);
            }

            if (!CoderUtils.isSequenceSet(elementInfo))
            {
                resultSize += encodeHeader(
                    BERCoderUtils.getTagValueForElement(
                        elementInfo,
                        TagClasses.Universal,
                        ElementType.Constructed,
                        UniversalTags.Sequence)
                    , resultSize, stream);
            }
            else
            {
                resultSize += encodeHeader(
                    BERCoderUtils.getTagValueForElement(
                        elementInfo,
                        TagClasses.Universal,
                        ElementType.Constructed,
                        UniversalTags.Set)
                    , resultSize, stream);
            }
            return resultSize;
        }

        public override int encodeChoice(object obj, Stream stream, ElementInfo elementInfo)
        {
            int result = 0;
            int sizeOfChoiceField = base.encodeChoice(obj, stream, elementInfo);
            if (
                (elementInfo.hasPreparedInfo() && elementInfo.hasPreparedASN1ElementInfo() && elementInfo.PreparedASN1ElementInfo.HasTag)
                || (elementInfo.ASN1ElementInfo != null && elementInfo.ASN1ElementInfo.HasTag))
            {
                result += encodeHeader(BERCoderUtils.getTagValueForElement(elementInfo, TagClasses.ContextSpecific, ElementType.Constructed, UniversalTags.LastUniversal), sizeOfChoiceField, stream);
            }
            result += sizeOfChoiceField;
            return result;
        }

        public override int encodeEnumItem(object enumConstant, Type enumClass, Stream stream, ElementInfo elementInfo)
        {
            int resultSize = 0;
            ASN1EnumItem enumObj = //elementInfo.AnnotatedClass.getAnnotation(typeof(ASN1EnumItem));
                elementInfo.getAttribute<ASN1EnumItem>();
            int szOfInt = encodeIntegerValue(enumObj.Tag, stream);
            resultSize += szOfInt;
            resultSize += encodeLength(szOfInt, stream);
            resultSize += encodeTag(BERCoderUtils.getTagValueForElement(elementInfo, TagClasses.Universal, ElementType.Primitive, UniversalTags.Enumerated), stream);
            return resultSize;
        }

        public override int encodeBoolean(object obj, Stream stream, ElementInfo elementInfo)
        {
            int resultSize = 1;
            bool value = (bool)obj;
            stream.WriteByte((byte)(value ? 0xFF : 0x00));

            resultSize += encodeLength(1, stream);
            resultSize += encodeTag(BERCoderUtils.getTagValueForElement(elementInfo, TagClasses.Universal, ElementType.Primitive, UniversalTags.Boolean), stream);
            return resultSize;
        }

        public override int encodeAny(object obj, Stream stream, ElementInfo elementInfo)
        {
            int resultSize = 0, sizeOfString = 0;
            byte[] buffer = (byte[])obj;
            sizeOfString = buffer.Length;
            CoderUtils.checkConstraints(sizeOfString, elementInfo);
            stream.Write(buffer, 0, buffer.Length);
            resultSize += sizeOfString;
            return resultSize;
        }

        protected internal int encodeIntegerValue(long val, Stream stream)
        {
            int resultSize = CoderUtils.getIntegerLength(val);
            for (int i = 0; i < resultSize; i++)
            {
                stream.WriteByte((byte)val);
                val = val >> 8;
            }
            return resultSize;
        }

        public override int encodeInteger(object obj, Stream stream, ElementInfo elementInfo)
        {
            int resultSize = 0;
            int szOfInt = 0;
            if (obj.GetType().Equals(typeof(int)))
            {
                int val = (int)obj;
                CoderUtils.checkConstraints(val, elementInfo);
                szOfInt = encodeIntegerValue(val, stream);
            }
            else
            {
                long val = (long)obj;
                CoderUtils.checkConstraints(val, elementInfo);
                szOfInt = encodeIntegerValue(val, stream);
            }
            resultSize += szOfInt;
            resultSize += encodeLength(szOfInt, stream);
            resultSize += encodeTag(BERCoderUtils.getTagValueForElement(elementInfo, TagClasses.Universal, ElementType.Primitive, UniversalTags.Integer), stream);
            return resultSize;
        }

        public override int encodeReal(object obj, Stream stream, ElementInfo elementInfo)
        {
            int resultSize = 0;
            Double value = (Double)obj;
            //CoderUtils.checkConstraints(value,elementInfo);
            int szOfInt = 0;
#if PocketPC
            byte[] dblValAsBytes =  System.BitConverter.GetBytes(value);
            long asLong = System.BitConverter.ToInt64(dblValAsBytes, 0);
#else
            long asLong = BitConverter.DoubleToInt64Bits(value);
#endif
            if (value == Double.PositiveInfinity)
            {
                // positive infinity
                stream.WriteByte(0x40); // 01000000 Value is PLUS-INFINITY
            }
            else if (value == Double.NegativeInfinity)
            {
                // negative infinity            
                stream.WriteByte(0x41); // 01000001 Value is MINUS-INFINITY
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

                szOfInt += encodeIntegerValue(mantissa, stream);
                int szOfExp = CoderUtils.getIntegerLength(exponent);
                szOfInt += encodeIntegerValue(exponent, stream);

                byte realPreamble = 0x80;
                realPreamble |= (byte)(szOfExp - 1);
                if (((ulong)asLong & 0x8000000000000000L) == 1)
                {
                    realPreamble |= 0x40; // Sign
                }
                stream.WriteByte(realPreamble);
                szOfInt += 1;
            }
            resultSize += szOfInt;
            resultSize += encodeLength(szOfInt, stream);
            resultSize += encodeTag(BERCoderUtils.getTagValueForElement(elementInfo, TagClasses.Universal, ElementType.Primitive, UniversalTags.Real), stream);
            return resultSize;
        }

        public override int encodeOctetString(object obj, Stream stream, ElementInfo elementInfo)
        {
            int resultSize = 0, sizeOfString = 0;
            byte[] buffer = (byte[])obj;
            sizeOfString = buffer.Length;
            CoderUtils.checkConstraints(sizeOfString, elementInfo);

            stream.Write(buffer, 0, buffer.Length);

            resultSize += sizeOfString;
            resultSize += encodeLength(sizeOfString, stream);
            resultSize += encodeTag(BERCoderUtils.getTagValueForElement(elementInfo, TagClasses.Universal, ElementType.Primitive, UniversalTags.OctetString), stream);
            return resultSize;
        }

        public override int encodeString(object obj, Stream stream, ElementInfo elementInfo)
        {
            int resultSize = 0, sizeOfString = 0;
            byte[] buffer = CoderUtils.ASN1StringToBuffer(obj, elementInfo);
            sizeOfString = buffer.Length;
            CoderUtils.checkConstraints(sizeOfString, elementInfo);

            stream.Write(buffer, 0, buffer.Length);

            resultSize += sizeOfString;
            resultSize += encodeLength(sizeOfString, stream);
            resultSize += encodeTag(BERCoderUtils.getTagValueForElement(elementInfo, TagClasses.Universal, ElementType.Primitive, CoderUtils.getStringTagForElement(elementInfo)), stream);
            return resultSize;
        }

        public override int encodeSequenceOf(object obj, Stream stream, ElementInfo elementInfo)
        {
            int resultSize = 0;
            IList collection = (IList)obj;
            CoderUtils.checkConstraints(collection.Count, elementInfo);

            int sizeOfCollection = 0;
            for (int i = 0; i < collection.Count; i++)
            {
                object item = collection[collection.Count - 1 - i];
                ElementInfo info = new ElementInfo();
                info.AnnotatedClass = item.GetType();
                info.ParentAnnotatedClass = elementInfo.AnnotatedClass;

                if (elementInfo.hasPreparedInfo())
                {
                    ASN1SequenceOfMetadata seqOfMeta = (ASN1SequenceOfMetadata)elementInfo.PreparedInfo.TypeMetadata;
                    info.PreparedInfo = (seqOfMeta.getItemClassMetadata());
                }

                sizeOfCollection += encodeClassType(item, stream, info);
            }
            resultSize += sizeOfCollection;
            resultSize += encodeLength(sizeOfCollection, stream);

            if (!CoderUtils.isSequenceSetOf(elementInfo))
            {
                resultSize += encodeTag(BERCoderUtils.getTagValueForElement(elementInfo, TagClasses.Universal, ElementType.Constructed, UniversalTags.Sequence), stream);
            }
            else
            {
                resultSize += encodeTag(BERCoderUtils.getTagValueForElement(elementInfo, TagClasses.Universal, ElementType.Constructed, UniversalTags.Set), stream);
            }
            return resultSize;
        }

        protected internal int encodeHeader(DecodedObject<int> tagValue, int contentLen, Stream stream)
        {
            int resultSize = encodeLength(contentLen, stream);
            resultSize += encodeTag(tagValue, stream);
            return resultSize;
        }

        protected internal int encodeTag(DecodedObject<int> tagValue, Stream stream)
        {
            int resultSize = tagValue.Size;
            int value = tagValue.Value;
            for (int i = 0; i < tagValue.Size; i++)
            {
                stream.WriteByte((byte)value);
                value = value >> 8;
            }
            return resultSize;

            /*int resultSize = 0;
            if (tagValue.Size == 1)
            {
                stream.WriteByte((byte)tagValue.Value);
                resultSize++;
            }
            else
                resultSize += encodeIntegerValue(tagValue.Value, stream);
            return resultSize;*/
        }

        protected internal int encodeLength(int length, Stream stream)
        {
            return BERCoderUtils.encodeLength(length, stream);
        }

        public override int encodeNull(object obj, Stream stream, ElementInfo elementInfo)
        {
            stream.WriteByte((byte)0);
            int resultSize = 1;
            resultSize += encodeTag(BERCoderUtils.getTagValueForElement(elementInfo, TagClasses.Universal, ElementType.Primitive, UniversalTags.Null), stream);
            return resultSize;
        }

        public override int encodeBitString(Object obj, Stream stream, ElementInfo elementInfo)
        {
            int resultSize = 0, sizeOfString = 0;
            BitString str = (BitString)obj;
            CoderUtils.checkConstraints(str.getLengthInBits(), elementInfo);
            byte[] buffer = str.Value;
            stream.Write(buffer, 0, buffer.Length);
            stream.WriteByte((byte)str.getTrailBitsCnt());
            sizeOfString = buffer.Length + 1;

            resultSize += sizeOfString;
            resultSize += encodeLength(sizeOfString, stream);
            resultSize += encodeTag(
                BERCoderUtils.getTagValueForElement(elementInfo, TagClasses.Universal, ElementType.Primitive, UniversalTags.Bitstring),
                stream
                );
            return resultSize;
        }

        public override int encodeObjectIdentifier(Object obj, Stream stream, ElementInfo elementInfo)
        {
            ObjectIdentifier oid = (ObjectIdentifier)obj;
            int[] ia = oid.getIntArray();
            byte[] buffer = BERObjectIdentifier.Encode(ia);
            stream.Write(buffer, 0, buffer.Length);
            int resultSize = buffer.Length;
            resultSize += encodeLength(resultSize, stream);
            resultSize += encodeTag(
                BERCoderUtils.getTagValueForElement(elementInfo, TagClasses.Universal, ElementType.Primitive, UniversalTags.ObjectIdentifier),
                stream
                );
            return resultSize;
        }
    }
}