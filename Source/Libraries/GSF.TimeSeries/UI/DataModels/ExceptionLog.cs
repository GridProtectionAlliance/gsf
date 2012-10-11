//******************************************************************************************************
//  ExceptionLog.cs - Gbtc
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
//  03/26/2012 - prasanthgs
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using TimeSeriesFramework.UI;

namespace openPDC.UI.DataModels
{
    /// <summary>
    /// Represents a record of information as defined in the log list collection.
    /// </summary>
    public class ExceptionLog : DataModelBase
    {
        #region [ Members ]

        private DateTime m_dateandTime;
        private string m_exceptionSource;
        private string m_exceptionType;
        private string m_exceptionMessage;
        private int m_index;
        private string m_details;

        #endregion

        #region [ Constructor ]
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ExceptionLog()
        {
            DateandTime = default(DateTime);
            ExceptionSource = String.Empty;
            ExceptionType = String.Empty;
            ExceptionMessage = String.Empty;
            Index = default(int);
            Details = String.Empty;
        }

        #endregion  

        #region [ Properties ]
        /// <summary>
        /// Gets or Sets Date and Time.
        /// </summary>
        public DateTime DateandTime
        {
            set
            {
                m_dateandTime = value;
            }
            get
            {
                return m_dateandTime;
            }
        }
        /// <summary>
        /// Gets or Sets exception source.
        /// </summary>
        public string ExceptionSource
        {
            set
            {
                m_exceptionSource = value;
            }
            get
            {
                return m_exceptionSource;
            }
        }
        /// <summary>
        /// Gets or Sets exception type.
        /// </summary>
        public string ExceptionType
        {
            set
            {
                m_exceptionType = value;
            }
            get
            {
                return m_exceptionType;
            }
        }
        /// <summary>
        /// Gets or Sets exception index.
        /// </summary>
        public int Index
        {
            set
            {
                m_index = value;
            }
            get
            {
                return m_index;
            }
        }
        /// <summary>
        /// Gets or Sets exception Message.
        /// </summary>
        public string ExceptionMessage
        {
            set
            {
                m_exceptionMessage = value;
            }
            get
            {
                return m_exceptionMessage;
            }
        }
        /// <summary>
        /// Gets or Sets exception brief details.
        /// </summary>
        public string Details
        {
            get 
            { 
                return m_details; 
            }
            set 
            {
                m_details = value; 
            }
        }

        #endregion
    }
}
