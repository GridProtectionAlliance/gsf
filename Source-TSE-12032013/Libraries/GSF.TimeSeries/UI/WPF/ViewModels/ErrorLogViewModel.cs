//******************************************************************************************************
//  ErrorLogViewModel.cs - Gbtc
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
//  3/26/2012 - prasanthgs
//       Generated original version of source code.
//  04/12/2012 - prasanthgs
//       Reworked as per the comments of codeplex reviewers.
//       Code Optimized.
//  09/10/2012 - Aniket Salver
//       Added paging technique and implemented sorting.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GSF.TimeSeries.UI.Commands;
using GSF.TimeSeries.UI.DataModels;
using GSF.TimeSeries.UI.Modal;

namespace GSF.TimeSeries.UI.ViewModels
{
    internal class ErrorLogViewModel : PagedViewModelBase<ErrorLog, int>
    {
        #region [ Members ]

        //private ErrorMonitor m_exMonitor;
          private Dispatcher m_dispatcher;
        private RelayCommand m_showCommand;
        //private string m_currentSortMemberPath;
        //private ListSortDirection m_currentSortDirection;

        #endregion

        #region [ Constructor]
        
        /// <summary>
        /// Creates a new instance of <see cref="ErrorLogViewModel"/> class.
        /// </summary>
        /// <param name="itemsPerPage"> No of Maximum items in each page.</param>
        /// <param name="autoSave">Default value true.</param>
        public ErrorLogViewModel(int itemsPerPage, bool autoSave = true)
            : base(itemsPerPage, autoSave)
        {
            m_dispatcher = Dispatcher.CurrentDispatcher;
        }
        #endregion

        #region [ Properties ]

        /// <summary>
        /// Checks for new item in Error list.
        /// Since this is an error viewer, no new records can be added.
        /// Always returns false.
        /// </summary>
        public override bool IsNewRecord
        {
            get { return false; }
        }

        /// <summary>
        /// Gets Show command.
        /// </summary>
        public ICommand ShowCommand
        {
            get
            {
                if (m_showCommand == null)
                    m_showCommand = new RelayCommand(ShowErrorLog);

                return m_showCommand;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initialization to be done before the initial call to <see cref="Load"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            SortMember = "ID";
            SortDirection = "DESC";
        }

        /// <summary>
        /// Sorts model data.
        /// </summary>
        /// <param name="sortMemberPath">Member path for sorting.</param>
        /// <param name="sortDirection">Ascending or descending.</param>
        public override void SortData(string sortMemberPath, ListSortDirection sortDirection)
        {

            SortMember = sortMemberPath;
            SortDirection = (sortDirection == ListSortDirection.Descending) ? "DESC" : "ASC";
            ItemsKeys = null;
            Load();
        }

        /// <summary>
        /// Show brief error log details.
        /// </summary>
        private void ShowErrorLog()
        {
            ErrorDetailDisplay logDisplay = new ErrorDetailDisplay(CurrentItem.Detail);
            logDisplay.ShowDialog();
        }

        /// <summary>
        /// Overriden method
        /// </summary>
        public override void Load()
        {
            List<int> pageKeys;

            try
            {
                if ((object)ItemsKeys == null)
                {
                    ItemsKeys = ErrorLog.LoadKeys(null, SortMember, SortDirection);

                    if ((object)SortSelector != null)
                    {
                        if (SortDirection == "ASC")
                            ItemsKeys = ItemsKeys.OrderBy(SortSelector).ToList();
                        else
                            ItemsKeys = ItemsKeys.OrderByDescending(SortSelector).ToList();
                    }
                }

                pageKeys = ItemsKeys.Skip((CurrentPageNumber - 1) * ItemsPerPage).Take(ItemsPerPage).ToList();
                ItemsSource = ErrorLog.Load(null, pageKeys);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    Popup(ex.Message + Environment.NewLine + "Inner Exception: " + ex.InnerException.Message, "Load " + DataModelName + " Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, "Load " + DataModelName, ex.InnerException);
                }
                else
                {
                    Popup(ex.Message, "Load " + DataModelName + " Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, "Load " + DataModelName, ex);
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Overriden method
        /// Use current item index as key for setting 
        /// error list selected item after refresh.
        /// </summary>
        /// <returns>Returns current item index.</returns>
        public override int GetCurrentItemKey()
        {
            return CurrentItem.ID;
        }

        /// <summary>
        /// Overriden method
        /// Get current item index.
        /// </summary>
        /// <returns>Returns String representation of current index.</returns>
        public override string GetCurrentItemName()
        {
            return CurrentItem.ID.ToString();
        }

        #endregion
    }
}
