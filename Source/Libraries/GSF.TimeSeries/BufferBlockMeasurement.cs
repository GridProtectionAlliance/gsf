//******************************************************************************************************
//  BufferBlockMeasurement.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  01/17/2013 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GSF.TimeSeries;

/// <summary>
/// Represents a byte buffer that can be transported through the system as a <see cref="IMeasurement"/>.
/// </summary>
public class BufferBlockMeasurement : Measurement
{
    #region [ Constructors ]

    /// <summary>
    /// Creates a new <see cref="BufferBlockMeasurement"/>.
    /// </summary>
    public BufferBlockMeasurement()
    {
        Value = double.NaN; // Value of measurement should be indeterminate since this a buffer
    }

    /// <summary>
    /// Creates a new <see cref="BufferBlockMeasurement"/> from an existing buffer.
    /// </summary>
    /// <param name="buffer">Source buffer.</param>
    /// <param name="startIndex">Start index of valid data in source buffer.</param>
    /// <param name="length">Valid length of source buffer.</param>
    /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="startIndex"/> or <paramref name="length"/> is less than 0 -or- 
    /// <paramref name="startIndex"/> and <paramref name="length"/> will exceed <paramref name="buffer"/> length.
    /// </exception>
    public BufferBlockMeasurement(byte[] buffer, int startIndex, int length) : this()
    {
        // Validate buffer parameters
        buffer.ValidateParameters(startIndex, length);

        // We don't hold on to source buffer (we don't own it), so we instantiate a new one
        Buffer = new byte[length];

        // Copy buffer contents onto our local buffer
        System.Buffer.BlockCopy(buffer, startIndex, Buffer, 0, length);
        Length = length;
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Cached buffer image.
    /// </summary>
    public byte[] Buffer { get; }

    /// <summary>
    /// Valid length of cached buffer image.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Gets or sets a value indicating whether reception of this buffer block must be acknowledged
    /// by the subscriber with a <c>ConfirmBufferBlock</c> command.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Maps to IEEE Std 2664-2024 Table 8 bit <c>0x01</c> (REQUIRE CONFIRMATION) on the wire.
    /// Default value is <c>true</c>, preserving STTP's traditional retransmission semantics where
    /// the publisher caches each buffer block until acknowledged and retransmits on timeout.
    /// </para>
    /// <para>
    /// When set to <c>false</c>, the buffer block is fire-and-forget: no retransmission cache entry
    /// is added, the retransmission timer is not (re)started, and the subscriber does not emit a
    /// confirmation. Useful for high-rate buffer-block streams over reliable transports (TCP)
    /// where the round-trip acknowledgement overhead is unnecessary.
    /// </para>
    /// </remarks>
    public bool RequireConfirmation { get; set; } = true;

    /// <summary>
    /// Gets or sets the raw buffer-block flag byte as it appeared on the wire (IEEE Std 2664-2024
    /// Table 8). On receive, this carries the publisher's full intent (REQUIRE CONFIRMATION,
    /// COMPRESSED, CACHE INDEX, etc.); on send, this field is unused by the wire codec - the codec
    /// constructs the byte from <see cref="RequireConfirmation"/> and other per-block state.
    /// </summary>
    /// <remarks>
    /// Stored as a raw <see cref="byte"/> so this assembly need not take a dependency on the STTP
    /// flag enum. Cast to <c>BufferBlockFlags</c> at the call site if structured inspection is needed.
    /// </remarks>
    public byte Flags { get; set; }

    #endregion
}