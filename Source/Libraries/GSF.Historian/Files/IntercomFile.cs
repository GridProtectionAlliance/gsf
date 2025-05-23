﻿//******************************************************************************************************
//  IntercomFile.cs - Gbtc
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
//  03/09/2007 - Pinal C. Patel
//       Generated original version of source code.
//  04/21/2009 - Pinal C. Patel
//       Converted to C#.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/16/2009 - Pinal C. Patel
//       Changed the default value for SettingsCategory property to the type name.
//  10/11/2010 - Mihir Brahmbhatt
//       Updated header and license agreement.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.Drawing;
using GSF.IO;

namespace GSF.Historian.Files;

/// <summary>
/// Represents a file containing <see cref="IntercomRecord"/>s.
/// </summary>
/// <seealso cref="IntercomRecord"/>
[ToolboxBitmap(typeof(IntercomFile))]
public class IntercomFile : IsamDataFileBase<IntercomRecord>
{
    #region [ Constructors ]

    /// <summary>
    /// Initializes a new instance of the <see cref="IntercomFile"/> class.
    /// </summary>
    public IntercomFile()
    {
        SettingsCategory = GetType().Name;
    }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Gets the binary size of a <see cref="IntercomRecord"/>.
    /// </summary>
    /// <returns>A 32-bit signed integer.</returns>
    protected override int GetRecordSize()
    {
        return IntercomRecord.FixedLength;
    }

    /// <summary>
    /// Creates a new <see cref="IntercomRecord"/> with the specified <paramref name="recordIndex"/>.
    /// </summary>
    /// <param name="recordIndex">1-based index of the <see cref="IntercomRecord"/>.</param>
    /// <returns>A <see cref="IntercomRecord"/> object.</returns>
    protected override IntercomRecord CreateNewRecord(int recordIndex)
    {
        return new IntercomRecord(recordIndex);
    }

    #endregion
}