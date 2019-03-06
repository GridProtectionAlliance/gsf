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

#include "CommonTypes.h"
#include <boost/date_time/posix_time/posix_time.hpp>
#include <boost/thread/thread.hpp> 

namespace GSF
{
    class Timer;
    typedef std::function<void(Timer* timer, void* userData)> TimerElapsedCallback;

    class Timer // NOLINT
    {
    private:
        SharedPtr<GSF::Thread> m_timerThread;
        int32_t m_interval;
        TimerElapsedCallback m_callback;
        void* m_userData;
        bool m_autoReset;
        bool m_running;

        void TimerThread()
        {
            m_running = true;

            do
            {
                boost::this_thread::sleep(boost::posix_time::milliseconds(m_interval));
                m_callback(this, m_userData);
            }
            while (m_autoReset && m_running);

            m_running = false;
        }

    public:
        Timer() : Timer(1000, nullptr, false)
        {
        }

        Timer(const int32_t interval, TimerElapsedCallback callback, const bool autoReset = false) :
            m_timerThread(nullptr),
            m_interval(interval),
            m_callback(std::move(callback)),
            m_userData(nullptr),
            m_autoReset(autoReset),
            m_running(false)
        {
        }

        ~Timer()
        {
            Stop();
        }

        int32_t GetInterval() const
        {
            return m_interval;
        }

        void SetInterval(const int32_t value)
        {
            if (value != m_interval)
            {
                const bool restart = m_running;
                Stop();

                m_interval = value;

                if (restart)
                    Start();
            }
        }

        TimerElapsedCallback GetCallback() const
        {
            return m_callback;
        }

        void SetCallback(TimerElapsedCallback value)
        {
            m_callback = std::move(value);
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

            if (m_running)
                Stop();

            m_timerThread = NewSharedPtr<GSF::Thread>(boost::bind(&Timer::TimerThread, this));
        }

        void Stop()
        {
            m_running = false;

            if (m_timerThread != nullptr)
                m_timerThread->interrupt();

            m_timerThread.reset();
        }
    };

    typedef GSF::SharedPtr<Timer> TimerPtr;
}

#endif