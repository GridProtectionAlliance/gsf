﻿//******************************************************************************************************
//  ArchiveFileStatistics.cs - Gbtc
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
//  06/26/2009 - Pinal C. Patel
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/24/2009 - Pinal C. Patel
//       Added member initialization to the constructor.
//  10/11/2010 - Mihir Brahmbhatt
//       Updated header and license agreement.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using GSF.Units;

namespace GSF.Historian.Files;

/// <summary>
/// A class that contains the statistics of an <see cref="ArchiveFile"/>.
/// </summary>
/// <seealso cref="ArchiveFile"/>
public class ArchiveFileStatistics
{
    /// <summary>
    /// Current usage (in %) of the <see cref="ArchiveFile"/>.
    /// </summary>
    public float FileUsage;

    /// <summary>
    /// Current rate of data compression (in %) in the <see cref="ArchiveFile"/>.
    /// </summary>
    public float CompressionRate;

    /// <summary>
    /// <see cref="Time"/> over which the <see cref="AverageWriteSpeed"/> is calculated.
    /// </summary>
    public Time AveragingWindow;

    /// <summary>
    /// Average number of time-series data points written to the <see cref="ArchiveFile"/> in one second.
    /// </summary>
    public int AverageWriteSpeed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveFileStatistics"/> class.
    /// </summary>
    internal ArchiveFileStatistics()
    {
        AveragingWindow = Time.MinValue;
    }
}