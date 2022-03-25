//******************************************************************************************************
//  FrameQueue.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/02/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  11/18/2010 - Mehulbhai P Thakkar
//       Fixed issue in the Pop() method by checking if m_frameQueue has any element in it.
//  02/08/2011 - J. Ritchie Carroll
//       Added ExamineQueueState method to analyze real-time queue state.
//  05/10/2011 - J. Ritchie Carroll
//       Updated frame queue locks to use a ReaderWriterSpinLock as an optimization.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GSF.TimeSeries
{
    /// <summary>
    /// Represents a real-time queue of <see cref="TrackingFrame"/> instances used by the <see cref="ConcentratorBase"/> class.
    /// </summary>
    internal class FrameQueue : IDisposable
    {
        #region [ Members ]

        // Delegates
        internal delegate IFrame CreateNewFrameFunction(Ticks timestamp);

        // Fields
        private CreateNewFrameFunction m_createNewFrame;               // IFrame creation function
        private LinkedList<TrackingFrame> m_frameList;                 // We keep this list sorted by timestamp so frames are processed in order
        private ConcurrentDictionary<long, TrackingFrame> m_frameHash; // Fast frame lookup dictionary
        private SpinLock m_queueLock;                                  // Spinning lock used for synchronizing access to frame list
        private long m_publishedTicks;                                 // Timestamp of last published frame
        private volatile TrackingFrame m_head;                         // Reference to current top of the frame collection
        private volatile TrackingFrame m_last;                         // Reference to last published frame
        private int m_framesPerSecond;                                 // Cached frames per second
        private bool m_disposed;                                       // Object disposed flag

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="FrameQueue"/>.
        /// </summary>
        internal FrameQueue(CreateNewFrameFunction createNewFrame)
        {
            m_createNewFrame = createNewFrame;
            m_frameList = new();
            m_frameHash = new();
            m_queueLock = new();
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
        /// Gets or sets the number of frames per second to be used by <see cref="FrameQueue"/>.
        /// </summary>
        /// <remarks>
        /// Valid frame rates are greater than 0 frames per second.
        /// </remarks>
        public int FramesPerSecond
        {
            get => m_framesPerSecond;
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(value), "Frames per second must be greater than 0");

                m_framesPerSecond = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum time resolution to use when sorting measurements by timestamps into their proper destination frame.
        /// </summary>
        public long TimeResolution { get; set; }

        /// <summary>
        /// Gets or sets a value to indicate whether to round to the nearest
        /// frame timestamp rather than rounding down to the nearest timestamps.
        /// </summary>
        public bool RoundToNearestTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="GSF.TimeSeries.DownsamplingMethod"/> to be used by the <see cref="FrameQueue"/>.
        /// </summary>
        public DownsamplingMethod DownsamplingMethod { get; set; } = DownsamplingMethod.LastReceived;

        /// <summary>
        /// Returns the next <see cref="TrackingFrame"/> in the <see cref="FrameQueue"/>, if any.
        /// </summary>
        /// <remarks>
        /// This property is tracked separately from the internal frame collection, as a
        /// result this property may be called at any time without a locking penalty.
        /// </remarks>
        public TrackingFrame Head => m_head; // We track the head separately to avoid lock on frame list to safely access first item...

        /// <summary>
        /// Gets the last processed <see cref="TrackingFrame"/> in the <see cref="FrameQueue"/>.
        /// </summary>
        /// <remarks>
        /// This property is tracked separately from the internal frame collection, as a
        /// result this property may be called at any time without a locking penalty.
        /// </remarks>
        public TrackingFrame Last => m_last;

        /// <summary>
        /// Returns the total number of frames in the <see cref="FrameQueue"/>.
        /// </summary>
        public int Count => m_frameList.Count;

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
            if (m_disposed)
                return;

            try
            {
                if (disposing)
                {
                    Clear();
                    m_frameList = null;
                    m_frameHash = null;

                    m_createNewFrame = null;
                    m_head = null;
                    m_last = null;
                }
            }
            finally
            {
                m_disposed = true; // Prevent duplicate dispose.
            }
        }

        /// <summary>
        /// Examines and returns the status of the <see cref="FrameQueue"/>.
        /// </summary>
        /// <param name="expectedMeasurements">Number of expected measurements per frame.</param>
        public string ExamineQueueState(int expectedMeasurements)
        {
            StringBuilder status = new();
            bool locked = false;

            try
            {
                m_queueLock.Enter(ref locked);

                status.AppendLine("Concentrator frame queue detail:");
                status.AppendLine();
                status.AppendLine($" Ordered frame queue count: {m_frameList.Count}");
                status.AppendLine($"    Frame hash-table count: {m_frameHash.Count}");

                if (m_frameList.Count > 0)
                {
                    LinkedListNode<TrackingFrame> node = m_frameList.First;

                    status.AppendLine();

                    for (int i = 0; i < m_frameList.Count; i++)
                    {
                        if (node is not null)
                        {
                            IFrame frame = node.Value?.SourceFrame;

                            if (frame is null)
                            {
                                status.AppendFormat("Frame {0} @ <null frame>", i.ToString().PadLeft(4, '0'));
                            }
                            else
                            {
                                status.AppendFormat("Frame {0} @ {1:dd-MMM-yyyy HH:mm:ss.fff} - {2} measurements, {3:##0.00%} received",
                                    i.ToString().PadLeft(4, '0'), new DateTime(frame.Timestamp),
                                    frame.Measurements.Count, frame.Measurements.Count / (double)expectedMeasurements);
                            }

                            status.AppendLine();
                            node = node.Next;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                return status.ToString();
            }
            finally
            {
                if (locked)
                    m_queueLock.Exit();
            }
        }

        /// <summary>
        /// Clears the <see cref="FrameQueue"/>.
        /// </summary>
        public void Clear()
        {
            bool locked = false;

            try
            {
                m_queueLock.Enter(ref locked);

                if (m_frameList is not null)
                    m_frameList.Clear();

                if (m_frameHash is not null)
                    m_frameHash.Clear();
            }
            finally
            {
                if (locked)
                    m_queueLock.Exit();
            }
        }

        /// <summary>
        /// Removes current <see cref="Head"/> frame from the <see cref="FrameQueue"/> after it has been processed and assigns a new <see cref="Head"/>.
        /// </summary>
        public void Pop()
        {
            // We track latest published ticks - don't want to allow slow moving measurements
            // to inject themselves after a certain publication time frame has passed - this
            // avoids any possible out-of-sequence frame publication...
            m_last = m_head;
            m_head = null;
            long publishedTicks = m_last.Timestamp;
            Thread.VolatileWrite(ref m_publishedTicks, publishedTicks);

            // Assign next node, if any, as quickly as possible. Still have to wait for queue
            // lock - tick-tock, time's-a-wastin' and user function needs a frame to publish.
            bool locked = false;

            try
            {
                m_queueLock.Enter(ref locked);

                if (m_frameList.Count > 0)
                {
                    LinkedListNode<TrackingFrame> nextNode = m_frameList.First.Next;

                    // If next frame is available, go ahead and assign it...
                    if (nextNode is not null)
                        m_head = nextNode.Value;

                    // Clean up frame queues
                    m_frameList.RemoveFirst();
                }
            }
            finally
            {
                if (locked)
                    m_queueLock.Exit();
            }

            m_frameHash.TryRemove(publishedTicks, out _);
        }

        /// <summary>
        /// Gets <see cref="TrackingFrame"/> from the queue with the specified timestamp, in ticks.  If no frame exists for
        /// the specified timestamp, one will be created.
        /// </summary>
        /// <param name="ticks">Timestamp, in ticks, for which to get or create <see cref="TrackingFrame"/>.</param>
        /// <remarks>
        /// Ticks can be any point in time so long time requested is greater than time of last published frame; this queue is
        /// used in a real-time scenario with time moving forward.  If a frame is requested for an old timestamp, null will
        /// be returned. Note that frame returned will be "best-fit" for given timestamp based on <see cref="FramesPerSecond"/>.
        /// </remarks>
        /// <returns>An existing or new <see cref="TrackingFrame"/> from the queue for the specified timestamp.</returns>
        public TrackingFrame GetFrame(long ticks)
        {
            TrackingFrame frame = null;
            bool nodeAdded = false;

            // Calculate destination ticks for this frame
            long destinationTicks = RoundToNearestTimestamp ? 
                Ticks.RoundToSubsecondDistribution(ticks, m_framesPerSecond) : 
                Ticks.AlignToSubsecondDistribution(ticks, m_framesPerSecond, TimeResolution);

            // Make sure ticks are newer than latest published ticks...
            if (destinationTicks > Thread.VolatileRead(ref m_publishedTicks))
            {
                // See if requested frame is already available (can do this outside lock with concurrent dictionary)
                if (m_frameHash.TryGetValue(destinationTicks, out frame))
                    return frame;

                // Didn't find frame for this timestamp so we need to add a new one to the queue
                bool locked = false;

                try
                {
                    m_queueLock.Enter(ref locked);

                    // Another thread may have gotten to this task already, so check for this contingency...
                    if (m_frameHash.TryGetValue(destinationTicks, out frame))
                        return frame;

                    // TODO: Add a flag to add frames to publish even if no data arrives for them as an alternate mode
                    // TODO: of operation. In this mode pre-populate frame queue with at least one second of data from
                    // TODO: given start time (was thinking up next even second). This mode is useful for generically
                    // TODO: using concentrator to create a set of frame data from a data set with possible missing
                    // TODO: data (e.g., a historian) to create an evenly timestamped export

                    // Create a new frame for this timestamp
                    frame = new(m_createNewFrame(destinationTicks), DownsamplingMethod);

                    if (m_frameList.Count > 0)
                    {
                        // Insert frame into proper sorted position...
                        LinkedListNode<TrackingFrame> node = m_frameList.Last;

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
                        while (node is not null);
                    }

                    if (!nodeAdded)
                    {
                        m_frameList.AddFirst(frame);
                        m_head = frame;
                    }

                    // Since we'll be requesting this frame over and over, we'll use
                    // a hash table for quick frame lookups by timestamp
                    m_frameHash[destinationTicks] = frame;
                }
                finally
                {
                    if (locked)
                        m_queueLock.Exit();
                }
            }

            return frame;
        }

        #region [ GetFrame Testing Algorithm ]

        // Copy this code into a console application and reference GSF.Core.dll to test.

        //using System;
        //using System.Collections.Generic;
        //using System.Linq;
        //using System.Text;
        //using GSF;

        //namespace FrameTimestampTest
        //{
        //    public class Program
        //    {
        //        private static long s_timeResolution;
        //        private static long s_framesPerSecond;
        //        private static double s_ticksPerFrame;
        //        private static double s_millisecondsPerFrame;

        //        public static void Main(string[] args)
        //        {
        //            s_framesPerSecond = 30;
        //            s_ticksPerFrame = Ticks.PerSecond / (double)s_framesPerSecond;
        //            s_timeResolution = Ticks.PerMillisecond;
        //            s_millisecondsPerFrame = 1000.0 / s_framesPerSecond;

        //            // Test BPA PDCstream style milliseconds
        //            DateTime currentTime = DateTime.Now;
        //            int[] bpaMilliseconds = new int[] { 000, 033, 066, 100, 133, 166, 200, 233, 266, 300, 333, 366, 400, 433, 466, 500, 533, 566, 599, 633, 666, 699, 733, 766, 800, 833, 866, 900, 933, 966 };

        //            s_timeResolution = Ticks.PerMillisecond * 33;
        //            Console.WriteLine("BPA millisecond sorting test, TimeResolution = {0}", s_timeResolution);

        //            for (int i = 0; i < bpaMilliseconds.Length; i++)
        //            {
        //                long longTicks = (new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, currentTime.Minute, currentTime.Second, bpaMilliseconds[i])).Ticks;
        //                long destination = GetFrame(longTicks);
        //                Console.WriteLine(string.Format("{0:000} ms : {1:000} ms", bpaMilliseconds[i], (new DateTime(destination)).Millisecond));
        //            }

        //            s_timeResolution = Ticks.PerMillisecond;
        //            Ticks sourceTime = ((Ticks)DateTime.Now).BaselinedTimestamp(BaselineTimeInterval.Second);

        //            for (int i = 0; i < s_framesPerSecond; i++)
        //            {
        //                int milliseconds = (int)(s_millisecondsPerFrame * i);
        //                long longTicks = ((new DateTime(sourceTime)).AddMilliseconds((double)milliseconds)).Ticks;
        //                long destination = GetFrame(longTicks);
        //                Console.WriteLine(string.Format("{0} - {1:000} ms : {2} - {3:000} ms", longTicks, milliseconds, destination, (new DateTime(destination)).Millisecond));
        //            }
        //            Console.WriteLine();

        //            double ticks = Ticks.PerSecond;

        //            // Test truncated timestamps
        //            for (int i = 0; i < s_framesPerSecond; i++)
        //            {
        //                long longTicks = (long)(ticks / s_timeResolution) * s_timeResolution;
        //                Console.WriteLine(string.Format("{0} : {1}", longTicks, GetFrame(longTicks)));
        //                ticks += s_ticksPerFrame;
        //            }
        //            Console.WriteLine();

        //            ticks = 2 * Ticks.PerSecond;

        //            // Test rounded timestamps
        //            for (int i = 0; i < s_framesPerSecond; i++)
        //            {
        //                long longTicks = (long)Math.Round(ticks / s_timeResolution) * s_timeResolution;
        //                Console.WriteLine(string.Format("{0} : {1}", longTicks, GetFrame(longTicks)));
        //                ticks += s_ticksPerFrame;
        //            }
        //            Console.WriteLine();

        //            ticks = 3 * Ticks.PerSecond;

        //            // Test upper range limits
        //            for (int i = 0; i < s_framesPerSecond; i++)
        //            {
        //                ticks += s_ticksPerFrame;
        //                long longTicks = (long)(ticks / s_timeResolution) * s_timeResolution;
        //                longTicks -= s_timeResolution;
        //                Console.WriteLine(string.Format("{0} : {1}", longTicks, GetFrame(longTicks)));
        //            }
        //            Console.ReadLine();
        //        }

        //        public static long GetFrame(long ticks)
        //        {
        //            long baseTicks, ticksBeyondSecond, frameIndex, destinationTicks, nextDestinationTicks;

        //            // Baseline timestamp to the top of the second
        //            baseTicks = ticks - ticks % Ticks.PerSecond;

        //            // Remove the seconds from ticks
        //            ticksBeyondSecond = ticks - baseTicks;

        //            // Calculate a frame index between 0 and s_framesPerSecond-1, corresponding to ticks
        //            // rounded down to the nearest frame
        //            frameIndex = (long)(ticksBeyondSecond / s_ticksPerFrame);

        //            // Calculate the timestamp of the nearest frame rounded up
        //            nextDestinationTicks = (frameIndex + 1) * Ticks.PerSecond / s_framesPerSecond;

        //            // Determine whether the desired frame is the nearest
        //            // frame rounded down or the nearest frame rounded up
        //            if (s_timeResolution <= 1)
        //            {
        //                if (nextDestinationTicks <= ticksBeyondSecond)
        //                    destinationTicks = nextDestinationTicks;
        //                else
        //                    destinationTicks = frameIndex * Ticks.PerSecond / s_framesPerSecond;
        //            }
        //            else
        //            {
        //                // If, after translating nextDestinationTicks to the time resolution, it is less than
        //                // or equal to ticks, nextDestinationTicks corresponds to the desired frame
        //                if ((nextDestinationTicks / s_timeResolution) * s_timeResolution <= ticksBeyondSecond)
        //                    destinationTicks = nextDestinationTicks;
        //                else
        //                    destinationTicks = frameIndex * Ticks.PerSecond / s_framesPerSecond;
        //            }

        //            // Recover the seconds that were removed
        //            destinationTicks += baseTicks;

        //            return destinationTicks;
        //        }
        //    }
        //}

        #endregion

        #endregion
    }
}
