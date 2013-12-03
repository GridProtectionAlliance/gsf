//******************************************************************************************************
//  ASN1PreparedElementData.cs - Gbtc
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
using GSF.ASN1.Attributes;
using GSF.ASN1.Attributes.Constraints;
using GSF.ASN1.Metadata;
using GSF.ASN1.Metadata.Constraints;
using GSF.ASN1.Types;

namespace GSF.ASN1.Coders
{
    public class ASN1PreparedElementData : IASN1PreparedElementData
    {
        private ASN1ElementMetadata asn1ElementInfo;
        private IASN1ConstraintMetadata constraint;
        private MethodInfo doSelectMethod;
        private MethodInfo isPresentMethod;
        private MethodInfo isSelectedMethod;
        private PropertyInfo[] properties;
        private ASN1PreparedElementData[] propertiesMetadata;
        private ASN1Metadata typeMeta;
        private ASN1PreparedElementData valueMetadata;
        private PropertyInfo valueProperty;

        public ASN1PreparedElementData(Type objectClass)
        {
            setupMetadata(objectClass, objectClass);
            setupConstructed(objectClass);
        }

        public ASN1PreparedElementData(Type parentClass, PropertyInfo field)
        {
            setupMetadata(field, field.PropertyType);
            setupAccessors(parentClass, field);
        }

        public ASN1Metadata TypeMetadata
        {
            get
            {
                return typeMeta;
            }
        }

        public IASN1ConstraintMetadata Constraint
        {
            get
            {
                return constraint;
            }
        }

        public bool hasConstraint()
        {
            return constraint != null;
        }

        public PropertyInfo[] Properties
        {
            get
            {
                return properties;
            }
        }

        public PropertyInfo getProperty(int index)
        {
            return properties[index];
        }

        public ASN1PreparedElementData getPropertyMetadata(int index)
        {
            return propertiesMetadata[index];
        }

        public PropertyInfo ValueProperty
        {
            get
            {
                return valueProperty;
            }
        }

        public ASN1PreparedElementData ValueMetadata
        {
            get
            {
                return valueMetadata;
            }
        }

        public ASN1ElementMetadata ASN1ElementInfo
        {
            get
            {
                return asn1ElementInfo;
            }
        }

        public bool hasASN1ElementInfo()
        {
            return asn1ElementInfo != null;
        }

        public object invokeDoSelectMethod(object obj, object param)
        {
            return doSelectMethod.Invoke(obj, new[] { param });
        }

        public object invokeIsSelectedMethod(object obj, object param)
        {
            return isSelectedMethod.Invoke(obj, null);
        }

        public MethodInfo IsPresentMethod
        {
            get
            {
                return isPresentMethod;
            }
        }

        /*public ASN1PreparedElementData(Type parentClass, String propertyName)
        {
            try
            {
                PropertyInfo field = parentClass.GetProperty(propertyName);
                setupMetadata(field, field.PropertyType);
                setupAccessors(parentClass, field);
            }
            catch (Exception ex)
            {
                ex = null;
            }
        }*/


        private void setupMetadata(ICustomAttributeProvider annotated, Type objectClass)
        {
            if (CoderUtils.isAttributePresent<ASN1SequenceOf>(annotated))
            {
                typeMeta = new ASN1SequenceOfMetadata(CoderUtils.getAttribute<ASN1SequenceOf>(annotated),
                                                      objectClass,
                                                      annotated
                    );
            }
            else if (CoderUtils.isAttributePresent<ASN1Sequence>(annotated))
            {
                typeMeta = new ASN1SequenceMetadata(CoderUtils.getAttribute<ASN1Sequence>(annotated));
            }
            else if (CoderUtils.isAttributePresent<ASN1Choice>(annotated))
            {
                typeMeta = new ASN1ChoiceMetadata(CoderUtils.getAttribute<ASN1Choice>(annotated));
            }
            else if (CoderUtils.isAttributePresent<ASN1Enum>(annotated))
            {
                typeMeta = new ASN1EnumMetadata(CoderUtils.getAttribute<ASN1Enum>(annotated));
            }
            else if (CoderUtils.isAttributePresent<ASN1Boolean>(annotated))
            {
                typeMeta = new ASN1BooleanMetadata(CoderUtils.getAttribute<ASN1Boolean>(annotated));
            }
            else if (CoderUtils.isAttributePresent<ASN1Any>(annotated))
            {
                typeMeta = new ASN1AnyMetadata(CoderUtils.getAttribute<ASN1Any>(annotated));
            }
            else if (CoderUtils.isAttributePresent<ASN1Integer>(annotated))
            {
                typeMeta = new ASN1IntegerMetadata(CoderUtils.getAttribute<ASN1Integer>(annotated));
            }
            else if (CoderUtils.isAttributePresent<ASN1Real>(annotated))
            {
                typeMeta = new ASN1RealMetadata(CoderUtils.getAttribute<ASN1Real>(annotated));
            }
            else if (CoderUtils.isAttributePresent<ASN1OctetString>(annotated))
            {
                typeMeta = new ASN1OctetStringMetadata(CoderUtils.getAttribute<ASN1OctetString>(annotated));
            }
            else if (CoderUtils.isAttributePresent<ASN1BitString>(annotated) || objectClass.Equals(typeof(BitString)))
            {
                typeMeta = new ASN1BitStringMetadata(CoderUtils.getAttribute<ASN1BitString>(annotated));
            }
            else if (CoderUtils.isAttributePresent<ASN1ObjectIdentifier>(annotated) || objectClass.Equals(typeof(ObjectIdentifier)))
            {
                typeMeta = new ASN1ObjectIdentifierMetadata(CoderUtils.getAttribute<ASN1ObjectIdentifier>(annotated));
            }
            else if (CoderUtils.isAttributePresent<ASN1String>(annotated))
            {
                typeMeta = new ASN1StringMetadata(CoderUtils.getAttribute<ASN1String>(annotated));
            }
            else if (CoderUtils.isAttributePresent<ASN1Null>(annotated))
            {
                typeMeta = new ASN1NullMetadata(CoderUtils.getAttribute<ASN1Null>(annotated));
            }
            else if (CoderUtils.isAttributePresent<ASN1BoxedType>(annotated))
            {
                typeMeta = new ASN1BoxedTypeMetadata(objectClass, CoderUtils.getAttribute<ASN1BoxedType>(annotated));
            }
            else if (CoderUtils.isAttributePresent<ASN1Element>(annotated))
            {
                typeMeta = new ASN1ElementMetadata(CoderUtils.getAttribute<ASN1Element>(annotated));
            }
            else if (objectClass.Equals(typeof(String)))
            {
                typeMeta = new ASN1StringMetadata();
            }
            else if (objectClass.Equals(typeof(int)))
            {
                typeMeta = new ASN1IntegerMetadata();
            }
            else if (objectClass.Equals(typeof(long)))
            {
                typeMeta = new ASN1IntegerMetadata();
            }
            else if (objectClass.Equals(typeof(double)))
            {
                typeMeta = new ASN1RealMetadata();
            }
            else if (objectClass.Equals(typeof(bool)))
            {
                typeMeta = new ASN1BooleanMetadata();
            }
            else if (objectClass.Equals(typeof(byte[])))
            {
                typeMeta = new ASN1OctetStringMetadata();
            }

            if (CoderUtils.isAttributePresent<ASN1Element>(annotated))
            {
                asn1ElementInfo = new ASN1ElementMetadata(CoderUtils.getAttribute<ASN1Element>(annotated));
            }

            setupConstraint(annotated);
        }

        private void setupConstructed(Type objectClass)
        {
            int count = 0;
            PropertyInfo[] srcFields = null;
            if (typeMeta != null && typeMeta is ASN1SequenceMetadata && ((ASN1SequenceMetadata)typeMeta).IsSet)
            {
                SortedList<int, PropertyInfo> fieldOrder = CoderUtils.getSetOrder(objectClass);
                srcFields = new PropertyInfo[fieldOrder.Count];
                fieldOrder.Values.CopyTo(srcFields, 0);
                count = srcFields.Length;
            }
            else
            {
                srcFields = objectClass.GetProperties(); //objectClass.getDeclaredFields();
                foreach (PropertyInfo field in srcFields)
                {
                    if (!field.PropertyType.Equals(typeof(IASN1PreparedElementData)))
                    {
                        count++;
                    }
                }
            }

            properties = new PropertyInfo[count];
            propertiesMetadata = new ASN1PreparedElementData[count];
            int idx = 0;
            foreach (PropertyInfo field in srcFields)
            {
                if (!field.PropertyType.Equals(typeof(IASN1PreparedElementData)))
                {
                    properties[idx] = field;
                    propertiesMetadata[idx] = new ASN1PreparedElementData(objectClass, field);

                    if (properties[idx].Name.Equals("Value"))
                    {
                        setValueField(field, propertiesMetadata[idx]);
                    }
                    idx++;
                }
            }
        }

        public void setValueField(PropertyInfo valueProperty, ASN1PreparedElementData valuePropertyMeta)
        {
            this.valueProperty = valueProperty;
            valueMetadata = valuePropertyMeta;
        }

        private void setupAccessors(Type objectClass, PropertyInfo field)
        {
            try
            {
                doSelectMethod = CoderUtils.findDoSelectMethodForField(field, objectClass);
            }
            catch (Exception)
            {
            }

            try
            {
                isSelectedMethod = CoderUtils.findIsSelectedMethodForField(field, objectClass);
            }
            catch (Exception)
            {
            }

            try
            {
                isPresentMethod = CoderUtils.findIsPresentMethodForField(field, objectClass);
            }
            catch (Exception)
            {
            }
        }


        private void setupConstraint(ICustomAttributeProvider annotated)
        {
            if (CoderUtils.isAttributePresent<ASN1SizeConstraint>(annotated))
            {
                constraint = new ASN1SizeConstraintMetadata(CoderUtils.getAttribute<ASN1SizeConstraint>(annotated));
            }
            else if (CoderUtils.isAttributePresent<ASN1ValueRangeConstraint>(annotated))
            {
                constraint = new ASN1ValueRangeConstraintMetadata(CoderUtils.getAttribute<ASN1ValueRangeConstraint>(annotated));
            }
        }
    }
}