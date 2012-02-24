//******************************************************************************************************
//  RaisedAlarms.cs - Gbtc
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
//  02/16/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace TimeSeriesFramework.UI.ViewModels
{
    internal class RaisedAlarms : PagedViewModelBase<DataModels.RaisedAlarm, int>
    {
        #region [ Members ]

        // Fields
        private AlarmMonitor m_monitor;
        private Dispatcher m_dispatcher;
        private string m_currentSortMemberPath;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="RaisedAlarms"/> class.
        /// </summary>
        /// <param name="itemsPerPage">Integer value to determine number of items per page.</param>
        /// <param name="autoSave">Boolean value to determine is user changes should be saved automatically.</param>
        public RaisedAlarms(int itemsPerPage, bool autoSave = true)
            : base(itemsPerPage, autoSave)
        {
            m_dispatcher = Dispatcher.CurrentDispatcher;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="AlarmMonitor"/> used to receive updates about raised alarms.
        /// </summary>
        public AlarmMonitor Monitor
        {
            get
            {
                return m_monitor;
            }
            set
            {
                if((object)m_monitor != null)
                    m_monitor.RefreshedAlarms -= Monitor_RefreshedAlarms;

                m_monitor = value;

                if ((object)m_monitor != null)
                {
                    UpdateList();
                    m_monitor.RefreshedAlarms += Monitor_RefreshedAlarms;
                }
            }
        }

        /// <summary>
        /// Gets flag that indicates if <see cref="PagedViewModelBase{T1,T2}.CurrentItem"/> state is valid and can be saved.
        /// </summary>
        public override bool CanSave
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets flag that indicates if <see cref="PagedViewModelBase{T1,T2}.CurrentItem"/> can be deleted.
        /// </summary>
        public override bool CanDelete
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets flag that determines if <see cref="PagedViewModelBase{T1,T2}.CurrentItem"/> is a new record.
        /// </summary>
        public override bool IsNewRecord
        {
            get
            {
                return CurrentItem.ID == 0;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Sets the color of the given row based on the associated alarm's severity.
        /// </summary>
        /// <param name="row">The row whose color is to be set.</param>
        public void SetRowColor(DataGridRow row)
        {
            DataModels.RaisedAlarm item = row.Item as DataModels.RaisedAlarm;

            if ((object)item != null)
            {
                if (item.Severity >= (int)AlarmSeverity.Error)
                    row.Background = Brushes.LightGray;
                else if (item.Severity >= (int)AlarmSeverity.Critical)
                    row.Background = Brushes.Red;
                else if (item.Severity >= (int)AlarmSeverity.High)
                    row.Background = Brushes.Orange;
                else if (item.Severity >= (int)AlarmSeverity.Medium)
                    row.Background = Brushes.Yellow;
                else if(item.Severity >= (int)AlarmSeverity.Low)
                    row.Background = Brushes.LightGreen;
                else
                    row.Background = Brushes.White;
            }
        }

        /// <summary>
        /// Gets the primary key value of the <see cref="PagedViewModelBase{T1,T2}.CurrentItem"/>.
        /// </summary>
        /// <returns>The primary key value of the <see cref="PagedViewModelBase{T1,T2}.CurrentItem"/>.</returns>
        public override int GetCurrentItemKey()
        {
            return CurrentItem.ID;
        }

        /// <summary>
        /// Gets the string based named identifier of the <see cref="PagedViewModelBase{T1,T2}.CurrentItem"/>.
        /// </summary>
        /// <returns>The string based named identifier of the <see cref="PagedViewModelBase{T1,T2}.CurrentItem"/>.</returns>
        public override string GetCurrentItemName()
        {
            return CurrentItem.TagName;
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public override void Load()
        {
            // Do nothing.
            // Data is loaded via the monitor event.
        }

        /// <summary>
        /// Sorts model data.
        /// </summary>
        /// <param name="sortMemberPath">Member path for sorting.</param>
        public override void SortData(string sortMemberPath)
        {
            List<DataModels.RaisedAlarm> itemsSource;

            m_currentSortMemberPath = sortMemberPath;

            if (sortMemberPath != "Severity")
            {
                base.SortData(sortMemberPath);
            }
            else
            {
                if ((object)m_monitor != null)
                {
                    itemsSource = ItemsSource.OrderBy(alarm => alarm.Severity)
                        .OrderBy(alarm => alarm.Severity == (int)AlarmSeverity.Error)
                        .ToList();

                    ItemsSource = new ObservableCollection<DataModels.RaisedAlarm>(itemsSource);
                }
            }
        }

        // Handles the alarm monitor's RefreshedAlarms event.
        private void Monitor_RefreshedAlarms(object sender, EventArgs e)
        {
            m_dispatcher.Invoke(new Action(UpdateList));
        }

        // Updates the list of alarms displayed to the user.
        private void UpdateList()
        {
            int key;
            DataModels.RaisedAlarm newCurrentItem;

            if ((object)m_monitor != null)
            {
                key = GetCurrentItemKey();
                ItemsSource = m_monitor.GetAlarmList();
                newCurrentItem = ItemsSource.SingleOrDefault(alarm => alarm.ID == key);

                if ((object)m_currentSortMemberPath != null)
                    SortData(m_currentSortMemberPath);

                if ((object)newCurrentItem != null)
                {
                    CurrentItem = newCurrentItem;
                }
                else
                {
                    Clear();
                    CurrentSelectedIndex = -1;
                }
            }
        }

        #endregion
    }
}
