//******************************************************************************************************
//  IChannelFrame.cs - Gbtc
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
//  01/14/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/5/2012 - Gavin E. Holden
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Runtime.Serialization;
using GSF.TimeSeries;

namespace GSF.PhasorProtocols
{
    #region [ Enumerations ]

    /// <summary>
    /// Fundamental frame types enumeration.
    /// </summary>
    [Serializable]
    public enum FundamentalFrameType
    {
        /// <summary>
        /// Configuration frame.
        /// </summary>
        ConfigurationFrame,
        /// <summary>
        /// Data frame.
        /// </summary>
        DataFrame,
        /// <summary>
        /// Header frame.
        /// </summary>
        HeaderFrame,
        /// <summary>
        /// Command frame.
        /// </summary>
        CommandFrame,
        /// <summary>
        /// Undetermined frame type.
        /// </summary>
        Undetermined
    }

    #endregion

    /// <summary>
    /// Represents a protocol independent interface representation of any kind of frame of data that contains
    /// a collection of <see cref="IChannelCell"/> objects.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The phasor protocols library defines a "frame" as a collection of cells (logical units of data).
    /// For example, a <see cref="IDataCell"/> would be defined as a PMU within a frame of data, a <see cref="IDataFrame"/>
    /// (derived from <see cref="IChannelFrame"/>), that contains multiple PMU's coming from a PDC.
    /// </para>
    /// <para>
    /// This interface inherits <see cref="IFrame"/> so that classes implementing this interface can be cooperatively
    /// integrated into measurement concentration (see <see cref="ConcentratorBase"/>).
    /// </para>
    /// </remarks>
    public interface IChannelFrame : IChannel, IFrame, ISerializable
    {
        // We keep this IChannelFrame as a simple non-generic interface to prevent complex circular type definitions
        // that would be caused by a strongly-typed "parent" reference in IChannelCell. The parent reference is
        // extremely useful in the actual protocol implementations since many of the properties of a cell and a
        // frame may be shared. The strongly-typed generic version of the IChannelFrame interface is below.

        /// <summary>
        /// Gets the <see cref="FundamentalFrameType"/> for this <see cref="IChannelFrame"/>.
        /// </summary>
        FundamentalFrameType FrameType { get; }

        /// <summary>
        /// Gets the simple reference to the collection of cells for this <see cref="IChannelFrame"/>.
        /// </summary>
        object Cells { get; }

        /// <summary>
        /// Gets or sets the ID code of this <see cref="IChannelFrame"/>.
        /// </summary>
        ushort IDCode { get; set; }

        /// <summary>
        /// Gets UNIX based time representation of the ticks of this <see cref="IChannelFrame"/>.
        /// </summary>
        UnixTimeTag TimeTag { get; }
    }

    /// <summary>
    /// Represents a strongly-typed protocol independent interface representation of any kind of frame of data that
    /// contains a collection of <see cref="IChannelCell"/> objects.
    /// </summary>
    /// <remarks>
    /// This interface inherits <see cref="IFrame"/> so that classes implementing this interface can be cooperatively
    /// integrated into measurement concentration (see <see cref="ConcentratorBase"/>).
    /// </remarks>
    /// <typeparam name="T">Specific <see cref="IChannelCell"/> type that the <see cref="IChannelFrame{T}"/> contains.</typeparam>
    public interface IChannelFrame<T> : IChannelFrame where T : IChannelCell
    {
        /// <summary>
        /// Gets the strongly-typed reference to the collection of cells for this <see cref="IChannelFrame{T}"/>.
        /// </summary>
        new IChannelCellCollection<T> Cells { get; }

        /// <summary>
        /// Gets or sets the parsing state for this <see cref="IChannelFrame{T}"/>.
        /// </summary>
        new IChannelFrameParsingState<T> State { get; set; }
    }
}