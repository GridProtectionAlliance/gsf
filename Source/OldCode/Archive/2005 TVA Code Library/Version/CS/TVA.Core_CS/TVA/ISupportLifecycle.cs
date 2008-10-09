using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVA
{
    /// <summary>
    /// Specifies that this object provides supporting methods throughout its lifecycle from birth 
    /// (<see cref="Initialize()"/>) to death (<see cref="IDisposable.Dispose()"/>).
    /// </summary>
    interface ISupportLifecycle : IDisposable
    {
        /// <summary>
        /// Initializes the state of the object.
        /// </summary>
        /// <remarks>Typically this method should be implemented to allow the object state to be initialized only once.</remarks>
        void Initialize();

        /// <summary>
        /// Gets or set a boolean value that indicates whether the object is enabled.
        /// </summary>
        bool Enabled { get; set; }
    }
}
