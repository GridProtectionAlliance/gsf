//*******************************************************************************************************
//  IListeningAdapter.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  06/01/2006 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;

namespace TVA.Measurements.Routing
{
    /// <summary>
    /// Represents the abstract interface for any incoming adapter.
    /// </summary>
    [CLSCompliant(false)]
    public interface IInputAdapter : IAdapter
	{
        /// <summary>
        /// Provides new measurements from input adapter.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is a collection of new measurements for host to process.
        /// </remarks>
        event EventHandler<EventArgs<ICollection<IMeasurement>>> NewMeasurements;

        /// <summary>
        /// Gets the total number of measurements received thus far by the <see cref="IInputAdapter"/>.
        /// </summary>
        long ReceivedMeasurements { get; }
	}	
}
