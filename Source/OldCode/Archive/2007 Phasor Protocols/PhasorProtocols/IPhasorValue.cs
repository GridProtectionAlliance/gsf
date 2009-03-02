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

using System.Units;

namespace PCS.PhasorProtocols
{
    // TODO: Determine where this was used - and why...  If not used - delete.
    ///// <summary>
    ///// Defines function signature for creating new phasor values.
    ///// </summary>
    ///// <param name="parent">Parent <see cref="IDataCell"/>.</param>
    ///// <param name="phasorDefinition">Associated <see cref="IPhasorDefinition"/>.</param>
    ///// <param name="real">Real value of new phasor measurement.</param>
    ///// <param name="imaginary">Imaginary value of new phasor measurement.</param>
    ///// <returns>New <see cref="IPhasorValue"/> instance.</returns>
    //protected delegate IPhasorValue CreateNewPhasorValueFunctionSignature(IDataCell parent, IPhasorDefinition phasorDefinition, double real, double imaginary);

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
        /// Gets or sets the <see cref="System.Units.Angle"/> value (a.k.a., the argument) of this <see cref="IPhasorValue"/>, in radians.
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