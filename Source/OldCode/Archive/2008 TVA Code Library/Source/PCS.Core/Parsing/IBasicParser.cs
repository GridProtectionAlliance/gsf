//*******************************************************************************************************
//  IFrameParser.vb - Frame parsing interface
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2008
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/14/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

using System;

namespace PCS.Parsing
{
    /// <summary>
    /// This interface represents the protocol independent representation of a binary image parser suitable for common,
    /// simple formatted, binary data streams returning the parsed data via events
    /// </summary>
    /// <typeparam name="TTypeIdentifier">Type of identifier used to distinguish output types.</typeparam>
    /// <typeparam name="TOutputType">Type of the interface or class used to represent outputs.</typeparam>
    public interface IBasicParser<TTypeIdentifier, TOutputType> : IParser
    {
        /// <summary>
        /// Occurs when a data image is deserialized successfully to one of the output types that the data
        /// image represented.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the object that was deserialized from the binary image.
        /// </remarks>
        event EventHandler<EventArgs<TOutputType>> DataParsed;

        /// <summary>
        /// Occurs when matching a output type for deserializing the data image cound not be found.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the ID of the output type that could not be found.
        /// </remarks>
        event EventHandler<EventArgs<TTypeIdentifier>> OutputTypeNotFound;

        /// <summary>
        /// Gets or sets a boolean value that indicates if data types get constructed in an optimized fashion.
        /// </summary>
        bool OptimizeTypeConstruction { get; set; }
    }
}