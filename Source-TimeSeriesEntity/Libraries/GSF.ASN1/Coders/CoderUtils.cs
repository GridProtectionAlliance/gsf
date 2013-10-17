//******************************************************************************************************
//  CoderUtils.cs - Gbtc
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
using System.Reflection;
using System.Text;
using GSF.ASN1.Attributes;
using GSF.ASN1.Attributes.Constraints;
using GSF.ASN1.Metadata;
using GSF.ASN1.Types;

namespace GSF.ASN1.Coders
{
    public class CoderUtils
    {
        private static readonly byte[] hexTable = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF };

        public static T getAttribute<T>(ICustomAttributeProvider field)
        {
            object[] attrs = field.GetCustomAttributes(typeof(T), false);
            if (attrs != null && attrs.Length > 0)
            {
                T attribute = (T)attrs[0];
                return attribute;
            }
            else
                return default(T);
        }

        public static bool isAttributePresent<T>(ICustomAttributeProvider field)
        {
            object[] attrs = field.GetCustomAttributes(typeof(T), false);
            if (attrs != null && attrs.Length > 0)
                return true;
            else
                return false;
        }

        public static int getIntegerLength(int val)
        {
            int mask = 0x7f800000;
            int sizeOfInt = 4;
            if (val < 0)
            {
                while (((mask & val) == mask) && (sizeOfInt > 1))
                {
                    mask = mask >> 8;
                    sizeOfInt--;
                }
            }
            else
            {
                while (((mask & val) == 0) && (sizeOfInt > 1))
                {
                    mask = mask >> 8;
                    sizeOfInt--;
                }
            }
            return sizeOfInt;
        }

        public static int getIntegerLength(long val)
        {
            long mask = 0x7f80000000000000;
            int sizeOfInt = 8;
            if (val < 0)
            {
                while (((mask & val) == mask) && (sizeOfInt > 1))
                {
                    mask = mask >> 8;
                    sizeOfInt--;
                }
            }
            else
            {
                while (((mask & val) == 0) && (sizeOfInt > 1))
                {
                    mask = mask >> 8;
                    sizeOfInt--;
                }
            }
            return sizeOfInt;
        }

        public static int getPositiveIntegerLength(int val)
        {
            if (val < 0)
            {
                int mask = 0x7f800000;
                int sizeOfInt = 4;
                while (((mask & ~val) == mask) && (sizeOfInt > 1))
                {
                    mask = mask >> 8;
                    sizeOfInt--;
                }
                return sizeOfInt;
            }
            else
                return getIntegerLength(val);
        }

        public static int getPositiveIntegerLength(long val)
        {
            if (val < 0)
            {
                long mask = 0x7f80000000000000L;
                int sizeOfInt = 4;
                while (((mask & ~val) == mask) && (sizeOfInt > 1))
                {
                    mask = mask >> 8;
                    sizeOfInt--;
                }
                return sizeOfInt;
            }
            else
                return getIntegerLength(val);
        }

        public static BitString defStringToOctetString(string bhString)
        {
            if (bhString.Length < 4)
                return new BitString(new byte[0]);
            if (bhString.LastIndexOf('B') == bhString.Length - 1)
                return bitStringToOctetString(bhString.Substring(1, bhString.Length - 2));
            else
                return hexStringToOctetString(bhString.Substring(1, bhString.Length - 2));
        }

        private static BitString bitStringToOctetString(string bhString)
        {
            int trailBits = 0;
            bool hasTrailBits = (bhString.Length - 1) % 2 != 0;
            byte[] result = new byte[(bhString.Length - 1) / 8 + (hasTrailBits ? 1 : 0)];
            int currentStrPos = 0;
            for (int i = 0; i < result.Length; i++)
            {
                byte bt = 0x00;
                int bitCnt = currentStrPos;
                while (bitCnt < currentStrPos + 8 && bitCnt < bhString.Length - 1)
                {
                    if (bhString[bitCnt] != '0')
                        bt |= (byte)(0x01 << (7 - (bitCnt - currentStrPos)));
                    bitCnt++;
                }
                currentStrPos += 8;
                if (bitCnt != currentStrPos)
                    trailBits = 8 - (currentStrPos - bitCnt);
                // hi-byte
                result[i] = bt;
            }
            return new BitString(result, trailBits);
        }

        private static BitString hexStringToOctetString(string bhString)
        {
            bool hasTrailBits = (bhString.Length - 1) % 2 != 0;
            byte[] resultBuf = new byte[(bhString.Length - 1) / 2 + (hasTrailBits ? 1 : 0)];

            for (int i = 0; i < resultBuf.Length; i++)
            {
                // high byte
                resultBuf[i] = (byte)(hexTable[((int)(bhString[i * 2]) - 0x30)] << 4);
                if (!hasTrailBits || (hasTrailBits && i < resultBuf.Length - 1))
                    resultBuf[i] |= (byte)(hexTable[((int)(bhString[i * 2 + 1]) - 0x30)] & 0x0F);
            }
            return new BitString(resultBuf, hasTrailBits ? 4 : 0);
        }

        public static SortedList<int, PropertyInfo> getSetOrder(Type objClass)
        {
            SortedList<int, PropertyInfo> fieldOrder = new SortedList<int, PropertyInfo>();
            int tagNA = -1;
            foreach (PropertyInfo field in objClass.GetProperties())
            {
                ASN1Element element = getAttribute<ASN1Element>(field);
                if (element != null)
                {
                    try
                    {
                        if (element.HasTag)
                            fieldOrder.Add(element.Tag, field);
                        else
                            fieldOrder.Add(tagNA--, field);
                    }
                    catch
                    {
                        if (element.HasTag)
                            throw new Exception("Duplicate tag [" + element.Tag + "] for set has been detected!");

                        throw new Exception("Cannot be specified more field in SET without tag specified");
                    }
                }
            }
            return fieldOrder;
        }

        public static int getStringTagForElement(ElementInfo elementInfo)
        {
            int result = UniversalTags.PrintableString;
            if (elementInfo.hasPreparedInfo())
            {
                result = ((ASN1StringMetadata)elementInfo.PreparedInfo.TypeMetadata).StringType;
            }
            else if (elementInfo.isAttributePresent<ASN1String>())
            {
                ASN1String val = elementInfo.getAttribute<ASN1String>();
                result = val.StringType;
            }
            else if (elementInfo.ParentAnnotatedClass != null && elementInfo.isParentAttributePresent<ASN1String>())
            {
                ASN1String value = elementInfo.getParentAttribute<ASN1String>();
                result = value.StringType;
            }
            return result;
        }

        public static void checkConstraints(long val, ElementInfo elementInfo)
        {
            if (elementInfo.hasPreparedInfo())
            {
                if (elementInfo.PreparedInfo.hasConstraint())
                    if (!elementInfo.PreparedInfo.Constraint.checkValue(val))
                        throw new Exception("Length of '" + elementInfo.AnnotatedClass + "' out of bound");
            }
            else
            {
                if (elementInfo.isAttributePresent<ASN1ValueRangeConstraint>())
                {
                    ASN1ValueRangeConstraint constraint = elementInfo.getAttribute<ASN1ValueRangeConstraint>();
                    if (val > constraint.Max || val < constraint.Min)
                        throw new Exception("Length of '" + elementInfo.AnnotatedClass + "' out of bound");
                }
                else if (elementInfo.isAttributePresent<ASN1SizeConstraint>())
                {
                    ASN1SizeConstraint constraint = elementInfo.getAttribute<ASN1SizeConstraint>();
                    if (val != constraint.Max)
                        throw new Exception("Length of '" + elementInfo.AnnotatedClass + "' out of bound");
                }
            }
        }

        public static bool isImplements(ICustomAttributeProvider objectClass, Type interfaceClass)
        {
            return isAttributePresent<ASN1PreparedElement>(objectClass); // isAnnotationPresent(ASN1PreparedElement.class);
            /*for(Class item: objectClass.getInterfaces()) {
                if(item.equals(interfaceClass)) {
                    return true;
                }
            }
            return false;*/
        }

        public static bool isAnyField(ICustomAttributeProvider field, ElementInfo elementInfo)
        {
            bool isAny = false;
            if (elementInfo.hasPreparedInfo())
            {
                isAny = elementInfo.PreparedInfo.TypeMetadata is ASN1AnyMetadata;
            }
            else
                isAny = isAttributePresent<ASN1Any>(field); //. isAnnotationPresent(.class);        
            return isAny;
        }

        public static bool isNullField(ICustomAttributeProvider field, ElementInfo elementInfo)
        {
            bool isNull = false;
            if (elementInfo.hasPreparedInfo())
            {
                isNull = elementInfo.PreparedInfo.TypeMetadata is ASN1NullMetadata;
            }
            else
            {
                isNull = isAttributePresent<ASN1Null>(field);
            }
            return isNull;
        }


        public static bool isOptionalField(ICustomAttributeProvider field, ElementInfo elementInfo)
        {
            if (elementInfo.hasPreparedInfo())
            {
                if (elementInfo.hasPreparedASN1ElementInfo())
                    return elementInfo.PreparedASN1ElementInfo.IsOptional ||
                           elementInfo.PreparedASN1ElementInfo.HasDefaultValue;
                return false;
            }
            else if (isAttributePresent<ASN1Element>(field))
            {
                ASN1Element info = getAttribute<ASN1Element>(field);
                if (info.IsOptional || info.HasDefaultValue)
                    return true;
            }
            return false;
        }

        public static bool isOptional(ElementInfo elementInfo)
        {
            bool result = false;
            if (elementInfo.hasPreparedInfo())
            {
                result = elementInfo.PreparedASN1ElementInfo.IsOptional
                         || elementInfo.PreparedASN1ElementInfo.HasDefaultValue;
            }
            else
                result = elementInfo.ASN1ElementInfo != null && elementInfo.ASN1ElementInfo.IsOptional;
            return result;
        }


        public static void checkForOptionalField(PropertyInfo field, ElementInfo elementInfo)
        {
            if (isOptionalField(field, elementInfo))
                return;
            throw new Exception("The mandatory field '" + field.Name + "' does not have a value!");
        }


        public static bool isSequenceSet(ElementInfo elementInfo)
        {
            bool isEqual = false;
            if (elementInfo.hasPreparedInfo())
            {
                isEqual = ((ASN1SequenceMetadata)elementInfo.PreparedInfo.TypeMetadata).IsSet;
            }
            else
            {
                ASN1Sequence seq = getAttribute<ASN1Sequence>(elementInfo.AnnotatedClass);
                isEqual = seq.IsSet;
            }
            return isEqual;
        }

        public static bool isSequenceSetOf(ElementInfo elementInfo)
        {
            bool isEqual = false;
            if (elementInfo.hasPreparedInfo())
            {
                isEqual = ((ASN1SequenceOfMetadata)elementInfo.PreparedInfo.TypeMetadata).IsSetOf;
            }
            else
            {
                ASN1SequenceOf seq = getAttribute<ASN1SequenceOf>(elementInfo.AnnotatedClass);
                isEqual = seq.IsSetOf;
            }
            return isEqual;
        }

        public static MethodInfo findDoSelectMethodForField(PropertyInfo field, Type objClass)
        {
            string methodName = "select" + field.Name.ToUpper().Substring(0, (1) - (0)) + field.Name.Substring(1);
            return objClass.GetMethod(methodName);
        }

        public static MethodInfo findIsSelectedMethodForField(PropertyInfo field, Type objClass)
        {
            string methodName = "is" + field.Name.ToUpper().Substring(0, (1) - (0)) + field.Name.Substring(1) + "Selected";
            return objClass.GetMethod(methodName);
        }

        public static MethodInfo findIsPresentMethodForField(PropertyInfo field, Type objClass)
        {
            string methodName = "is" + field.Name.ToUpper().Substring(0, (1) - (0)) + field.Name.Substring(1) + "Present";
            return objClass.GetMethod(methodName, new Type[0]);
        }

        public static byte[] ASN1StringToBuffer(object obj, ElementInfo elementInfo)
        {
            int stringTag = getStringTagForElement(elementInfo);
            byte[] buffer = null;

            if (stringTag == UniversalTags.UTF8String)
                buffer = Encoding.UTF8.GetBytes((string)obj);
            else if (stringTag == UniversalTags.BMPString)
                buffer = Encoding.BigEndianUnicode.GetBytes((string)obj);
            else
                buffer = Encoding.ASCII.GetBytes((string)obj);
            return buffer;
        }

        public static string bufferToASN1String(byte[] byteBuf, ElementInfo elementInfo)
        {
            string result = null;
            int stringTag = getStringTagForElement(elementInfo);
            if (stringTag == UniversalTags.UTF8String)
            {
                result = new string(
                    Encoding.UTF8.GetChars(byteBuf)
                    );
            }
            else if (stringTag == UniversalTags.BMPString)
            {
                result = new string(
                    Encoding.BigEndianUnicode.GetChars(byteBuf)
                    );
            }
            else
            {
                result = new string(
                    Encoding.ASCII.GetChars(byteBuf)
                    );
            }
            return result;
        }
    }
}