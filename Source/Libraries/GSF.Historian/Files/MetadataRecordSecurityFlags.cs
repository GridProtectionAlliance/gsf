//******************************************************************************************************
//  MetadataRecordSecurityFlags.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  03/06/2007 - Pinal C. Patel
//       Generated original version of code based on DatAWare system specifications by Brian B. Fox, GSF.
//  04/20/2009 - Pinal C. Patel
//       Converted to C#.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/11/2010 - Mihir Brahmbhatt
//       Updated header and license agreement.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

namespace GSF.Historian.Files;

/// <summary>
/// Defines the security level for a <see cref="MetadataRecord"/>.
/// </summary>
/// <seealso cref="MetadataRecord"/>
public class MetadataRecordSecurityFlags
{
    #region [ Members ]

    // Constants
    private const Bits RecordSecurityMask = Bits.Bit00 | Bits.Bit01 | Bits.Bit02;
    private const Bits AccessSecurityMask = Bits.Bit03 | Bits.Bit04 | Bits.Bit05;

    // Fields

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets the access level required for a user to edit the <see cref="MetadataRecord"/>.
    /// </summary>
    public int ChangeSecurity
    {
        get => Value.GetMaskedValue(RecordSecurityMask);
        set => Value = Value.SetMaskedValue(RecordSecurityMask, value);
    }

    /// <summary>
    /// Gets or sets the access level required for a user to retrieve archived data associated to a <see cref="MetadataRecord"/>.
    /// </summary>
    public int AccessSecurity
    {
        get => Value.GetMaskedValue(AccessSecurityMask) >> 3; // <- 1st 3 bits are record security.
        set => Value = Value.SetMaskedValue(AccessSecurityMask, value << 3);
    }

    /// <summary>
    /// Gets or sets the 32-bit integer value used for defining the security level for a <see cref="MetadataRecord"/>.
    /// </summary>
    public int Value { get; set; }

    #endregion
}