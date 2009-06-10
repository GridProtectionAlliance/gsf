//*******************************************************************************************************
//  IAnalogDefintion.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/18/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace PhasorProtocols
{
    #region [ Enumerations ]

    /// <summary>
    /// Analog types enumeration.
    /// </summary>
    [Serializable()]
    public enum AnalogType : byte
    {
        /// <summary>
        /// Single point-on-wave.
        /// </summary>
        SinglePointOnWave = 0,
        /// <summary>
        /// RMS of analog input.
        /// </summary>
        RmsOfAnalogInput = 1,
        /// <summary>
        /// Peak of analog input.
        /// </summary>
        PeakOfAnalogInput = 2
    }

    #endregion

    /// <summary>
    /// Represents a protocol independent interface representation of a definition of an <see cref="IAnalogValue"/>.
    /// </summary>
    public interface IAnalogDefinition : IChannelDefinition
    {
        /// <summary>
        /// Gets or sets <see cref="AnalogType"/> of this <see cref="IAnalogDefinition"/>.
        /// </summary>
        AnalogType AnalogType { get; set; }
    }
}