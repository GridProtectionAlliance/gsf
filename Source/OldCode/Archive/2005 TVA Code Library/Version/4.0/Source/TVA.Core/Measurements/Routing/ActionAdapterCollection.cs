//*******************************************************************************************************
//  ActionAdapterCollection.cs
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
//  05/07/2009 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using TVA;
using TVA.Measurements;

namespace TVA.Measurements.Routing
{
    /// <summary>
    /// Represents a collection of <see cref="IActionAdapter"/> implementations.
    /// </summary>
    [CLSCompliant(false)]
    public class ActionAdapterCollection : AdapterCollectionBase<IActionAdapter>, IActionAdapter
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// This event will be raised when there are new measurements available from the action adapter.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is collection of new measurements for host to process.
        /// </remarks>
        public event EventHandler<EventArgs<ICollection<IMeasurement>>> NewMeasurements;

        /// <summary>
        /// This event is raised every second allowing consumer to track current number of unpublished seconds of data in the queue.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the total number of unpublished seconds of data.
        /// </remarks>
        public event EventHandler<EventArgs<int>> UnpublishedSamples;

        // Fields
        private IMeasurement[] m_outputMeasurements;
        private MeasurementKey[] m_inputMeasurementKeys;
        private int m_minimumMeasurementsToUse;
        private int m_framesPerSecond;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new instance of the <see cref="ActionAdapterCollection"/>.
        /// </summary>
        public ActionAdapterCollection()
        {
            base.Name = "Action Adapter Collection";
            base.DataMember = "ActionAdapters";
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets output measurements that the action adapter will produce, if any.
        /// </summary>
        public virtual IMeasurement[] OutputMeasurements
        {
            get
            {
                return m_outputMeasurements;
            }
            set
            {
                m_outputMeasurements = value;
            }
        }

        /// <summary>
        /// Gets or sets primary keys of input measurements the action adapter expects.
        /// </summary>
        public virtual MeasurementKey[] InputMeasurementKeys
        {
            get
            {
                return m_inputMeasurementKeys;
            }
            set
            {
                m_inputMeasurementKeys = value;
            }
        }

        /// <summary>
        /// Gets or sets minimum number of input measurements required for calculation.  Set to -1 to require all.
        /// </summary>
        public virtual int MinimumMeasurementsToUse
        {
            get
            {
                // Default to all measurements if minimum is not specified
                if (m_minimumMeasurementsToUse < 1)
                    return m_inputMeasurementKeys.Length;

                return m_minimumMeasurementsToUse;
            }
            set
            {
                m_minimumMeasurementsToUse = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of frames per second.
        /// </summary>
        public int FramesPerSecond
        {
            get
            {
                return m_framesPerSecond;
            }
            set
            {
                m_framesPerSecond = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Queues a collection of measurements for processing to each <see cref="IActionAdapter"/> implementation in
        /// this <see cref="ActionAdapterCollection"/>.
        /// </summary>
        /// <param name="measurements">Measurements to queue for processing.</param>
        public virtual void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            foreach (IActionAdapter item in this)
            {
                item.QueueMeasurementsForProcessing(measurements);
            }
        }

        /// <summary>
        /// Raises the <see cref="NewMeasurements"/> event.
        /// </summary>
        /// <param name="measurements">New measurements.</param>
        protected virtual void OnNewMeasurements(ICollection<IMeasurement> measurements)
        {
            if (NewMeasurements != null)
                NewMeasurements(this, new EventArgs<ICollection<IMeasurement>>(measurements));
        }

        /// <summary>
        /// Raises the <see cref="UnpublishedSamples"/> event.
        /// </summary>
        /// <param name="unpublishedSamples">Total number of unpublished seconds of data in the queue.</param>
        protected virtual void OnUnpublishedSamples(int unpublishedSamples)
        {
            if (UnpublishedSamples != null)
                UnpublishedSamples(this, new EventArgs<int>(unpublishedSamples));
        }

        /// <summary>
        /// Wires events and initializes new <see cref="IActionAdapter"/> implementation.
        /// </summary>
        /// <param name="item">New <see cref="IActionAdapter"/> implementation.</param>
        protected override void InitializeItem(IActionAdapter item)
        {
            if (item != null)
            {
                // Wire up events
                item.NewMeasurements += NewMeasurements;
                item.UnpublishedSamples += UnpublishedSamples;
                base.InitializeItem(item);
            }
        }

        /// <summary>
        /// Unwires events and disposes of <see cref="IActionAdapter"/> implementation.
        /// </summary>
        /// <param name="item"><see cref="IActionAdapter"/> to dispose.</param>
        protected override void DisposeItem(IActionAdapter item)
        {
            if (item != null)
            {
                // Un-wire events
                item.NewMeasurements -= NewMeasurements;
                item.UnpublishedSamples -= UnpublishedSamples;
                base.DisposeItem(item);
            }
        }

        #endregion
    }
}