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
//  11/08/2007 - J. Ritchie Carroll
//       Optimized "Pop" call to be a no-wait operation
//  02/19/2008 - J. Ritchie Carroll
//       Added code to detect and avoid redundant calls to Dispose().
//  08/20/2008 - J. Ritchie Carroll
//       Removed process queue for thread reduction optimization
//  09/15/2008 - J. Ritchie Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;

namespace TVA.Measurements
{
    public delegate IFrame CreateNewFrameFunctionSignature(long ticks);

    public class FrameQueue : IDisposable
    {            
        #region [ Members ]

        // Fields
        private LinkedList<IFrame> m_frameList;         // We keep this list sorted by timestamp so frames are processed in order
        private Dictionary<long, IFrame> m_frameHash;   // This list not guaranteed to be sorted, but used for fast frame lookup
        private long m_publishedTicks;
        private IFrame m_head;
        private IFrame m_last;
        private decimal m_ticksPerFrame;
        private CreateNewFrameFunctionSignature m_createNewFrameFunction;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        internal FrameQueue(decimal ticksPerFrame, int initialCapacity, CreateNewFrameFunctionSignature createNewFrameFunction)
        {
            m_frameList = new LinkedList<IFrame>();
            m_frameHash = new Dictionary<long, IFrame>(initialCapacity);

            m_ticksPerFrame = ticksPerFrame;
            m_createNewFrameFunction = createNewFrameFunction;
        }

        ~FrameQueue()
        {
            Dispose(true);
        }

        #endregion

        #region [ Properties ]

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

        public CreateNewFrameFunctionSignature CreateNewFrameFunction
        {
            get
            {
                return m_createNewFrameFunction;
            }
        }

        /// <summary>
        /// Returns the next frame in queue, if any
        /// </summary>
        public IFrame Head
        {
            get
            {
                // We track the head separately to avoid sync-lock on frame list to safely access first item...
                return m_head;
            }
        }

        public IFrame Last
        {
            get
            {
                return m_last;
            }
        }

        public int Count
        {
            get
            {
                return m_frameList.Count;
            }
        }

        #endregion

        #region [ Methods ]

        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    if (m_frameList != null)
                        m_frameList.Clear();

                    m_frameList = null;

                    if (m_frameHash != null)
                        m_frameHash.Clear();

                    m_frameHash = null;

                    m_createNewFrameFunction = null;
                }
            }

            m_disposed = true;
        }

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

        public void Pop()
        {
            // We track latest published ticks - don't want to allow slow moving measurements
            // to inject themselves after a certain publication timeframe has passed - this
            // avoids any possible out-of-sequence frame publication...
            m_last = m_head;
            m_head = null;
            m_publishedTicks = m_last.Ticks;

            // Assign next node, if any, as quickly as possible. We wait for queue lock because
            // times-a-wastin and user function needs a frame to publish.
            lock (m_frameList)
            {
                LinkedListNode<IFrame> nextNode = m_frameList.First.Next;

                // Next frame available, go ahead and assign it...
                if (nextNode != null)
                    m_head = nextNode.Value;

                // Clean up frame queues
                m_frameList.RemoveFirst();
                m_frameHash.Remove(m_publishedTicks);
            }
        }

        public IFrame GetFrame(long ticks)
        {
            // Calculate destination ticks for this frame
            long destinationTicks = (long)((long)(ticks / m_ticksPerFrame) * m_ticksPerFrame);
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
                    frame = m_createNewFrameFunction(destinationTicks);

                    if (m_frameList.Count > 0)
                    {
                        // Insert frame into proper sorted position...
                        LinkedListNode<IFrame> node = m_frameList.Last;

                        do
                        {
                            if (destinationTicks > node.Value.Ticks)
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