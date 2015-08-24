//*******************************************************************************************************
//  IPhasorValue.cs
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

using TVA.Units;

namespace TVA.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent interface representation of a phasor value.
    /// </summary>
    public interface IPhasorValue : IChannelValue<IPhasorDefinition>
    {
        /// <summary>
        /// Gets the <see cref="PhasorProtocols.CoordinateFormat"/> of this <see cref="IPhasorValue"/>.
        /// </summary>
        CoordinateFormat CoordinateFormat { get; }

        /// <summary>
        /// Gets the <see cref="PhasorType"/> of this <see cref="IPhasorValue"/>.
        /// </summary>
        PhasorType Type { get; }

        /// <summary>
        /// Gets or sets the <see cref="TVA.Units.Angle"/> value (a.k.a., the argument) of this <see cref="IPhasorValue"/>, in radians.
        /// </summary>
        Angle Angle { get; set; }

        /// <summary>
        /// Gets or sets the magnitude value (a.k.a., the absolute value or modulus) of this <see cref="IPhasorValue"/>.
        /// </summary>
        double Magnitude { get; set; }

        /// <summary>
        /// Gets or sets the real floating point value of this <see cref="IPhasorValue"/>.
        /// </summary>
        double Real { get; set; }

        /// <summary>
        /// Gets or sets the imaginary floating point value of this <see cref="IPhasorValue"/>.
        /// </summary>
        double Imaginary { get; set; }

        /// <summary>
        /// Gets or sets the unscaled integer representation of the real value of this <see cref="IPhasorValue"/>.
        /// </summary>
        int UnscaledReal { get; set; }

        /// <summary>
        /// Gets or sets the unscaled integer representation of the imaginary value of this <see cref="IPhasorValue"/>.
        /// </summary>
        int UnscaledImaginary { get; set; }
    }
}