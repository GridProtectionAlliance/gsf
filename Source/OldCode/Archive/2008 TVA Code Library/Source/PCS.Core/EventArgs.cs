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
//  11/03/2008 - Pinal C. Patel
//      Edited code comments.
//  11/03/2008 - J. Ritchie Carroll
//      Added type overloads for 2, 3 and 4 argument generic EventArg classes.
//
//*******************************************************************************************************

using System;

namespace PCS
{
    /// <summary>
    /// Represents a generic event arguments class with one data argument.
    /// </summary>
    /// <typeparam name="T">Type of data argument for this event arguments instance.</typeparam>
    public class EventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets or sets the data argument for the event using <see cref="EventArgs{T}"/> for its <see cref="EventArgs"/>.
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
        /// <param name="argument">The argument data for the event.</param>
        public EventArgs(T argument)
        {
            Argument = argument;
        }
    }

    /// <summary>
    /// Represents a generic event arguments class with two data arguments.
    /// </summary>
    /// <typeparam name="T1">The type of the first data argument for this event arguments instance.</typeparam>
    /// <typeparam name="T2">The type of the second data argument for this event arguments instance.</typeparam>
    public class EventArgs<T1, T2> : EventArgs
    {
        /// <summary>
        /// Gets or sets the first data argument for the event using <see cref="EventArgs{T1,T2}"/> for its <see cref="EventArgs"/>.
        /// </summary>
        public T1 Argument1;

        /// <summary>
        /// Gets or sets the second data argument for the event using <see cref="EventArgs{T1,T2}"/> for its <see cref="EventArgs"/>.
        /// </summary>
        public T2 Argument2;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventArgs{T1,T2}"/> class.
        /// </summary>
        public EventArgs()
        {
            Argument1 = default(T1);
            Argument2 = default(T2);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventArgs{T1,T2}"/> class.
        /// </summary>
        /// <param name="argument1">The first data argument for the event.</param>
        /// <param name="argument2">The second data argument for the event.</param>
        public EventArgs(T1 argument1, T2 argument2)
        {
            Argument1 = argument1;
            Argument2 = argument2;
        }
    }

    /// <summary>
    /// Represents a generic event arguments class with three data arguments.
    /// </summary>
    /// <typeparam name="T1">The type of the first data argument for this event arguments instance.</typeparam>
    /// <typeparam name="T2">The type of the second data argument for this event arguments instance.</typeparam>
    /// <typeparam name="T3">The type of the third data argument for this event arguments instance.</typeparam>
    public class EventArgs<T1, T2, T3> : EventArgs
    {
        /// <summary>
        /// Gets or sets the first data argument for the event using <see cref="EventArgs{T1,T2,T3}"/> for its <see cref="EventArgs"/>.
        /// </summary>
        public T1 Argument1;

        /// <summary>
        /// Gets or sets the second data argument for the event using <see cref="EventArgs{T1,T2,T3}"/> for its <see cref="EventArgs"/>.
        /// </summary>
        public T2 Argument2;

        /// <summary>
        /// Gets or sets the third data argument for the event using <see cref="EventArgs{T1,T2,T3}"/> for its <see cref="EventArgs"/>.
        /// </summary>
        public T3 Argument3;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventArgs{T1,T2,T3}"/> class.
        /// </summary>
        public EventArgs()
        {
            Argument1 = default(T1);
            Argument2 = default(T2);
            Argument3 = default(T3);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventArgs{T1,T2,T3}"/> class.
        /// </summary>
        /// <param name="argument1">The first data argument for the event.</param>
        /// <param name="argument2">The second data argument for the event.</param>
        /// <param name="argument3">The third data argument for the event.</param>
        public EventArgs(T1 argument1, T2 argument2, T3 argument3)
        {
            Argument1 = argument1;
            Argument2 = argument2;
            Argument3 = argument3;
        }
    }

    /// <summary>
    /// Represents a generic event arguments class with three data arguments.
    /// </summary>
    /// <typeparam name="T1">The type of the first data argument for this event arguments instance.</typeparam>
    /// <typeparam name="T2">The type of the second data argument for this event arguments instance.</typeparam>
    /// <typeparam name="T3">The type of the third data argument for this event arguments instance.</typeparam>
    /// <typeparam name="T4">The type of the fourth data argument for this event arguments instance.</typeparam>
    public class EventArgs<T1, T2, T3, T4> : EventArgs
    {
        /// <summary>
        /// Gets or sets the first data argument for the event using <see cref="EventArgs{T1,T2,T3,T4}"/> for its <see cref="EventArgs"/>.
        /// </summary>
        public T1 Argument1;

        /// <summary>
        /// Gets or sets the second data argument for the event using <see cref="EventArgs{T1,T2,T3,T4}"/> for its <see cref="EventArgs"/>.
        /// </summary>
        public T2 Argument2;

        /// <summary>
        /// Gets or sets the third data argument for the event using <see cref="EventArgs{T1,T2,T3,T4}"/> for its <see cref="EventArgs"/>.
        /// </summary>
        public T3 Argument3;

        /// <summary>
        /// Gets or sets the fourth data argument for the event using <see cref="EventArgs{T1,T2,T3,T4}"/> for its <see cref="EventArgs"/>.
        /// </summary>
        public T4 Argument4;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventArgs{T1,T2,T3,T4}"/> class.
        /// </summary>
        public EventArgs()
        {
            Argument1 = default(T1);
            Argument2 = default(T2);
            Argument3 = default(T3);
            Argument4 = default(T4);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventArgs{T1,T2,T3}"/> class.
        /// </summary>
        /// <param name="argument1">The first data argument for the event.</param>
        /// <param name="argument2">The second data argument for the event.</param>
        /// <param name="argument3">The third data argument for the event.</param>
        /// <param name="argument4">The fourth data argument for the event.</param>
        public EventArgs(T1 argument1, T2 argument2, T3 argument3, T4 argument4)
        {
            Argument1 = argument1;
            Argument2 = argument2;
            Argument3 = argument3;
            Argument4 = argument4;
        }
    }
}