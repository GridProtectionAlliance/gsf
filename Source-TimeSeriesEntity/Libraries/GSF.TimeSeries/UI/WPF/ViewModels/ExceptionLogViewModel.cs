//******************************************************************************************************
//  ExceptionLogViewModel.cs - Gbtc
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
//  3/26/2012 - prasanthgs
//       Generated original version of source code.
//  04/12/2012 - prasanthgs
//       Reworked as per the comments of codeplex reviewers.
//       Code Optimized.
//
//******************************************************************************************************

using System;
using System.Linq;
using System.Windows.Input;
using openPDC.UI.DataModels;
using TimeSeriesFramework.UI.Commands;
using TimeSeriesFramework.UI;
using openPDC.UI.Modal;
using System.Windows.Threading;

namespace openPDC.UI.ViewModels
{
    /// <summary>
    /// Class which implements page view model for the ExceptionLog
    /// </summary>
    internal class ExceptionLogViewModel : PagedViewModelBase<ExceptionLog, int>
    {
        #region [ Members ]

        private ExceptionMonitor m_exMonitor;
        private Dispatcher m_dispatcher;
        private string m_LastSortMemberPath;
        private RelayCommand m_showCommand;

        #endregion

        #region [ Constructor]
        
        /// <summary>
        /// Creates a new instance of <see cref="ExceptionLogViewModel"/> class.
        /// </summary>
        /// <param name="itemsPerPage"> No of Maximum items in each page.</param>
        /// <param name="autoSave">Default value true.</param>
        public ExceptionLogViewModel(int itemsPerPage, bool autoSave = true)
            : base(itemsPerPage, autoSave)
        {
            m_dispatcher = Dispatcher.CurrentDispatcher;
            LastSortMemberPath = String.Empty;
        }
        #endregion

        #region [ Properties ]
        /// <summary>
        /// Gets or sets <see cref="LastSortMemberPath"/> Sort Member Path of DataGrid.
        /// </summary>
        public String LastSortMemberPath
        {
            get
            {
                return m_LastSortMemberPath;
            }

            set
            {
                m_LastSortMemberPath = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ExceptionMonitor"/> used to receive
        /// updates about recent Exceptions.
        /// </summary>
        public ExceptionMonitor Monitor
        {
            get
            {
                return m_exMonitor;
            }
            set
            {
                if ((object)m_exMonitor != null)
                    m_exMonitor.UpdatedExceptions -= Monitor_UpdatedExceptions;

                m_exMonitor = value;

                if ((object)m_exMonitor != null)
                {
                    LoadExceptions();
                    m_exMonitor.UpdatedExceptions += Monitor_UpdatedExceptions;
                }
            }
        }

        void Monitor_UpdatedExceptions(object sender, EventArgs e)
        {
            m_dispatcher.Invoke(new Action(LoadExceptions));
        }

        /// <summary>
        /// Gets Show command.
        /// </summary>
        public ICommand ShowCommand
        {
            get
            {
                if (m_showCommand == null)
                    m_showCommand = new RelayCommand(ShowExceptionLog);

                return m_showCommand;
            }
        }

        #endregion

        #region [ Methods ]
        /// <summary>
        /// Update the DataGrid with latest exception list.
        /// Also sorts the data if sort is applied before refresh.
        /// </summary>
        private void LoadExceptions()
        {
            int CurIdx = default(int);

            if ((object)m_exMonitor != null)
            {
                CurIdx = GetCurrentItemKey();
                ItemsSource = m_exMonitor.GetRecentExceptions();
                
                //Sort and select current item.
                Sort(CurIdx);
            }
        }

        /// <summary>
        /// Initiates parent sortmethod.
        /// Set current selected item after sort based on display index.
        /// </summary>
        /// <param name="idxCurItem">Index of selected item before sort</param>
        public void Sort(int idxCurItem)
        {
            ExceptionLog newItem = ItemsSource.SingleOrDefault(ex => ex.Index == idxCurItem);

            if ((object)LastSortMemberPath != null && LastSortMemberPath != String.Empty)
                SortData(LastSortMemberPath);

            if ((object)newItem != null)
            {
                CurrentItem = newItem;
            }
            else
            {
                CurrentSelectedIndex = -1;
            }
        }

        /// <summary>
        /// Show brief exception log details.
        /// </summary>
        private void ShowExceptionLog()
        {
            LogDisplay _logDisplay = new LogDisplay(CurrentItem.Details);
            _logDisplay.ShowDialog();
        }

        /// <summary>
        /// Overriden method
        /// </summary>
        public override void Load()
        {
        }

        /// <summary>
        /// Overriden method
        /// Use current item index as key for setting 
        /// exception list selected item after refresh.
        /// </summary>
        /// <returns>Returns current item index.</returns>
        public override int GetCurrentItemKey()
        {
            return CurrentItem.Index;
        }

        /// <summary>
        /// Overriden method
        /// Get current item index.
        /// </summary>
        /// <returns>Returns String representation of current index.</returns>
        public override string GetCurrentItemName()
        {
            return CurrentItem.Index.ToString();
        }
        
        /// <summary>
        /// Checks for new item in Exception list.
        /// Since this is an exception viewer, no new records can be added.
        /// Always returns false.
        /// </summary>
        public override bool IsNewRecord
        {
            get { return false; }
        } 

        #endregion
    }
}
