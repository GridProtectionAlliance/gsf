//******************************************************************************************************
//  IASN1TypesEncoder.cs - Gbtc
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
    public interface IASN1TypesEncoder
    {
        int encodeClassType(object obj, Stream stream, ElementInfo elementInfo);
        int encodeSequence(object obj, Stream stream, ElementInfo elementInfo);
        int encodeChoice(object obj, Stream stream, ElementInfo elementInfo);
        int encodeEnum(object obj, Stream stream, ElementInfo elementInfo);
        int encodeEnumItem(Object enumConstant, Type enumClass, Stream stream, ElementInfo elementInfo);
        int encodeBoolean(object obj, Stream stream, ElementInfo elementInfo);
        int encodeAny(object obj, Stream stream, ElementInfo elementInfo);
        int encodeNull(object obj, Stream stream, ElementInfo elementInfo);
        int encodeInteger(object obj, Stream steam, ElementInfo elementInfo);
        int encodeReal(object obj, Stream steam, ElementInfo elementInfo);
        int encodeOctetString(object obj, Stream steam, ElementInfo elementInfo);
        int encodeBitString(object obj, Stream steam, ElementInfo elementInfo);
        int encodeObjectIdentifier(object obj, Stream steam, ElementInfo elementInfo);
        int encodeString(object obj, Stream steam, ElementInfo elementInfo);
        int encodeSequenceOf(object obj, Stream steam, ElementInfo elementInfo);
        int encodeElement(object obj, Stream stream, ElementInfo elementInfo);
        int encodeBoxedType(object obj, Stream stream, ElementInfo elementInfo);
        int encodePreparedElement(object obj, Stream stream, ElementInfo elementInfo);
        object invokeGetterMethodForField(PropertyInfo field, object obj, ElementInfo elementInfo);
        bool invokeSelectedMethodForField(PropertyInfo field, object obj, ElementInfo elementInfo);
    }
}