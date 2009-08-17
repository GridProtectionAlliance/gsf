//*******************************************************************************************************
//  IAdapter.cs
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
using System.Data;
using TVA;

namespace TVA.Measurements.Routing
{
    /// <summary>
    /// Represents the abstract interface for any adapter.
    /// </summary>
    [CLSCompliant(false)]
    public interface IAdapter : ISupportLifecycle, IProvideStatus
	{
        /// <summary>
        /// Provides status messages to consumer.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is new status message.
        /// </remarks>
        event EventHandler<EventArgs<string>> StatusMessage;

        /// <summary>
        /// Event is raised when there is an exception encountered while processing.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Implementations of this interface are expected to capture any exceptions that might be thrown by
        /// user code in any processing to prevent third-party code from causing an unhandled exception
        /// in the host.  Errors are reported via this event so host administrators will be aware of the exception.
        /// Any needed connection cycle to data adapter should be restarted when an exception is encountered.
        /// </para>
        /// <para>
        /// <see cref="EventArgs{T}.Argument"/> is the exception that was thrown.
        /// </para>
        /// </remarks>
        event EventHandler<EventArgs<Exception>> ProcessException;

        /// <summary>
        /// Gets or sets <see cref="DataSet"/> based data source available to <see cref="IAdapter"/>.
        /// </summary>
        DataSet DataSource { get; set; }

        /// <summary>
        /// Gets or sets key/value pair connection information specific to <see cref="IAdapter"/>.
        /// </summary>
        string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets name of this <see cref="IAdapter"/>.
        /// </summary>
        new string Name { get; set; }

        /// <summary>
        /// Gets or sets the numeric ID associated with this <see cref="IAdapter"/>.
        /// </summary>
        uint ID { get; set; }

        /// <summary>
        /// Gets or sets flag indicating if the adapter has been initialized successfully.
        /// </summary>
        /// <remarks>
        /// Implementors only need to track this value.
        /// </remarks>
        bool Initialized { get; set; }

        /// <summary>
        ///  Starts the adapter, if it is not already running.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the adapter.
        /// </summary>
        void Stop();

        /// <summary>
        /// Gets a short one-line adapter status.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current adapter status.</returns>
        string GetShortStatus(int maxLength);
    }
}