//******************************************************************************************************
//  ErrorLogUserControl.xaml.cs - Gbtc
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
//  03/27/2012 - prasanthgs
//       Generated original version of source code.
//  04/12/2012 - prasanthgs
//       Reworked as per the comments of codeplex reviewers.
//       Code Optimized.
//
//******************************************************************************************************

using System;
using System.Windows;
using System.Windows.Controls;
using TimeSeriesFramework.UI.ViewModels;

namespace TimeSeriesFramework.UI.UserControls
{
    /// <summary>
    /// Interaction logic for ErrorLogUserControl.xaml
    /// </summary>
    public partial class ErrorLogUserControl : UserControl
    {
        #region [ Members ]

        private ErrorLogViewModel m_dataContext;
        private ErrorMonitor m_ErrorMonitor;

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Constructor for Error user control class.
        /// Creates data source with page size 18.
        /// </summary>
        public ErrorLogUserControl()
        {
            InitializeComponent();
            m_dataContext = new ErrorLogViewModel(18);
            this.DataContext = m_dataContext;
        }

        #endregion

        #region [ Methods ]

        private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            m_dataContext.LastSortMemberPath = e.Column.SortMemberPath;
            m_dataContext.Sort(m_dataContext.GetCurrentItemKey());
        }

        private void ButtonRestore_Click(object sender, RoutedEventArgs e)
        {
            m_dataContext.Monitor.ResetRefreshInterval();
            TextBlockErrorRefreshInterval.Text = m_dataContext.Monitor.RefreshInterval.ToString();
            TextBoxRefreshInterval.Text = m_dataContext.Monitor.RefreshInterval.ToString();
            PopupSettings.IsOpen = false;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            PopupSettings.IsOpen = false;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            int refreshInterval = default(int);

            try
            {
                if (int.TryParse(TextBoxRefreshInterval.Text, out refreshInterval) && (refreshInterval > 0))
                {
                    m_dataContext.Monitor.RefreshInterval = refreshInterval;
                    TextBlockErrorRefreshInterval.Text = refreshInterval.ToString();
                }
                else
                {
                    m_dataContext.DisplayStatusMessage("Please provide an integer value between 1 and " + Int32.MaxValue / 1000);
                }
            }
            catch
            {
                m_dataContext.DisplayStatusMessage("Please provide an integer value between 1 and " + Int32.MaxValue / 1000);
            }
            finally
            {
                PopupSettings.IsOpen = false;
            }
        }

        private void ButtonDisplaySettings_Click(object sender, RoutedEventArgs e)
        {
            PopupSettings.IsOpen = true;
        }

        private void ErrorViewer_Loaded(object sender, RoutedEventArgs e)
        {
            if ((object)ErrorMonitor.Default == null)
                m_ErrorMonitor = new ErrorMonitor(true);

            m_dataContext.Monitor = ErrorMonitor.Default ?? m_ErrorMonitor;

            TextBlockErrorRefreshInterval.Text = m_dataContext.Monitor.RefreshInterval.ToString();
            TextBoxRefreshInterval.Text = m_dataContext.Monitor.RefreshInterval.ToString();
            m_dataContext.Monitor.Start();
        }

        private void ErrorViewer_UnLoaded(object sender, RoutedEventArgs e)
        {
            if ((object)m_dataContext.Monitor != null)
                m_dataContext.Monitor.Dispose();
        }

        #endregion
    }
}
