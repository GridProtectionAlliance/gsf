//******************************************************************************************************
//  Decoder.cs - Gbtc
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
using System.Reflection;
using GSF.ASN1.Attributes;
using GSF.ASN1.Metadata;
using GSF.ASN1.Types;

namespace GSF.ASN1.Coders
{
    public abstract class Decoder : IASN1TypesDecoder, IDecoder
    {
        public virtual DecodedObject<object> decodeClassType(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream)
        {
            if (CoderUtils.isImplements(objectClass, typeof(IASN1PreparedElement)))
            {
                return decodePreparedElement(decodedTag, objectClass, elementInfo, stream);
            }
            else if (elementInfo.hasPreparedInfo())
            {
                return elementInfo.PreparedInfo.TypeMetadata.decode(
                    this, decodedTag, objectClass, elementInfo, stream
                    );
            }
            else if (elementInfo.isAttributePresent<ASN1SequenceOf>())
            {
                return decodeSequenceOf(decodedTag, objectClass, elementInfo, stream);
            }
            else if (elementInfo.isAttributePresent<ASN1Sequence>())
            {
                return decodeSequence(decodedTag, objectClass, elementInfo, stream);
            }
            else if (elementInfo.isAttributePresent<ASN1Choice>())
            {
                return decodeChoice(decodedTag, objectClass, elementInfo, stream);
            }
            else if (elementInfo.isAttributePresent<ASN1BoxedType>())
            {
                return decodeBoxedType(decodedTag, objectClass, elementInfo, stream);
            }
            else if (elementInfo.isAttributePresent<ASN1Enum>())
            {
                return decodeEnum(decodedTag, objectClass, elementInfo, stream);
            }
            else if (elementInfo.isAttributePresent<ASN1Boolean>())
            {
                return decodeBoolean(decodedTag, objectClass, elementInfo, stream);
            }
            else if (elementInfo.isAttributePresent<ASN1Any>())
            {
                return decodeAny(decodedTag, objectClass, elementInfo, stream);
            }
            else if (elementInfo.isAttributePresent<ASN1Integer>())
            {
                return decodeInteger(decodedTag, objectClass, elementInfo, stream);
            }
            else if (elementInfo.isAttributePresent<ASN1Real>())
            {
                return decodeReal(decodedTag, objectClass, elementInfo, stream);
            }
            else if (elementInfo.isAttributePresent<ASN1OctetString>())
            {
                return decodeOctetString(decodedTag, objectClass, elementInfo, stream);
            }
            else if (elementInfo.isAttributePresent<ASN1BitString>() || elementInfo.AnnotatedClass.Equals(typeof(BitString)))
            {
                return decodeBitString(decodedTag, objectClass, elementInfo, stream);
            }
            else if (elementInfo.isAttributePresent<ASN1ObjectIdentifier>() || elementInfo.AnnotatedClass.Equals(typeof(ObjectIdentifier)))
            {
                return decodeObjectIdentifier(decodedTag, objectClass, elementInfo, stream);
            }
            else if (elementInfo.isAttributePresent<ASN1String>())
            {
                return decodeString(decodedTag, objectClass, elementInfo, stream);
            }
            else if (elementInfo.isAttributePresent<ASN1Null>())
            {
                return decodeNull(decodedTag, objectClass, elementInfo, stream);
            }
            else if (elementInfo.isAttributePresent<ASN1Element>())
            {
                return decodeElement(decodedTag, objectClass, elementInfo, stream);
            }
            else
                return decodeCSElement(decodedTag, objectClass, elementInfo, stream);
        }

        public DecodedObject<object> decodePreparedElement(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream)
        {
            IASN1PreparedElementData saveInfo = elementInfo.PreparedInfo;
            IASN1PreparedElement preparedInstance = (IASN1PreparedElement)createInstanceForElement(objectClass, elementInfo);

            elementInfo.PreparedInstance = preparedInstance;
            ASN1ElementMetadata elementDataSave = null;
            if (elementInfo.hasPreparedASN1ElementInfo())
            {
                elementDataSave = elementInfo.PreparedASN1ElementInfo;
            }
            elementInfo.PreparedInfo = preparedInstance.PreparedData;
            if (elementDataSave != null)
                elementInfo.PreparedASN1ElementInfo = elementDataSave;
            DecodedObject<object> result = preparedInstance.PreparedData.TypeMetadata.decode(
                this, decodedTag, objectClass, elementInfo, stream
                );
            elementInfo.PreparedInfo = saveInfo;
            return result;
        }


        public void invokeSetterMethodForField(PropertyInfo field, object obj, object param, ElementInfo info)
        {
            field.SetValue(obj, param, null);
        }

        public void invokeSelectMethodForField(PropertyInfo field, object obj, object param, ElementInfo info)
        {
            if (info.hasPreparedInfo())
            {
                info.PreparedInfo.invokeDoSelectMethod(obj, param);
            }
            else
            {
                MethodInfo method = CoderUtils.findDoSelectMethodForField(field, obj.GetType());
                method.Invoke(obj, new[] { param });
            }
        }


        public virtual DecodedObject<object> decodeSequence(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream)
        {
            object sequence = createInstanceForElement(objectClass, elementInfo);
            initDefaultValues(sequence);
            DecodedObject<object> fieldTag = null;
            int maxSeqLen = elementInfo.MaxAvailableLen;
            int sizeOfSequence = 0;
            if (maxSeqLen == -1 || maxSeqLen > 0)
            {
                fieldTag = decodeTag(stream);
                if (fieldTag != null)
                    sizeOfSequence += fieldTag.Size;
            }
            PropertyInfo[] fields = elementInfo.getProperties(objectClass);

            for (int i = 0; i < fields.Length; i++)
            {
                PropertyInfo field = fields[i];
                DecodedObject<object> obj = decodeSequenceField(
                    fieldTag, sequence, i, field, stream, elementInfo, true
                    );
                if (obj != null)
                {
                    sizeOfSequence += obj.Size;

                    bool isAny = false;
                    if (i + 1 == fields.Length - 1)
                    {
                        ElementInfo info = new ElementInfo();
                        info.AnnotatedClass = (fields[i + 1]);
                        info.MaxAvailableLen = (elementInfo.MaxAvailableLen);
                        if (elementInfo.hasPreparedInfo())
                        {
                            info.PreparedInfo = (elementInfo.PreparedInfo.getPropertyMetadata(i + 1));
                        }
                        else
                            info.ASN1ElementInfo = CoderUtils.getAttribute<ASN1Element>(fields[i + 1]);
                        isAny = CoderUtils.isAnyField(fields[i + 1], info);
                    }

                    if (maxSeqLen != -1)
                    {
                        elementInfo.MaxAvailableLen = (maxSeqLen - sizeOfSequence);
                    }

                    if (!isAny)
                    {
                        if (i < fields.Length - 1)
                        {
                            if (maxSeqLen == -1 || elementInfo.MaxAvailableLen > 0)
                            {
                                fieldTag = decodeTag(stream);
                                if (fieldTag != null)
                                {
                                    sizeOfSequence += fieldTag.Size;
                                }
                                else
                                    break;
                            }
                            else
                                fieldTag = null;
                        }
                    }
                }
                ;
            }
            return new DecodedObject<object>(sequence, sizeOfSequence);
        }

        public virtual DecodedObject<object> decodeChoice(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream)
        {
            object choice = createInstanceForElement(objectClass, elementInfo);
            DecodedObject<object> val = null;
            PropertyInfo[] fields = elementInfo.getProperties(objectClass);
            int fieldIdx = 0;
            foreach (PropertyInfo field in fields)
            {
                ElementInfo info = new ElementInfo();
                info.AnnotatedClass = field;
                if (elementInfo.hasPreparedInfo())
                {
                    info.PreparedInfo = elementInfo.PreparedInfo.getPropertyMetadata(fieldIdx);
                }
                else
                    info.ASN1ElementInfo = CoderUtils.getAttribute<ASN1Element>(field);

                val = decodeClassType(decodedTag, field.PropertyType, info, stream);
                fieldIdx++;
                if (val != null)
                {
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
                return new DecodedObject<object>(choice, val != null ? val.Size : 0);
        }

        public virtual DecodedObject<object> decodeEnum(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream)
        {
            Type enumClass = null;
            foreach (MemberInfo member in objectClass.GetMembers())
            {
                if (member is Type)
                {
                    Type cls = (Type)member;
                    if (cls.IsEnum)
                    {
                        enumClass = cls;
                        break;
                    }
                }
            }
            ;

            PropertyInfo field = objectClass.GetProperty("Value");


            DecodedObject<object> itemValue = decodeEnumItem(decodedTag, field.PropertyType, enumClass, elementInfo, stream);

            FieldInfo param = null;
            if (itemValue != null)
            {
                object result = createInstanceForElement(objectClass, elementInfo);

                foreach (FieldInfo enumItem in enumClass.GetFields())
                {
                    if (CoderUtils.isAttributePresent<ASN1EnumItem>(enumItem))
                    {
                        ASN1EnumItem meta =
                            CoderUtils.getAttribute<ASN1EnumItem>(enumItem);
                        if (meta.Tag.Equals(itemValue.Value))
                        {
                            param = enumItem;
                            break;
                        }
                    }
                }
                invokeSetterMethodForField(field, result, param.GetValue(null), null);
                return new DecodedObject<object>(result, itemValue.Size);
            }
            else
                return null;
        }

        public abstract DecodedObject<object> decodeEnumItem(DecodedObject<object> decodedTag, Type objectClass, Type enumClass, ElementInfo elementInfo, Stream stream);


        public virtual DecodedObject<object> decodeElement(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream)
        {
            elementInfo.AnnotatedClass = objectClass;
            return decodeClassType(decodedTag, objectClass, elementInfo, stream);
        }

        public virtual DecodedObject<object> decodeBoxedType(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream)
        {
            object resultObj = createInstanceForElement(objectClass, elementInfo);

            DecodedObject<object> result = new DecodedObject<object>(resultObj);

            PropertyInfo field = objectClass.GetProperty("Value");
            if (elementInfo.ASN1ElementInfo == null)
            {
                elementInfo.ASN1ElementInfo = CoderUtils.getAttribute<ASN1Element>(field);
            }
            else
            {
                if (!elementInfo.ASN1ElementInfo.HasTag)
                {
                    ASN1Element fieldInfo = CoderUtils.getAttribute<ASN1Element>(field);
                    if (fieldInfo != null && fieldInfo.HasTag)
                    {
                        elementInfo.ASN1ElementInfo.HasTag = true;
                        elementInfo.ASN1ElementInfo.TagClass = fieldInfo.TagClass;
                        elementInfo.ASN1ElementInfo.IsImplicitTag = fieldInfo.IsImplicitTag;
                        elementInfo.ASN1ElementInfo.Tag = fieldInfo.Tag;
                    }
                }
            }
            elementInfo.AnnotatedClass = field;
            //elementInfo.setGenericInfo(field.getGenericType());

            DecodedObject<object> val = null;
            if (CoderUtils.isAttributePresent<ASN1Null>(field))
            {
                val = decodeNull(decodedTag, field.PropertyType, elementInfo, stream);
            }
            else
            {
                val = decodeClassType(decodedTag, field.PropertyType, elementInfo, stream);
                if (val != null)
                {
                    result.Size = val.Size;
                    invokeSetterMethodForField(field, resultObj, val.Value, elementInfo);
                }
                else
                    result = null;
            }
            return result;
        }

        public abstract DecodedObject<object> decodeBoolean(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);

        public abstract DecodedObject<object> decodeAny(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);

        public abstract DecodedObject<object> decodeNull(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);

        public abstract DecodedObject<object> decodeInteger(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);

        public abstract DecodedObject<object> decodeReal(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);

        public abstract DecodedObject<object> decodeOctetString(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);

        public abstract DecodedObject<object> decodeBitString(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);

        public abstract DecodedObject<object> decodeString(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);

        public abstract DecodedObject<object> decodeSequenceOf(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);

        public abstract DecodedObject<object> decodeTag(Stream stream);

        public abstract DecodedObject<object> decodeObjectIdentifier(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);

        public virtual T decode<T>(Stream stream)
        {
            Type objectClass = typeof(T);

            ElementInfo elemInfo = new ElementInfo();
            elemInfo.AnnotatedClass = objectClass;
            object objectInstance = null;
            if (CoderUtils.isImplements(objectClass, (typeof(IASN1PreparedElement))))
            {
                objectInstance = createInstanceForElement(objectClass, elemInfo);
            }

            if (objectInstance != null && objectInstance is IASN1PreparedElement)
            {
                elemInfo.PreparedInstance = objectInstance;
                return (T)decodePreparedElement(decodeTag(stream), objectClass, elemInfo, stream).Value;
            }
            else
            {
                elemInfo.ASN1ElementInfo = CoderUtils.getAttribute<ASN1Element>(objectClass);
                return (T)decodeClassType(decodeTag(stream), objectClass, elemInfo, stream).Value;
            }
        }

        public virtual DecodedObject<object> decodeCSElement(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream)
        {
            if (elementInfo.AnnotatedClass.Equals(typeof(string)))
            {
                return decodeString(decodedTag, objectClass, elementInfo, stream);
            }
            else if (elementInfo.AnnotatedClass.Equals(typeof(int)))
            {
                return decodeInteger(decodedTag, objectClass, elementInfo, stream);
            }
            else if (elementInfo.AnnotatedClass.Equals(typeof(long)))
            {
                return decodeInteger(decodedTag, objectClass, elementInfo, stream);
            }
            else if (elementInfo.AnnotatedClass.Equals(typeof(double)))
            {
                return decodeReal(decodedTag, objectClass, elementInfo, stream);
            }
            else if (elementInfo.AnnotatedClass.Equals(typeof(bool)))
            {
                return decodeBoolean(decodedTag, objectClass, elementInfo, stream);
            }
            else if (elementInfo.AnnotatedClass.Equals(typeof(byte[])))
            {
                return decodeOctetString(decodedTag, objectClass, elementInfo, stream);
            }
            else if (objectClass.IsEnum)
            {
                return decodeCSEnum(decodedTag, objectClass, elementInfo, stream);
            }
            else
                return null;
        }

        public virtual DecodedObject<object> decodeCSEnum(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream)
        {
            Type declaringType = Enum.GetUnderlyingType(objectClass);
            DecodedObject<object> result = decodeInteger(decodedTag, declaringType, elementInfo, stream);
            Object enumObj = Enum.ToObject(objectClass, result.Value);
            result.Value = enumObj;
            return result;
        }

        public virtual void initDefaultValues(object obj)
        {
            try
            {
                if (obj is IASN1PreparedElement)
                {
                    ((IASN1PreparedElement)obj).initWithDefaults();
                }
                else
                {
                    string methodName = "initWithDefaults";
                    MethodInfo method = obj.GetType().GetMethod(methodName);
                    method.Invoke(obj, null);
                }
            }
            catch (Exception)
            {
            }
            ;
        }

        public object createInstanceForElement(Type objectClass, ElementInfo info)
        {
            if (info.PreparedInstance != null)
            {
                return info.PreparedInstance;
            }
            else
                return Activator.CreateInstance(objectClass);
        }

        public virtual DecodedObject<object> decodeSequenceField(DecodedObject<object> fieldTag, object sequenceObj, int fieldIdx, PropertyInfo field, Stream stream, ElementInfo elementInfo, bool checkOptional)
        {
            ElementInfo info = new ElementInfo();
            info.AnnotatedClass = field;
            info.MaxAvailableLen = elementInfo.MaxAvailableLen;
            if (elementInfo.hasPreparedInfo())
            {
                info.PreparedInfo = elementInfo.PreparedInfo.getPropertyMetadata(fieldIdx);
            }
            else
                info.ASN1ElementInfo = CoderUtils.getAttribute<ASN1Element>(field);

            if (CoderUtils.isNullField(field, info))
            {
                return decodeNull(fieldTag, field.PropertyType, info, stream);
            }
            else
            {
                DecodedObject<object> val = decodeClassType(fieldTag, field.PropertyType, info, stream);
                if (val != null)
                {
                    invokeSetterMethodForField(field, sequenceObj, val.Value, info);
                }
                else
                {
                    if (checkOptional)
                        CoderUtils.checkForOptionalField(field, info);
                }
                return val;
            }
        }
    }
}