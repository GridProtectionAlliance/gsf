//******************************************************************************************************
//  IsolatedQueueTest.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  1/4/2013 - Steven E. Chisholm
//       Generated original version of source code. 
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using GSF.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSF.Core.Tests.GSF.Collections
{
    [TestClass]
    public class IsolatedQueueTest
    {
        private IsolatedQueue<int> m_collection;
        private const int cnt = 1000000;
        private ManualResetEvent m_wait;

        [TestMethod]
        public void BenchmarkWrite()
        {

            string value = null;
            int x = 0;
            const int cnt = 10000;
            const int loop = 10000;

            List<string> items = new List<string>(cnt);
            for (x = 0; x < cnt; x++)
            {
                items.Add(x.ToString());
            }

            var queue = new IsolatedQueue<string>();

            Stopwatch swDequeue = new Stopwatch();
            Stopwatch swEnqueue = new Stopwatch();

            for (int repeat = 0; repeat < loop; repeat++)
            {
                swEnqueue.Start();
                foreach (var item in items)
                    queue.Enqueue(item);
                swEnqueue.Stop();

                swDequeue.Start();
                while (queue.TryDequeue(out value))
                    ;
                swDequeue.Stop();
            }


            swEnqueue.Reset();
            swDequeue.Reset();

            for (int repeat = 0; repeat < loop; repeat++)
            {
                swEnqueue.Start();
                foreach (var item in items)
                    queue.Enqueue(item);
                swEnqueue.Stop();

                swDequeue.Start();
                while (queue.TryDequeue(out value))
                    ;
                swDequeue.Stop();
            }


            System.Console.WriteLine("Enqueue " + (cnt * loop / swEnqueue.Elapsed.TotalSeconds / 1000000));
            System.Console.WriteLine("Dequeue " + (cnt * loop / swDequeue.Elapsed.TotalSeconds / 1000000));
            System.Console.WriteLine(swDequeue.Elapsed.TotalSeconds);
        }

        [TestMethod]
        public void BenchmarkBulkWrite()
        {

            //string value = null;
            //int x = 0;
            const int cnt = 10000;
            const int loop = 10000;

            byte[] items = new byte[cnt];
            byte[] itemsRead = new byte[cnt];

            var queue = new IsolatedQueue<byte>();

            Stopwatch swDequeue = new Stopwatch();
            Stopwatch swEnqueue = new Stopwatch();

            for (int repeat = 0; repeat < loop; repeat++)
            {
                swEnqueue.Start();
                queue.Enqueue(items, 0, items.Length);
                swEnqueue.Stop();

                swDequeue.Start();
                while (queue.Dequeue(itemsRead, 0, items.Length) > 0)
                    ;
                swDequeue.Stop();
            }


            swEnqueue.Reset();
            swDequeue.Reset();

            for (int repeat = 0; repeat < loop; repeat++)
            {
                swEnqueue.Start();
                queue.Enqueue(items, 0, items.Length);
                swEnqueue.Stop();

                swDequeue.Start();
                while (queue.Dequeue(itemsRead, 0, items.Length) > 0)
                    ;
                swDequeue.Stop();
            }


            System.Console.WriteLine("Enqueue " + (cnt * loop / swEnqueue.Elapsed.TotalSeconds / 1000000));
            System.Console.WriteLine("Dequeue " + (cnt * loop / swDequeue.Elapsed.TotalSeconds / 1000000));
            System.Console.WriteLine(swDequeue.Elapsed.TotalSeconds);
        }

        [TestMethod]
        public void BenchmarkConcurrentQueue()
        {

            string value = null;
            int x = 0;
            const int cnt = 10000;
            const int loop = 5000;

            List<string> items = new List<string>(cnt);
            for (x = 0; x < cnt; x++)
            {
                items.Add(x.ToString());
            }

            var queue = new ConcurrentQueue<string>();

            Stopwatch swDequeue = new Stopwatch();
            Stopwatch swEnqueue = new Stopwatch();

            for (int repeat = 0; repeat < loop; repeat++)
            {
                swEnqueue.Start();
                foreach (var item in items)
                    queue.Enqueue(item);
                swEnqueue.Stop();

                swDequeue.Start();
                while (queue.TryDequeue(out value))
                    ;
                swDequeue.Stop();
            }


            swEnqueue.Reset();
            swDequeue.Reset();

            for (int repeat = 0; repeat < loop; repeat++)
            {
                swEnqueue.Start();
                foreach (var item in items)
                    queue.Enqueue(item);
                swEnqueue.Stop();

                swDequeue.Start();
                while (queue.TryDequeue(out value))
                    ;
                swDequeue.Stop();
            }


            System.Console.WriteLine("Enqueue " + (cnt * loop / swEnqueue.Elapsed.TotalSeconds / 1000000));
            System.Console.WriteLine("Dequeue " + (cnt * loop / swDequeue.Elapsed.TotalSeconds / 1000000));
            System.Console.WriteLine(swDequeue.Elapsed.TotalSeconds);
        }

        [TestMethod]
        public void Test()
        {
            m_wait = new ManualResetEvent(false);
            m_collection = new IsolatedQueue<int>();

            ThreadPool.QueueUserWorkItem(RunTwo);
            m_wait.WaitOne();
            m_wait.Reset();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int x = 0; x < cnt; x++)
            {
                if (x % 10000 == 0)
                {
                    System.Console.WriteLine(m_collection.Count);
                    Thread.Sleep(1);
                }
                m_collection.Enqueue(x);
            }
            sw.Stop();
            System.Console.WriteLine(sw.Elapsed.TotalSeconds);
            m_wait.WaitOne();
        }

        private void RunTwo(object state)
        {
            m_wait.Set();
            //SpinWait wait = new SpinWait();
            //Thread.Sleep(1000);
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int value = 0;
            int count = 0;
            int countRepeats = 0;
            while (count < cnt)
            {
                while (m_collection.TryDequeue(out value))
                {
                    Assert.AreEqual(count, value);
                    count++;
                }
                countRepeats++;
                Thread.Sleep(1);
            }
            sw.Stop();
            System.Console.WriteLine(sw.Elapsed.TotalSeconds);
            System.Console.WriteLine(countRepeats);
            m_wait.Set();
            Assert.AreEqual(count, cnt);
        }

        [TestMethod]
        public void TestBulk()
        {
            int[] sendBuffer = new int[3];

            m_wait = new ManualResetEvent(false);
            m_collection = new IsolatedQueue<int>();

            ThreadPool.QueueUserWorkItem(RunTwoBulk);
            m_wait.WaitOne();
            m_wait.Reset();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int x = 0; x < cnt; x++)
            {
                if (x % 10000 == 0)
                {
                    System.Console.WriteLine(m_collection.Count);
                    Thread.Sleep(1);
                }
                sendBuffer[0] = x;
                sendBuffer[1] = x;
                sendBuffer[2] = x;
                m_collection.Enqueue(sendBuffer, 0, 3);
            }
            sw.Stop();
            System.Console.WriteLine(sw.Elapsed.TotalSeconds);
            m_wait.WaitOne();
        }

        private void RunTwoBulk(object state)
        {
            m_wait.Set();
            //SpinWait wait = new SpinWait();
            //Thread.Sleep(1000);
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int value = 0;
            int count = 0;
            int countRepeats = 0;
            while (count < cnt)
            {
                while (m_collection.TryDequeue(out value))
                {
                    if (value != count)
                        throw new Exception();
                    while (!m_collection.TryDequeue(out value)) ;
                    
                    if (value != count)
                        throw new Exception();
                    while (!m_collection.TryDequeue(out value)) ;

                    if (value != count)
                        throw new Exception();

                    count++;
                }
                countRepeats++;
                Thread.Sleep(1);
            }
            sw.Stop();
            System.Console.WriteLine(sw.Elapsed.TotalSeconds);
            System.Console.WriteLine(countRepeats);
            m_wait.Set();
            Assert.AreEqual(count, cnt);
        }


        [TestMethod]
        public void TestCount()
        {
            m_collection = new IsolatedQueue<int>();
            int value = 0;
            long count = 0;

            for (int x = 0; x < 100000; x++)
            {
                if (m_collection.Count != count)
                    Assert.Fail("Count is wrong");
                m_collection.Enqueue(x);
                count++;

                if (x % 10 == 0)
                {
                    m_collection.TryDequeue(out value);
                    {
                        count--;
                        if (m_collection.Count != count)
                            Assert.Fail("Count is wrong");
                    }
                }
            }

            while (m_collection.TryDequeue(out value))
            {
                count--;
                if (m_collection.Count != count)
                    Assert.Fail("Count is wrong");
            }
        }


        [TestMethod]
        public void TestBulkDequeue()
        {
            m_wait = new ManualResetEvent(false);
            m_collection = new IsolatedQueue<int>();

            ThreadPool.QueueUserWorkItem(RunTwoBulkDequeue);
            m_wait.WaitOne();
            m_wait.Reset();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int x = 0; x < cnt; x++)
            {
                if (x % 10000 == 0)
                {
                    System.Console.WriteLine(m_collection.Count);
                    Thread.Sleep(1);
                }
                m_collection.Enqueue(x);
            }
            sw.Stop();
            System.Console.WriteLine(sw.Elapsed.TotalSeconds);
            m_wait.WaitOne();
        }

        private void RunTwoBulkDequeue(object state)
        {
            m_wait.Set();
            //SpinWait wait = new SpinWait();
            //Thread.Sleep(1000);
            Stopwatch sw = new Stopwatch();
            sw.Start();

            const int ArrayCount = 140;
            int[] array = new int[ArrayCount];

            int count = 0;
            int countRepeats = 0;
            while (count < cnt)
            {
                int totalCount = 0;
            ReReadNow:

                int itemCount = m_collection.Dequeue(array, 0, ArrayCount);
                totalCount += itemCount;
                if (itemCount > 0)
                {
                    for (int x = 0; x < itemCount; x++)
                    {
                        Assert.AreEqual(array[x], count);
                        count++;
                    }
                    if (itemCount == ArrayCount)
                        goto ReReadNow;
                }

                System.Console.WriteLine("Dequeued: " + totalCount);

                countRepeats++;
                Thread.Sleep(1);
            }
            sw.Stop();
            System.Console.WriteLine(sw.Elapsed.TotalSeconds);
            System.Console.WriteLine(countRepeats);
            m_wait.Set();
            Assert.AreEqual(count, cnt);
        }


    }
}
