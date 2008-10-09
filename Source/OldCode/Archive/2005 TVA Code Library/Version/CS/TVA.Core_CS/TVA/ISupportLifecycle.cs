//*******************************************************************************************************
//  ISupportLifecycle.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  10/09/2008 - Pinal C Patel
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA
{
    /// <summary>
    /// Specifies that this object provides supporting methods throughout its lifecycle from birth 
    /// (<see cref="Initialize()"/>) to death (<see cref="IDisposable.Dispose()"/>).
    /// </summary>
    public interface ISupportLifecycle : IDisposable
    {
        /// <summary>
        /// Initializes the state of the object.
        /// </summary>
        /// <remarks>
        /// Typical implementation of this method should allow the object state to be initialized only once.
        /// </remarks>
        void Initialize();

        /// <summary>
        /// Gets or set a boolean value that indicates whether the object is enabled.
        /// </summary>
        /// <remarks>
        /// Typical implementation of this property should suspend the internal processing when the object is 
        /// disabled and resume processing when the object is enabled.
        /// </remarks>
        bool Enabled { get; set; }
    }
}
