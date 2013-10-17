//******************************************************************************************************
//  ASN1SequenceOfMetadata.cs - Gbtc
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
using GSF.ASN1.Coders;

namespace GSF.ASN1.Metadata
{
    public class ASN1SequenceOfMetadata : ASN1FieldMetadata
    {
        private readonly bool isSetOf;
        private readonly ASN1PreparedElementData itemClassMetadata;
        private Type itemClass;

        public ASN1SequenceOfMetadata(String name, bool isSetOf, Type itemClass, ICustomAttributeProvider seqFieldAnnotatedElem)
            : base(name)
        {
            this.isSetOf = isSetOf;
            this.itemClass = itemClass;
            Type paramType = itemClass.GetGenericArguments()[0];
            itemClassMetadata = new ASN1PreparedElementData(paramType);
            if (itemClassMetadata.TypeMetadata != null)
                itemClassMetadata.TypeMetadata.setParentAnnotated(seqFieldAnnotatedElem);
        }

        public ASN1SequenceOfMetadata(ASN1SequenceOf annotation, Type itemClass, ICustomAttributeProvider seqFieldAnnotatedElem)
            : this(annotation.Name, annotation.IsSetOf, itemClass, seqFieldAnnotatedElem)
        {
        }

        public bool IsSetOf
        {
            get
            {
                return isSetOf;
            }
        }

        public IASN1PreparedElementData getItemClassMetadata()
        {
            return itemClassMetadata;
        }

        public override int encode(IASN1TypesEncoder encoder, object obj, Stream stream, ElementInfo elementInfo)
        {
            return encoder.encodeSequenceOf(obj, stream, elementInfo);
        }

        public override DecodedObject<object> decode(IASN1TypesDecoder decoder, DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream)
        {
            return decoder.decodeSequenceOf(decodedTag, objectClass, elementInfo, stream);
        }
    }
}