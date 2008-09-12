using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Threading;
using TVA.Collections;
using TVA.DateTime;
//using TVA.DateTime.Common;

//*******************************************************************************************************
//  TVA.Measurements.FrameQueue.vb - Implementation of a queue of IFrame's
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
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
//
//*******************************************************************************************************


namespace TVA
{
    namespace Measurements
    {

        public delegate IFrame CreateNewFrameFunctionSignature(long ticks);

        public class FrameQueue : IDisposable
        {



            private LinkedList<IFrame> m_frameList; // We keep this list sorted by timestamp so frames are processed in order
            private Dictionary<long, IFrame> m_frameHash; // This list not guaranteed to be sorted, but used for fast frame lookup
            private ProcessQueue<long> m_popQueue;
            private AutoResetEvent m_syncSignal;
            private long m_publishedTicks;
            private IFrame m_head;
            private IFrame m_last;
            private decimal m_ticksPerFrame;
            private CreateNewFrameFunctionSignature m_createNewFrameFunction;
            private bool m_disposed = false;

            public FrameQueue(decimal ticksPerFrame, int initialCapacity, CreateNewFrameFunctionSignature createNewFrameFunction)
			{
				
				m_frameList = new LinkedList<IFrame>;
				m_frameHash = new Dictionary<long, IFrame>(initialCapacity);
				m_popQueue = ProcessQueue<long>.CreateRealTimeQueue(new System.EventHandler(PopStub), new System.EventHandler(CanPop));
				m_syncSignal = new AutoResetEvent(false);
				
				m_ticksPerFrame = ticksPerFrame;
				m_createNewFrameFunction = createNewFrameFunction;
				
			}

            ~FrameQueue()
            {

                Dispose(true);

            }

            protected virtual void Dispose(bool disposing)
            {

                if (!m_disposed)
                {
                    if (disposing)
                    {
                        if (m_popQueue != null)
                        {
                            m_popQueue.Dispose();
                        }
                        m_popQueue = null;
                        if (m_syncSignal != null)
                        {
                            m_syncSignal.Close();
                        }
                        m_syncSignal = null;
                        if (m_frameList != null)
                        {
                            m_frameList.Clear();
                        }
                        m_frameList = null;
                        if (m_frameHash != null)
                        {
                            m_frameHash.Clear();
                        }
                        m_frameHash = null;
                        m_createNewFrameFunction = null;
                    }
                }

                m_disposed = true;

            }

            public void Dispose()
            {

                // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
                Dispose(true);
                GC.SuppressFinalize(this);

            }

            public void Start()
            {

                m_popQueue.Start();

            }

            public void @Stop()
            {

                m_popQueue.Stop();

            }

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

            public void Pop()
            {

                // We track latest published ticks - don't want to allow slow moving measurements
                // to inject themselves after a certain publication timeframe has passed - this
                // avoids any possible out-of-sequence frame publication...
                m_last = m_head;
                m_head = null;
                m_publishedTicks = m_last.Ticks;

                // Frame's already been handled so there's no rush in removing it
                m_popQueue.Add(m_publishedTicks);

            }

            private bool CanPop(long publishedTicks)
            {

                // We didn't try for an immediate lock to remove top frame from original
                // "Pop" call - this call was running on the already burdened publication
                // thread and we need to give consumer as much of the available timeslice
                // as possible. So now we're running on an independent thread and we'll
                // hang around until we can get our work done. This process "smooths" the
                // frame publication process by not waiting on lock synchrnonization
                // for queue clean up - the separate thread should also avoid a potential
                // deadlock that could be caused by waiting for locks between the frame
                // queue and frame measurements, i.e., sorting vs. publication contention

                // Attempt a lock, if we get it handle pop - otherwise return False and
                // process queue will retry...
                if (Monitor.TryEnter(m_frameList))
                {
                    try
                    {
                        m_frameList.RemoveFirst();

                        if (m_frameList.Count > 0)
                        {
                            this.Head = m_frameList.First.Value;
                        }
                        else
                        {
                            this.Head = null;
                        }

                        m_frameHash.Remove(publishedTicks);
                    }
                    finally
                    {
                        Monitor.Exit(m_frameList);
                    }

                    return true;
                }

                return false;

            }

            private void PopStub(long publishedTicks)
            {

                // CanPop does the real work - nothing for us to do now... :)

            }

            public IFrame Head
            {
                get
                {
                    // Wait until new head has been assigned...
                    m_syncSignal.WaitOne();

                    // We track the head separately to avoid sync-lock on frame list to safely access first item...
                    return m_head;
                }
                private set
                {
                    // New head assigned
                    m_head = value;

                    // Release waiting thread...
                    m_syncSignal.Set();
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

            public IFrame GetFrame(long ticks)
            {

                // Calculate destination ticks for this frame
                long destinationTicks = ((int)(ticks / m_ticksPerFrame)) * m_ticksPerFrame;
                IFrame frame;
                bool nodeAdded;

                // Make sure ticks are newer than latest published ticks...
                if (destinationTicks > m_publishedTicks)
                {
                    // Wait for queue lock - we wait because calling function demands a destination frame
                    lock (m_frameList)
                    {
                        if (!m_frameHash.TryGetValue(destinationTicks, frame))
                        {
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
                                } while (!(node == null));
                            }

                            if (!nodeAdded)
                            {
                                m_frameList.AddFirst(frame);
                                this.Head = frame;
                            }

                            // Since we'll be requesting this frame over and over, we'll use
                            // a hash table for quick frame lookups by timestamp
                            m_frameHash.Add(destinationTicks, frame);
                        }
                    }
                }

                return frame;

            }

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

        }

    }
}
