//******************************************************************************************************
//  ThreadSafeQueue.h - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  03/27/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

#ifndef __THREAD_SAFE_QUEUE_H
#define __THREAD_SAFE_QUEUE_H

#include <queue>

#include "CommonTypes.h"

namespace GSF {
namespace TimeSeries
{
    // Simple queue with locking mechanisms to make its operations thread-safe.
    //
    // The ThreadSafeQueue was designed for a multiple-producer/single-consumer
    // scenario in order to synchronize an operation by dispatching it to a
    // single thread and process in FIFO order.
    //
    // The use of multiple consumers may cause undesired effects.
    template <class T>
    class ThreadSafeQueue
    {
    private:
        Mutex m_mutex;
        WaitHandle m_dataWaitHandle;
        std::queue<T> m_queue;
        bool m_release;

    public:
        // Creates a new instance.
        ThreadSafeQueue()
            : m_release(false)
        {
        }

        // Releases all threads waiting for data.
        ~ThreadSafeQueue();

        // Inserts an item into the queue.
        void Enqueue(T item);

        // Removes an item from the
        // queue and returns that item.
        T Dequeue();

        // Empties the queue.
        void Clear();

        // Returns the number of
        // items left in the queue.
        uint32_t Size();

        // Waits for data to be inserted into the queue.
        // If there is already data in the queue,
        // this method will not wait.
        //
        // Since the queue was designed for only a single consumer,
        // calling this method from multiple threads may have undesired effects.
        void WaitForData();

        // Releases all threads waiting for data.
        //
        // Further calls to WaitForData will not wait regardless of the amount
        // of data in the queue. To make the queue usable again, call Reset.
        void Release();

        // Resets the "release valve" for threads calling WaitForData.
        void Reset();
    };

    // Releases all threads waiting for data.
    template <class T>
    ThreadSafeQueue<T>::~ThreadSafeQueue()
    {
        Release();
    }

    // Inserts an item into the queue.
    template <class T>
    void ThreadSafeQueue<T>::Enqueue(T item)
    {
        ScopeLock lock(m_mutex);
        m_queue.push(item);
        m_dataWaitHandle.notify_one();
    }

    // Removes an item from the
    // queue and returns that item.
    template <class T>
    T ThreadSafeQueue<T>::Dequeue()
    {
        ScopeLock lock(m_mutex);
        T item = m_queue.front();
        m_queue.pop();
        return item;
    }

    // Empties the queue.
    template <class T>
    void ThreadSafeQueue<T>::Clear()
    {
        ScopeLock lock(m_mutex);

        while (!m_queue.empty())
            m_queue.pop();
    }

    // Returns the number of
    // items left in the queue.
    template <class T>
    uint32_t ThreadSafeQueue<T>::Size()
    {
        ScopeLock lock(m_mutex);
        return m_queue.size();
    }

    // Waits for data to be inserted into the queue.
    // If there is already data in the queue,
    // this method will not wait.
    template <class T>
    void ThreadSafeQueue<T>::WaitForData()
    {
        UniqueLock lock(m_mutex);

        while (m_queue.empty() && !m_release)
            m_dataWaitHandle.wait(lock);
    }

    // Releases all threads waiting for data.
    template <class T>
    void ThreadSafeQueue<T>::Release()
    {
        ScopeLock lock(m_mutex);

        m_release = true;
        m_dataWaitHandle.notify_all();
    }

    // Resets the "release valve" for threads calling WaitForData.
    // This can be called after Release so that the queue can be
    // used again.
    template <class T>
    void ThreadSafeQueue<T>::Reset()
    {
        ScopeLock lock(m_mutex);
        m_release = false;
    }
}}

#endif