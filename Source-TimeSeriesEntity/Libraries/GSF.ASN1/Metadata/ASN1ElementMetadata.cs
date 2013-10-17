//******************************************************************************************************
//  ASN1ElementMetadata.cs - Gbtc
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
using GSF.ASN1.Attributes;
using GSF.ASN1.Coders;

namespace GSF.ASN1.Metadata
{
    public class ASN1ElementMetadata : ASN1FieldMetadata
    {
        private readonly bool hasDefaultValue;
        private readonly bool hasTag;
        private readonly bool isImplicitTag;
        private readonly bool isOptional = true;
        private readonly int tag;
        private readonly int tagClass = TagClasses.ContextSpecific;

        public ASN1ElementMetadata(ASN1Element annotation) :
            this(
            annotation.Name,
            annotation.IsOptional,
            annotation.HasTag,
            annotation.IsImplicitTag,
            annotation.TagClass,
            annotation.Tag,
            annotation.HasDefaultValue
            )
        {
        }

        public ASN1ElementMetadata(String name,
                                   bool isOptional,
                                   bool hasTag,
                                   bool isImplicitTag,
                                   int tagClass,
                                   int tag,
                                   bool hasDefaultValue)
            : base(name)
        {
            this.isOptional = isOptional;
            this.hasTag = hasTag;
            this.isImplicitTag = isImplicitTag;
            this.tagClass = tagClass;
            this.tag = tag;
            this.hasDefaultValue = hasDefaultValue;
        }

        public bool IsOptional
        {
            get
            {
                return isOptional;
            }
        }


        public bool HasTag
        {
            get
            {
                return hasTag;
            }
        }

        public bool IsImplicitTag
        {
            get
            {
                return isImplicitTag;
            }
        }

        public int TagClass
        {
            get
            {
                return tagClass;
            }
        }

        public int Tag
        {
            get
            {
                return tag;
            }
        }

        public bool HasDefaultValue
        {
            get
            {
                return hasDefaultValue;
            }
        }

        public override int encode(IASN1TypesEncoder encoder, object obj, Stream stream, ElementInfo elementInfo)
        {
            return encoder.encodePreparedElement(obj, stream, elementInfo);
        }

        public override DecodedObject<object> decode(IASN1TypesDecoder decoder, DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream)
        {
            elementInfo.PreparedInstance = null;
            return decoder.decodeElement(decodedTag, objectClass, elementInfo, stream);
        }
    }
}