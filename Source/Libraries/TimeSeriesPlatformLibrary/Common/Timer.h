//******************************************************************************************************
//  Timer.h - Gbtc
//
//  Copyright © 2019, Grid Protection Alliance.  All Rights Reserved.
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
//  01/29/2019 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#ifndef _TIMER_H
#define _TIMER_H

#include <utility>
#include "CommonTypes.h"

namespace GSF
{
    class Timer;
    typedef std::function<void(Timer* timer, void* userData)> TimerElapsedCallback;

    class Timer // NOLINT
    {
    private:
        SharedPtr<GSF::Thread> m_timerThread;
        GSF::IOContext m_timerContext;
        boost::asio::deadline_timer m_timer;
        int32_t m_interval;
        TimerElapsedCallback m_callback;
        void* m_userData;
        bool m_autoReset;
        bool m_disposing;

        void TimerThread()
        {
            try
            {
                // Running context will block while items are queued to execute
                m_timerContext.run();
            }
            catch (...)
            {
                return;
            }

            if (m_disposing)
                return;

            // Reset timer thread when context has nothing left to run
            m_timerThread.reset();
            m_timerThread = nullptr;

            // Restart context in preparation for next run
            m_timerContext.restart();
        }

        void TimerElaspsed(const GSF::ErrorCode& error)
        {
            if (error)
                return;

            m_callback(this, m_userData);

            if (m_autoReset)
                Start();
        }

    public:
        Timer() : Timer(1000, nullptr, false)
        {
        }

        Timer(const int32_t interval, TimerElapsedCallback callback, const bool autoReset = false) :
            m_timerThread(nullptr),
            m_timer(m_timerContext),
            m_interval(interval),
            m_callback(std::move(callback)),
            m_userData(nullptr),
            m_autoReset(autoReset),
            m_disposing(false)
        {
        }

        ~Timer()
        {
            m_disposing = true;
            Stop();
        }

        int32_t GetInterval() const
        {
            return m_interval;
        }

        void SetInterval(const int32_t value)
        {
            m_interval = value;
        }

        TimerElapsedCallback GetCallback() const
        {
            return m_callback;
        }

        void SetCallback(TimerElapsedCallback value)
        {
            m_callback = value;
        }

        const void* GetUserData() const
        {
            return m_userData;
        }

        void SetUserData(void* value)
        {
            m_userData = value;
        }

        bool GetAutoReset() const
        {
            return m_autoReset;
        }

        void SetAutoReset(const bool value)
        {
            m_autoReset = value;
        }

        void Start()
        {
            if (m_callback == nullptr)
                throw std::invalid_argument("Cannot start timer, no callback function has been defined.");

            m_timer.expires_from_now(boost::posix_time::milliseconds(m_interval));
            m_timer.async_wait(boost::bind(&Timer::TimerElaspsed, this, boost::asio::placeholders::error));

            if (m_timerThread == nullptr)
                m_timerThread = NewSharedPtr<GSF::Thread>(boost::bind(&Timer::TimerThread, this));
        }

        void Stop()
        {
            m_timer.cancel();
        }
    };

    typedef GSF::SharedPtr<Timer> TimerPtr;
}

#endif