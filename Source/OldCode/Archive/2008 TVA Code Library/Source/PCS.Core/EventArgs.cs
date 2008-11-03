//*******************************************************************************************************
//  EventArgs.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/24/2006 - Pinal C. Patel
//      Generated original version of source code.
//  09/08/2008 - J. Ritchie Carroll
//      Converted to C#.
//  11/03/2006 - Pinal C. Patel
//      Edited code comments.
//
//*******************************************************************************************************

using System;

namespace PCS
{
    /// <summary>
    /// Represetns a generic event arguments class.
    /// </summary>
    /// <typeparam name="T">Type of argument for this event arguments instance.</typeparam>
    public class EventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets or sets the data for the event using <see cref="EventArgs{T}"/> for its <see cref="EventArgs"/>.
        /// </summary>
        public T Argument;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventArgs{T}"/> class.
        /// </summary>
        public EventArgs()
        {
            Argument = default(T);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventArgs{T}"/> class.
        /// </summary>
        /// <param name="argument">The data for the event.</param>
        public EventArgs(T argument)
        {
            Argument = argument;
        }
    }
}