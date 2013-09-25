//******************************************************************************************************
//  ElementInfo.cs - Gbtc
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
using System.Reflection;
using GSF.ASN1.Attributes;
using GSF.ASN1.Metadata;

namespace GSF.ASN1.Coders
{
    public class ElementInfo
    {
        private ICustomAttributeProvider annotatedClass;
        private ASN1Element element;
        private int maxAvailableLen = -1;
        private ICustomAttributeProvider parentAnnotatedClass;
        private ASN1ElementMetadata preparedElementMetadata;
        private IASN1PreparedElementData preparedInfo;
        private Object preparedInstance;

        public ICustomAttributeProvider ParentAnnotatedClass
        {
            get
            {
                return parentAnnotatedClass;
            }
            set
            {
                parentAnnotatedClass = value;
            }
        }

        public ASN1Element ASN1ElementInfo
        {
            get
            {
                return element;
            }
            set
            {
                element = value;
            }
        }

        public ICustomAttributeProvider AnnotatedClass
        {
            get
            {
                return annotatedClass;
            }
            set
            {
                annotatedClass = value;
            }
        }

        public IASN1PreparedElementData PreparedInfo
        {
            get
            {
                return preparedInfo;
            }
            set
            {
                preparedInfo = value;
                if (preparedInfo != null)
                {
                    PreparedASN1ElementInfo = preparedInfo.ASN1ElementInfo;
                }
            }
        }

        public Object PreparedInstance
        {
            get
            {
                return preparedInstance;
            }
            set
            {
                preparedInstance = value;
            }
        }

        public ASN1ElementMetadata PreparedASN1ElementInfo
        {
            get
            {
                return preparedElementMetadata;
            }
            set
            {
                preparedElementMetadata = value;
            }
        }

        public int MaxAvailableLen
        {
            get
            {
                return maxAvailableLen;
            }
            set
            {
                maxAvailableLen = value;
            }
        }

        public bool hasPreparedInfo()
        {
            return preparedInfo != null;
        }

        public bool hasPreparedASN1ElementInfo()
        {
            return preparedElementMetadata != null;
        }

        public PropertyInfo[] getProperties(Type objClass)
        {
            if (hasPreparedInfo())
            {
                return PreparedInfo.Properties;
            }
            else
                return objClass.GetProperties();
        }


        public bool isAttributePresent<T>()
        {
            return CoderUtils.isAttributePresent<T>(annotatedClass);
        }

        public T getAttribute<T>()
        {
            return CoderUtils.getAttribute<T>(annotatedClass);
        }

        public bool isParentAttributePresent<T>()
        {
            return CoderUtils.isAttributePresent<T>(parentAnnotatedClass);
        }

        public T getParentAttribute<T>()
        {
            return CoderUtils.getAttribute<T>(parentAnnotatedClass);
        }
    }
}