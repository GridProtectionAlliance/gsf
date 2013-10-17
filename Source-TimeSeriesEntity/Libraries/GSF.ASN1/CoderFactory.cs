//******************************************************************************************************
//  CoderFactory.cs - Gbtc
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
using GSF.ASN1.Coders;
using GSF.ASN1.Coders.BER;
using GSF.ASN1.Coders.DER;
using GSF.ASN1.Coders.PER;

namespace GSF.ASN1
{
    public class CoderFactory
    {
        private static readonly CoderFactory instance = new CoderFactory();

        public static CoderFactory getInstance()
        {
            return instance;
        }

        public IEncoder newEncoder()
        {
            return newEncoder("BER");
        }

        public IEncoder newEncoder(String encodingSchema)
        {
            if (encodingSchema.Equals("BER", StringComparison.CurrentCultureIgnoreCase))
            {
                return new BEREncoder();
            }
            else if (encodingSchema.Equals("PER", StringComparison.CurrentCultureIgnoreCase) ||
                     encodingSchema.Equals("PER/Aligned", StringComparison.CurrentCultureIgnoreCase) ||
                     encodingSchema.Equals("PER/A", StringComparison.CurrentCultureIgnoreCase))
            {
                return new PERAlignedEncoder();
            }
            else if (encodingSchema.Equals("PER/Unaligned", StringComparison.CurrentCultureIgnoreCase) ||
                     encodingSchema.Equals("PER/U", StringComparison.CurrentCultureIgnoreCase))
            {
                return new PERUnalignedEncoder();
            }
            else if (encodingSchema.Equals("DER", StringComparison.CurrentCultureIgnoreCase))
            {
                return new DEREncoder();
            }
            else
                return null;
        }

        public IDecoder newDecoder()
        {
            return newDecoder("BER");
        }

        public IDecoder newDecoder(String encodingSchema)
        {
            if (encodingSchema.Equals("BER", StringComparison.CurrentCultureIgnoreCase))
            {
                return new BERDecoder();
            }
            else if (encodingSchema.Equals("PER", StringComparison.CurrentCultureIgnoreCase) ||
                     encodingSchema.Equals("PER/Aligned", StringComparison.CurrentCultureIgnoreCase) ||
                     encodingSchema.Equals("PER/A", StringComparison.CurrentCultureIgnoreCase))
            {
                return new PERAlignedDecoder();
            }
            else if (encodingSchema.Equals("PER/Unaligned", StringComparison.CurrentCultureIgnoreCase) ||
                     encodingSchema.Equals("PER/U", StringComparison.CurrentCultureIgnoreCase))
            {
                return new PERUnalignedDecoder();
            }
            else if (encodingSchema.Equals("DER", StringComparison.CurrentCultureIgnoreCase))
            {
                return new DERDecoder();
            }
            else
                return null;
        }

        public IASN1PreparedElementData newPreparedElementData(Type typeInfo)
        {
            return new ASN1PreparedElementData(typeInfo);
        }
    }
}