//******************************************************************************************************
//  ViewModelBase.cs - Gbtc
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
//  03/25/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System.ComponentModel;
using System;
using System.Windows;
using System.Collections.ObjectModel;
using TimeSeriesFramework.UI.Commands;
using System.Windows.Input;

namespace TimeSeriesFramework.UI
{
    /// <summary>
    /// Represents an abstract class which all ViewModel objects derive from.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        #region [ Members ]
        
        //Events
        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        
        #endregion

        #region [ Properties ]

        public Action<string, string, MessageBoxImage> Popup
        {
            get 
            { 
                return (Action<string, string, MessageBoxImage>)((message, caption, messageBoxImage) => MessageBox.Show( Application.Current.MainWindow, message, caption, MessageBoxButton.OK, messageBoxImage)); 
            }
        }

        public Func<string, string, bool> Confirm
        {
            get 
            {
                return (Func<string, string, bool>)((message, caption) => MessageBox.Show(Application.Current.MainWindow, message, caption, MessageBoxButton.YesNo) == MessageBoxResult.Yes);
            }
        }

        #endregion

        #region [ Methods ]

        #region [ INotifyPropertyChanged Implementation ]

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion

        #endregion
    }
}
