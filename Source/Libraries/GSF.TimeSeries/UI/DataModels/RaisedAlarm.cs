//******************************************************************************************************
//  RaisedAlarm.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  02/16/2012 - Stephen C. Wills
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

namespace GSF.TimeSeries.UI.DataModels
{
    /// <summary>
    /// Represents a record of <see cref="RaisedAlarm"/> information as defined in the database.
    /// </summary>
    public class RaisedAlarm : DataModelBase
    {
        #region [ Members ]

        // Fields
        private int m_id;
        private int m_severity;
        private string m_timeRaised;
        private string m_tagName;
        private string m_description;
        //private double m_value;

        private string m_severityText;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="RaisedAlarm"/> ID
        /// </summary>
        public int ID
        {
            get
            {
                return m_id;
            }
            set
            {
                m_id = value;
                OnPropertyChanged("ID");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="RaisedAlarm"/> Severity
        /// </summary>
        public int Severity
        {
            get
            {
                return m_severity;
            }
            set
            {
                m_severity = value;
                OnPropertyChanged("Severity");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="RaisedAlarm"/> TimeRaised
        /// </summary>
        public string TimeRaised
        {
            get
            {
                return m_timeRaised;
            }
            set
            {
                m_timeRaised = value;
                OnPropertyChanged("TimeRaised");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="RaisedAlarm"/> TagName
        /// </summary>
        public string TagName
        {
            get
            {
                return m_tagName;
            }
            set
            {
                m_tagName = value;
                OnPropertyChanged("TagName");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="RaisedAlarm"/> Description
        /// </summary>
        public string Description
        {
            get
            {
                return m_description;
            }
            set
            {
                m_description = value;
                OnPropertyChanged("Description");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="RaisedAlarm"/> SeverityText
        /// </summary>
        public string SeverityText
        {
            get
            {
                return m_severityText;
            }
            set
            {
                m_severityText = value;
                OnPropertyChanged("SeverityText");
            }
        }

        #endregion
    }
}
