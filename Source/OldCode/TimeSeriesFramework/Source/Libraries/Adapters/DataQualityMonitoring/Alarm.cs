//******************************************************************************************************
//  Alarm.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  01/31/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using Ciloci.Flee;
using TimeSeriesFramework;
using TVA;

namespace DataQualityMonitoring
{
    public class Alarm
    {
        #region [ Members ]

        // Fields
        private string m_expressionText;
        private ExpressionContext m_expressionContext;
        private IGenericExpression<bool> m_expression;

        private double m_lastValue;
        private long m_lastChanged;
        private long m_latestTimestamp;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="Alarm"/> class.
        /// </summary>
        public Alarm()
        {
            m_expressionContext = new ExpressionContext(this);
            m_expressionContext.Imports.AddType(typeof(Math));
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the expression text which determines
        /// the condition on which an alarm event occurs.
        /// </summary>
        public string ExpressionText
        {
            get
            {
                return m_expressionText;
            }
            set
            {
                m_expressionContext.Variables["value"] = 0.0;
                m_expression = m_expressionContext.CompileGeneric<bool>(value);
                m_expressionText = value;
            }
        }
        
        /// <summary>
        /// Gets or sets the identification number of the alarm.
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// Gets or sets the identification number of the
        /// signal whose value is monitored by the alarm.
        /// </summary>
        public Guid SignalID { get; set; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Tests the given measurement to determine
        /// whether its value triggers an alarm event.
        /// </summary>
        /// <param name="signal">The signal whose value is to be checked.</param>
        /// <returns>True if the event is triggers; false otherwise.</returns>
        public bool Condition(IMeasurement signal)
        {
            long signalTimestamp = signal.Timestamp;

            // Keep track of the last time the value changed
            if (signal.Value != m_lastValue)
            {
                m_lastValue = signal.Value;
                m_lastChanged = signalTimestamp;
            }

            // Keep track of the latest timestamp
            m_latestTimestamp = signalTimestamp;

            // Set the value of the expression variable
            m_expressionContext.Variables["value"] = signal.AdjustedValue;

            // Evaluate the expression
            return m_expression.Evaluate();
        }

        // Determines whether the value has
        // flatlined over the given time interval
        private bool Flatlined(double seconds)
        {
            long dist = Ticks.FromSeconds(seconds);
            long diff = m_latestTimestamp - m_lastChanged;
            return diff >= dist;
        }

        #endregion
    }
}
