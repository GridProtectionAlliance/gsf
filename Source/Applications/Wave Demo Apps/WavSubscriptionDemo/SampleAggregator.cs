//******************************************************************************************************
//  SampleAggregator.cs - Gbtc
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
//  07/07/2011 - Stephen C. Wills
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

//******************************************************************************************************
//
//  Code translated from "NAudioWpfDemo" developed by Mark Heath
//  found in the "NAudio" project: http://naudio.codeplex.com/
//
//  Copyright (c) Mark Heath 2008
//  Microsoft Public License (Ms-PL): http://www.opensource.org/licenses/ms-pl
//
//******************************************************************************************************

#endregion

using System;
using System.Diagnostics;
using System.Windows;

namespace WavSubscriptionDemo
{
    internal class SampleAggregator
    {
        // volume
        public event EventHandler<MaxSampleEventArgs> MaximumCalculated;

        private float m_maxValue;
        private float m_minValue;
        private int m_count;

        public int NotificationCount { get; set; }

        public void Reset()
        {
            m_count = 0;
            m_maxValue = m_minValue = 0;
        }

        public void Add(float leftValue, float rightValue)
        {
            m_maxValue = Math.Max(m_maxValue, Math.Max(leftValue, rightValue));
            m_minValue = Math.Min(m_minValue, Math.Min(leftValue, rightValue));
            m_count++;

            if (m_count < NotificationCount || NotificationCount <= 0)
                return;

            MaximumCalculated?.Invoke(this, new MaxSampleEventArgs(m_minValue, m_maxValue));

            Reset();
        }
    }

    public class MaxSampleEventArgs : EventArgs
    {
        [DebuggerStepThrough]
        public MaxSampleEventArgs(float minValue, float maxValue)
        {
            MaxSample = maxValue;
            MinSample = minValue;
        }

        public float MaxSample { get; }

        public float MinSample { get; }
    }
}

