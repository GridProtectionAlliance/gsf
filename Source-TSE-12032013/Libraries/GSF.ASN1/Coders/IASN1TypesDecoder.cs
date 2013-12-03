//******************************************************************************************************
//  IASN1TypesDecoder.cs - Gbtc
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

namespace GSF.ASN1.Coders
{
    public interface IASN1TypesDecoder
    {
        DecodedObject<object> decodeTag(Stream stream);
        DecodedObject<object> decodeClassType(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);
        DecodedObject<object> decodeSequence(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);
        DecodedObject<object> decodeChoice(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);
        DecodedObject<object> decodeBoolean(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);
        DecodedObject<object> decodeAny(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);
        DecodedObject<object> decodeNull(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);
        DecodedObject<object> decodeInteger(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);
        DecodedObject<object> decodeReal(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);
        DecodedObject<object> decodeOctetString(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);
        DecodedObject<object> decodeBitString(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);
        DecodedObject<object> decodeObjectIdentifier(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);
        DecodedObject<object> decodeString(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);
        DecodedObject<object> decodeSequenceOf(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);
        DecodedObject<object> decodeEnum(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);
        DecodedObject<object> decodeEnumItem(DecodedObject<object> decodedTag, Type objectClass, Type enumClass, ElementInfo elementInfo, Stream stream);
        DecodedObject<object> decodeBoxedType(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);
        DecodedObject<object> decodeElement(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);
        DecodedObject<object> decodePreparedElement(DecodedObject<object> decodedTag, Type objectClass, ElementInfo elementInfo, Stream stream);
        void invokeSetterMethodForField(PropertyInfo field, object obj, Object param, ElementInfo elementInfo);
        void invokeSelectMethodForField(PropertyInfo field, object obj, Object param, ElementInfo elementInfo);
    }
}