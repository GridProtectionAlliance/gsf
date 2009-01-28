//*******************************************************************************************************
//  FrameQueue.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/01/2007 - J. Ritchie Carroll
//       Initial version of source generated
//  09/15/2008 - J. Ritchie Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;

namespace PCS.Measurements
{
    /// <summary>
    /// Represents a real-time queue of <see cref="IFrame"/> instances used by the <see cref="ConcentratorBase"/> class.
    /// </summary>
    public class FrameQueue : IDisposable
    {            
        #region [ Members ]

        // Fields
        private ConcentratorBase m_parent;              // Reference to parent concentrator instance
        private LinkedList<IFrame> m_frameList;         // We keep this list sorted by timestamp so frames are processed in order
        private Dictionary<Ticks, IFrame> m_frameHash;  // This list not guaranteed to be sorted, but used for fast frame lookup
        private Ticks m_publishedTicks;                 // Timstamp of last published frame
        private IFrame m_head;                          // Reference to current top of the frame collection
        private IFrame m_last;                          // Reference to last published frame
        private decimal m_ticksPerFrame;                // Cached ticks per frame
        private bool m_disposed;                        // Object disposed flag

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="FrameQueue"/>.
        /// </summary>
        /// <param name="parent">Reference to parent concentrator instance.</param>
        internal FrameQueue(ConcentratorBase parent)
        {
            // Calculate initial dictionary capacity based on concentrator specifications
            int initialCapacity = (int)((1.0D + parent.LagTime + parent.LeadTime) * parent.FramesPerSecond);

            m_parent = parent;
            m_frameList = new LinkedList<IFrame>();
            m_frameHash = new Dictionary<Ticks, IFrame>(initialCapacity);
            m_ticksPerFrame = parent.TicksPerFrame;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="FrameQueue"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~FrameQueue()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets number of ticks per frame to be used by <see cref="FrameQueue"/>.
        /// </summary>
        public decimal TicksPerFrame
        {
            get
            {
                return m_ticksPerFrame;
            }
            set
            {
                m_ticksPerFrame = value;
            }
        }
        /// <summary>
        /// Returns the next <see cref="IFrame"/> in the <see cref="FrameQueue"/>, if any.
        /// </summary>
        /// <remarks>
        /// This property is tracked separately from the internal <see cref="IFrame"/> collection, as a
        /// result this property may be called at any time without a locking penalty.
        /// </remarks>
        public IFrame Head
        {
            get
            {
                // We track the head separately to avoid sync-lock on frame list to safely access first item...
                return m_head;
            }
        }

        /// <summary>
        /// Gets the last processed <see cref="IFrame"/> in the <see cref="FrameQueue"/>.
        /// </summary>
        /// <remarks>
        /// This property is tracked separately from the internal <see cref="IFrame"/> collection, as a
        /// result this property may be called at any time without a locking penalty.
        /// </remarks>
        public IFrame Last
        {
            get
            {
                return m_last;
            }
        }

        /// <summary>
        /// Returns the total number of <see cref="IFrame"/>'s currently in the <see cref="FrameQueue"/>.
        /// </summary>
        public int Count
        {
            get
            {
                return m_frameList.Count;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="FrameQueue"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="FrameQueue"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if (m_frameList != null)
                            m_frameList.Clear();

                        m_frameList = null;

                        if (m_frameHash != null)
                            m_frameHash.Clear();

                        m_frameHash = null;

                        m_parent = null;
                        m_head = null;
                        m_last = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Clears the <see cref="FrameQueue"/>.
        /// </summary>
        public void Clear()
        {
            lock (m_frameList)
            {
                if (m_frameList != null)
                    m_frameList.Clear();

                if (m_frameHash != null)
                    m_frameHash.Clear();
            }
        }

        /// <summary>
        /// Removes current <see cref="Head"/> frame from the <see cref="FrameQueue"/> after it has been processed and assigns a new <see cref="Head"/>.
        /// </summary>
        public void Pop()
        {
            // We track latest published ticks - don't want to allow slow moving measurements
            // to inject themselves after a certain publication timeframe has passed - this
            // avoids any possible out-of-sequence frame publication...
            m_last = m_head;
            m_head = null;
            m_publishedTicks = m_last.Timestamp;

            // Assign next node, if any, as quickly as possible. Still have to wait for queue
            // lock - tick-tock, time's-a-wastin' and user function needs a frame to publish.
            lock (m_frameList)
            {
                LinkedListNode<IFrame> nextNode = m_frameList.First.Next;

                // If next frame is available, go ahead and assign it...
                if (nextNode != null)
                    m_head = nextNode.Value;

                // Clean up frame queues
                m_frameList.RemoveFirst();
                m_frameHash.Remove(m_publishedTicks);
            }
        }

        /// <summary>
        /// Gets <see cref="IFrame"/> from the queue with the specified timestamp, in ticks.  If no <see cref="IFrame"/> exists for
        /// the specified timestamp, one will be created.
        /// </summary>
        /// <param name="timestamp">Timestamp, in ticks, for which to get or create <see cref="IFrame"/>.</param>
        /// <remarks>
        /// Ticks can be any point in time so long time requested is greater than time of last published frame; this queue
        /// is used in a real-time scenario with time moving forward.  If a frame is requested for an old timestamp, null
        /// will be returned. Note that frame returned will be "best-fit" for given timestamp based on the number of 
        /// <see cref="ConcentratorBase.FramesPerSecond"/> of the parent <see cref="ConcentratorBase"/> implementation.
        /// </remarks>
        /// <returns>An existing or new <see cref="IFrame"/> from the queue for the specified timestamp.</returns>
        public IFrame GetFrame(Ticks timestamp)
        {
            // Calculate destination ticks for this frame
            Ticks destinationTicks = (long)((long)(timestamp / m_ticksPerFrame) * m_ticksPerFrame);
            IFrame frame = null;
            bool nodeAdded = false;

            // Make sure ticks are newer than latest published ticks...
            if (destinationTicks > m_publishedTicks)
            {
                // Wait for queue lock - we wait because calling function demands a destination frame
                lock (m_frameList)
                {
                    // See if requested frame is already available...
                    if (m_frameHash.TryGetValue(destinationTicks, out frame))
                        return frame;

                    // Didn't find frame for this timestamp so we create one
                    frame = m_parent.CreateNewFrame(destinationTicks);

                    if (m_frameList.Count > 0)
                    {
                        // Insert frame into proper sorted position...
                        LinkedListNode<IFrame> node = m_frameList.Last;

                        do
                        {
                            if (destinationTicks > node.Value.Timestamp)
                            {
                                m_frameList.AddAfter(node, frame);
                                nodeAdded = true;
                                break;
                            }

                            node = node.Previous;
                        }
                        while (node != null);
                    }

                    if (!nodeAdded)
                    {
                        m_frameList.AddFirst(frame);
                        m_head = frame;
                    }

                    // Since we'll be requesting this frame over and over, we'll use
                    // a hash table for quick frame lookups by timestamp
                    m_frameHash.Add(destinationTicks, frame);
                }
            }

            return frame;
        }

        #region [ Possible Future Optimization ]

        //Private Sub LoadFramesProc()

        //    'Dim framesPerSecond As Integer = CInt(CDec(TicksPerSecond) / m_ticksPerFrame)
        //    Dim x, destinationTicks As Long
        //    Dim frame As IFrame
        //    Dim frameIndex As Integer

        //    Do While True
        //        ' Attempt a lock, no need to wait...
        //        If Monitor.TryEnter(m_frames) Then
        //            Try
        //                ' We have a lock now, so let's check to see if we need to add some frames,
        //                ' we'll try to keep a full second's worth of future frames out there
        //                frame = Nothing

        //                For x = m_currentTicks To m_currentTicks + TicksPerSecond Step m_ticksPerFrame
        //                    destinationTicks = CLng(x / m_ticksPerFrame) * m_ticksPerFrame
        //                    frameIndex = m_frames.BinarySearch(New Frame(destinationTicks))

        //                    If frameIndex < 0 Then
        //                        ' Didn't find frame for this timestamp so we create one
        //                        frame = m_createNewFrameFunction(destinationTicks)
        //                        m_frames.Add(frame)
        //                    End If
        //                Next

        //                If frame IsNot Nothing Then
        //                    If m_tail Is Nothing OrElse frame.CompareTo(m_tail) > 0 Then
        //                        m_tail = frame
        //                    Else
        //                        m_frames.Sort()
        //                    End If

        //                    If m_head Is Nothing AndAlso m_frames.Count = 1 Then m_head = m_tail
        //                End If
        //            Finally
        //                Monitor.Exit(m_frames)
        //            End Try
        //        End If

        //        ' This is a lazy process, we are always snoozing and trying again...
        //        Thread.Sleep(10)
        //    Loop

        //End Sub

        #endregion

        #endregion
    }
}

#region [ Old Code ]

///// <summary>
///// Gets or sets the create frame function delegate used by <see cref="FrameQueue"/>.
///// </summary>
///// <remarks>
///// Function signature: <see cref="IFrame"/> CreateFrame(long ticks).
///// </remarks>
//public Func<long, IFrame> CreateNewFrameFunction
//{
//    get
//    {
//        return m_createNewFrameFunction;
//    }
//    set
//    {
//        m_createNewFrameFunction = value;
//    }
//}

#endregion