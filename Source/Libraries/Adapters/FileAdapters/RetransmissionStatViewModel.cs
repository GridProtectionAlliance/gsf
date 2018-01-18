//******************************************************************************************************
//  RetransmissionStatViewModel.cs - Gbtc
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
//  07/31/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

#if !MONO
using System.ComponentModel;
using System.Windows;

namespace FileAdapters
{
    /// <summary>
    /// View model for the <see cref="RetransmissionStatPicker"/>.
    /// </summary>
    public class RetransmissionStatViewModel : DependencyObject, INotifyPropertyChanged
    {
        #region [ Members ]

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        // Fields
        private bool m_useFilterExpression;
        private int m_signalIndex;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="RetransmissionStatViewModel"/> class.
        /// </summary>
        public RetransmissionStatViewModel()
        {
            UseFilterExpression = true;
            SignalIndex = 13;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the flag which determines whether to use a filter
        /// expression to filter measurements in the measurement picker.
        /// </summary>
        public bool UseFilterExpression
        {
            get
            {
                return m_useFilterExpression;
            }
            set
            {
                m_useFilterExpression = value;
                OnPropertyChanged("UseFilterExpression");
                OnPropertyChanged("FilterExpression");
            }
        }

        /// <summary>
        /// Gets or sets the signal index used for filtering
        /// measurements in the measurement picker.
        /// </summary>
        public int SignalIndex
        {
            get
            {
                return m_signalIndex;
            }
            set
            {
                m_signalIndex = value;
                OnPropertyChanged("SignalIndex");
                OnPropertyChanged("FilterExpression");
            }
        }

        /// <summary>
        /// Gets the filter expression used for filtering
        /// measurements in the measurement picker.
        /// </summary>
        public string FilterExpression
        {
            get
            {
                return m_useFilterExpression
                    ? string.Format("SignalReference LIKE '%!PUB-ST{0}'", m_signalIndex)
                    : string.Empty;
            }
        }

        #endregion

        #region [ Methods ]

        // Triggers the PropertyChanged event.
        private void OnPropertyChanged(string propertyName)
        {
            if ((object)PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
#endif